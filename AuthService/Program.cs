using System.Text;
using AuthService.Data;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

#region Configuración de servicios

// ==========================================================================================
// Base de Datos - Inyección del DbContext con SQL Server
// ==========================================================================================
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sql")));

// ==========================================================================================
// Inyección de dependencias personalizadas
// ==========================================================================================
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<OAuthService>();
builder.Services.AddScoped<JwtService>();

// ==========================================================================================
// CORS - Política para permitir origen específico (Angular frontend en localhost)
// ==========================================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        // Asegúrate de que el front-end esté en esta URL
        policy.WithOrigins("http://localhost:4200") // O cambia por tu dominio de producción
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials() // Esto es crucial si usas cookies o autenticación basada en sesiones
            .SetPreflightMaxAge(TimeSpan.FromSeconds(3600)); // Tiempo máximo para la cache de la pre-solicitud CORS
    });
});

// ==========================================================================================
// JWT Bearer Authentication - Seguridad basada en tokens
// ==========================================================================================
var secretKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("El Jwt:SecretKey no está configurado en el archivo de configuración.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true; // Forzar el uso de HTTPS
        options.SaveToken = true; // Guarda el token en el contexto
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1), // Tolerancia máxima al reloj
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey))
        };
    });

// ==========================================================================================
// Swagger - Documentación de API (solo en desarrollo)
// ==========================================================================================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthService API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Ingrese el token como: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// ==========================================================================================
// Servicios Web API
// ==========================================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

#endregion

#region Configuración del pipeline HTTP

var app = builder.Build();

// ==========================================================================================
// Redirección a HTTPS y uso de HSTS (en producción)
// ==========================================================================================
if (!app.Environment.IsDevelopment()) 
{
    app.UseHsts(); // HSTS (HTTP Strict Transport Security)
}

app.UseHttpsRedirection(); // Redirige solicitudes HTTP a HTTPS

// ==========================================================================================
// Middleware de seguridad - Cabeceras HTTP avanzadas
// ==========================================================================================
app.Use(async (context, next) =>
{
    // Elimina cabeceras que pueden revelar información del servidor
    context.Response.Headers.Remove("Server"); // Evita que se revele la información del servidor que podría ser utilizada en ataques
    context.Response.Headers.Remove("X-Powered-By"); // Elimina la cabecera que indica el software de servidor utilizado

    // Seguridad del transporte
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload"; // HSTS: Forza el uso de HTTPS y protege contra ataques "man-in-the-middle"

    // Política de seguridad de contenidos (CSP)
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " + // Permite cargar solo contenido de la misma fuente
        "img-src 'self' data:; " + // Permite cargar imágenes solo desde la misma fuente o en formato base64
        "script-src 'self'; " + // Permite cargar scripts solo desde la misma fuente
        "style-src 'self' 'unsafe-inline'; " + // Permite cargar estilos solo desde la misma fuente y estilos en línea
        "object-src 'none'; " + // No permite cargar objetos o plugins
        "frame-ancestors 'none';"; // Evita que la página sea embebida en un iframe (previene clickjacking)

    // Prevención contra ataques comunes
    context.Response.Headers["X-Frame-Options"] = "DENY"; // Evita clickjacking, impidiendo que la página sea cargada en un iframe
    context.Response.Headers["X-Content-Type-Options"] = "nosniff"; // Evita que el navegador adivine el tipo de contenido, lo que podría llevar a vulnerabilidades
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block"; // Filtro básico contra ataques XSS (Cross-Site Scripting)

    // Política de privacidad y navegación
    context.Response.Headers["Referrer-Policy"] = "no-referrer"; // No se comparte la URL de referencia al hacer peticiones
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()"; // Limita el acceso a ciertas APIs, como la geolocalización, micrófono y cámara

    // Cross-Origin Resource Security
    context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp"; // Requiere que los recursos embebidos provengan de dominios confiables
    context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin"; // Aísla la ventana de contexto actual de ventanas de otros orígenes
    context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-site"; // Solo permite recursos de la misma fuente

    // Política de cross-domain y almacenamiento
    context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none"; // No permite políticas de cross-domain que puedan comprometer la seguridad
    context.Response.Headers["Clear-Site-Data"] = "\"cache\", \"cookies\", \"storage\", \"executionContexts\""; // Limpia el almacenamiento local, cookies, y otros datos relacionados con el sitio

    // Control de caché para datos sensibles
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate"; // Evita que el navegador almacene en caché información sensible
    context.Response.Headers["Pragma"] = "no-cache"; // Similar a Cache-Control, previene la caché de respuestas
    context.Response.Headers["Expires"] = "0"; // Asegura que la respuesta no se almacene en caché

    await next.Invoke(); // Continúa con el siguiente middleware en la cadena
});


// ==========================================================================================
// Middlewares de seguridad y ruteo
// ==========================================================================================
app.UseCors("AllowSpecificOrigin"); // Política CORS personalizada
app.UseRouting();
app.UseAuthentication(); // Autenticación JWT
app.UseAuthorization(); // Autorización basada en identidad

// ==========================================================================================
// Swagger solo disponible en entorno de desarrollo
// ==========================================================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ==========================================================================================
// Mapeo de controladores a endpoints HTTP
// ==========================================================================================
app.MapControllers();

// ==========================================================================================
// Configuración de URL personalizada para el servidor
// ==========================================================================================
app.Urls.Add("http://localhost:5001");

// ==========================================================================================
// Inicia la aplicación
// ==========================================================================================
app.Run();

#endregion
