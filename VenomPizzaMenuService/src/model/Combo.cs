using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using VenomPizzaMenuService.src.dto;

namespace VenomPizzaMenuService.src.model;
[Table("combos")]
public class Combo : Product
{
    public List<ComboProduct> Products { get; set; }=new();
    public double Profit { get; }
    public Combo(int id,string title) : base(id, title) { }
    public Combo(ComboDto dto):base(dto)
    {
        
    }
}
