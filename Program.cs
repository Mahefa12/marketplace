using Microsoft.EntityFrameworkCore;
using AutoMapper;
using FluentValidation.AspNetCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddAuthentication("AdminAuth").AddCookie("AdminAuth", options =>
{
    options.LoginPath = "/Admin/Login";
    options.AccessDeniedPath = "/Admin/Login";
});

builder.Services.AddDbContext<Marketplace.Data.MarketplaceDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=marketplace.db";
    options.UseSqlite(conn);
});
builder.Services.AddMemoryCache();
builder.Services.AddScoped<Marketplace.Services.IBookService, Marketplace.Services.BookService>();
builder.Services.AddScoped<Marketplace.Services.IPurchaseService, Marketplace.Services.PurchaseService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Marketplace.Data.MarketplaceDbContext>();
    db.Database.Migrate();
    await Marketplace.Data.DbSeeder.SeedAsync(db);
}

app.Run();
