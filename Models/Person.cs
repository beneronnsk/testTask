namespace TestTask.Models
{
    public class Person
    {
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ChangeDateLocal => CreatedDate.ToLocalTime();

        public string Surname { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Patronymic { get; set; } = string.Empty;

        public DateTime Birthday { get; set; } = DateTime.UtcNow;

        public int PersonStatusDictId { get; set; }

        public PersonStatusDict? PersonStatusDict { get; set; }
    }

    public class PersonStatusDict
    {
        public int Id { get; set; } = 1;

        public string Status { get; set; } = "Активен";
    }
}