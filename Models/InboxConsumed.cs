using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankAccountsApi.Models
{
    [Table("inbox_consumed")]
    public class InboxConsumed
    {
        [Key]
        public Guid MessageId { get; set; }

        public DateTime ConsumedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string Handler { get; set; } = null!;
    }
}
