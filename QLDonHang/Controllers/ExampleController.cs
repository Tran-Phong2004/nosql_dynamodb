using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using QLDonHang.Const;
using QLDonHang.DynamoDB;
using System.Threading.Tasks;

namespace QLDonHang.Controllers
{
    // Controller này để làm ví dụ minh họa cách sử dụng DynamoDbService và ItemBuilder để làm việc với DynamoDB
    public class ExampleController : Controller
    {
        private readonly DynamoDbService _dynamoDbService;
        public ExampleController(DynamoDbService dynamoDbService)
        {
            _dynamoDbService = dynamoDbService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Cách tạo item gốc ban đầu
            //var item = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>
            //{
            //    { "Id", new Amazon.DynamoDBv2.Model.AttributeValue { S = "123" } },
            //    { "Age", new Amazon.DynamoDBv2.Model.AttributeValue { N = "30" } }
            //};

            // Cách tạo item bằng cách dùng builder
            var itemAdd = ItemBuilder.Create()
                                  .Add("Id", "123")
                                  .Add("Name", "h")
                                  .Add("Age", 30)
                                  .Build();
            var itemQuery = ItemBuilder.Create()
                                  .Add("Id", "123")
                                  .Add("Name", "h")
                                  .Build();
            await _dynamoDbService.AddOrUpdateItemAsync(TableDb.TEST_TABLE, itemAdd); // thêm hoặc cập nhật item
            Dictionary<string, AttributeValue> result = await _dynamoDbService.GetItemAsync(TableDb.TEST_TABLE, itemQuery); // lấy 1 item
            SampleModel? model = DynamoDbMapper.ToObject<SampleModel>(result); // chuyển đổi sang object c#
            ViewBag.Model = model;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TestQuery()
        {
            // Lấy danh sách item với điều kiện
            var query = new QueryRequest()
            {
                TableName = TableDb.TEST_TABLE,
                KeyConditionExpression = "Id = :v_id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    [":v_id"] = new AttributeValue { S = "123" } 
                }
            };
            List<Dictionary<string, AttributeValue>> result = await _dynamoDbService.QueryTableAsync(query);
            ViewBag.ListData = result.Select(value => DynamoDbMapper.ToObject<SampleModel>(value)).ToList();
            return View();
        }

        public async Task<IActionResult> TestDelete()
        {
            var item = ItemBuilder.Create()
                                  .Add("Id", "123")
                                  .Add("Name", "h")
                                  .Build();
            await _dynamoDbService.DeleteItemAsync("TestTable", item);
            return RedirectToAction("TestQuery");
        }

        public async Task<IActionResult> TestUpdate()
        {
            // where theo item này
            var where = ItemBuilder.Create()
                                  .Add("Id", "123")
                                  .Add("Name", "phong")
                                  .Build();

            // giá trị sẽ update cho item
            var value = ItemBuilder.Create()
                                   .Add(":a", 1)
                                   .Build();
            var set = "SET Age = :a";
            await _dynamoDbService.UpdateItemAsync(TableDb.TEST_TABLE, where, set, value);
            return RedirectToAction("TestQuery");
        }
    }

    public class SampleModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
