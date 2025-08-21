using System.ComponentModel.DataAnnotations;

namespace MBH.Dtos
{
    public record ScheduledTaskCreateDto(
    [Required, MaxLength(200)] string Title,
    [MaxLength(4000)] string? Description,
    [Required] DateTime ScheduledTimeUtc 
);

    public record ScheduledTaskUpdateDto(
        [Required, MaxLength(200)] string Title,
        [MaxLength(4000)] string? Description,
        [Required] DateTime ScheduledTimeUtc
    );
}
