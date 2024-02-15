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

        [HttpGet]
        public async Task<IActionResult> All()
        {
            var model = _context.Events.Select(e => new EventFormViewModel()).FirstOrDefault();

            model.Types = await GetTypes();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Add() 
        {
            var model = _context.Events.Select(e => new EventFormViewModel()).FirstOrDefault();

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

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        public async Task<IList<TypeViewModel>> GetTypes() 
        {
            return await _context.Types.Select(t => new TypeViewModel
            {
             Id = t.Id,
             Name = t.Name,
            }).ToListAsync();
        }
    }
}
