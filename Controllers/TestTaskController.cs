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

        public async Task<IActionResult> Index(int page = 1, string sortBy = "name", bool sortDesc = false)
        {
            int pageSize = 15;
            var query = dbContext.Persons.Include(p => p.PersonStatusDict).AsQueryable();

            query = sortBy switch
            {
                "status" => sortDesc
                    ? query.OrderByDescending(p => p.PersonStatusDict.Status)
                    : query.OrderBy(p => p.PersonStatusDict.Status),

                "birthday" => sortDesc
                    ? query.OrderByDescending(p => p.Birthday)
                    : query.OrderBy(p => p.Birthday),

                "name" or _ => sortDesc
                    ? query.OrderByDescending(p => p.Surname).ThenByDescending(p => p.Name)
                    : query.OrderBy(p => p.Surname).ThenBy(p => p.Name),
            };

            var totalItems = await query.CountAsync();
            var persons = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SortBy = sortBy;
            ViewBag.SortDesc = sortDesc;

            return View(persons);
        }

        public ActionResult AddPerson()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPersonDb(Person person)
        {
            person.Birthday = DateTime.SpecifyKind(person.Birthday, DateTimeKind.Utc);
            person.CreatedDate = DateTime.SpecifyKind(person.CreatedDate, DateTimeKind.Utc);

            var status = await dbContext.Status.FirstOrDefaultAsync(s => s.Status == "Активен");

            if (status != null)
            {
                person.PersonStatusDictId = status.Id;
            }

            await dbContext.Persons.AddAsync(person);

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
    person.Birthday = DateTime.SpecifyKind(person.Birthday, DateTimeKind.Utc);
    
    person.CreatedDate = DateTime.SpecifyKind(person.CreatedDate, DateTimeKind.Utc);

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
            ChangeDate = DateTime.UtcNow 
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