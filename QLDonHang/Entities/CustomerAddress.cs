using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDonHang.Entities
{
    public class CustomerAddress
    {
        // sort key
        public string AddressId { get; set; }

        // khóa chính
        public string CustomerId { get; set; }
        public string AddressType { get; set; }
        public string CountryCode { get; set; }
        public string City { get; set; }
        public int ZipCode { get; set; }
    }
}
