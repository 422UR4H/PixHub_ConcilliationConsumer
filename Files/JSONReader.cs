using System.Text.Json;
using ConcilliationConsumer.Dtos;
using Domain.Transactions;

namespace ConcilliationConsumer.Files;

public class JSONReader
{
  public static void ReadFile(StreamReader fileReader, Transaction transactions)
  {
    if (fileReader.EndOfStream) return;

    string? line = fileReader.ReadLine();
    for (int i = 0; i < transactions.BatchSize && line is not null; i++)
    {
      try
      {
        FileTransactionDTO dto = JsonSerializer.Deserialize<FileTransactionDTO>(line) ??
          throw new Exception();

        transactions.CheckStatusAndManageFileData(dto);
      }
      catch
      {
        Console.WriteLine("Error when deserializing this line!");
      }
      if (i + 1 < transactions.BatchSize) line = fileReader.ReadLine();
    }
  }
}
