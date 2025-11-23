using Amazon.DynamoDBv2.DataModel;

namespace QLDonHang.Entities
{
    // Giá sản phẩm
    public class ProductPricing
    {
        // Partition Key
        public string ProductId { get; set; } = string.Empty;

        // Sort Key
        public string Region { get; set; } = string.Empty; // VN, US, JP, CN...

        public decimal Price { get; set; }

        public string Currency { get; set; } = string.Empty; // VND, USD, JPY...

        public decimal? Discount { get; set; } // % giảm giá (0-100)

        public decimal? DiscountAmount { get; set; } // Số tiền giảm cố định

        public DateTime? DiscountStartDate { get; set; }

        public DateTime? DiscountEndDate { get; set; }

        public decimal? Tax { get; set; } // % thuế

        public DateTime UpdatedAt { get; set; }

        // Tính toán
        [DynamoDBIgnore]
        public decimal? FinalPrice => DiscountAmount.HasValue
            ? Price - DiscountAmount.Value
            : Price * (1 - Discount / 100);

        [DynamoDBIgnore]
        public decimal? PriceWithTax => FinalPrice * (1 + Tax / 100);

        [DynamoDBIgnore]
        public bool IsOnSale => Discount > 0 || DiscountAmount > 0;
    }
}
