using System.ComponentModel;

namespace TaskManage.Models
{
    public enum PriorityEnum
    {
        Niski,
        Średni,
        Wysoki
    }

    public class TaskItem
    {
        public int Id { get; set; }
        [DisplayName("Nazwa")]
        public string Name { get; set; }
        [DisplayName("Opis")]
        public string? Description { get; set; }
        [DisplayName("Data zakończenia")]
        public DateTime DueDate { get; set; }
        [DisplayName("Data utworzenia")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [DisplayName("Priorytet")]
        public PriorityEnum Priority { get; set; }
        [DisplayName("Czy zakończono?")]
        public bool IsDone { get; set; } = false;
        public ICollection<Comment>? Comments { get; set; }
        public int? GroupId { get; set; }
        public virtual Group? Group { get; set; }

    }
}
