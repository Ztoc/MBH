using System.ComponentModel.DataAnnotations;

namespace MBH.Models
{
    public class ScheduledTask : IValidatableObject
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(4000)]
        public string? Description { get; set; }

        [Required]
        public DateTime ScheduledTime { get; set; }

        public bool IsExecuted { get; set; }
        public DateTime? ExecutedAt { get; set; }

        [Required] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp] public byte[]? RowVersion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext _)
        {
            if (ScheduledTime <= DateTime.UtcNow)
                yield return new ValidationResult(
                    "ScheduledTime must be in the future (UTC).",
                    new[] { nameof(ScheduledTime) });
        }
    }
}
