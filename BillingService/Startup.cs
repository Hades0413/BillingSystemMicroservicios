using System.Text;
using System.Threading.RateLimiting;
using BillingService.Data;
using BillingService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace BillingService
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public IConfiguration Configuration { get; }

        // Constructor de Startup para inicializar la configuración y el entorno de la aplicación
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _env = env;
        }

        // Método para configurar los servicios de la aplicación
        public void ConfigureServices(IServiceCollection services)
        {
            // ======================================================================================
            // Configuración de bases de datos (contextos de venta y cotización)
            // ======================================================================================
            ConfigureDatabase<VentaDBContext>(services);
            ConfigureDatabase<CotizacionDBContext>(services);

            // ======================================================================================
            // Configuración de Seguridad JWT (Json Web Token)
            // ======================================================================================
            var jwtSettings = Configuration.GetSection("Jwt");
            services.Configure<JwtSettings>(jwtSettings);

            var jwtConfig = jwtSettings.Get<JwtSettings>();
            if (jwtConfig == null)
            {
                throw new ArgumentNullException("La configuración de JWT no se encuentra en appsettings.");
            }

            ValidateJwtConfiguration(jwtConfig);

            // Configuración de autenticación JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => ConfigureJwtOptions(options, jwtConfig));

            // ======================================================================================
            // Configuración de políticas de autorización
            // ======================================================================================
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy =>
                    policy.RequireRole("Administrator"));
            });

            // ======================================================================================
            // Inyección de dependencias para servicios de negocio
            // ======================================================================================
            services.AddScoped<VentaService>();
            services.AddScoped<VentaProductoService>();
            services.AddScoped<CotizacionService>();
            services.AddScoped<CotizacionProductoService>();

            // ======================================================================================
            // Configuración de CORS (Cross-Origin Resource Sharing)
            // ======================================================================================
            ConfigureCors(services);

            // ======================================================================================
            // Configuración de controladores y API REST
            // ======================================================================================
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.WriteIndented = _env.IsDevelopment();
                });

            // ======================================================================================
            // Configuración de Swagger con autenticación JWT
            // ======================================================================================
            ConfigureSwagger(services);

            // ======================================================================================
            // Configuración de Health Checks (verificación de la salud de la aplicación)
            // ======================================================================================
            services.AddHealthChecks()
                .AddDbContextCheck<VentaDBContext>()
                .AddDbContextCheck<CotizacionDBContext>();

            // ======================================================================================
            // Configuración de Rate Limiting (limitación de velocidad de peticiones)
            // ======================================================================================
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("ApiLimiter", limiter =>
                {
                    limiter.PermitLimit = 100;  // Número de solicitudes permitidas por ventana
                    limiter.Window = TimeSpan.FromMinutes(1);  // Duración de la ventana
                    limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiter.QueueLimit = 10;  // Límites de la cola de solicitudes
                });
            });

            // ======================================================================================
            // Configuración de compresión de respuestas (gzip y brotli)
            // ======================================================================================
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
        }

        // Método genérico para configurar la base de datos (para distintos DbContexts)
        private void ConfigureDatabase<TContext>(IServiceCollection services) where TContext : DbContext
        {
            var connectionString = Configuration.GetConnectionString("sql") 
                ?? throw new ArgumentNullException("Connection string 'sql' no configurada");

            services.AddDbContext<TContext>(options =>
            {
                options.UseSqlServer(connectionString);
                
                // Configuración adicional para desarrollo
                if (_env.IsDevelopment())
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            });
        }

        // Configuración de las opciones JWT para la autenticación
        private void ConfigureJwtOptions(JwtBearerOptions options, JwtSettings jwtConfig)
        {
            options.RequireHttpsMetadata = !_env.IsDevelopment();
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey)),
                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha512 }
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (_env.IsDevelopment())
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                    }
                    return Task.CompletedTask;
                }
            };
        }

        // Configuración de CORS
        private void ConfigureCors(IServiceCollection services)
        {
            // Obtener los orígenes permitidos desde el archivo de configuración
            var allowedOrigins = Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

            // Configuración de CORS
            services.AddCors(options =>
            {
                options.AddPolicy("SecureCors", policy =>
                {
                    // Verifica si hay orígenes configurados
                    if (allowedOrigins != null && allowedOrigins.Length > 0)
                    {
                        // Permitir solo los orígenes configurados
                        policy.WithOrigins(allowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .SetPreflightMaxAge(TimeSpan.FromHours(1))
                            .WithExposedHeaders("X-Total-Count", "Content-Disposition");
                    }
                    else
                    {
                        // Si no se configuraron orígenes, lanzar un error
                        throw new InvalidOperationException("No se han configurado orígenes permitidos para CORS.");
                    }
                });
            });
        }

        // Configuración de Swagger para documentación de la API
        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "Billing Service API", 
                    Version = "v1",
                    Contact = new OpenApiContact 
                    { 
                        Name = "Soporte Técnico", 
                        Email = "soporte@empresa.com" 
                    }
                });

                // Configuración de la autenticación JWT en Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header usando el esquema Bearer."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
        }

        // Método para configurar el pipeline de la aplicación (middleware)
        public void Configure(IApplicationBuilder app)
        {
            // ======================================================================================
            // Pipeline de ejecución
            // ======================================================================================
            
            // Manejo de excepciones y configuración básica
            app.UseExceptionHandler(_env.IsDevelopment() ? "/error-local" : "/error");
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseResponseCompression();
            
            // Aplicar headers de seguridad personalizados
            app.UseSecurityHeaders();
            
            // Habilitar Swagger en desarrollo
            if (_env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => 
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Billing API v1");
                    c.DisplayRequestDuration();
                });
            }

            // Configuración de Routing y CORS
            app.UseRouting();
            app.UseCors("SecureCors");
            
            // Configuración de autenticación y autorización
            app.UseAuthentication();
            app.UseAuthorization();
            
            // Activación de rate limiter
            app.UseRateLimiter();
            
            // Configuración de endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        var result = new
                        {
                            status = report.Status.ToString(),
                            checks = report.Entries.Select(e => new
                            {
                                name = e.Key,
                                status = e.Value.Status.ToString(),
                                exception = e.Value.Exception?.Message,
                                duration = e.Value.Duration
                            })
                        };
                        
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(result);
                    }
                });
            });
        }

        // Validación de la configuración JWT (para asegurar que todos los valores están presentes)
        private void ValidateJwtConfiguration(JwtSettings config)
        {
            if (string.IsNullOrEmpty(config?.SecretKey))
                throw new ArgumentNullException("JWT SecretKey no configurado");
            
            if (string.IsNullOrEmpty(config.Issuer))
                throw new ArgumentNullException("JWT Issuer no configurado");
            
            if (string.IsNullOrEmpty(config.Audience))
                throw new ArgumentNullException("JWT Audience no configurado");
            
            if (config.SecretKey.Length < 64)
                throw new ArgumentException("JWT SecretKey debe tener al menos 64 caracteres");
        }
    }

    // Extensiones personalizadas para mejor organización
    public static class SecurityHeadersMiddlewareExtensions
    {
        // Middleware personalizado para agregar headers de seguridad
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            var policyCollection = new HeaderPolicyCollection()
                .AddFrameOptionsDeny()
                .AddContentTypeOptionsNoSniff()
                .AddReferrerPolicyStrictOriginWhenCrossOrigin()
                .AddCrossOriginOpenerPolicy(builder => builder.SameOrigin())
                .AddCrossOriginEmbedderPolicy(builder => builder.RequireCorp())
                .AddCrossOriginResourcePolicy(builder => builder.SameOrigin())
                .AddContentSecurityPolicy(builder =>
                {
                    builder.AddDefaultSrc().Self();
                    builder.AddObjectSrc().None();
                    builder.AddFormAction().Self();
                    builder.AddImgSrc().Self().Data();
                    builder.AddScriptSrc().Self().UnsafeInline().WithNonce();
                    builder.AddStyleSrc().Self().UnsafeInline();
                    builder.AddUpgradeInsecureRequests();
                    //builder.AddReportUri().To("https://reporturi.example.com/csp"); // Reporte de violaciones CSP
                })
                .RemoveServerHeader()
                .AddPermissionsPolicy(builder =>
                {
                    builder.AddAccelerometer().None();
                    builder.AddCamera().None();
                    builder.AddGeolocation().None();
                    builder.AddMicrophone().None();
                    builder.AddPayment().None();
                });

            return app.UseSecurityHeaders(policyCollection);
        }
    }

    // Configuración de los parámetros JWT
    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpirationMinutes { get; set; } = 1;
    }
}
