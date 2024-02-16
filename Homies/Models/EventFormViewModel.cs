using System.ComponentModel.DataAnnotations;

using static Homies.Data.DataConstants;

namespace Homies.Models
{
    public class EventFormViewModel
    {
        [Required]
        [StringLength(EventNameMaxLength, MinimumLength = EventNameMinLength)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(EventDescriptionMaxLength, MinimumLength = EventDescriptionMinLength)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Start { get; set; } = string.Empty;

        [Required]
        public string End { get; set; } = string.Empty;

        [Required]
        public int TypeId { get; set; }

        public IList<TypeViewModel> Types { get; set; } = new List<TypeViewModel>();
    }
}
