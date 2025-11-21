namespace QLDonHang.Entities
{
    public class Order
    {
        // sort key
        public string OrderId { get; set; }

        // Khóa chính CustomerId
        public string CustomerId { get; set; }
        public string CountryCode { get; set; }
        public string Currency { get; set; } // loại tiền thanh toán
        public string WarehouseId { get; set; }
        public decimal TaxAmount { get; set; } // thuế
        public decimal ShippingFee { get; set; } // phí vận chuyển
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; } // tổng tiền
        public string Status { get; set; } // trạng thái đơn hàng
    }
}
