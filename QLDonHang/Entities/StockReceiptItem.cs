namespace QLDonHang.Entities
{
    public class StockReceiptItem
    {
        // Partition Key
        public string ReceiptId { get; set; } = string.Empty;
        // Sort Key
        public string ProductId { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; } // Số lượng nhập
        public decimal UnitPrice { get; set; } // Giá nhập/đơn vị
        public decimal TotalPrice { get; set; } // Tổng giá = Quantity * UnitPrice
        public DateTime? ExpiryDate { get; set; } // Hạn sử dụng (nếu có)
        public string BatchNumber { get; set; } = string.Empty; // Số lô
        public DateTime CreatedAt { get; set; }
    }
}
