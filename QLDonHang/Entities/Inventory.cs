namespace QLDonHang.Entities
{
    // Hàng tồn kho
    public class Inventory
    {
        // Khóa chính WarehouseId
        public string WarehouseId { get; set; }

        // sort key
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
