namespace QLDonHang.Entities
{
    // Kho hàng theo quốc gia
    public class Warehouse
    {
        // Khóa chính WarehouseId
        public string WarehouseId { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
