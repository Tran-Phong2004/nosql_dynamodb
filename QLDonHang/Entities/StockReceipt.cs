using Amazon.DynamoDBv2.DataModel;

namespace QLDonHang.Entities
{
    // Phiếu nhập kho
    public class StockReceipt
    {
        // Partition Key
        public string ReceiptId { get; set; } = string.Empty; // Mã phiếu nhập

        public string WarehouseId { get; set; } = string.Empty; // Kho nhập
        public string SupplierId { get; set; } = string.Empty; // Nhà cung cấp
        public string SupplierName { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; } // Ngày nhập
        public string Status { get; set; } = "Pending"; // Pending, Approved, Completed, Cancelled
        public string Notes { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty; // Người tạo phiếu
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? ApprovedBy { get; set; } 
        public DateTime? ApprovedAt { get; set; } 

        public string? CompletedBy { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string? CancelledBy { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Computed property
        [DynamoDBIgnore]
        public decimal TotalAmount { get; set; } // Tổng tiền (tính từ items)
    }
}
