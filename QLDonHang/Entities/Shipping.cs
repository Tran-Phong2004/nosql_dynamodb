namespace QLDonHang.Entities
{
    public class Shipping
    {
        // Khóa chính ShippingId
        public string ShippingId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty; // Đơn vị vận chuyển
        public DateTime EstimateDelivery { get; set; } // Ngày dự kiến đến
        public DateTime DeliveryDate { get; set; } // Ngày vận chuyển
    }
}
