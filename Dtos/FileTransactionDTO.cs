using System.Text.Json.Serialization;

namespace ConcilliationConsumer.Dtos;

public class FileTransactionDTO(Guid id, string status)
{
  [JsonPropertyName("id")]
  public Guid Id { get; } = id;

  [JsonPropertyName("status")]
  public string Status { get; } = status;
}