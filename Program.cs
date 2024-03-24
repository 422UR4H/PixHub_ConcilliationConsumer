using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Http.Json;
using Npgsql;
using Domain.Transactions;
using ConcilliationConsumer.Dtos;
using ConcilliationConsumer.Files;
using ConcilliationConsumer.Database;


/** Database **/
var connString = "Host=localhost;Username=postgres;Password=postgres;Database=postgres";

/** RabbitMQ **/
string queueName = "concilliation";
ConnectionFactory factory = new()
{
  HostName = "localhost",
  UserName = "admin",
  Password = "admin"
};
IConnection connection = factory.CreateConnection();
IModel channel = connection.CreateModel();

channel.QueueDeclare(
  queue: queueName,
  durable: true,
  exclusive: false,
  autoDelete: false,
  arguments: null
);

const int MIN_BATCH_SIZE = 200_000;

Console.WriteLine("[*] Waiting for messages...");

EventingBasicConsumer consumer = new(channel);
consumer.Received += async (model, ea) =>
{
  var startTime = DateTime.Now;

  await using var conn = new NpgsqlConnection(connString);
  await conn.OpenAsync();

  HttpClient httpClient = new();
  var body = ea.Body.ToArray();
  var message = Encoding.UTF8.GetString(body);
  // Console.WriteLine("[x] Received {0}", message);

  PublishConcilliationDTO? dto = JsonSerializer.Deserialize<PublishConcilliationDTO>(message);

  if (dto is null)
  {
    Console.WriteLine("Content body is not valid! Concilliation faild.");
    channel.BasicReject(ea.DeliveryTag, false);
    return;
  }
  Console.WriteLine($"Processing concilliation");

  string sqlString = @"
    SELECT COUNT(*)
      FROM ""Payments"" AS p
    INNER JOIN ""PaymentProviderAccount"" AS origin
      ON origin.""Id"" = p.""PaymentProviderAccountId""
    INNER JOIN ""PixKey"" AS pk
      ON pk.""Id"" = p.""PixKeyId""
    INNER JOIN ""PaymentProviderAccount"" AS destiny
      ON destiny.""Id"" = pk.""PaymentProviderAccountId""
    WHERE date_trunc('day', p.""CreatedAt"") = @date AND (
      origin.""PaymentProviderId"" = @providerid OR
      destiny.""PaymentProviderId"" = @providerid
    )";

  int batchSize;
  await using (var cmd = new NpgsqlCommand(sqlString, conn))
  {
    cmd.Parameters.AddWithValue("date", dto.Date);
    cmd.Parameters.AddWithValue("providerid", dto.PaymentProviderId);

    await using var dbReader = await cmd.ExecuteReaderAsync();
    if (dbReader.Read())
    {
      batchSize = dbReader.GetInt32(0);
    }
    else
    {
      Console.WriteLine("Counting by this date and provider is not possible!");
      return;
    }
  }
  // refactor this magic number
  batchSize /= 5;
  if (batchSize < MIN_BATCH_SIZE) batchSize = MIN_BATCH_SIZE;
  Transaction transactions = new(batchSize);

  sqlString = @"
    SELECT p.""TransactionId"", p.""Status""
      FROM ""Payments"" AS p
    INNER JOIN ""PaymentProviderAccount"" AS origin
      ON origin.""Id"" = p.""PaymentProviderAccountId""
    INNER JOIN ""PixKey"" AS pk
      ON pk.""Id"" = p.""PixKeyId""
    INNER JOIN ""PaymentProviderAccount"" AS destiny
      ON destiny.""Id"" = pk.""PaymentProviderAccountId""
    WHERE date_trunc('day', p.""CreatedAt"") = @date AND (
      origin.""PaymentProviderId"" = @providerid OR
      destiny.""PaymentProviderId"" = @providerid
    )
    ORDER BY p.""Id""
    OFFSET @offset
    LIMIT @limit";

  await using (var cmd = new NpgsqlCommand(sqlString, conn))
  {
    cmd.Parameters.AddWithValue("date", dto.Date);
    cmd.Parameters.AddWithValue("providerid", dto.PaymentProviderId);
    cmd.Parameters.AddWithValue("offset", 0);
    cmd.Parameters.AddWithValue("limit", transactions.BatchSize);

    if (!File.Exists(dto.File))
    {
      Console.WriteLine("This file does not exist!");
      return;
    }

    int i = 0;
    using StreamReader fileReader = new(dto.File);
    do
    {
      await using var dbReader = await cmd.ExecuteReaderAsync();

      JSONReader.ReadFile(fileReader, transactions);
      DBReader.ReadData(dbReader, transactions);

      i++;
      cmd.Parameters.Remove("offset");
      cmd.Parameters.AddWithValue("offset", i * transactions.BatchSize);
    }
    while (!fileReader.EndOfStream);

    fileReader.Close();
  }
  channel.BasicAck(ea.DeliveryTag, false);

  var endTime = DateTime.Now;
  Console.WriteLine($"Execution time: {(endTime - startTime).TotalMilliseconds}ms");

  try
  {
    await httpClient.PostAsJsonAsync(dto.Postback, new OutputConcilliationDTO(transactions));
  }
  catch
  {
    // TODO: treat this case
    Console.WriteLine("Rota inválida para o PaymentProviderId: " + dto.PaymentProviderId);
  }
};

channel.BasicConsume(
  queue: queueName,
  autoAck: false,
  consumer: consumer
);

Console.WriteLine("Press [enter] to exit");
Console.ReadLine();
