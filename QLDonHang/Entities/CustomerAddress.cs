using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDonHang.Entities
{
    public class CustomerAddress
    {
        // Khóa chính - CustomerId
        public string CustomerId { get; set; } = string.Empty;

        // Sort key - AddressId
        public string AddressId { get; set; } = string.Empty;

        // Loại địa chỉ: Home, Office, Other
        public string AddressType { get; set; } = string.Empty;

        // Mã quốc gia
        public string CountryCode { get; set; } = string.Empty;

        // Thành phố
        public string City { get; set; } = string.Empty;

        // Địa chỉ chi tiết
        public string AddressLine { get; set; } = string.Empty;

        // Mã bưu điện (optional)
        public string? PostalCode { get; set; }

        // Có phải địa chỉ mặc định không
        public bool IsDefault { get; set; } = false;

        // Ngày tạo
        public DateTime CreatedAt { get; set; }

        // Ngày cập nhật
        public DateTime? UpdatedAt { get; set; }
    }
}
