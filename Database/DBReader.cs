using ConcilliationConsumer.Dtos;
using Domain.Transactions;
using Npgsql;

namespace ConcilliationConsumer.Database;

public class DBReader
{
  public static void ReadData(NpgsqlDataReader? reader, Transaction transactions)
  {
    if (reader is null) return;

    for (int i = 0; i < transactions.BatchSize && reader.Read(); i++)
    {
      var transactionId = reader.GetGuid(0);
      var status = reader.GetString(1);
      transactions.CheckStatusAndManageDBData(new TransferStatusDTO(transactionId, status));
    }
  }
}