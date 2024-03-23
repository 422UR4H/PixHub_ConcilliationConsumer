namespace ConcilliationConsumer.Dtos;

public class TransferStatusDTO(Guid transactionId, string status)
{
  public Guid TransactionId { get; } = transactionId;
  public string Status { get; } = status;
}