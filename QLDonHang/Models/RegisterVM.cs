using System.ComponentModel.DataAnnotations;

namespace QLDonHang.Models
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3-50 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ chứa chữ cái, số và dấu gạch dưới")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^[0-9]{9,15}$", ErrorMessage = "Số điện thoại phải có 9-15 chữ số")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quốc gia là bắt buộc")]
        public string CountryCode { get; set; } = "VN";

        [Display(Name = "Tôi đồng ý với điều khoản sử dụng")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Bạn phải đồng ý với điều khoản sử dụng")]
        public bool AgreeToTerms { get; set; }

        // Thông tin địa chỉ
        [Required(ErrorMessage = "Loại địa chỉ không được để trống")]
        public string AddressType { get; set; } = "Home"; // Home, Office, Other

        [Required(ErrorMessage = "Thành phố không được để trống")]
        [StringLength(100, ErrorMessage = "Tên thành phố không được quá 100 ký tự")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ chi tiết không được để trống")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được quá 500 ký tự")]
        public string AddressLine { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Mã bưu điện không được quá 20 ký tự")]
        public string? PostalCode { get; set; }

        public bool IsDefaultAddress { get; set; } = true;
    }
}
