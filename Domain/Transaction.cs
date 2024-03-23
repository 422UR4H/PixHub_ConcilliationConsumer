using ConcilliationConsumer.Dtos;

namespace Domain.Transactions;

public class Transaction(int batchSize)
{
  public int BatchSize { get; } = batchSize;
  public Dictionary<Guid, string> DatabaseToFile { get; } = [];
  public Dictionary<Guid, string> FileToDatabase { get; } = [];
  public ICollection<Guid> DifferentStatus { get; } = [];

  public void CheckStatusAndManageDBData(TransferStatusDTO dbData)
  {
    FileToDatabase.TryGetValue(dbData.TransactionId, out string? status);
    if (status is null)
    {
      DatabaseToFile.Add(dbData.TransactionId, dbData.Status);
      return;
    }
    if (status == dbData.Status)
    {
      FileToDatabase.Remove(dbData.TransactionId);
      return;
    }

    // TODO: refactor to save in file
    DifferentStatus.Add(dbData.TransactionId);
  }

  public void CheckStatusAndManageFileData(FileTransactionDTO dbData)
  {
    DatabaseToFile.TryGetValue(dbData.Id, out string? status);
    if (status is null)
    {
      FileToDatabase.Add(dbData.Id, dbData.Status);
      return;
    }
    if (status == dbData.Status)
    {
      DatabaseToFile.Remove(dbData.Id);
      return;
    }
    DifferentStatus.Add(dbData.Id);
  }
}