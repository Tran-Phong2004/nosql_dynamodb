namespace QLDonHang.Entities
{
    public class Order
    {
        // sort key
        public string OrderId { get; set; } = string.Empty;

        // Khóa chính CustomerId
        public string CustomerId { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty; // loại tiền thanh toán
        public string WarehouseId { get; set; } = string.Empty;
        public decimal TaxAmount { get; set; } // thuế
        public decimal ShippingFee { get; set; } // phí vận chuyển
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; } // tổng tiền
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Cancelled
    }
}
