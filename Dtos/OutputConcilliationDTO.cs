using Domain.Transactions;

namespace ConcilliationConsumer.Dtos;

public class OutputConcilliationDTO(Transaction transaction)
{
  public ICollection<FileTransactionDTO> DatabaseToFile { get; } = ConvertToTransactionList(transaction.DatabaseToFile);
  public ICollection<FileTransactionDTO> FileToDatabase { get; } = ConvertToTransactionList(transaction.FileToDatabase);
  public ICollection<DifferentStatusDTO> DifferentStatus { get; } = ConvertToIdList(transaction.DifferentStatus);

  private static List<FileTransactionDTO> ConvertToTransactionList(Dictionary<Guid, string> dictionary)
  {
    return dictionary.Select(pair => new FileTransactionDTO(pair.Key, pair.Value)).ToList();
  }

  private static List<DifferentStatusDTO> ConvertToIdList(ICollection<Guid> collection)
  {
    return collection.Select(pair => new DifferentStatusDTO(pair)).ToList();
  }
}