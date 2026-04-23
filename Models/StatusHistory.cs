namespace TestTask.Models
{
    public class StatusHistory
    {
        public int Id { get; set; }

        public int PersonId { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;

        public DateTime ChangeDateLocal => ChangeDate.ToLocalTime();

        public Person? Person { get; set; }
    }
}