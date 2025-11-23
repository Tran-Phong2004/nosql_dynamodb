using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Authentication.Cookies;
using QLDonHang.DynamoDB;
using QLDonHang.DynamoDB.Seed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
{
    // Cấu hình DynamoDB
    var config = new AmazonDynamoDBConfig
    {
        RegionEndpoint = Amazon.RegionEndpoint.APSoutheast1, // chọn region
        ServiceURL = "http://localhost:8000" // URL của DynamoDB
    };
    return new AmazonDynamoDBClient("accessKey", "secretKey", config); 
});
builder.Services.AddSingleton<DynamoDbService>();
builder.Services.AddSingleton<DbSeeder>();
builder.Services.AddHostedService<DbSeedHosted>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";       // Trang login
        options.AccessDeniedPath = "/Auth/AccessDenied"; // Trang không đủ quyền
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
    });
var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
