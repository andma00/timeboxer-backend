using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace Timebox.Models
{
    public class Task
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int Duration { get; set; } // Duration in minutes

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public bool IsCompleted => CompletedAt.HasValue;

        public List<Goal> Goals { get; set; } = new List<Goal>();

        [ForeignKey("UserId")]
        public string UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
    }

    public class Goal
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Description { get; set; }

        [ForeignKey("TaskId")]
        public string TaskId { get; set; }

        [JsonIgnore]
        public Task? Task { get; set; }
    }
}


