using azure_form_file_upload.Settings.Configuration;
using Serilog;
using webapi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
// configs
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("Settings/appsecrets.json")
    .Build();
builder.Services.Configure<AzureBlobStorageConfig>(configuration.GetSection("Azure").GetSection("BlobStorage"));
//services
builder.Services.AddSingleton<FileUploadService>();
//log,serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
);
//cors
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "_allowSpecificOrigins",
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("_allowSpecificOrigins");

app.UseSerilogRequestLogging();

app.MapControllerRoute(
    name: "default",
    pattern: "api/{controller}/{action=Index}/{id?}");

app.Run();
