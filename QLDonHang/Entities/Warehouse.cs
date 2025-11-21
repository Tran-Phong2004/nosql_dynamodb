namespace QLDonHang.Entities
{
    // Kho hàng theo quốc gia
    public class Warehouse
    {
        // Khóa chính WarehouseId
        public string WarehouseId { get; set; }
        public string CountryCode { get; set; }
        public string Address { get; set; }
    }
}
