using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace QLDonHang.DynamoDB
{
    public class DynamoDbService
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly ILogger<DynamoDbService> _logger;
        public IAmazonDynamoDB DynamoDbClient => _dynamoDb; // DynamoDB client - có thể sử dụng trực tiếp nếu cần
        public DynamoDbService(IAmazonDynamoDB dynamoDB, ILogger<DynamoDbService> logger)
        {
            _dynamoDb = dynamoDB;
            _logger = logger;
        }

        /// <summary>
        /// Tạo bảng mới trong DynamoDB
        /// </summary>
        /// <param name="createTableRequest">Cấu hình các tham số cho bảng</param>
        /// <returns></returns>
        public async Task<CreateTableResponse> CreateTableAsync(CreateTableRequest createTableRequest)
        {
            //var request = new Amazon.DynamoDBv2.Model.CreateTableRequest 
            //{
            //    //TableName = tableName,
            //    //AttributeDefinitions = attributes,
            //    //KeySchema = keySchemas,
            //    //LocalSecondaryIndexes = localSecondaryIndices ?? new List<LocalSecondaryIndex>(),
            //    //GlobalSecondaryIndexes = globalSecondaryIndices ?? new List<GlobalSecondaryIndex>(),
            //    //// Tính theo read/write capacity units
            //    ////ProvisionedThroughput = new Amazon.DynamoDBv2.Model.ProvisionedThroughput
            //    ////{
            //    ////    ReadCapacityUnits = 5,
            //    ////    WriteCapacityUnits = 5
            //    ////},
            //    // Tính tiền theo request
            //    BillingMode = BillingMode.PAY_PER_REQUEST,
            //};
            createTableRequest.BillingMode = BillingMode.PAY_PER_REQUEST;
            var tableResponse = await _dynamoDb.CreateTableAsync(createTableRequest);
            return tableResponse;
        }

        /// <summary>
        /// Tạo table nếu chưa tồn tại, hàm này có handle lỗi còn hàm CreateTableAsync thì ném lỗi trực tiếp
        /// </summary>
        /// <param name="createTableRequest"></param>
        /// <returns></returns>
        public async Task<CreateTableResponse?> CreateTableIfNotExistsAsync(CreateTableRequest createTableRequest)
        {
            createTableRequest.BillingMode = BillingMode.PAY_PER_REQUEST;
            if (await TableExistsAsync(createTableRequest.TableName) || string.IsNullOrEmpty(createTableRequest.TableName))
                return null; 
            var tableResponse = await _dynamoDb.CreateTableAsync(createTableRequest);
            return tableResponse;
        }

        /// <summary>
        /// Thêm một dòng mới vào bảng.
        /// Nếu đã tồn tại thì cập nhật lại theo partition key và sort key
        /// </summary>
        /// <param name="tableName">Tên bảng</param>
        /// <param name="item">item sẽ thêm</param>
        /// <returns></returns>
        public async Task<PutItemResponse> AddOrUpdateItemAsync(string tableName, Dictionary<string, AttributeValue> item)
        {
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = item
            };
            return await _dynamoDb.PutItemAsync(request);
        }

        /// <summary>
        /// Truy vấn lấy một dòng trong bảng
        /// </summary>
        /// <param name="tableName">Tên bảng</param>
        /// <param name="key">partition key or sort key</param>
        /// <returns></returns>
        public async Task<Dictionary<string, AttributeValue>> GetItemAsync(string tableName, Dictionary<string, AttributeValue> key)
        {
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = key
            };
            var response = await _dynamoDb.GetItemAsync(request);
            return response.Item;
        }

        /// <summary>
        /// Thực hiện câu query trên bảng
        /// Đa dạng hơn get item
        /// </summary>
        /// <param name="query">cấu hình câu query</param>
        /// <returns></returns>
        public async Task<List<Dictionary<string, AttributeValue>>> QueryTableAsync(QueryRequest query)
        {
            if (query.ExpressionAttributeValues == null)
                query.ExpressionAttributeValues = new Dictionary<string, AttributeValue>();
            var response = await _dynamoDb.QueryAsync(query);
            return response.Items;
        }

        /// <summary>
        /// Quét toàn bộ bảng không cần partition key (hiệu năng thấp)
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public async Task<List<Dictionary<string, AttributeValue>>> ScanTableAsync(string tableName)
        {
            var response = await _dynamoDb.ScanAsync(new ScanRequest
            {
                TableName = tableName
            });
            return response.Items;
        }

        /// <summary>
        /// Xóa một dòng trong bảng theo key
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<DeleteItemResponse> DeleteItemAsync(string tableName, Dictionary<string, AttributeValue> key)
        {
            var request = new DeleteItemRequest
            {
                TableName = tableName,
                Key = key
            };
            var response = await _dynamoDb.DeleteItemAsync(request);
            return response;
        }

        /// <summary>
        /// Cập nhật một dòng trong bảng theo key
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="key"></param>
        /// <param name="updateExpression"></param>
        /// <param name="expressionValues"></param>
        /// <returns></returns>
        public async Task<UpdateItemResponse> UpdateItemAsync(string tableName, Dictionary<string, AttributeValue> key, string updateExpression, Dictionary<string, AttributeValue> expressionValues)
        {
            var response = await _dynamoDb.UpdateItemAsync(new UpdateItemRequest
            {
                TableName = tableName,
                Key = key,
                UpdateExpression = updateExpression, // ví dụ: "SET Age = :a"
                ExpressionAttributeValues = expressionValues, // { ":a", new AttributeValue { N = "30" } }
                ReturnValues = "ALL_NEW" // trả về item mới sau update
            });
            return response;
        }

        public async Task<bool> TableExistsAsync(string tableName)
        {
            try
            {
                var response = await _dynamoDb.DescribeTableAsync(tableName);
                return response.Table.TableStatus == "ACTIVE" || response.Table.TableStatus == "CREATING";
            }
            catch (ResourceNotFoundException rnf)
            {
                return false; // bảng không tồn tại
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "");
                return false;
            }
        }

    }
}
