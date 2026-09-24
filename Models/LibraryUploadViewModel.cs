using System.ComponentModel.DataAnnotations;

namespace EBook.Models
{
    public class LibraryUploadViewModel
    {
        [Required]
        public string Name { get; set; }

        public string Scope { get; set; }

        public string Curriculum { get; set; }

        [Required]
        [Display(Name = "Upload File")]
        public IFormFile File { get; set; } // Used to receive the uploaded stream from form submission
    }
}
