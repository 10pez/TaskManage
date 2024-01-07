namespace TaskManage.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public int TaskItemId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

    }
}
