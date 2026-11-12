using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using QuestPDF.Infrastructure;
using RPPP_WebApp.Model;
using System.Reflection;
using System.Text.Json.Serialization;
using QuestPDF.Infrastructure;
namespace RPPP_WebApp;

public static class StartupExtensions
{
  public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        // Add MVC services
        builder.Services.AddControllersWithViews();

        builder.Services.AddControllers()
        .AddJsonOptions(options => 
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.WriteIndented = true;
        });

        // Add DbContext with connection string
        builder.Services.AddDbContext<Rppp08Context>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("RPPP08"));
        });

        // Swagger
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RPPP08 API",
                Version = "v1",
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        });
        
        // Add user secrets in development
        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>();
        }
        QuestPDF.Settings.License = LicenseType.Community;
        return builder.Build();
    }


  public static WebApplication ConfigurePipeline(this WebApplication app)
  {
    #region Needed for nginx and Kestrel (do not remove or change this region)
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
      ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                         ForwardedHeaders.XForwardedProto
    });
    string pathBase = app.Configuration["PathBase"];
    if (!string.IsNullOrWhiteSpace(pathBase))
    {
      app.UsePathBase(pathBase);
    }
    #endregion

    if (app.Environment.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RPPP08 API");
        c.RoutePrefix = "swagger";
    });

        app.UseStaticFiles()
       .UseRouting();

    app.MapControllers();
    app.MapDefaultControllerRoute();

    return app;
  }
}