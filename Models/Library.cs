using Microsoft.Identity.Client;
using System.Reflection.Emit;
using System.ComponentModel.DataAnnotations;
namespace EBook.Models
{
    public class Library
    {
        public string Name { get; set; }
        public int id { get; set; }
        public string Scope { get; set; }
        public string Curriculum { get; set; }


        public string FilePath { get; set; }
    }
}
