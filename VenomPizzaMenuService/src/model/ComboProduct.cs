using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VenomPizzaMenuService.src.model
{
    public class ComboProduct
    {
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public int ComboId { get; set; }
        public Product Product { get; set; }
        public Combo Combo { get; set; }
        public ComboProduct(int comboId,int productId,int quantity=1) {
            ComboId = comboId;
            ProductId = productId;
            Quantity = quantity;
        }
    }
}
