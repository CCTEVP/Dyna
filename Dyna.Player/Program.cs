using Dyna.Player.Pages.Shared.Components;
using Dyna.Player.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddRazorPages().AddViewComponentsAsServices();
// Add Razor Runtime Compilation (Correct .NET 6+ and .NET 9 version)
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation(); // Crucial!
builder.Services.AddHttpClient();
// Add the custom services configuration
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<QueryParameterService>();
builder.Services.AddScoped<DebugService>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<FileService>();
// Configure View Location Formats for Widgets
builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(o => {
    o.ViewLocationFormats.Insert(0,"/Pages/Shared/Components/PageViews/{0}/{1}.cshtml"); // Page Views
    o.ViewLocationFormats.Insert(1, "/Pages/Shared/Components/Widgets/{0}/{1}.cshtml"); // Widgets
    // You can add other view location formats here if needed
});
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IBundleService, BundleService>();
builder.Services.AddMemoryCache(); // Add memory cache for bundle caching
// Configure FileServiceOptions
builder.Services.Configure<FileServiceOptions>(builder.Configuration.GetSection("FileServiceOptions"));

// Register FileService and IFileService
builder.Services.AddScoped<IFileService, FileService>();

// Configure the HTTP request pipeline.
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // This handles static files in wwwroot

app.UseRouting();

app.UseAuthorization();

// Add middleware to clear the asset list between requests
app.Use(async (context, next) =>
{
    // Clear the asset list at the beginning of each request
    Dyna.Player.TagHelpers.AssetTagHelper.ClearPresentAssets();
    
    await next();
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllers();
});

// Route for asset files (individual component assets)
app.MapGet("/{assetType}/{assetLocation}/{assetName}.{extension}", async (HttpContext context, string assetType, string assetLocation, string assetName, string extension, [FromServices] IAssetService assetsService) =>
{
    // Validate that assetType matches extension
    if (assetType != extension)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync($"Asset type '{assetType}' does not match extension '{extension}'");
        return;
    }

    // Check for debug mode via query parameter
    bool debugMode = context.Request.Query.ContainsKey("debug") && 
                    context.Request.Query["debug"] != "false";
    
    var asset = await assetsService.GetAssetAsync(assetName, assetLocation, extension, debugMode);

    if (asset.StartsWith("Invalid") || asset.StartsWith("Non existing"))
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync(asset);
        return;
    }

    context.Response.ContentType = extension switch
    {
        "js" => "text/javascript",
        "css" => "text/css",
        _ => "text/plain" // Default content type
    };
    
    // Add debug information in headers if in debug mode
    if (debugMode)
    {
        context.Response.Headers.Add("X-Debug-Info", $"Asset: {assetLocation}/{assetName}.{extension}");
    }
    
    await context.Response.WriteAsync(asset);
});

// Note: The bundle files are now served by the static file middleware
// since they're stored in wwwroot/css and wwwroot/js

app.Run();