using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBook.Models
{
    public class Payment
    {
        public int id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [Display(Name ="Payment Method")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; }
        [Required]
        public string UserId { get; set; }
        public virtual IdentityUser? User { get; set; }


    }
}
