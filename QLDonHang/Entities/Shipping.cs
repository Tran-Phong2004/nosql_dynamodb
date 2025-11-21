namespace QLDonHang.Entities
{
    public class Shipping
    {
        // Khóa chính ShippingId
        public string ShippingId { get; set; }
        public string OrderId { get; set; }
        public string Status { get; set; }
        public string Carrier { get; set; } // Đơn vị vận chuyển
        public DateTime EstimateDelivery { get; set; } // Ngày dự kiến đến
        public DateTime DeliveryDate { get; set; } // Ngày vận chuyển
    }
}
