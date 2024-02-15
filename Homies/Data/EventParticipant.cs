using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homies.Data
{
    public class EventParticipant
    {
        [Required]
        public string HelperId { get; set; } = string.Empty;

        [ForeignKey(nameof(HelperId))]
        public IdentityUser Helper { get; set; } =  null!;

        [Required]
        public string EventId { get; set; } = string.Empty;

        [ForeignKey(nameof(Event))]
        public Event Event { get; set; } = null!;
    }
}