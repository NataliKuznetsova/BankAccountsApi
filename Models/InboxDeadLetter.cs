using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankAccountsApi.Models
{
    [Table("inbox_dead_letters")]
    public class InboxDeadLetter
    {
        [Key]
        [Column("message_id")]
        public Guid MessageId { get; set; }

        [Required]
        [Column("received_at")]
        public DateTime ReceivedAt { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("handler")]
        public string Handler { get; set; } = "";

        [Required]
        [Column("payload", TypeName = "jsonb")]
        public string Payload { get; set; } = "";

        [Required]
        [MaxLength(1000)]
        [Column("error")]
        public string Error { get; set; } = "";
    }
}
