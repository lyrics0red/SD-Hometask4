namespace OrdersHandler.Tools
{
    public class OrderForm
    {
        public int UserId { get; set; }
        public string SpecialRequests { get; set; }
        public List<int> DishIds { get; set; }
        public List<int> Counts { get; set; }
    }
}
