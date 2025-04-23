using System.Text;
using BillingService.Data;
using BillingService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace BillingService;

/// <summary>
/// Clase de configuración principal de la aplicación ASP.NET Core.
/// Configura servicios, autenticación, autorización, seguridad y middlewares.
/// </summary>
public class Startup
{
    private const string CorsPolicyName = "AllowSpecificOrigin";

    /// <summary>
    /// Acceso a la configuración de la aplicación (appsettings.json, variables de entorno, etc.)
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Constructor que recibe la configuración global de la aplicación.
    /// </summary>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// Método para registrar servicios en el contenedor de dependencias.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        // === Configuración de los contextos de base de datos ===
        services.AddDbContext<VentaDBContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("sql"))
                   .EnableSensitiveDataLogging(false)
                   .LogTo(Console.WriteLine, LogLevel.Information));

        services.AddDbContext<CotizacionDBContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("sql"))
                   .EnableSensitiveDataLogging(false)
                   .LogTo(Console.WriteLine, LogLevel.Information));

        // === Configuración de autenticación JWT ===
        var secretKey = Encoding.UTF8.GetBytes(Configuration["Jwt:SecretKey"] ?? string.Empty);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true; // Requiere HTTPS
                options.SaveToken = true; // Guarda el token en el contexto
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(1), // Tolerancia de tiempo
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey)
                };
            });

        services.AddAuthorization(); // Habilita la autorización

        // === Inyección de dependencias para servicios personalizados ===
        services.AddScoped<VentaService>();
        services.AddScoped<VentaProductoService>();
        services.AddScoped<CotizacionService>();
        services.AddScoped<CotizacionProductoService>();

        // === Configuración de CORS para permitir solicitudes del frontend ===
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, builder =>
            {
                // Configuración CORS basada en el entorno
                if (Configuration.GetValue<string>("Environment") == "Development")
                {
                    // En entorno de desarrollo, solo permitir solicitudes desde http://localhost:4200
                    builder.WithOrigins("http://localhost:4200")  // Origen solo para desarrollo
                        .AllowAnyHeader()                   // Permitir cualquier encabezado
                        .AllowAnyMethod()                   // Permitir cualquier método HTTP
                        .AllowCredentials()                 // Permitir el uso de credenciales (cookies, JWT, etc.)
                        .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));  // Cacheo preflight durante 1 hora
                }
                else
                {
                    // En producción, usar orígenes permitidos configurados en appsettings.json
                    var allowedOrigins = Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
                    builder.WithOrigins(allowedOrigins)           // Orígenes permitidos desde appsettings.json
                        .WithHeaders("Content-Type", "Authorization") // Permitir solo los encabezados necesarios
                        .WithMethods("GET", "POST", "PUT") // Limitar métodos HTTP permitidos
                        .AllowCredentials()               // Permitir el uso de credenciales
                        .SetPreflightMaxAge(TimeSpan.FromMinutes(10));  // Cacheo preflight durante 10 minutos
                }
            });
        });


        // === Configuración de Swagger para documentación de la API ===
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Description = "Ingrese un token JWT válido (formato: Bearer {token})",
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services.AddControllers(); // Registro de controladores
        services.AddEndpointsApiExplorer(); // Habilita exploración de endpoints
    }

    /// <summary>
    /// Método que configura el pipeline de middleware de la aplicación.
    /// </summary>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // === Entorno de desarrollo: habilita Swagger y página de errores ===
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // === Seguridad adicional usando NWebsec ===
        app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains().Preload());
        app.UseXContentTypeOptions(); // Previene inferencia MIME
        app.UseXfo(options => options.Deny()); // Deniega inclusión en iframes
        app.UseXXssProtection(options => options.EnabledWithBlockMode()); // Protección XSS
        app.UseReferrerPolicy(opts => opts.NoReferrer()); // Política de referencia
        app.UseCsp(options =>
        {
            options.BlockAllMixedContent();
            options.DefaultSources(s => s.Self());
            options.ScriptSources(s => s.Self().UnsafeInline());
            options.StyleSources(s => s.Self().UnsafeInline());
            options.ImageSources(s => s.Self().CustomSources("data:"));
            options.ObjectSources(s => s.None());
            options.FormActions(s => s.Self());
            options.FrameAncestors(s => s.None());
            options.UpgradeInsecureRequests();
            options.ReportUris(r => r.Uris("https://reporturi.example.com/csp")); // Cambiar URI real
        });
        app.UseXDownloadOptions();
        app.UseXRobotsTag(options => options.NoIndex().NoFollow());

        // === Cabeceras de seguridad personalizadas adicionales ===
        app.Use(async (context, next) =>
        {
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
            context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
            context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
            context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
            //context.Response.Headers["Clear-Site-Data"] = "\"cookies\", \"storage\"";//momentaneo ya que sino no carga lo relacionado a venta ya que elimina las cookies, storage y cosas locales
            context.Response.Headers.Remove("Server"); // Oculta detalles del servidor
            await next();
        });

        // === Middleware de ASP.NET Core ===
        app.UseRouting(); // Enrutamiento
        app.UseCors(CorsPolicyName); // Aplicación de política CORS
        app.UseAuthentication(); // Autenticación JWT
        app.UseAuthorization(); // Autorización de acceso
        app.UseEndpoints(endpoints => endpoints.MapControllers()); // Mapea controladores a rutas
    }
}
