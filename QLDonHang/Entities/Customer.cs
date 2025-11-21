using System.ComponentModel.DataAnnotations;

namespace QLDonHang.Entities
{
    public class Customer
    {
        // Khóa chính CustomerId
        public string CustomerId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string CountryCode { get; set; }
        public string Phone { get; set; }
    }
}
