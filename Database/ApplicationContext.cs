using Microsoft.EntityFrameworkCore;
using TestTask.Models;

namespace TestTask.Database
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {

            Database.EnsureCreated();
        }

        public DbSet<Person> Persons { get; set; }

        public DbSet<PersonStatusDict> Status { get; set; }

        public DbSet<StatusHistory> StatusHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            PersonStatusDict personStatusActive = new PersonStatusDict { Id = 1, Status = "Активен" };
            PersonStatusDict personStatusBlocked = new PersonStatusDict { Id = 2, Status = "Заблокирован" }; ;
            PersonStatusDict personStatusDelete = new PersonStatusDict { Id = 3, Status = "Удален" }; ;

            modelBuilder.Entity<PersonStatusDict>().HasData([personStatusActive, personStatusBlocked, personStatusDelete]);

            modelBuilder.Entity<Person>().HasData(new Person
            {
                Id = 1,
                Surname = "Иванов",
                Name = "Иван",
                Patronymic = "Иванович",
                Birthday = DateTime.SpecifyKind(new DateTime(2000, 1, 1), DateTimeKind.Unspecified),
                PersonStatusDictId = 1
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}