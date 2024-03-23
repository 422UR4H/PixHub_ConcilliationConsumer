using System.Text.Json.Serialization;

namespace ConcilliationConsumer.Dtos;

public class DifferentStatusDTO(Guid id)
{
  [JsonPropertyName("id")]
  public Guid Id { get; } = id;
}