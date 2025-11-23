using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.CodeAnalysis.Scripting;
using QLDonHang.Const;
using QLDonHang.Entities;

namespace QLDonHang.DynamoDB.Seed
{
    public class DbSeeder
    {
        private readonly DynamoDbService _dynamoDbService;
        public DbSeeder(DynamoDbService dynamoDbService) 
        {
            _dynamoDbService = dynamoDbService;
        }

        public async Task DeleteAllTableAsync()
        {
            await _dynamoDbService.DeleteTableAsync(TableDb.CUSTOMERS);
            await _dynamoDbService.DeleteTableAsync(TableDb.CUSTOMER_ADDRESSES);
            await _dynamoDbService.DeleteTableAsync(TableDb.ORDERS);
            await _dynamoDbService.DeleteTableAsync(TableDb.ORDER_ITEM);
            await _dynamoDbService.DeleteTableAsync(TableDb.INVENTORY);
            await _dynamoDbService.DeleteTableAsync(TableDb.PRODUCTS);
            await _dynamoDbService.DeleteTableAsync(TableDb.PRODUCT_PRICING);
            await _dynamoDbService.DeleteTableAsync(TableDb.WAREHOUSES);
            await _dynamoDbService.DeleteTableAsync(TableDb.SHIPPING);
            await _dynamoDbService.DeleteTableAsync(TableDb.STOCK_RECEIPTS);
            await _dynamoDbService.DeleteTableAsync(TableDb.STOCK_RECEIPT_ITEMS);
        }


        // Thêm dữ liệu mẫu vào cơ sở dữ liệu DynamoDB
        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            await CreateTablesAsync(cancellationToken);
            await AddDataAsync(cancellationToken);
        }

        private async Task CreateTablesAsync(CancellationToken cancellationToken = default)
        {
            await CreateTableCustomerAsync();
            await CreateTableCustomerAddressAsync();
            await CreateTableOrderAsync();
            await CreateTableOrderItemAsync();
            await CreateTableInventoryAsync();
            await CreateTableProductAsync();
            await CreateTableProductPricingAsync();
            await CreateTableWarehouseAsync();
            await CreateTableShippingAsync();
            await CreateTableStockReceiptsAsync();
            await CreateTableStockReceiptItemAsync();
            // code ví dụ tạo bảng
            //var table = new CreateTableRequest
            //{
            //    TableName = TableDb.TEST_TABLE,
            //    AttributeDefinitions = new List<AttributeDefinition>
            //    {
            //        new AttributeDefinition
            //        {
            //            AttributeName = "Id",
            //            AttributeType = "S" // S - String, N - Number, B - Binary
            //        },
            //        new AttributeDefinition
            //        {
            //            AttributeName = "Name",
            //            AttributeType = "S" // S - String, N - Number, B - Binary
            //        },
            //    },
            //    KeySchema = new List<KeySchemaElement>
            //    {
            //        new KeySchemaElement
            //        {
            //            AttributeName = "Id",
            //            KeyType = "HASH" // Partition key
            //        },
            //        new KeySchemaElement
            //        {
            //            AttributeName = "Name",
            //            KeyType = "RANGE" // Sort key
            //        }
            //    }
            //};
            //await _dynamoDbService.CreateTableIfNotExistsAsync(table);
        }

        // Thêm dữ liệu mẫu nếu cần

        private async Task AddDataAsync(CancellationToken cancellationToken)
        {
            // 1. Seed Customers
            await SeedCustomersAsync();

            // 2. Seed Warehouses
            await SeedWarehousesAsync();

            // 3. Seed Products
            await SeedProductsAsync();

            // 4. Seed Product Pricing
            await SeedProductPricingAsync();

            // 5. Seed Inventory
            await SeedInventoryAsync();

            // 6. Seed Customer Addresses
            await SeedCustomerAddressesAsync();

            // 7. Seed Orders
            await SeedOrdersAsync();

            // 8. Seed Order Items
            await SeedOrderItemsAsync();

            // 9. Seed Shipping
            await SeedShippingAsync();

            // 10. Seed Stock Receipts
            await SeedStockReceiptsAsync();

            // 11. Seed Stock Receipt Items
            await SeedStockReceiptItemsAsync();
        }

        #region Seed Methods

