using System.ComponentModel.DataAnnotations;

namespace ASPProject.Models
{
    public class CartItem
    {
        public Product? Product { get; set; }

        [Required(ErrorMessage = "Please enter quantity")]
        [Range(1, 15, ErrorMessage = "Please enter an amount between 1 and 15")]
        public int Quantity { get; set; }
    }
}
