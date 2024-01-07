using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TaskManage.Models
{
    public class Group
    {
        public int Id { get; set; }
        [DisplayName("Nazwa")]
        public string? Name { get; set; }
        [DisplayName("Opis")]
        public string? Description { get; set; }
        public ICollection<TaskItem>? Tasks { get; set; }
        public ICollection<ApplicationUser>? Users { get; set; }
    }
}
