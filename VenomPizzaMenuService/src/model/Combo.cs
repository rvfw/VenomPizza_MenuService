using VenomPizzaMenuService.src.dto;

namespace VenomPizzaMenuService.src.model;

public class Combo : Product
{
    public List<ComboProduct> Products { get; set; }=new();
    public double Profit { get; }
    public Combo(int id,string title) : base(id, title) { }
    public Combo(ComboDto dto):base(dto)
    {
        
    }
}
