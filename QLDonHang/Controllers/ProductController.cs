using Microsoft.AspNetCore.Mvc;
using QLDonHang.DynamoDB;
using QLDonHang.Entities;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using QLDonHang.Const;
using Microsoft.AspNetCore.Authorization;

namespace QLDonHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly DynamoDbService _dynamoDbService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProductController> _logger;

        public ProductController(DynamoDbService dynamoDbService, ILogger<ProductController> logger, IWebHostEnvironment environment)
        {
            _dynamoDbService = dynamoDbService;
            _logger = logger;
            _environment = environment;
        }

        // GET: Product/Index
        [Authorize]
        public async Task<IActionResult> Index(string? category, string? search, string? sortBy)
        {
            try
            {
                var items = await _dynamoDbService.ScanTableAsync(TableDb.PRODUCTS);
                var products = items.Select(item => DynamoDbMapper.ToObject<Products>(item)).ToList();

                // Lọc theo danh mục
                if (!string.IsNullOrEmpty(category))
                {
                    products = products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Tìm kiếm
                if (!string.IsNullOrEmpty(search))
                {
                    products = products.Where(p =>
                        p.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        p.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        p.Brand.Contains(search, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                // Sắp xếp
                products = sortBy switch
                {
                    "name" => products.OrderBy(p => p.ProductName).ToList(),
                    "newest" => products.OrderByDescending(p => p.CreatedAt).ToList(),
                    "oldest" => products.OrderBy(p => p.CreatedAt).ToList(),
                    _ => products.OrderBy(p => p.ProductName).ToList()
                };

                ViewBag.Categories = products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
                ViewBag.CurrentCategory = category;
                ViewBag.CurrentSearch = search;
                ViewBag.CurrentSort = sortBy;

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách sản phẩm");
                TempData["ErrorMessage"] = "Không thể tải danh sách sản phẩm. Vui lòng thử lại.";
                return View(new List<Products>());
            }
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var key = new Dictionary<string, AttributeValue>
                {
                    { "ProductId", new AttributeValue { S = id } }
                };

                var item = await _dynamoDbService.GetItemAsync(TableDb.PRODUCTS, key);

                if (item == null || !item.Any())
                {
                    return NotFound();
                }

                var product = DynamoDbMapper.ToObject<Products>(item);

                // Lấy giá theo quốc gia
                var pricingItems = await GetProductPricingAsync(id);
                ViewBag.Pricing = pricingItems;

                // Lấy thông tin tồn kho
                var inventoryItems = await GetProductInventoryAsync(id);
                ViewBag.Inventory = inventoryItems;

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải chi tiết sản phẩm {id}");
                return NotFound();
            }
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Products product, IFormFile imageFile)
        {
            try
            {
                product.ProductId = Guid.NewGuid().ToString();
                product.CreatedAt = DateTime.UtcNow;
                product.IsActive = true;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadResult = await UploadImageAsync(imageFile);
                    if (uploadResult.Success)
                    {
                        product.ImageUrl = uploadResult.FilePath;
                    }
                    else
                    {
                        ModelState.AddModelError("", uploadResult.ErrorMessage ?? "Không thể upload ảnh.");
                        return View(product);
                    }
                }

                var item = DynamoDbMapper.ToAttributeMap(product);
                await _dynamoDbService.AddOrUpdateItemAsync(TableDb.PRODUCTS, item);

                TempData["SuccessMessage"] = "Sản phẩm đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo sản phẩm");
                ModelState.AddModelError("", "Không thể tạo sản phẩm. Vui lòng thử lại.");
                return View(product);
            }
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var key = new Dictionary<string, AttributeValue>
                {
                    { "ProductId", new AttributeValue { S = id } }
                };

                var item = await _dynamoDbService.GetItemAsync(TableDb.PRODUCTS, key);

                if (item == null || !item.Any())
                {
                    return NotFound();
                }

                var product = DynamoDbMapper.ToObject<Products>(item);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải sản phẩm {id}");
                return NotFound();
            }
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Products product, IFormFile? imageFile)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            try
            {
                product.Tags = Request.Form["Tags"].ToString().Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray();
                product.UpdatedAt = DateTime.UtcNow;

                // Xử lý upload ảnh mới
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        DeleteImage(product.ImageUrl);
                    }

                    var uploadResult = await UploadImageAsync(imageFile);
                    if (uploadResult.Success)
                    {
                        product.ImageUrl = uploadResult.FilePath;
                    }
                    else
                    {
                        ModelState.AddModelError("", uploadResult.ErrorMessage ?? "Không thể upload ảnh.");
                        return View(product);
                    }
                }

                var item = DynamoDbMapper.ToAttributeMap(product);
                await _dynamoDbService.AddOrUpdateItemAsync(TableDb.PRODUCTS, item);

                TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật sản phẩm");
                ModelState.AddModelError("", "Không thể cập nhật sản phẩm. Vui lòng thử lại.");
                return View(product);
            }
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var key = new Dictionary<string, AttributeValue>
                {
                    { "ProductId", new AttributeValue { S = id } }
                };

                var item = await _dynamoDbService.GetItemAsync(TableDb.PRODUCTS, key);

                if (item == null || !item.Any())
                {
                    return NotFound();
                }

                var product = DynamoDbMapper.ToObject<Products>(item);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải sản phẩm {id}");
                return NotFound();
            }
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var key = new Dictionary<string, AttributeValue>
                {
                    { "ProductId", new AttributeValue { S = id } }
                };

                await _dynamoDbService.DeleteItemAsync(TableDb.PRODUCTS, key);

                TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm");
                TempData["ErrorMessage"] = "Không thể xóa sản phẩm. Vui lòng thử lại.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Product/AddPricing/productId
        public async Task<IActionResult> AddPricing(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                // Kiểm tra sản phẩm có tồn tại không
                var key = new Dictionary<string, AttributeValue>
                {
                    { "ProductId", new AttributeValue { S = id } }
                };

                var item = await _dynamoDbService.GetItemAsync(TableDb.PRODUCTS, key);

                if (item == null || !item.Any())
                {
                    return NotFound();
                }

                var product = DynamoDbMapper.ToObject<Products>(item);
                ViewBag.Product = product;

                // Tạo model mới với ProductId
                var pricing = new ProductPricing
                {
                    ProductId = id
                };
                return View(pricing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải trang thêm giá cho sản phẩm {id}");
                return NotFound();
            }
        }

        // POST: Product/AddPricing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPricing(ProductPricing pricing)
        {
            try
            {
                // Kiểm tra xem đã có giá cho region này chưa
                var existingPricing = await GetProductPricingByRegionAsync(pricing.ProductId, pricing.Region);

                if (existingPricing != null)
                {
                    ModelState.AddModelError("Region", "Đã có giá cho khu vực này. Vui lòng chọn khu vực khác hoặc sửa giá hiện tại.");

                    var key = new Dictionary<string, AttributeValue>
                    {
                        { "ProductId", new AttributeValue { S = pricing.ProductId } }
                    };
                    var item = await _dynamoDbService.GetItemAsync(TableDb.PRODUCTS, key);
                    var product = DynamoDbMapper.ToObject<Products>(item);
                    ViewBag.Product = product;

                    return View(pricing);
                }
                pricing.Tax = pricing.Tax == 0 ? null : pricing.Tax;
                pricing.Discount = pricing.Discount == 0 ? null : pricing.Discount;
                pricing.DiscountAmount = pricing.DiscountAmount == 0 ? null : pricing.DiscountAmount;

                pricing.Tax ??= 0;
                pricing.Discount ??= 0;
                pricing.DiscountAmount ??= 0;
                pricing.UpdatedAt = DateTime.UtcNow;

                var pricingItem = DynamoDbMapper.ToAttributeMap(pricing);
                await _dynamoDbService.AddOrUpdateItemAsync(TableDb.PRODUCT_PRICING, pricingItem);

                TempData["SuccessMessage"] = "Đã thêm giá thành công!";
                return RedirectToAction(nameof(Details), new { id = pricing.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm giá sản phẩm");
                ModelState.AddModelError("", "Không thể thêm giá. Vui lòng thử lại.");

                var key = new Dictionary<string, AttributeValue>
                {
                    { "ProductId", new AttributeValue { S = pricing.ProductId } }
                };
                var item = await _dynamoDbService.GetItemAsync(TableDb.PRODUCTS, key);
                var product = DynamoDbMapper.ToObject<Products>(item);
                ViewBag.Product = product;

                return View(pricing);
            }
        }

        // GET: Product/EditPricing/productId/region
        [HttpGet("EditPricing/{id}/{region}")]
        public async Task<IActionResult> EditPricing(string id, string region)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(region))
            {
                return NotFound();
            }

            try
            {
                // Lấy thông tin sản phẩm
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
                ViewBag.Product = product;

                // Lấy thông tin giá
                var pricing = await GetProductPricingByRegionAsync(id, region);

                if (pricing == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin giá cho khu vực này.";
                    return RedirectToAction("Index");
                };

                return View(pricing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải trang sửa giá {id} - {region}");
                TempData["ErrorMessage"] = "Không thể tải thông tin giá. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }
        }

        // POST: Product/EditPricing
        [HttpPost("EditPricing/{id}/{region}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPricing([FromForm] ProductPricing pricing)
        {
            try
            {
                pricing.UpdatedAt = DateTime.UtcNow;

                var pricingItem = DynamoDbMapper.ToAttributeMap(pricing);
                await _dynamoDbService.AddOrUpdateItemAsync(TableDb.PRODUCT_PRICING, pricingItem);

                TempData["SuccessMessage"] = "Đã cập nhật giá thành công!";
                return RedirectToAction(nameof(Details), new { id = pricing.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật giá sản phẩm");
                ModelState.AddModelError("", "Không thể cập nhật giá. Vui lòng thử lại.");

                var key = new Dictionary<string, AttributeValue>
                {
                    { "ProductId", new AttributeValue { S = pricing.ProductId } }
                };
                var item = await _dynamoDbService.GetItemAsync(TableDb.PRODUCTS, key);
                var product = DynamoDbMapper.ToObject<Products>(item);
                ViewBag.Product = product;

                return View(pricing);
            }
        }

        // POST: Product/DeletePricing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePricing(string productId, string region)
        {
            try
            {
                var key = new Dictionary<string, AttributeValue>
                {
                    { "ProductId", new AttributeValue { S = productId } },
                    { "Region", new AttributeValue { S = region } }
                };

                await _dynamoDbService.DeleteItemAsync(TableDb.PRODUCT_PRICING, key);

                TempData["SuccessMessage"] = "Đã xóa giá thành công!";
                return RedirectToAction(nameof(Details), new { id = productId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa giá sản phẩm");
                TempData["ErrorMessage"] = "Không thể xóa giá. Vui lòng thử lại.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }
        }

        // Helper method
        private async Task<ProductPricing?> GetProductPricingByRegionAsync(string productId, string region)
        {
            try
            {
                var query = new QueryRequest
                {
                    TableName = TableDb.PRODUCT_PRICING,
                    KeyConditionExpression = "ProductId = :pid AND #R = :region",
                    ExpressionAttributeNames = new Dictionary<string, string>
                    {
                        { "#R", "Region" }
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":pid", new AttributeValue { S = productId } },
                        { ":region", new AttributeValue { S = region } }
                    }
                };


                var items = await _dynamoDbService.QueryTableAsync(query);

                if (items.Any())
                {
                    return DynamoDbMapper.ToObject<ProductPricing>(items.First());
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // Upload Image
        private async Task<(bool Success, string? FilePath, string? ErrorMessage)> UploadImageAsync(IFormFile file)
        {
            try
            {
                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return (false, null, "Chỉ cho phép upload ảnh định dạng JPG, PNG, GIF, WEBP.");
                }

                // Kiểm tra kích thước file (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return (false, null, "Kích thước ảnh không được vượt quá 5MB.");
                }

                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Trả về đường dẫn relative
                var relativePath = $"/uploads/products/{fileName}";
                return (true, relativePath, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi upload ảnh");
                return (false, null, "Có lỗi xảy ra khi upload ảnh.");
            }
        }

        // Delete Image
        private void DeleteImage(string? imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl) || !imageUrl.StartsWith("/uploads/"))
                {
                    return;
                }

                var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa ảnh");
            }
        }

        private async Task<List<ProductPricing>> GetProductPricingAsync(string productId)
        {
            try
            {
                var query = new QueryRequest
                {
                    TableName = TableDb.PRODUCT_PRICING,
                    KeyConditionExpression = "ProductId = :pid",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":pid", new AttributeValue { S = productId } }
                    }
                };

                var items = await _dynamoDbService.QueryTableAsync(query);
                return items.Select(item => new ProductPricing
                {
                    ProductId = item["ProductId"].S,
                    Region = item["Region"].S,
                    Price = decimal.Parse(item["Price"].N),
                    Currency = item["Currency"].S,
                    Discount = item.ContainsKey("Discount") ? decimal.Parse(item["Discount"].N) : 0,
                    Tax = item.ContainsKey("Tax") ? decimal.Parse(item["Tax"].N) : 0
                }).ToList();
            }
            catch
            {
                return new List<ProductPricing>();
            }
        }

        private async Task<List<Inventory>> GetProductInventoryAsync(string productId)
        {
            try
            {
                var scanRequest = new ScanRequest
                {
                    TableName = TableDb.INVENTORY,
                    FilterExpression = "ProductId = :pid",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":pid", new AttributeValue { S = productId } }
                    }
                };

                var items = await _dynamoDbService.ScanTableAsync(scanRequest);
                return items.Select(item => new Inventory
                {
                    WarehouseId = item["WarehouseId"].S,
                    ProductId = item["ProductId"].S,
                    Quantity = int.Parse(item["Quantity"].N)
                }).ToList();
            }
            catch
            {
                return new List<Inventory>();
            }
        }
    }
}