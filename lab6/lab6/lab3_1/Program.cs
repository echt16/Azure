using Microsoft.EntityFrameworkCore;
using lab3_1.Models.Database;
using lab3_1.Models.Services.DatabaseServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//string connectionString = builder.Configuration.GetConnectionString("StorageSystemDB");


//var cosmosUri = builder.Configuration["CosmosDb:AzureUri"];
//var cosmosKey = builder.Configuration["CosmosDb:AzureKey"];
//var cosmosDatabaseName = builder.Configuration["CosmosDb:DatabaseName"];

//builder.Services.AddDbContext<StorageSystemDbContext>(options =>
//    options.UseCosmos(
//        cosmosUri,
//        cosmosKey,
//        cosmosDatabaseName
//    ));


var conn = builder.Configuration["CosmosDb:AzureConnection"];
var nameDb = builder.Configuration["CosmosDb:DatabaseName"];


builder.Services.AddDbContext<StorageSystemDbContext>(options =>
options.UseCosmos(conn, nameDb));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddScoped<DatabaseService>();
var app = builder.Build();
app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();