using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using QLDonHang.Const;
using QLDonHang.DynamoDB;
using QLDonHang.Entities;
using QLDonHang.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace QLDonHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DynamoDbService _dynamoDbService;

        public HomeController(ILogger<HomeController> logger, DynamoDbService dynamoDbService)
        {
            _logger = logger;
            _dynamoDbService = dynamoDbService;
        }

        // GET: Home/Index
        public async Task<IActionResult> Index(string? category, string? search, string? region = "VN")
        {
            try
            {
                var productItems = await _dynamoDbService.ScanTableAsync(TableDb.PRODUCTS);
                var products = productItems.Select(item => DynamoDbMapper.ToObject<Products>(item))
                    .Where(p => p.IsActive)
                    .ToList();

                if (!string.IsNullOrEmpty(category))
                {
                    products = products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(search))
                {
                    products = products.Where(p =>
                        p.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        p.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        p.SKU.Contains(search, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                var productPricings = new Dictionary<string, ProductPricing>();
                foreach (var product in products)
                {
                    var pricingKey = new Dictionary<string, AttributeValue>
                    {
                        { "ProductId", new AttributeValue { S = product.ProductId } },
                        { "Region", new AttributeValue { S = region } }
                    };

                    try
                    {
                        var pricingItem = await _dynamoDbService.GetItemAsync(TableDb.PRODUCT_PRICING, pricingKey);
                        if (pricingItem != null && pricingItem.Any())
                        {
                            productPricings[product.ProductId] = DynamoDbMapper.ToObject<ProductPricing>(pricingItem);
                        }
                    }
                    catch { }
                }

                ViewBag.ProductPricings = productPricings;
                ViewBag.CurrentCategory = category;
                ViewBag.CurrentSearch = search;
                ViewBag.CurrentRegion = region;
                ViewBag.Categories = products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang chủ");
                TempData["ErrorMessage"] = "Không thể tải danh sách sản phẩm.";
                return View(new List<Products>());
            }
        }

        // GET: Home/ProductDetails/5
        public async Task<IActionResult> ProductDetails(string id, string region = "VN")
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var productKey = new Dictionary<string, AttributeValue>
                {
                    { "ProductId", new AttributeValue { S = id } }
                };

                var productItem = await _dynamoDbService.GetItemAsync(TableDb.PRODUCTS, productKey);
                if (productItem == null || !productItem.Any())
                {
                    return NotFound();
                }

                var product = DynamoDbMapper.ToObject<Products>(productItem);

                var pricingKey = new Dictionary<string, AttributeValue>
                {
                    { "ProductId", new AttributeValue { S = id } },
                    { "Region", new AttributeValue { S = region } }
                };

                var pricingItem = await _dynamoDbService.GetItemAsync(TableDb.PRODUCT_PRICING, pricingKey);
                ProductPricing? pricing = null;
                if (pricingItem != null && pricingItem.Any())
                {
                    pricing = DynamoDbMapper.ToObject<ProductPricing>(pricingItem);
                }

                ViewBag.Pricing = pricing;
                ViewBag.CurrentRegion = region;

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải chi tiết sản phẩm {id}");
                return NotFound();
            }
        }

        // POST: Home/PlaceOrder - XỬ LÝ MUA HÀNG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string productId, int quantity = 1, string region = "VN")
        {
            try
            {
                // 1. KIỂM TRA ĐĂNG NHẬP
                var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(customerId))
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập để đặt hàng.";
                    return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("ProductDetails", new { id = productId, region }) });
                }

                // 2. VALIDATE SỐ LƯỢNG
                if (quantity <= 0)
                {
                    TempData["ErrorMessage"] = "Số lượng phải lớn hơn 0.";
                    return RedirectToAction(nameof(ProductDetails), new { id = productId, region });
                }

                // 3. LẤY THÔNG TIN SẢN PHẨM
                var productKey = new Dictionary<string, AttributeValue>
                {
                    { "ProductId", new AttributeValue { S = productId } }
                };

                var productItem = await _dynamoDbService.GetItemAsync(TableDb.PRODUCTS, productKey);
                if (productItem == null || !productItem.Any())
                {
                    TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                    return RedirectToAction(nameof(Index));
                }

                var product = DynamoDbMapper.ToObject<Products>(productItem);

                if (!product.IsActive)
                {
                    TempData["ErrorMessage"] = "Sản phẩm hiện không khả dụng.";
                    return RedirectToAction(nameof(ProductDetails), new { id = productId, region });
                }

                // 4. LẤY GIÁ SẢN PHẨM
                var pricingKey = new Dictionary<string, AttributeValue>
                {
                    { "ProductId", new AttributeValue { S = productId } },
                    { "Region", new AttributeValue { S = region } }
                };

                var pricingItem = await _dynamoDbService.GetItemAsync(TableDb.PRODUCT_PRICING, pricingKey);
                if (pricingItem == null || !pricingItem.Any())
                {
                    TempData["ErrorMessage"] = "Không tìm thấy giá sản phẩm cho khu vực này.";
                    return RedirectToAction(nameof(ProductDetails), new { id = productId, region });
                }

                var pricing = DynamoDbMapper.ToObject<ProductPricing>(pricingItem);

                // 5. TÍNH TOÁN GIÁ
                var unitPrice = pricing.FinalPrice ?? pricing.Price;
                var subtotal = unitPrice * quantity;
                var taxAmount = subtotal * (pricing.Tax ?? 0) / 100;
                var shippingFee = CalculateShippingFee(region, product.Weight, quantity);
                var totalAmount = subtotal + taxAmount + shippingFee;

                // 6. TẠO ĐỚN HÀNG
                var orderId = GenerateOrderId();
                var order = new Order
                {
                    CustomerId = customerId,
                    OrderId = orderId,
                    CountryCode = region,
                    Currency = pricing.Currency,
                    WarehouseId = $"WH_{region}",
                    TaxAmount = taxAmount,
                    ShippingFee = shippingFee,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Status = "Pending"
                };

                var orderMap = DynamoDbMapper.ToAttributeMap(order);
                await _dynamoDbService.AddOrUpdateItemAsync(TableDb.ORDERS, orderMap);

                // 7. TẠO ORDER ITEM
                var orderItem = new OrderItem
                {
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    Currency = pricing.Currency
                };

                var orderItemMap = DynamoDbMapper.ToAttributeMap(orderItem);
                await _dynamoDbService.AddOrUpdateItemAsync(TableDb.ORDER_ITEM, orderItemMap);

                _logger.LogInformation($"Đơn hàng {orderId} được tạo bởi {customerId}");

                TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng: {orderId}";
                return RedirectToAction("OrderDetails", "Order", new { id = orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi đặt hàng sản phẩm {productId}");
                TempData["ErrorMessage"] = "Không thể đặt hàng. Vui lòng thử lại.";
                return RedirectToAction(nameof(ProductDetails), new { id = productId, region });
            }
        }

        // HELPER METHODS
        private string GenerateOrderId()
        {
            return $"ORD{DateTime.UtcNow:yyyyMMddHHmmssfff}{new Random().Next(100, 999)}";
        }

        private decimal CalculateShippingFee(string region, double weight, int quantity)
        {
            // Phí ship cơ bản theo khu vực
            var baseFee = region switch
            {
                "VN" => 30000m,
                "US" => 15m,    // USD
                "JP" => 1500m,  // JPY
                "CN" => 50m,    // CNY
                _ => 30000m
            };

            // Tính phí theo trọng lượng (nếu > 2kg)
            var totalWeight = weight * quantity;
            if (totalWeight > 2)
            {
                baseFee += (decimal)(totalWeight - 2) * (baseFee * 0.1m);
            }

            return baseFee;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}