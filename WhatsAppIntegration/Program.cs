using System.Reflection;
using Microsoft.OpenApi.Models;
using WhatsAppIntegration.Configuration;
using WhatsAppIntegration.Models;
using WhatsAppIntegration.Repositories;
using WhatsAppIntegration.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure WhatsApp settings
builder.Services.Configure<WhatsAppConfig>(
    builder.Configuration.GetSection("WhatsApp"));

// Configure Shopify settings
builder.Services.Configure<ShopifyConfig>(
    builder.Configuration.GetSection("Shopify"));

// Configure MongoDB settings from environment variables
builder.Services.Configure<MongoDbConfig>(options =>
{
    options.ConnectionString = builder.Configuration["MongoDB__ConnectionString"] ?? "mongodb://localhost:27017";
    options.DatabaseName = builder.Configuration["MongoDB__DatabaseName"] ?? "WhatsAppIntegration";
    options.CategorizedOrdersCollection = builder.Configuration["MongoDB__CategorizedOrdersCollection"] ?? "CategorizedOrders";
});

// Add HttpClient for WhatsApp service
builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>();

// Add HttpClient for Shopify service
builder.Services.AddHttpClient<IShopifyService, ShopifyService>();

// Register WhatsApp service
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();

// Register Shopify service
builder.Services.AddScoped<IShopifyService, ShopifyService>();

// Register MongoDB repository
builder.Services.AddScoped<ICategorizedOrdersRepository, CategorizedOrdersRepository>();

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "WhatsApp Integration API",
        Description = "A comprehensive ASP.NET Core Web API for integrating with WhatsApp Business API, providing webhook support for receiving messages and APIs for sending various types of messages.",
        Contact = new OpenApiContact
        {
            Name = "WhatsApp Integration API",
            Email = "support@example.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add security definition for Bearer token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "WhatsApp Business API Access Token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Group endpoints by tags
    c.TagActionsBy(api => new[] { api.GroupName ?? "Default" });
    c.DocInclusionPredicate((name, api) => true);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "api/docs/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api/docs/v1/swagger.json", "WhatsApp Integration API v1");
        c.RoutePrefix = "api/docs";
        c.DocumentTitle = "WhatsApp Integration API Documentation";
        c.DefaultModelsExpandDepth(2);
        c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make the implicit Program class public so it can be referenced by tests
public partial class Program { }
