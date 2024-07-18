namespace ICTPRG535_556.Models
{
    public class SessionCartDTO
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Price { get; set; }
        public int ItemID { get; set; }
        public int ListID { get; set; }
        public int ListIndex { get; set; }
        public int UserID { get; set; }
        public int Quantity { get; set; }
        public string ListName { get; set; }
        public List<ProduceDTO> ProduceItems { get; set; }
    }
}
