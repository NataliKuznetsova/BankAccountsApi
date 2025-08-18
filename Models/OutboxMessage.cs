using System.ComponentModel.DataAnnotations;

namespace BankAccountsApi.Models
{
    public class OutboxMessage
    {
        [Key]
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFailed { get; set; } = false;
        public string? FailureReason { get; set; }
        public DateTime? FailedAt { get; set; }
    }
}
