using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using QLDonHang.Const;
using QLDonHang.DynamoDB;
using QLDonHang.Entities;

namespace QLDonHang.Controllers
{
    public class CustomerController : Controller
    {
        private readonly DynamoDbService _dynamoDbService;
        public CustomerController(DynamoDbService dynamoDbService) 
        {
            _dynamoDbService = dynamoDbService;
        }

        // GET: Customer
        [Authorize]
        public async Task<IActionResult> Index(string? countryCode = null, string? email = null)
        {
            var filterExpressions = new List<string>();
            var attributeValues = new Dictionary<string, AttributeValue>();

            if (!string.IsNullOrEmpty(countryCode))
            {
                filterExpressions.Add("CountryCode = :country");
                attributeValues[":country"] = new AttributeValue { S = countryCode };
            }
            if (!string.IsNullOrEmpty(email))
            {
                filterExpressions.Add("contains(Email, :email)");
                attributeValues[":email"] = new AttributeValue { S = email };
            }
            // Không phải admin
            filterExpressions.Add("Username <> :admin");
            attributeValues[":admin"] = new AttributeValue { S = "admin" };

            var scanRequest = new ScanRequest
            {
                TableName = TableDb.CUSTOMERS
            };
            if (filterExpressions.Any())
            {
                scanRequest.FilterExpression = string.Join(" AND ", filterExpressions);
                scanRequest.ExpressionAttributeValues = attributeValues;
            }

            var result = await _dynamoDbService.ScanTableAsync(scanRequest);
            var customers = result
                .Select(DynamoDbMapper.ToObject<Customer>)
                .OrderBy(c => c.CountryCode)
                .ThenByDescending(c => c.CreatedAt)
                .ToList();

            ViewBag.CountryList = new List<SelectListItem>
            {
                new SelectListItem { Value = "",  Text = "-- Tất cả --", Selected = string.IsNullOrEmpty(countryCode) },
                new SelectListItem { Value = "VN", Text = "🇻🇳 Việt Nam", Selected = countryCode == "VN" },
                new SelectListItem { Value = "US", Text = "🇺🇸 Hoa Kỳ", Selected = countryCode == "US" },
                new SelectListItem { Value = "JP", Text = "🇯🇵 Nhật Bản", Selected = countryCode == "JP" },
                new SelectListItem { Value = "KR", Text = "🇰🇷 Hàn Quốc", Selected = countryCode == "KR" },
                new SelectListItem { Value = "SG", Text = "🇸🇬 Singapore", Selected = countryCode == "SG" }
            };
            ViewBag.EmailFilter = email ?? string.Empty;

            return View(customers);
        }

        // GET: Block
        public async Task<IActionResult> Block([FromRoute(Name = "id")] string customerId = null)
        {
            if(string.IsNullOrEmpty(customerId))
            {
                ViewData["ErrorMessage"] = "Customer ID is required to block a customer";
                return RedirectToAction(nameof(Index));
            }
            var where = ItemBuilder.Create()
                              .Add("CustomerId", customerId)
                              .Build();
            var set = "SET IsActive = :isActive";
            var value = ItemBuilder.Create()
                                   .Add(":isActive", false)
                                   .Build();
            var response = await _dynamoDbService.UpdateItemAsync(TableDb.CUSTOMERS, where, set, value);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK &&
                response.Attributes != null &&
                response.Attributes.ContainsKey("IsActive") &&
                response.Attributes["IsActive"].BOOL == false)
            {
                TempData["SuccessMessage"] = "Customer blocked successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to block customer.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Unblock
        public async Task<IActionResult> Unblock([FromRoute(Name = "id")] string customerId = null)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                ViewData["ErrorMessage"] = "Customer ID is required to block a customer";
                return RedirectToAction(nameof(Index));
            }
            var where = ItemBuilder.Create()
                              .Add("CustomerId", customerId)
                              .Build();
            var set = "SET IsActive = :isActive";
            var value = ItemBuilder.Create()
                                   .Add(":isActive", true)
                                   .Build();
            var response = await _dynamoDbService.UpdateItemAsync(TableDb.CUSTOMERS, where, set, value);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                TempData["SuccessMessage"] = "Customer unblock successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to block customer.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
