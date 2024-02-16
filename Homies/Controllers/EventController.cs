using Homies.Data;
using Homies.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using static Homies.Data.DataConstants;

namespace Homies.Controllers
{
    public class EventController : Controller
    {
        private readonly HomiesDbContext _context;

        public EventController(HomiesDbContext context)
        {
                this._context = context;
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            var model = await _context.Events
                .AsNoTracking()
                .Select(e => new EventInfoViewModel 
            {
                Id = e.Id,
                Name = e.Name,
                Start = e.Start.ToString(EventDateTimeFormat),
                Organiser = e.Organiser.UserName,
                Type = e.Type.Name
            })
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var model = new EventFormViewModel();

            model.Types = await GetTypes();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(EventFormViewModel model)
        {
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;

            if (!DateTime.TryParseExact(
                model.Start,
                EventDateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out start)) 
            {
                ModelState.AddModelError(nameof(model.Start), DateTimeError);
            }

            if (!DateTime.TryParseExact(
                model.End,
                EventDateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out end))
            {
                ModelState.AddModelError(nameof(model.End), DateTimeError);
            }

            if (!ModelState.IsValid) 
            {
                model.Types = await GetTypes();

                return View(model);
            }

            var entity = new Event
            {
                Name = model.Name,
                Description = model.Description,
                Start = start,
                End = end,
                CreatedOn = DateTime.Now,
                OrganiserId = GetUserId(),
                TypeId = model.TypeId
            };

            await _context.Events.AddAsync(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }

        [HttpPost]
        public async Task<IActionResult> Join(int id) 
        {
            string userId = GetUserId();

            var e = await _context.Events
                .Where(e => e.Id == id)
                .Include(e => e.EventsParticipants)
                .FirstOrDefaultAsync();
                
            if (e == null) 
            {
                return BadRequest();
            }
            else if (e.OrganiserId == userId) 
            {
                return BadRequest();
            }

            if (!e.EventsParticipants.Any(e => e.HelperId == userId)) 
            {
                e.EventsParticipants.Add(new EventParticipant
                {
                    HelperId = userId,
                    EventId = id
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Joined));
        }

        public async Task<IActionResult> Joined()
        {
            string userId = GetUserId();

            var model = await _context.EventsParticipants
                .Where(ep => ep.HelperId == userId)
                .AsNoTracking()
                .Select(ep => new EventInfoViewModel
                {
                    Id = ep.Event.Id,
                    Name = ep.Event.Name,
                    Start = ep.Event.Start.ToString(EventDateTimeFormat),
                    Type = ep.Event.Type.Name,
                    Organiser = ep.Event.OrganiserId
                })
                .ToListAsync();


            return View(model);
        }

        public async Task<IActionResult> Leave(int id) 
        {
            string userId = GetUserId();

            var e = await _context.Events
                .Where(ep => ep.Id == id)
                .Include(e => e.EventsParticipants)
                .FirstOrDefaultAsync();

            if (e == null) 
            {
                return BadRequest();
            }

            var ep = e.EventsParticipants.FirstOrDefault(ep => ep.HelperId == userId);

            if (ep == null) 
            {
                return BadRequest();
            }

            e.EventsParticipants.Remove(ep);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id) 
        {
            var e = await _context.Events
                .Where(e => e.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (e == null) 
            {
                return BadRequest();
            }
            if (e.OrganiserId != GetUserId()) 
            {
                return Unauthorized();
            }

            var model = new EventFormViewModel
            {
                Name = e.Name,
                Description = e.Description,
                Start = e.Start.ToString(EventDateTimeFormat),
                End = e.End.ToString(EventDateTimeFormat),
                TypeId = e.TypeId
            };

            model.Types = await GetTypes();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EventFormViewModel model, int id) 
        {
            var e = await _context.Events
                .FindAsync(id);

            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;

            if (e == null) 
            {
                return BadRequest();
            }

            if (!DateTime.TryParseExact(
                model.Start,
                EventDateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out start))
            {
                ModelState.AddModelError(nameof(model.Start), DateTimeError);
            }

            if (!DateTime.TryParseExact(
                model.End,
                EventDateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out end))
            {
                ModelState.AddModelError(nameof(model.End), DateTimeError);
            }

            if (!ModelState.IsValid)
            {
                model.Types = await GetTypes();

                return View(model);
            }
            if (e.OrganiserId != GetUserId())
            {
                return Unauthorized();
            }

            e.Name = model.Name;
            e.Description = model.Description;
            e.Start = start;
            e.End = end;
            e.TypeId = model.TypeId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        public async Task<IList<TypeViewModel>> GetTypes() 
        {
            return await _context.Types
                .AsNoTracking()
                .Select(t => new TypeViewModel
            {
                Id = t.Id,
                Name = t.Name,
            }).ToListAsync();
        }
    }
}
