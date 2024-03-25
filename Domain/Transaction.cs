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
    if (status != dbData.Status)
    {
      DifferentStatus.Add(dbData.TransactionId);
    }
    FileToDatabase.Remove(dbData.TransactionId);
  }

  public void CheckStatusAndManageFileData(FileTransactionDTO fileData)
  {
    DatabaseToFile.TryGetValue(fileData.Id, out string? status);
    if (status is null)
    {
      FileToDatabase.Add(fileData.Id, fileData.Status);
      return;
    }
    if (status != fileData.Status)
    {
      DifferentStatus.Add(fileData.Id);
    }
    DatabaseToFile.Remove(fileData.Id);
  }
}