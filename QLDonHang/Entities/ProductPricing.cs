namespace QLDonHang.Entities
{
    // Giá sản phẩm
    public class ProductPricing
    {
        // Khóa chính ProductId
        public string ProductId { get; set; }

        // sort key
        public string Region { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } // loại tiền
        public decimal Discount { get; set; } // giảm giá
    }
}
