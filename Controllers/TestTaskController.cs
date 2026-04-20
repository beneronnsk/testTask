using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestTask.Database;
using TestTask.Models;

namespace TestTask.Controllers
{
    public class TestTaskController : Controller
    {
        public TestTaskController(ApplicationContext context)
        {
            dbContext = context;
        }

        private ApplicationContext dbContext { get; set; }

        public async Task<IActionResult> Index()
        {
            var persons = await dbContext.Persons
                       .Include(p => p.PersonStatusDict)
                       .ToListAsync();

            return View(persons);
        }

        public ActionResult AddPerson()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPersonDb(Person person)
        {
            PersonStatusDict status = (await dbContext.Status.FirstOrDefaultAsync(s => s.Status == "Активен"))!;
            person.PersonStatusDict = status;

            dbContext.Persons.Add(person);

            await dbContext.AddAsync(person);

            await dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditPagePerson(int idPerson)
        {
            var editPerson = await dbContext.Persons
                .Include(p => p.PersonStatusDict)
                .FirstOrDefaultAsync(s => s.Id == idPerson);

            if (editPerson == null) return NotFound();

            ViewBag.Statuses = await dbContext.Status.ToListAsync();

            return View(editPerson);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePersonDb(Person person)
        {

            var oldPerson = await dbContext.Persons
                .AsNoTracking()
                .Include(p => p.PersonStatusDict)
                .FirstOrDefaultAsync(p => p.Id == person.Id);

            if (oldPerson != null && oldPerson.PersonStatusDictId != person.PersonStatusDictId)
            {
                var newStatus = await dbContext.Status.FindAsync(person.PersonStatusDictId);

                var history = new StatusHistory
                {
                    PersonId = person.Id,
                    StatusName = newStatus?.Status ?? "Неизвестно",
                    ChangeDate = DateTime.Now
                };
                dbContext.StatusHistories.Add(history);
            }

            dbContext.Persons.Update(person);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PersonHistory(int id)
        {
            var person = await dbContext.Persons.FindAsync(id);
            var history = await dbContext.StatusHistories
                .Where(h => h.PersonId == id)
                .OrderByDescending(h => h.ChangeDate)
                .ToListAsync();

            ViewBag.PersonName = $"{person?.Surname} {person?.Name}";
            return View(history);
        }
    }
}