using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QLDonHang.Const;
using QLDonHang.DynamoDB;
using QLDonHang.Entities;
using QLDonHang.Models;
using System.Reflection;
using System.Security.Claims;

namespace QLDonHang.Controllers
{
    public class AuthController : Controller
    {
        private readonly DynamoDbService _dynamoDbService;
        public AuthController(DynamoDbService dynamoDbService) 
        {
            _dynamoDbService = dynamoDbService;
        }

        // GET: Auth
        public IActionResult Index()
        {
            return View();
        }

        //GET: Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var customer = await GetCustomerByUsernameAsync(model);
            if(customer == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                return View();
            }

            var passValid = ValidatePassword(model.Password, customer);
            if (!passValid)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                return View();
            }

            if(customer.IsActive == false)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customer.CustomerId),
                new Claim(ClaimTypes.Name, customer.FullName),
                new Claim(ClaimTypes.Email, customer.Email),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            //var authProperties = new AuthenticationProperties
            //{
            //    IsPersistent = model.RememberMe,
            //    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(12)
            //};

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            TempData["SuccessMessage"] = "Đăng nhập thành công";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra username đã tồn tại
            var existingUsername = await CheckUsernameExistsAsync(model.Username);
            if (existingUsername)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                return View(model);
            }

            // Kiểm tra email đã tồn tại
            var existingEmail = await CheckEmailExistsAsync(model.Email);
            if (existingEmail)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng");
                return View(model);
            }

            try
            {
                // Tạo customer mới
                var customerId = Guid.NewGuid().ToString();
                var customer = new Customer
                {
                    CustomerId = customerId,
                    Username = model.Username,
                    Email = model.Email,
                    FullName = model.FullName,
                    HashPassword = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Phone = model.Phone,
                    CountryCode = model.CountryCode,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Lưu customer vào DynamoDB
                var customerItem = DynamoDbMapper.ToAttributeMap(customer);
                await _dynamoDbService.AddOrUpdateItemAsync(TableDb.CUSTOMERS, customerItem);

                // Tạo địa chỉ khách hàng
                var addressId = Guid.NewGuid().ToString();
                var customerAddress = new CustomerAddress
                {
                    CustomerId = customerId,
                    AddressId = addressId,
                    AddressType = model.AddressType,
                    CountryCode = model.CountryCode,
                    City = model.City,
                    AddressLine = model.AddressLine,
                    PostalCode = model.PostalCode,
                    IsDefault = model.IsDefaultAddress,
                    CreatedAt = DateTime.UtcNow
                };

                // Lưu địa chỉ vào DynamoDB
                var addressItem = DynamoDbMapper.ToAttributeMap(customerAddress);
                await _dynamoDbService.AddOrUpdateItemAsync(TableDb.CUSTOMER_ADDRESSES, addressItem);

                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại.");
                return View(model);
            }
        }

        private async Task<bool> CheckUsernameExistsAsync(string username)
        {
            var scanRequest = new ScanRequest
            {
                TableName = TableDb.CUSTOMERS,
                FilterExpression = "Username = :username",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":username", new AttributeValue { S = username } }
                },
                Limit = 1
            };

            var response = await _dynamoDbService.ScanTableAsync(scanRequest);
            return response != null && response.Count > 0;
        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            var scanRequest = new ScanRequest
            {
                TableName = TableDb.CUSTOMERS,
                FilterExpression = "Email = :email",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":email", new AttributeValue { S = email } }
                },
                Limit = 1
            };

            var response = await _dynamoDbService.ScanTableAsync(scanRequest);
            return response != null && response.Count > 0;
        }

        private async Task<Customer?> GetCustomerByUsernameAsync(LoginVM model)
        {
            var where = new ScanRequest
            {
                TableName = TableDb.CUSTOMERS,
                FilterExpression = "Username = :username",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":username", new AttributeValue { S = model.Username } }
                },
                Limit = 1
            };
            var response = await _dynamoDbService.ScanTableAsync(where);
            if(response == null || response.Count == 0)
            {
                return null;
            }
            return DynamoDbMapper.ToObject<Customer>(response[0]);
        }

        private bool ValidatePassword(string password, Customer customer)
        {
            return BCrypt.Net.BCrypt.Verify(password, customer.HashPassword);
        }
    }
}
