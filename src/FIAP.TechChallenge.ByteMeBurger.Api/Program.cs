using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Bmb.Tools.Auth;
using Bmb.Tools.OpenApi;
using FIAP.TechChallenge.ByteMeBurger.DI;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace FIAP.TechChallenge.ByteMeBurger.Api;

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ILogger<Program>? logger = null;
        try
        {
            builder.Host.UseSerilog((context, configuration) =>
                configuration
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName));

            // Add services to the container.
            builder.Services.ConfigureJwt(builder.Configuration);
            builder.Services.AddAuthorization();
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            // Add CORS services to the DI container.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            // https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-8.0#log-automatic-400-responses
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddRouting(options => options.LowercaseUrls = true);
            builder.Services.ConfigBmbSwaggerGen();
            var jwtOptions = builder.Configuration
                .GetSection("JwtOptions")
                .Get<JwtOptions>();
            builder.Services.AddSingleton(jwtOptions);
            builder.Services.AddHttpLogging(o => { });
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            builder.Services.IoCSetup(builder.Configuration);
            builder.Services.AddExceptionHandler<DomainExceptionHandler>();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            AddHealthChecks(builder, builder.Configuration);

            var app = builder.Build();
            logger = app.Services.GetService<ILogger<Program>>();
            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseBmbSwaggerUi();
            }

            app.UseSerilogRequestLogging();
            app.UseHttpLogging();
            app.UseHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHttpsRedirection();
            app.UseCors("AllowSpecificOrigins");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
        catch (Exception ex)
        {
            logger?.LogCritical(ex, "Application start-up failed");
            Console.WriteLine(ex);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void AddHealthChecks(WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddHealthChecks()
            .AddMySql(configuration.GetConnectionString("MySql")!);
    }
}
