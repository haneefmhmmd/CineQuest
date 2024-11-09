using _301301555_301287005_Laylay_Muhammad__Lab3.Models;
using Microsoft.EntityFrameworkCore;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.DynamoDBv2.DataModel;
using _301301555_301287005_Laylay_Muhammad__Lab3.Middlewares;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add connection to parameter store
builder.Configuration.AddSystemsManager("/CineQuest", 
    new Amazon.Extensions.NETCore.Setup.AWSOptions { Region = Amazon.RegionEndpoint.USEast1 });

// Add RDS connection
var dbConnectionString = new SqlConnectionStringBuilder(builder.Configuration.GetConnectionString("Connection2RDS"));
dbConnectionString.UserID = builder.Configuration["DbUser"];
dbConnectionString.Password = builder.Configuration["DbPassword"];

builder.Services.AddDbContext<MovieappContext>(options => options.UseSqlServer(dbConnectionString.ConnectionString));

// Add AWS services
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions("AWS"));
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();

// Add distributed memory cache for session storage
builder.Services.AddDistributedMemoryCache(); // Required for session management
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Make cookie HTTP only
    options.Cookie.IsEssential = true; // Make the session cookie essential
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseMiddleware<SessionCheckMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
