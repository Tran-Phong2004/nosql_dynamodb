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
            var table = new CreateTableRequest
            {
                TableName = TableDb.TEST_TABLE,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "Id",
                        AttributeType = "S" // S - String, N - Number, B - Binary
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "Name",
                        AttributeType = "S" // S - String, N - Number, B - Binary
                    },
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "Id",
                        KeyType = "HASH" // Partition key
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "Name",
                        KeyType = "RANGE" // Sort key
                    }
                }
            };
            await _dynamoDbService.CreateTableIfNotExistsAsync(table);
        }

        private async Task AddDataAsync(CancellationToken cancellationToken)
        {
            await _dynamoDbService.AddOrUpdateItemAsync(TableDb.TEST_TABLE, ItemBuilder.Create()
                                                                                          .Add("Id", "1")
                                                                                          .Add("Name", "phong")
                                                                                          .Add("Age", 25)
                                                                                          .Build());
        }
    }
}
