using System.ComponentModel.DataAnnotations;

namespace QLDonHang.Entities
{
    public class Products
    {
        // Khóa chính ProductId
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty; // Mã sản phẩm

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Thông tin thêm
        public string[] Tags { get; set; } = Array.Empty<string>();

        public double Weight { get; set; } // Cân nặng (kg)

        public string Dimensions { get; set; } = string.Empty; // Kích thước (LxWxH cm)
    }
}
