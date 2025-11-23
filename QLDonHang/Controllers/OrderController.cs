using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using QLDonHang.Const;
using QLDonHang.DynamoDB;
using QLDonHang.Entities;
using System.Security.Claims;

namespace QLDonHang.Controllers
{
    public class OrderController : Controller
    {
        private readonly DynamoDbService _dynamoDbService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(DynamoDbService dynamoDbService, ILogger<OrderController> logger)
        {
            _dynamoDbService = dynamoDbService;
            _logger = logger;
        }

        // GET: Order/OrderDetails/ORD20250124...
        public async Task<IActionResult> OrderDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                // Lấy customerId từ Claims
                var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(customerId))
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem đơn hàng.";
                    return RedirectToAction("Login", "Auth");
                }

                // Query đơn hàng theo CustomerId và OrderId
                var queryRequest = new QueryRequest
                {
                    TableName = TableDb.ORDERS,
                    KeyConditionExpression = "CustomerId = :cid AND OrderId = :oid",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":cid", new AttributeValue { S = customerId } },
                        { ":oid", new AttributeValue { S = id } }
                    }
                };

                var orderItems = await _dynamoDbService.QueryTableAsync(queryRequest);

                if (orderItems == null || !orderItems.Any())
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem.";
                    return RedirectToAction("Index", "Home");
                }

                var order = DynamoDbMapper.ToObject<Order>(orderItems.First());

                // Lấy items của đơn hàng
                var itemQuery = new QueryRequest
                {
                    TableName = TableDb.ORDER_ITEM,
                    KeyConditionExpression = "OrderId = :oid",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":oid", new AttributeValue { S = id } }
                    }
                };

                var items = await _dynamoDbService.QueryTableAsync(itemQuery);
                var orderItemsList = items.Select(item => DynamoDbMapper.ToObject<OrderItem>(item)).ToList();

                // Lấy thông tin sản phẩm
                var productInfos = new Dictionary<string, Products>();
                foreach (var item in orderItemsList)
                {
                    var productKey = new Dictionary<string, AttributeValue>
                    {
                        { "ProductId", new AttributeValue { S = item.ProductId } }
                    };

                    try
                    {
                        var productItem = await _dynamoDbService.GetItemAsync(TableDb.PRODUCTS, productKey);
                        if (productItem != null && productItem.Any())
                        {
                            productInfos[item.ProductId] = DynamoDbMapper.ToObject<Products>(productItem);
                        }
                    }
                    catch { }
                }

                ViewBag.OrderItems = orderItemsList;
                ViewBag.ProductInfos = productInfos;

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải chi tiết đơn hàng {id}");
                TempData["ErrorMessage"] = "Không thể tải thông tin đơn hàng.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Order/History
        public async Task<IActionResult> History()
        {
            try
            {
                // Lấy customerId từ Claims
                var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(customerId))
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem đơn hàng.";
                    return RedirectToAction("Login", "Auth");
                }

                // Query tất cả đơn hàng của customer
                var queryRequest = new QueryRequest
                {
                    TableName = TableDb.ORDERS,
                    KeyConditionExpression = "CustomerId = :cid",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":cid", new AttributeValue { S = customerId } }
            },
                    ScanIndexForward = false // Sắp xếp theo thứ tự giảm dần (đơn hàng mới nhất trước)
                };

                var orderItems = await _dynamoDbService.QueryTableAsync(queryRequest);

                if (orderItems == null || !orderItems.Any())
                {
                    ViewBag.Orders = new List<Order>();
                    return View();
                }

                // Chuyển đổi sang danh sách Order
                var orders = orderItems.Select(item => DynamoDbMapper.ToObject<Order>(item))
                                       .OrderByDescending(o => o.OrderDate)
                                       .ToList();

                // Lấy thông tin tổng số sản phẩm cho mỗi đơn hàng
                var orderWithItemCounts = new Dictionary<string, int>();
                foreach (var order in orders)
                {
                    var itemQuery = new QueryRequest
                    {
                        TableName = TableDb.ORDER_ITEM,
                        KeyConditionExpression = "OrderId = :oid",
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                        {
                            { ":oid", new AttributeValue { S = order.OrderId } }
                        }
                    };

                    try
                    {
                        var itemsResponse = await _dynamoDbService.QueryTableAsync(itemQuery);
                        // Đếm số lượng items trong response
                        orderWithItemCounts[order.OrderId] = itemsResponse?.Count ?? 0;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Lỗi khi đếm items của đơn hàng {order.OrderId}");
                        orderWithItemCounts[order.OrderId] = 0;
                    }
                }

                ViewBag.Orders = orders;
                ViewBag.OrderItemCounts = orderWithItemCounts;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách đơn hàng");
                TempData["ErrorMessage"] = "Không thể tải danh sách đơn hàng.";
                ViewBag.Orders = new List<Order>();
                return View();
            }
        }
    }
}