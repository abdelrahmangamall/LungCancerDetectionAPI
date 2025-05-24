using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Models;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.AddScoped<IAIDetectionService, AIDetectionService>();

builder.Services.AddControllers()
    .AddNewtonsoftJson();

// Configure Swagger BEFORE building the app
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cancer Detection API",
        Version = "v1",
        Description = "API for detecting lung cancer from CT scan images."
    });


    //c.EnableAnnotations(); // <- Helpful
    c.OperationFilter<SwaggerFileOperationFilter>();
});


var app = builder.Build();  // This makes the service collection read-only

// Now we can only configure the pipeline, not services
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Create temp directory if it doesn't exist
var tempPath = Path.Combine(Directory.GetCurrentDirectory(),
    app.Configuration["AIModelSettings:TempImageStoragePath"]);
if (!Directory.Exists(tempPath))
{
    Directory.CreateDirectory(tempPath);
}

app.Run();
