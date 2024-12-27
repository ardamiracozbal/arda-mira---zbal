using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace OnlineSinav.ViewModel
{
    public class EditQuizViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Sınav başlığı zorunludur.")]
        [Display(Name = "Sınav Başlığı")]
        public string Title { get; set; }

        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        [Display(Name = "Başlangıç Tarihi")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Sınıf seçimi zorunludur.")]
        [Display(Name = "Sınıf")]
        public int ClassroomId { get; set; }

        public List<SelectListItem> AvailableClassrooms { get; set; }
    }
}
