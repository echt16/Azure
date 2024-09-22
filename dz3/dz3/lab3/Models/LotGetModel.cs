namespace lab3.Models
{
    public class LotGetModel
    {
        public List<string> Currencies { get; set; }
        public List<Lot>? Lots { get; set; } 

        public LotGetModel() { Lots = new List<Lot>(); }
    }
}
