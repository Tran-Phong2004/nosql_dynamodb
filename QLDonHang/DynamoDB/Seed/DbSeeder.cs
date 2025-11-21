using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.CodeAnalysis.Elfie.Model;
using QLDonHang.Const;

namespace QLDonHang.DynamoDB.Seed
{
    public class DbSeeder
    {
        private readonly DynamoDbService _dynamoDbService;
        public DbSeeder(DynamoDbService dynamoDbService) 
        {
            _dynamoDbService = dynamoDbService;
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
            // code ví dụ
            //await _dynamoDbService.AddOrUpdateItemAsync(TableDb.TEST_TABLE, ItemBuilder.Create()
            //                                                                              .Add("Id", "1")
            //                                                                              .Add("Name", "phong")
            //                                                                              .Add("Age", 25)
            //                                                                              .Build());
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