        private async Task SeedCustomersAsync()
        {
            var customers = new List<Customer>
            {
                new Customer
                {
                    CustomerId = "KH002",
                    Username = "tranthib",
                    HashPassword = BCrypt.Net.BCrypt.HashPassword("123"),
                    FullName = "Trần Thị B",
                    Email = "tranthib@gmail.com",
                    CountryCode = "VN",
                    Phone = "0902345678",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-5)
                },
                new Customer
                {
                    CustomerId = "KH003",
                    Username = "johnsmith",
                    HashPassword = BCrypt.Net.BCrypt.HashPassword("123"),
                    FullName = "John Smith",
                    Email = "john.smith@gmail.com",
                    CountryCode = "US",
                    Phone = "+15551234567",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-4)
                },
                new Customer
                {
                    CustomerId = "KH004",
                    Username = "tanakayuki",
                    HashPassword = BCrypt.Net.BCrypt.HashPassword("123"),
                    FullName = "Tanaka Yuki",
                    Email = "tanaka.yuki@gmail.com",
                    CountryCode = "JP",
                    Phone = "+819012345678",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                },
                new Customer
                {
                    CustomerId = "admin",
                    Username = "admin",
                    HashPassword = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FullName = "admin",
                    Email = "admin@company.com",
                    CountryCode = "VN",
                    Phone = "0900000000",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddYears(-1)
                }
            };

            foreach (var customer in customers)
            {
                await _dynamoDbService.AddOrUpdateItemAsync(
                    TableDb.CUSTOMERS,
                    DynamoDbMapper.ToAttributeMap(customer)
                );
            }
        }

        private async Task SeedWarehousesAsync()
        {
            var warehouses = new List<Warehouse>
            {
                new Warehouse
                {
                    WarehouseId = "WH-VN-HCM",
                    CountryCode = "VN",
                    Address = "123 Nguyễn Văn Linh, Quận 7, TP.HCM, Việt Nam"
                },
                new Warehouse
                {
                    WarehouseId = "WH-VN-HN",
                    CountryCode = "VN",
                    Address = "456 Giải Phóng, Hai Bà Trưng, Hà Nội, Việt Nam"
                },
                new Warehouse
                {
                    WarehouseId = "WH-US-CA",
                    CountryCode = "US",
                    Address = "789 Harbor Blvd, Los Angeles, CA 90001, USA"
                },
                new Warehouse
                {
                    WarehouseId = "WH-JP-TK",
                    CountryCode = "JP",
                    Address = "1-2-3 Shibuya, Tokyo 150-0002, Japan"
                },
                new Warehouse
                {
                    WarehouseId = "WH-CN-SH",
                    CountryCode = "CN",
                    Address = "100 Nanjing Road, Shanghai 200001, China"
                }
            };

            foreach (var warehouse in warehouses)
            {
                await _dynamoDbService.AddOrUpdateItemAsync(
                    TableDb.WAREHOUSES,
                    DynamoDbMapper.ToAttributeMap(warehouse)
                );
            }
        }

        private async Task SeedProductsAsync()
        {
            var products = new List<Products>
            {
                new Products
                {
                    ProductId = "PRD001",
                    ProductName = "iPhone 15 Pro Max",
                    Category = "Điện thoại",
                    Description = "iPhone 15 Pro Max 256GB - Titan Tự Nhiên",
                    Brand = "Apple",
                    SKU = "IP15PM-256-TIT",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-6),
                    Tags = new[] { "smartphone", "apple", "flagship" },
                    Weight = 0.221,
                    Dimensions = "159.9 x 76.7 x 8.25",
                },
                new Products
                {
                    ProductId = "PRD002",
                    ProductName = "Samsung Galaxy S24 Ultra",
                    Category = "Điện thoại",
                    Description = "Samsung Galaxy S24 Ultra 512GB - Titan Đen",
                    Brand = "Samsung",
                    SKU = "S24U-512-BLK",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-5),
                    Tags = new[] { "smartphone", "samsung", "android" },
                    Weight = 0.233,
                    Dimensions = "162.3 x 79.0 x 8.6"
                },
                new Products
                {
                    ProductId = "PRD003",
                    ProductName = "MacBook Pro 14 M3",
                    Category = "Laptop",
                    Description = "MacBook Pro 14 inch M3 Pro 18GB 512GB",
                    Brand = "Apple",
                    SKU = "MBP14-M3-18-512",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-4),
                    Tags = new[] { "laptop", "apple", "macbook" },
                    Weight = 1.55,
                    Dimensions = "31.26 x 22.12 x 1.55"
                },
                new Products
                {
                    ProductId = "PRD004",
                    ProductName = "Dell XPS 15",
                    Category = "Laptop",
                    Description = "Dell XPS 15 9530 i7-13700H 16GB 512GB RTX 4050",
                    Brand = "Dell",
                    SKU = "XPS15-9530-I7",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3),
                    Tags = new[] { "laptop", "dell", "gaming" },
                    Weight = 1.86,
                    Dimensions = "34.45 x 23.03 x 1.8"
                },
                new Products
                {
                    ProductId = "PRD005",
                    ProductName = "AirPods Pro 2",
                    Category = "Phụ kiện",
                    Description = "Apple AirPods Pro 2 USB-C",
                    Brand = "Apple",
                    SKU = "APP2-USBC",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2),
                    Tags = new[] { "audio", "apple", "wireless" },
                    Weight = 0.056,
                    Dimensions = "6.05 x 4.52 x 2.14"
                },
                new Products
                {
                    ProductId = "PRD006",
                    ProductName = "Sony WH-1000XM5",
                    Category = "Phụ kiện",
                    Description = "Sony WH-1000XM5 Wireless Noise Cancelling Headphones",
                    Brand = "Sony",
                    SKU = "WH1000XM5-BLK",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2),
                    Tags = new[] { "audio", "sony", "headphones" },
                    Weight = 0.250,
                    Dimensions = "20.0 x 17.5 x 8.5"
                },
                new Products
                {
                    ProductId = "PRD007",
                    ProductName = "iPad Pro 12.9 M2",
                    Category = "Máy tính bảng",
                    Description = "iPad Pro 12.9 inch M2 WiFi 256GB",
                    Brand = "Apple",
                    SKU = "IPADP-M2-256",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3),
                    Tags = new[] { "tablet", "apple", "ipad" },
                    Weight = 0.682,
                    Dimensions = "28.06 x 21.49 x 0.64"
                },
                new Products
                {
                    ProductId = "PRD008",
                    ProductName = "Samsung Galaxy Tab S9+",
                    Category = "Máy tính bảng",
                    Description = "Samsung Galaxy Tab S9+ 12.4 inch 256GB",
                    Brand = "Samsung",
                    SKU = "TABS9P-256",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2),
                    Tags = new[] { "tablet", "samsung", "android" },
                    Weight = 0.586,
                    Dimensions = "28.52 x 18.51 x 0.57"
                },
                new Products
                {
                    ProductId = "PRD009",
                    ProductName = "Apple Watch Series 9",
                    Category = "Đồng hồ thông minh",
                    Description = "Apple Watch Series 9 GPS 45mm",
                    Brand = "Apple",
                    SKU = "AW9-GPS-45",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1),
                    Tags = new[] { "smartwatch", "apple", "wearable" },
                    Weight = 0.051,
                    Dimensions = "4.5 x 3.8 x 1.07"
                },
                new Products
                {
                    ProductId = "PRD010",
                    ProductName = "Nintendo Switch OLED",
                    Category = "Gaming",
                    Description = "Nintendo Switch OLED Model White",
                    Brand = "Nintendo",
                    SKU = "NSW-OLED-WHT",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1),
                    Tags = new[] { "gaming", "nintendo", "console" },
                    Weight = 0.420,
                    Dimensions = "24.2 x 10.2 x 1.39"
                }
            };

            foreach (var product in products)
            {
                await _dynamoDbService.AddOrUpdateItemAsync(
                    TableDb.PRODUCTS,
                    DynamoDbMapper.ToAttributeMap(product)
                );
            }
        }

        private async Task SeedProductPricingAsync()
        {
            var pricings = new List<ProductPricing>
            {
                // iPhone 15 Pro Max
                new ProductPricing { ProductId = "PRD001", Region = "VN", Price = 34990000, Currency = "VND", Discount = 5, Tax = 10, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD001", Region = "US", Price = 1199, Currency = "USD", Discount = 0, Tax = 8, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD001", Region = "JP", Price = 189800, Currency = "JPY", Discount = 3, Tax = 10, UpdatedAt = DateTime.UtcNow },
                
                // Samsung Galaxy S24 Ultra
                new ProductPricing { ProductId = "PRD002", Region = "VN", Price = 33990000, Currency = "VND", Discount = 10, Tax = 10, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD002", Region = "US", Price = 1299, Currency = "USD", Discount = 5, Tax = 8, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD002", Region = "JP", Price = 179800, Currency = "JPY", Discount = 0, Tax = 10, UpdatedAt = DateTime.UtcNow },
                
                // MacBook Pro 14
                new ProductPricing { ProductId = "PRD003", Region = "VN", Price = 52990000, Currency = "VND", Discount = 0, Tax = 10, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD003", Region = "US", Price = 1999, Currency = "USD", Discount = 0, Tax = 8, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD003", Region = "JP", Price = 298800, Currency = "JPY", Discount = 0, Tax = 10, UpdatedAt = DateTime.UtcNow },
                
                // Dell XPS 15
                new ProductPricing { ProductId = "PRD004", Region = "VN", Price = 45990000, Currency = "VND", Discount = 15, Tax = 10, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD004", Region = "US", Price = 1799, Currency = "USD", Discount = 10, Tax = 8, UpdatedAt = DateTime.UtcNow },
                
                // AirPods Pro 2
                new ProductPricing { ProductId = "PRD005", Region = "VN", Price = 6490000, Currency = "VND", Discount = 8, Tax = 10, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD005", Region = "US", Price = 249, Currency = "USD", Discount = 0, Tax = 8, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD005", Region = "JP", Price = 39800, Currency = "JPY", Discount = 5, Tax = 10, UpdatedAt = DateTime.UtcNow },
                
                // Sony WH-1000XM5
                new ProductPricing { ProductId = "PRD006", Region = "VN", Price = 8990000, Currency = "VND", Discount = 12, Tax = 10, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD006", Region = "US", Price = 399, Currency = "USD", Discount = 15, Tax = 8, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD006", Region = "JP", Price = 49800, Currency = "JPY", Discount = 10, Tax = 10, UpdatedAt = DateTime.UtcNow },
                
                // iPad Pro 12.9
                new ProductPricing { ProductId = "PRD007", Region = "VN", Price = 29990000, Currency = "VND", Discount = 5, Tax = 10, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD007", Region = "US", Price = 1099, Currency = "USD", Discount = 0, Tax = 8, UpdatedAt = DateTime.UtcNow },
                
                // Samsung Tab S9+
                new ProductPricing { ProductId = "PRD008", Region = "VN", Price = 24990000, Currency = "VND", Discount = 10, Tax = 10, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD008", Region = "US", Price = 999, Currency = "USD", Discount = 8, Tax = 8, UpdatedAt = DateTime.UtcNow },
                
                // Apple Watch Series 9
                new ProductPricing { ProductId = "PRD009", Region = "VN", Price = 11490000, Currency = "VND", Discount = 0, Tax = 10, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD009", Region = "US", Price = 429, Currency = "USD", Discount = 0, Tax = 8, UpdatedAt = DateTime.UtcNow },
                
                // Nintendo Switch OLED
                new ProductPricing { ProductId = "PRD010", Region = "VN", Price = 8990000, Currency = "VND", Discount = 5, Tax = 10, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD010", Region = "US", Price = 349, Currency = "USD", Discount = 0, Tax = 8, UpdatedAt = DateTime.UtcNow },
                new ProductPricing { ProductId = "PRD010", Region = "JP", Price = 37980, Currency = "JPY", Discount = 0, Tax = 10, UpdatedAt = DateTime.UtcNow }
            };

            foreach (var pricing in pricings)
            {
                await _dynamoDbService.AddOrUpdateItemAsync(
                    TableDb.PRODUCT_PRICING,
                    DynamoDbMapper.ToAttributeMap(pricing)
                );
            }
        }

        private async Task SeedInventoryAsync()
        {
            var inventories = new List<Inventory>
            {
                // WH-VN-HCM
                new Inventory { WarehouseId = "WH-VN-HCM", ProductId = "PRD001", Quantity = 50 },
                new Inventory { WarehouseId = "WH-VN-HCM", ProductId = "PRD002", Quantity = 45 },
                new Inventory { WarehouseId = "WH-VN-HCM", ProductId = "PRD003", Quantity = 30 },
                new Inventory { WarehouseId = "WH-VN-HCM", ProductId = "PRD004", Quantity = 25 },
                new Inventory { WarehouseId = "WH-VN-HCM", ProductId = "PRD005", Quantity = 100 },
                new Inventory { WarehouseId = "WH-VN-HCM", ProductId = "PRD006", Quantity = 60 },
                new Inventory { WarehouseId = "WH-VN-HCM", ProductId = "PRD007", Quantity = 40 },
                new Inventory { WarehouseId = "WH-VN-HCM", ProductId = "PRD008", Quantity = 35 },
                new Inventory { WarehouseId = "WH-VN-HCM", ProductId = "PRD009", Quantity = 75 },
                new Inventory { WarehouseId = "WH-VN-HCM", ProductId = "PRD010", Quantity = 80 },
                
                // WH-VN-HN
                new Inventory { WarehouseId = "WH-VN-HN", ProductId = "PRD001", Quantity = 40 },
                new Inventory { WarehouseId = "WH-VN-HN", ProductId = "PRD002", Quantity = 38 },
                new Inventory { WarehouseId = "WH-VN-HN", ProductId = "PRD003", Quantity = 25 },
                new Inventory { WarehouseId = "WH-VN-HN", ProductId = "PRD005", Quantity = 90 },
                new Inventory { WarehouseId = "WH-VN-HN", ProductId = "PRD009", Quantity = 65 },
                
                // WH-US-CA
                new Inventory { WarehouseId = "WH-US-CA", ProductId = "PRD001", Quantity = 120 },
                new Inventory { WarehouseId = "WH-US-CA", ProductId = "PRD002", Quantity = 100 },
                new Inventory { WarehouseId = "WH-US-CA", ProductId = "PRD003", Quantity = 80 },
                new Inventory { WarehouseId = "WH-US-CA", ProductId = "PRD004", Quantity = 70 },
                new Inventory { WarehouseId = "WH-US-CA", ProductId = "PRD005", Quantity = 200 },
                
                // WH-JP-TK
                new Inventory { WarehouseId = "WH-JP-TK", ProductId = "PRD001", Quantity = 90 },
                new Inventory { WarehouseId = "WH-JP-TK", ProductId = "PRD005", Quantity = 150 },
                new Inventory { WarehouseId = "WH-JP-TK", ProductId = "PRD006", Quantity = 110 },
                new Inventory { WarehouseId = "WH-JP-TK", ProductId = "PRD010", Quantity = 200 }
            };

            foreach (var inventory in inventories)
            {
                await _dynamoDbService.AddOrUpdateItemAsync(
                    TableDb.INVENTORY,
                    DynamoDbMapper.ToAttributeMap(inventory)
                );
            }
        }

        private async Task SeedCustomerAddressesAsync()
        {
            var addresses = new List<CustomerAddress>
            {
                new CustomerAddress
                {
                    CustomerId = "KH002",
                    AddressId = "ADDR002",
                    City = "TP.HCM",
                    CountryCode = "VN",
                },
                new CustomerAddress
                {
                    CustomerId = "KH003",
                    AddressId = "ADDR003",
                    City = "Los Angeles",
                    CountryCode = "US",
                },
                new CustomerAddress
                {
                    CustomerId = "KH004",
                    AddressId = "ADDR004",
                    City = "Tokyo",
                    CountryCode = "JP",
                }
            };

            foreach (var address in addresses)
            {
                await _dynamoDbService.AddOrUpdateItemAsync(
                    TableDb.CUSTOMER_ADDRESSES,
                    DynamoDbMapper.ToAttributeMap(address)
                );
            }
        }

        private async Task SeedOrdersAsync()
        {
            var orders = new List<Order>
            {
                new Order
                {
                    OrderId = "ORD20250101001",
                    CustomerId = "KH001",
                    CountryCode = "VN",
                    Currency = "VND",
                    WarehouseId = "WH-VN-HCM",
                    TaxAmount = 3499000,
                    ShippingFee = 50000,
                    OrderDate = DateTime.UtcNow.AddDays(-10),
                    TotalAmount = 38539000,
                    Status = "Delivered"
                },
                new Order
                {
                    OrderId = "ORD20250105001",
                    CustomerId = "KH002",
                    CountryCode = "VN",
                    Currency = "VND",
                    WarehouseId = "WH-VN-HCM",
                    TaxAmount = 648200,
                    ShippingFee = 30000,
                    OrderDate = DateTime.UtcNow.AddDays(-5),
                    TotalAmount = 7160200,
                    Status = "Shipped"
                },
                new Order
                {
                    OrderId = "ORD20250110001",
                    CustomerId = "KH003",
                    CountryCode = "US",
                    Currency = "USD",
                    WarehouseId = "WH-US-CA",
                    TaxAmount = 159.92M,
                    ShippingFee = 15.00M,
                    OrderDate = DateTime.UtcNow.AddDays(-2),
                    TotalAmount = 2173.92M,
                    Status = "Processing"
                },
                new Order
                {
                    OrderId = "ORD20250112001",
                    CustomerId = "KH001",
                    CountryCode = "VN",
                    Currency = "VND",
                    WarehouseId = "WH-VN-HCM",
                    TaxAmount = 1149000,
                    ShippingFee = 40000,
                    OrderDate = DateTime.UtcNow.AddDays(-1),
                    TotalAmount = 12679000,
                    Status = "Pending"
                }
            };

            foreach (var order in orders)
            {
                await _dynamoDbService.AddOrUpdateItemAsync(
                    TableDb.ORDERS,
                    DynamoDbMapper.ToAttributeMap(order)
                );
            }
        }

        private async Task SeedOrderItemsAsync()
        {
            var orderItems = new List<OrderItem>
            {
                // ORD20250101001
                new OrderItem { OrderId = "ORD20250101001", ProductId = "PRD001", Quantity = 1, UnitPrice = 34990000, Currency = "VND" },
                
                // ORD20250105001
                new OrderItem { OrderId = "ORD20250105001", ProductId = "PRD005", Quantity = 1, UnitPrice = 6490000, Currency = "VND" },
                
                // ORD20250110001
                new OrderItem { OrderId = "ORD20250110001", ProductId = "PRD003", Quantity = 1, UnitPrice = 1999, Currency = "USD" },
                
                // ORD20250112001
                new OrderItem { OrderId = "ORD20250112001", ProductId = "PRD009", Quantity = 1, UnitPrice = 11490000, Currency = "VND" }
            };

            foreach (var orderItem in orderItems)
            {
                await _dynamoDbService.AddOrUpdateItemAsync(
                    TableDb.ORDER_ITEM,
                    DynamoDbMapper.ToAttributeMap(orderItem)
                );
            }
        }

        private async Task SeedShippingAsync()
        {
            var shippings = new List<Shipping>
            {
                new Shipping
                {
                    ShippingId = "SHIP001",
                    OrderId = "ORD20250101001",
                    Status = "Delivered",
                    Carrier = "Giao Hàng Nhanh",
                    EstimateDelivery = DateTime.UtcNow.AddDays(-8),
                    DeliveryDate = DateTime.UtcNow.AddDays(-7)
                },
                new Shipping
                {
                    ShippingId = "SHIP002",
                    OrderId = "ORD20250105001",
                    Status = "In Transit",
                    Carrier = "Giao Hàng Tiết Kiệm",
                    EstimateDelivery = DateTime.UtcNow.AddDays(2),
                    DeliveryDate = DateTime.UtcNow.AddDays(-4)
                },
                new Shipping
                {
                    ShippingId = "SHIP003",
                    OrderId = "ORD20250110001",
                    Status = "Preparing",
                    Carrier = "FedEx",
                    EstimateDelivery = DateTime.UtcNow.AddDays(5),
                    DeliveryDate = DateTime.UtcNow.AddDays(-1)
                }
            };

            foreach (var shipping in shippings)
            {
                await _dynamoDbService.AddOrUpdateItemAsync(
                    TableDb.SHIPPING,
                    DynamoDbMapper.ToAttributeMap(shipping)
                );
            }
        }

        private async Task SeedStockReceiptsAsync()
        {
            var receipts = new List<StockReceipt>
            {
                new StockReceipt
                {
                    ReceiptId = "SR20241201001",
                    WarehouseId = "WH-VN-HCM",
                    SupplierId = "SUP001",
                    SupplierName = "Apple Vietnam Ltd.",
                    ReceiptDate = DateTime.UtcNow.AddDays(-30),
                    Status = "Completed",
                    Notes = "Nhập hàng định kỳ tháng 12",
                    CreatedBy = "admin",
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-29),
                    ApprovedBy = "admin",
                    ApprovedAt = DateTime.UtcNow.AddDays(-30).AddHours(2),
                    CompletedBy = "admin",
                    CompletedAt = DateTime.UtcNow.AddDays(-29)
                },
                new StockReceipt
                {
                    ReceiptId = "SR20241215001",
                    WarehouseId = "WH-VN-HCM",
                    SupplierId = "SUP002",
                    SupplierName = "Samsung Electronics Vietnam",
                    ReceiptDate = DateTime.UtcNow.AddDays(-15),
                    Status = "Completed",
                    Notes = "Nhập hàng Samsung tháng 12",
                    CreatedBy = "admin",
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-14),
                    ApprovedBy = "admin",
                    ApprovedAt = DateTime.UtcNow.AddDays(-15).AddHours(1),
                    CompletedBy = "admin",
                    CompletedAt = DateTime.UtcNow.AddDays(-14)
                },
                new StockReceipt
                {
                    ReceiptId = "SR20250105001",
                    WarehouseId = "WH-VN-HN",
                    SupplierId = "SUP001",
                    SupplierName = "Apple Vietnam Ltd.",
                    ReceiptDate = DateTime.UtcNow.AddDays(-7),
                    Status = "Completed",
                    Notes = "Nhập hàng kho Hà Nội",
                    CreatedBy = "admin",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow.AddDays(-6),
                    ApprovedBy = "admin",
                    ApprovedAt = DateTime.UtcNow.AddDays(-7).AddHours(3),
                    CompletedBy = "admin",
                    CompletedAt = DateTime.UtcNow.AddDays(-6)
                },
                new StockReceipt
                {
                    ReceiptId = "SR20250110001",
                    WarehouseId = "WH-VN-HCM",
                    SupplierId = "SUP003",
                    SupplierName = "Sony Vietnam",
                    ReceiptDate = DateTime.UtcNow.AddDays(-3),
                    Status = "Approved",
                    Notes = "Nhập phụ kiện Sony",
                    CreatedBy = "admin",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3).AddHours(2),
                    ApprovedBy = "admin",
                    ApprovedAt = DateTime.UtcNow.AddDays(-3).AddHours(2)
                },
                new StockReceipt
                {
                    ReceiptId = "SR20250115001",
                    WarehouseId = "WH-VN-HCM",
                    SupplierId = "SUP004",
                    SupplierName = "Dell Vietnam Co., Ltd",
                    ReceiptDate = DateTime.UtcNow.AddDays(-1),
                    Status = "Pending",
                    Notes = "Nhập laptop Dell mới",
                    CreatedBy = "admin",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new StockReceipt
                {
                    ReceiptId = "SR20241220001",
                    WarehouseId = "WH-US-CA",
                    SupplierId = "SUP001",
                    SupplierName = "Apple Inc.",
                    ReceiptDate = DateTime.UtcNow.AddDays(-20),
                    Status = "Cancelled",
                    Notes = "Đã hủy bởi admin: Sai thông tin đơn hàng",
                    CreatedBy = "admin",
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow.AddDays(-19),
                    CancelledBy = "admin",
                    CancelledAt = DateTime.UtcNow.AddDays(-19)
                }
            };

            foreach (var receipt in receipts)
            {
                await _dynamoDbService.AddOrUpdateItemAsync(
                    TableDb.STOCK_RECEIPTS,
                    DynamoDbMapper.ToAttributeMap(receipt)
                );
            }
        }

        private async Task SeedStockReceiptItemsAsync()
        {
            var receiptItems = new List<StockReceiptItem>
            {
                // SR20241201001 - Apple products
                new StockReceiptItem
                {
                    ReceiptId = "SR20241201001",
                    ProductId = "PRD001",
                    ProductName = "iPhone 15 Pro Max",
                    SKU = "IP15PM-256-TIT",
                    Quantity = 50,
                    UnitPrice = 30000000,
                    TotalPrice = 1500000000,
                    BatchNumber = "BATCH-IP15-001",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new StockReceiptItem
                {
                    ReceiptId = "SR20241201001",
                    ProductId = "PRD003",
                    ProductName = "MacBook Pro 14 M3",
                    SKU = "MBP14-M3-18-512",
                    Quantity = 30,
                    UnitPrice = 48000000,
                    TotalPrice = 1440000000,
                    BatchNumber = "BATCH-MBP-001",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new StockReceiptItem
                {
                    ReceiptId = "SR20241201001",
                    ProductId = "PRD005",
                    ProductName = "AirPods Pro 2",
                    SKU = "APP2-USBC",
                    Quantity = 100,
                    UnitPrice = 5500000,
                    TotalPrice = 550000000,
                    BatchNumber = "BATCH-APP2-001",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new StockReceiptItem
                {
                    ReceiptId = "SR20241201001",
                    ProductId = "PRD007",
                    ProductName = "iPad Pro 12.9 M2",
                    SKU = "IPADP-M2-256",
                    Quantity = 40,
                    UnitPrice = 27000000,
                    TotalPrice = 1080000000,
                    BatchNumber = "BATCH-IPAD-001",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new StockReceiptItem
                {
                    ReceiptId = "SR20241201001",
                    ProductId = "PRD009",
                    ProductName = "Apple Watch Series 9",
                    SKU = "AW9-GPS-45",
                    Quantity = 75,
                    UnitPrice = 10000000,
                    TotalPrice = 750000000,
                    BatchNumber = "BATCH-AW9-001",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                
                // SR20241215001 - Samsung products
                new StockReceiptItem
                {
                    ReceiptId = "SR20241215001",
                    ProductId = "PRD002",
                    ProductName = "Samsung Galaxy S24 Ultra",
                    SKU = "S24U-512-BLK",
                    Quantity = 45,
                    UnitPrice = 28000000,
                    TotalPrice = 1260000000,
                    BatchNumber = "BATCH-S24-001",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new StockReceiptItem
                {
                    ReceiptId = "SR20241215001",
                    ProductId = "PRD008",
                    ProductName = "Samsung Galaxy Tab S9+",
                    SKU = "TABS9P-256",
                    Quantity = 35,
                    UnitPrice = 22000000,
                    TotalPrice = 770000000,
                    BatchNumber = "BATCH-TABS9-001",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                
                // SR20250105001 - Apple HN warehouse
                new StockReceiptItem
                {
                    ReceiptId = "SR20250105001",
                    ProductId = "PRD001",
                    ProductName = "iPhone 15 Pro Max",
                    SKU = "IP15PM-256-TIT",
                    Quantity = 40,
                    UnitPrice = 30000000,
                    TotalPrice = 1200000000,
                    BatchNumber = "BATCH-IP15-002",
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new StockReceiptItem
                {
                    ReceiptId = "SR20250105001",
                    ProductId = "PRD002",
                    ProductName = "Samsung Galaxy S24 Ultra",
                    SKU = "S24U-512-BLK",
                    Quantity = 38,
                    UnitPrice = 28000000,
                    TotalPrice = 1064000000,
                    BatchNumber = "BATCH-S24-002",
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new StockReceiptItem
                {
                    ReceiptId = "SR20250105001",
                    ProductId = "PRD003",
                    ProductName = "MacBook Pro 14 M3",
                    SKU = "MBP14-M3-18-512",
                    Quantity = 25,
                    UnitPrice = 48000000,
                    TotalPrice = 1200000000,
                    BatchNumber = "BATCH-MBP-002",
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new StockReceiptItem
                {
                    ReceiptId = "SR20250105001",
                    ProductId = "PRD005",
                    ProductName = "AirPods Pro 2",
                    SKU = "APP2-USBC",
                    Quantity = 90,
                    UnitPrice = 5500000,
                    TotalPrice = 495000000,
                    BatchNumber = "BATCH-APP2-002",
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new StockReceiptItem
                {
                    ReceiptId = "SR20250105001",
                    ProductId = "PRD009",
                    ProductName = "Apple Watch Series 9",
                    SKU = "AW9-GPS-45",
                    Quantity = 65,
                    UnitPrice = 10000000,
                    TotalPrice = 650000000,
                    BatchNumber = "BATCH-AW9-002",
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                
                // SR20250110001 - Sony products (Approved, chưa complete)
                new StockReceiptItem
                {
                    ReceiptId = "SR20250110001",
                    ProductId = "PRD006",
                    ProductName = "Sony WH-1000XM5",
                    SKU = "WH1000XM5-BLK",
                    Quantity = 60,
                    UnitPrice = 7500000,
                    TotalPrice = 450000000,
                    BatchNumber = "BATCH-SONY-001",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                
                // SR20250115001 - Dell products (Pending)
                new StockReceiptItem
                {
                    ReceiptId = "SR20250115001",
                    ProductId = "PRD004",
                    ProductName = "Dell XPS 15",
                    SKU = "XPS15-9530-I7",
                    Quantity = 25,
                    UnitPrice = 38000000,
                    TotalPrice = 950000000,
                    BatchNumber = "BATCH-DELL-001",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                
                // SR20241220001 - Cancelled
                new StockReceiptItem
                {
                    ReceiptId = "SR20241220001",
                    ProductId = "PRD001",
                    ProductName = "iPhone 15 Pro Max",
                    SKU = "IP15PM-256-TIT",
                    Quantity = 100,
                    UnitPrice = 30000000,
                    TotalPrice = 3000000000,
                    BatchNumber = "BATCH-IP15-CANCEL",
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                }
            };

            foreach (var item in receiptItems)
            {
                await _dynamoDbService.AddOrUpdateItemAsync(
                    TableDb.STOCK_RECEIPT_ITEMS,
                    DynamoDbMapper.ToAttributeMap(item)
                );
            }
        }

        #endregion

        private async Task CreateTableStockReceiptItemAsync()
        {
            var stockReceiptItemsTable = new CreateTableRequest
            {
                TableName = TableDb.STOCK_RECEIPT_ITEMS,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "ReceiptId", AttributeType = "S" },
                    new AttributeDefinition { AttributeName = "ProductId", AttributeType = "S" }
                },
                            KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement { AttributeName = "ReceiptId", KeyType = "HASH" },
                    new KeySchemaElement { AttributeName = "ProductId", KeyType = "RANGE" }
                }
            };

            await _dynamoDbService.CreateTableIfNotExistsAsync(stockReceiptItemsTable);
        }

        private async Task CreateTableStockReceiptsAsync()
        {
            var stockReceiptsTable = new CreateTableRequest
            {
                TableName = TableDb.STOCK_RECEIPTS,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "ReceiptId", AttributeType = "S" }
                },
                            KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement { AttributeName = "ReceiptId", KeyType = "HASH" }
                }
            };

            await _dynamoDbService.CreateTableIfNotExistsAsync(stockReceiptsTable);
        }

        // Tạo bảng Customer
        private async Task CreateTableCustomerAsync(CancellationToken cancellationToken = default)
        {
            var customerTable = new CreateTableRequest
            {
                    TableName = TableDb.CUSTOMERS,
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "CustomerId",
                            AttributeType = "S"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "CustomerId",
                            KeyType = "HASH"
                        }
                    },
            };
            await _dynamoDbService.CreateTableIfNotExistsAsync(customerTable);
        }

        // Tạo bảng CustomerAddress
        private async Task CreateTableCustomerAddressAsync(CancellationToken cancellationToken = default)
        {
            var addressTable = new CreateTableRequest
            {
                TableName = TableDb.CUSTOMER_ADDRESSES,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "AddressId", AttributeType = "S" },
                    new AttributeDefinition { AttributeName = "CustomerId", AttributeType = "S" }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "CustomerId",
                        KeyType = "HASH"
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "AddressId",
                        KeyType = "RANGE"
                    }
                },
            };
            await _dynamoDbService.CreateTableIfNotExistsAsync(addressTable);
        }

        // Tạo bảng Order
        private async Task CreateTableOrderAsync(CancellationToken cancellationToken = default)
        {
            var orderTable = new CreateTableRequest
            {
                TableName = TableDb.ORDERS,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "CustomerId", AttributeType = "S" },
                    new AttributeDefinition { AttributeName = "OrderId", AttributeType = "S" }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "CustomerId",
                        KeyType = "HASH"
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "OrderId",
                        KeyType = "RANGE"
                    }
                }
            };
            await _dynamoDbService.CreateTableIfNotExistsAsync(orderTable);
        }

        // Tạo bảng Inventory
        private async Task CreateTableInventoryAsync(CancellationToken cancellationToken = default)
        {
            var inventoryTable = new CreateTableRequest
            {
                TableName = TableDb.INVENTORY,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "WarehouseId", AttributeType = "S" },
                    new AttributeDefinition { AttributeName = "ProductId", AttributeType = "S" }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "WarehouseId",
                        KeyType = "HASH"
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "ProductId",
                        KeyType = "RANGE"
                    }
                },
            };
            await _dynamoDbService.CreateTableIfNotExistsAsync(inventoryTable);
        }

        // Tạo bảng OrderItem
        private async Task CreateTableOrderItemAsync(CancellationToken cancellationToken = default)
        {
            var orderItemTable = new CreateTableRequest
            {
                TableName = TableDb.ORDER_ITEM,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "OrderId", AttributeType = "S" },
                    new AttributeDefinition { AttributeName = "ProductId", AttributeType = "S" }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "OrderId",
                        KeyType = "HASH"
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "ProductId",
                        KeyType = "RANGE"
                    }
                },
            };
            await _dynamoDbService.CreateTableIfNotExistsAsync(orderItemTable);
        }

        // Tạo bảng ProductPricing
        private async Task CreateTableProductPricingAsync(CancellationToken cancellationToken = default)
        {
            var pricingTable = new CreateTableRequest
            {
                TableName = TableDb.PRODUCT_PRICING,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "ProductId", AttributeType = "S" },
                    new AttributeDefinition { AttributeName = "Region", AttributeType = "S" }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "ProductId",
                        KeyType = "HASH"
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "Region",
                        KeyType = "RANGE"
                    }
                },
            };
            await _dynamoDbService.CreateTableIfNotExistsAsync(pricingTable);
        }

        // Tạo bảng Product
        private async Task CreateTableProductAsync(CancellationToken cancellationToken = default)
        {
            var productTable = new CreateTableRequest
            {
                TableName = TableDb.PRODUCTS,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "ProductId", AttributeType = "S" }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "ProductId",
                        KeyType = "HASH"
                    }
                }
            };
            await _dynamoDbService.CreateTableIfNotExistsAsync(productTable);
        }

        // Tạo bảng Warehouse
        private async Task CreateTableWarehouseAsync(CancellationToken cancellationToken = default)
        {
            var warehouseTable = new CreateTableRequest
            {
                TableName = TableDb.WAREHOUSES,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "WarehouseId", AttributeType = "S" }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "WarehouseId",
                        KeyType = "HASH"
                    }
                }
            };
            await _dynamoDbService.CreateTableIfNotExistsAsync(warehouseTable);
        }

        // Tạo bảng Shipping
        private async Task CreateTableShippingAsync(CancellationToken cancellationToken = default)
        {
            var shippingTable = new CreateTableRequest
            {
                TableName = TableDb.SHIPPING,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "ShippingId", AttributeType = "S" },
                    new AttributeDefinition { AttributeName = "OrderId", AttributeType = "S" } // dùng cho GSI
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "ShippingId",
                        KeyType = "HASH"
                    }
                },
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                {
                    new GlobalSecondaryIndex
                    {
                        IndexName = "OrderIdIndex",
                        KeySchema = new List<KeySchemaElement>
                        {
                            new KeySchemaElement("OrderId", KeyType.HASH)
                        },
                        Projection = new Projection { ProjectionType = "ALL" },
                        ProvisionedThroughput = new ProvisionedThroughput(5, 5)
                    }
                },
            };
            await _dynamoDbService.CreateTableIfNotExistsAsync(shippingTable);
        }
    }
}
