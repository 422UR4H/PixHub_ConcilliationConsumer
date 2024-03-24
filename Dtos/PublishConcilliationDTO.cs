using System.ComponentModel.DataAnnotations;

namespace ConcilliationConsumer.Dtos;

public class PublishConcilliationDTO(long paymentProviderId, DateTime date, string file, string postback)
{
  [Required(ErrorMessage = "Field paymentProviderId is mandatory")]
  public long PaymentProviderId { get; } = paymentProviderId;
  
  [DataType(DataType.Date)]
  [Required(ErrorMessage = "Field date is mandatory")]
  public DateTime Date { get; } = date;

  // TODO: test with Uri type
  [Required(ErrorMessage = "Field file is mandatory")]
  public string File { get; } = file;

  [Required(ErrorMessage = "Field postback is mandatory")]
  public string Postback { get; } = postback;
}