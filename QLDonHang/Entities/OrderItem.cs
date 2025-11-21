namespace QLDonHang.Entities
{
    public class OrderItem
    {
        // Khóa chính OrderId
        public string OrderId { get; set; }

        // sort key
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Giá tại thời điểm đặt
        public string Currency { get; set; }
    }
}
