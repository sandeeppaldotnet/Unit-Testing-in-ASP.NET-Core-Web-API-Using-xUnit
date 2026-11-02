using ProductHub.API.Models;
using ProductHub.API.Repositories;
using ProductHub.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Register dependencies
builder.Services.AddSingleton<IProductRepository,
    InMemoryProductRepository>();

builder.Services.AddScoped<IProductService,
    ProductService>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Health endpoint
app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        Status = "Healthy",
        Message = "ProductHub API is running."
    });
});

// Get product
app.MapGet("/api/products/{id:int}",
    async (int id, IProductService service) =>
    {
        try
        {
            var product = await service.GetByIdAsync(id);

            return product is null
                ? Results.NotFound()
                : Results.Ok(product);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new
            {
                Message = ex.Message
            });
        }
    });

// Create product
app.MapPost("/api/products",
    async (Product product, IProductService service) =>
    {
        try
        {
            var createdProduct = await service.CreateAsync(product);

            return Results.Created(
                $"/api/products/{createdProduct.Id}",
                createdProduct);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new
            {
                Message = ex.Message
            });
        }
    });

// Calculate discount
app.MapGet("/api/products/discount",
    (decimal price,
     decimal discount,
     IProductService service) =>
    {
        try
        {
            var finalPrice = service.CalculateDiscount(
                price,
                discount);

            return Results.Ok(new
            {
                OriginalPrice = price,
                DiscountPercentage = discount,
                FinalPrice = finalPrice
            });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new
            {
                Message = ex.Message
            });
        }
    });


app.Run();

