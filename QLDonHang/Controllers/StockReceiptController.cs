using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using QLDonHang.Const;
using QLDonHang.DynamoDB;
using QLDonHang.Entities;
using System.Security.Claims;

namespace QLDonHang.Controllers
{
    public class StockReceiptController : Controller
    {
        private readonly DynamoDbService _dynamoDbService;
        private readonly ILogger<StockReceiptController> _logger;

        public StockReceiptController(DynamoDbService dynamoDbService, ILogger<StockReceiptController> logger)
        {
            _dynamoDbService = dynamoDbService;
            _logger = logger;
        }

        // POST: StockReceipt/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id)
        {
            try
            {
                // Lấy tên từ Claims
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(userName))
                {
                    TempData["ErrorMessage"] = "Không thể xác định người duyệt.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Lấy phiếu nhập
                var key = new Dictionary<string, AttributeValue>
                {
                    { "ReceiptId", new AttributeValue { S = id } }
                };

                var item = await _dynamoDbService.GetItemAsync(TableDb.STOCK_RECEIPTS, key);
                var receipt = DynamoDbMapper.ToObject<StockReceipt>(item);

                if (receipt.Status != "Pending")
                {
                    TempData["ErrorMessage"] = "Chỉ có thể duyệt phiếu đang chờ xử lý.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Cập nhật trạng thái VÀ LƯU NGƯỜI DUYỆT
                receipt.Status = "Approved";
                receipt.ApprovedBy = userName;
                receipt.ApprovedAt = DateTime.UtcNow;
                receipt.UpdatedAt = DateTime.UtcNow;

                var receiptItem = DynamoDbMapper.ToAttributeMap(receipt);
                await _dynamoDbService.AddOrUpdateItemAsync(TableDb.STOCK_RECEIPTS, receiptItem);

                TempData["SuccessMessage"] = $"Đã duyệt phiếu nhập kho bởi {userName}!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi duyệt phiếu nhập {id}");
                TempData["ErrorMessage"] = "Không thể duyệt phiếu nhập kho.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: StockReceipt/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(string id)
        {
            try
            {
                // Lấy tên từ Claims
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(userName))
                {
                    TempData["ErrorMessage"] = "Không thể xác định người hoàn thành.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Lấy phiếu nhập
                var key = new Dictionary<string, AttributeValue>
                {
                    { "ReceiptId", new AttributeValue { S = id } }
                };

                var item = await _dynamoDbService.GetItemAsync(TableDb.STOCK_RECEIPTS, key);
                var receipt = DynamoDbMapper.ToObject<StockReceipt>(item);

                if (receipt.Status != "Approved")
                {
                    TempData["ErrorMessage"] = "Chỉ có thể hoàn thành phiếu đã được duyệt.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Lấy chi tiết sản phẩm
                var receiptItems = await GetReceiptItemsAsync(id);

                // CẬP NHẬT TỒN KHO - CHỈ KHI HOÀN THÀNH
                foreach (var receiptItem in receiptItems)
                {
                    var inventoryKey = new Dictionary<string, AttributeValue>
                    {
                        { "WarehouseId", new AttributeValue { S = receipt.WarehouseId } },
                        { "ProductId", new AttributeValue { S = receiptItem.ProductId } }
                    };

                    var inventoryItem = await _dynamoDbService.GetItemAsync(TableDb.INVENTORY, inventoryKey);

                    if (inventoryItem != null && inventoryItem.Any())
                    {
                        // Cập nhật số lượng hiện có
                        var inventory = DynamoDbMapper.ToObject<Inventory>(inventoryItem);
                        inventory.Quantity += receiptItem.Quantity;

                        var updatedInventory = DynamoDbMapper.ToAttributeMap(inventory);
                        await _dynamoDbService.AddOrUpdateItemAsync(TableDb.INVENTORY, updatedInventory);
                    }
                    else
                    {
                        // Tạo mới tồn kho
                        var newInventory = new Inventory
                        {
                            WarehouseId = receipt.WarehouseId,
                            ProductId = receiptItem.ProductId,
                            Quantity = receiptItem.Quantity,
                        };

                        var newInventoryMap = DynamoDbMapper.ToAttributeMap(newInventory);
                        await _dynamoDbService.AddOrUpdateItemAsync(TableDb.INVENTORY, newInventoryMap);
                    }
                }

                // Cập nhật trạng thái phiếu nhập
                receipt.Status = "Completed";
                receipt.CompletedBy = userName;
                receipt.CompletedAt = DateTime.UtcNow;
                receipt.UpdatedAt = DateTime.UtcNow;

                var receiptMap = DynamoDbMapper.ToAttributeMap(receipt);
                await _dynamoDbService.AddOrUpdateItemAsync(TableDb.STOCK_RECEIPTS, receiptMap);

                TempData["SuccessMessage"] = $"Đã hoàn thành nhập kho bởi {userName} và cập nhật tồn kho!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi hoàn thành phiếu nhập {id}");
                TempData["ErrorMessage"] = "Không thể hoàn thành phiếu nhập kho.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: StockReceipt/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string id, string reason)
        {
            try
            {
                // Lấy tên từ Claims
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(userName))
                {
                    TempData["ErrorMessage"] = "Không thể xác định người hủy.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var key = new Dictionary<string, AttributeValue>
                {
                    { "ReceiptId", new AttributeValue { S = id } }
                };

                var item = await _dynamoDbService.GetItemAsync(TableDb.STOCK_RECEIPTS, key);
                var receipt = DynamoDbMapper.ToObject<StockReceipt>(item);

                if (receipt.Status == "Completed")
                {
                    TempData["ErrorMessage"] = "Không thể hủy phiếu đã hoàn thành.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                receipt.Status = "Cancelled";
                receipt.CancelledBy = userName;
                receipt.CancelledAt = DateTime.UtcNow;
                receipt.Notes = $"Đã hủy bởi {userName}: {reason}";
                receipt.UpdatedAt = DateTime.UtcNow;

                var receiptMap = DynamoDbMapper.ToAttributeMap(receipt);
                await _dynamoDbService.AddOrUpdateItemAsync(TableDb.STOCK_RECEIPTS, receiptMap);

                TempData["SuccessMessage"] = $"Đã hủy phiếu nhập kho bởi {userName}!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi hủy phiếu nhập {id}");
                TempData["ErrorMessage"] = "Không thể hủy phiếu nhập kho.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: StockReceipt/Index
        public async Task<IActionResult> Index(string? status, string? warehouse, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var items = await _dynamoDbService.ScanTableAsync(TableDb.STOCK_RECEIPTS);
                var receipts = items.Select(item => DynamoDbMapper.ToObject<StockReceipt>(item)).ToList();

                // Lọc theo trạng thái
                if (!string.IsNullOrEmpty(status))
                {
                    receipts = receipts.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Lọc theo kho
                if (!string.IsNullOrEmpty(warehouse))
                {
                    receipts = receipts.Where(r => r.WarehouseId.Equals(warehouse, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Lọc theo ngày
                if (fromDate.HasValue)
                {
                    receipts = receipts.Where(r => r.ReceiptDate >= fromDate.Value).ToList();
                }

                if (toDate.HasValue)
                {
                    receipts = receipts.Where(r => r.ReceiptDate <= toDate.Value).ToList();
                }

                // Sắp xếp theo ngày mới nhất
                receipts = receipts.OrderByDescending(r => r.CreatedAt).ToList();

                ViewBag.CurrentStatus = status;
                ViewBag.CurrentWarehouse = warehouse;
                ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
                ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

                return View(receipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách phiếu nhập kho");
                TempData["ErrorMessage"] = "Không thể tải danh sách phiếu nhập kho.";
                return View(new List<StockReceipt>());
            }
        }

        // GET: StockReceipt/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                // Lấy thông tin phiếu nhập
                var key = new Dictionary<string, AttributeValue>
                {
                    { "ReceiptId", new AttributeValue { S = id } }
                };

                var item = await _dynamoDbService.GetItemAsync(TableDb.STOCK_RECEIPTS, key);

                if (item == null || !item.Any())
                {
                    return NotFound();
                }

                var receipt = DynamoDbMapper.ToObject<StockReceipt>(item);

                // Lấy chi tiết các sản phẩm trong phiếu
                var receiptItems = await GetReceiptItemsAsync(id);
                ViewBag.ReceiptItems = receiptItems;

                // Tính tổng tiền
                receipt.TotalAmount = receiptItems.Sum(i => i.TotalPrice);

                return View(receipt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải chi tiết phiếu nhập {id}");
                return NotFound();
            }
        }

        // GET: StockReceipt/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Lấy danh sách kho
                var warehouses = await GetWarehousesAsync();
                ViewBag.Warehouses = warehouses;

                // Lấy danh sách sản phẩm
                var products = await GetProductsAsync();
                ViewBag.Products = products;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang tạo phiếu nhập");
                TempData["ErrorMessage"] = "Không thể tải trang tạo phiếu nhập.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: StockReceipt/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockReceipt receipt, List<StockReceiptItem> items)
        {
            try
            {
                // Lấy tên từ Claims
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(userName))
                {
                    ModelState.AddModelError("", "Không thể xác định người tạo phiếu.");
                    var warehouses = await GetWarehousesAsync();
                    ViewBag.Warehouses = warehouses;
                    var products = await GetProductsAsync();
                    ViewBag.Products = products;
                    return View(receipt);
                }

                // Lọc bỏ các item rỗng hoặc không hợp lệ
                if (items != null)
                {
                    items = items.Where(i =>
                        !string.IsNullOrEmpty(i.ProductId) &&
                        i.Quantity > 0 &&
                        i.UnitPrice >= 0
                    ).ToList();
                }

                // Validate
                if (items == null || !items.Any())
                {
                    ModelState.AddModelError("", "Vui lòng thêm ít nhất một sản phẩm vào phiếu nhập.");

                    var warehouses = await GetWarehousesAsync();
                    ViewBag.Warehouses = warehouses;
                    var products = await GetProductsAsync();
                    ViewBag.Products = products;

                    return View(receipt);
                }

                // Tạo phiếu nhập
                receipt.ReceiptId = $"SR{DateTime.UtcNow:yyyyMMddHHmmss}";
                receipt.Status = "Pending";
                receipt.CreatedBy = userName;
                receipt.CreatedAt = DateTime.UtcNow;
                receipt.ReceiptDate = DateTime.UtcNow;
                receipt.UpdatedAt = DateTime.UtcNow;

                var receiptItem = DynamoDbMapper.ToAttributeMap(receipt);
                await _dynamoDbService.AddOrUpdateItemAsync(TableDb.STOCK_RECEIPTS, receiptItem);

                // Lưu chi tiết sản phẩm
                foreach (var item in items)
                {
                    item.ReceiptId = receipt.ReceiptId;
                    item.CreatedAt = DateTime.UtcNow;
                    item.TotalPrice = item.Quantity * item.UnitPrice;

                    var itemMap = DynamoDbMapper.ToAttributeMap(item);
                    await _dynamoDbService.AddOrUpdateItemAsync(TableDb.STOCK_RECEIPT_ITEMS, itemMap);
                }

                TempData["SuccessMessage"] = "Tạo phiếu nhập kho thành công!";
                return RedirectToAction(nameof(Details), new { id = receipt.ReceiptId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phiếu nhập kho");
                ModelState.AddModelError("", "Không thể tạo phiếu nhập kho.");

                var warehouses = await GetWarehousesAsync();
                ViewBag.Warehouses = warehouses;
                var products = await GetProductsAsync();
                ViewBag.Products = products;

                return View(receipt);
            }
        }

        // Helper Methods
        private async Task<List<StockReceiptItem>> GetReceiptItemsAsync(string receiptId)
        {
            try
            {
                var query = new QueryRequest
                {
                    TableName = TableDb.STOCK_RECEIPT_ITEMS,
                    KeyConditionExpression = "ReceiptId = :rid",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":rid", new AttributeValue { S = receiptId } }
                    }
                };

                var items = await _dynamoDbService.QueryTableAsync(query);
                return items.Select(item => DynamoDbMapper.ToObject<StockReceiptItem>(item)).ToList();
            }
            catch
            {
                return new List<StockReceiptItem>();
            }
        }

        private async Task<List<string>> GetWarehousesAsync()
        {
            try
            {
                var items = await _dynamoDbService.ScanTableAsync(TableDb.INVENTORY);
                return items.Select(i => i["WarehouseId"].S).Distinct().OrderBy(w => w).ToList();
            }
            catch
            {
                return new List<string> { "WH001", "WH002", "WH003" }; // Default warehouses
            }
        }

        private async Task<List<Products>> GetProductsAsync()
        {
            try
            {
                var items = await _dynamoDbService.ScanTableAsync(TableDb.PRODUCTS);
                return items.Select(item => DynamoDbMapper.ToObject<Products>(item))
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.ProductName)
                    .ToList();
            }
            catch
            {
                return new List<Products>();
            }
        }
    }
}