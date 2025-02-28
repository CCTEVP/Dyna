using Dyna.Player.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddRazorPages().AddViewComponentsAsServices();
// Add Razor Runtime Compilation (Correct .NET 6+ and .NET 9 version)
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation(); // Crucial!
builder.Services.AddHttpClient();
// Add the custom services configuration
builder.Services.Configure<WidgetViewPaths>(builder.Configuration.GetSection("WidgetViewPaths"));

// Configure View Location Formats for Widgets
builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(o => {
    o.ViewLocationFormats.Insert(0,"/Pages/Shared/Components/PageViews/{0}/{1}.cshtml"); // Page Views
    o.ViewLocationFormats.Insert(1, "/Pages/Shared/Components/Widgets/{0}/{1}.cshtml"); // Widgets
    // You can add other view location formats here if needed
});
builder.Services.AddScoped<IAssetScriptService, AssetScriptService>();
// Configure the HTTP request pipeline.
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllers();
});
app.MapGet("/js/{assetType}/{assetName}.js", async (HttpContext context, string assetType, string assetName, [FromServices] IAssetScriptService scriptService) =>
{
    bool debugMode = context.Request.Query.ContainsKey("mode") && context.Request.Query["mode"] == "debug";

    var script = await scriptService.GetAssetScriptAsync(assetName, assetType, assetType, debugMode);

    if (script.StartsWith("Invalid") || script.StartsWith("Non existing"))
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync(script);
        return;
    }

    context.Response.ContentType = "application/javascript";
    await context.Response.WriteAsync(script);
});
app.Run();