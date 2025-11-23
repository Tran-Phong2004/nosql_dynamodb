namespace QLDonHang.Entities
{
    // Hàng tồn kho
    public class Inventory
    {
        // Khóa chính WarehouseId
        public string WarehouseId { get; set; } = string.Empty;

        // sort key
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
