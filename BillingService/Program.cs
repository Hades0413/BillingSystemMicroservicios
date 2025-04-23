// Importa el espacio de nombres BillingService
using BillingService;

var builder = WebApplication.CreateBuilder(args);

// Configura los servicios necesarios para la aplicación
// Crea una instancia de la clase Startup que es responsable de configurar los servicios y el pipeline de la aplicación
var startup = new Startup(builder.Configuration);

// Llama a ConfigureServices de Startup para registrar todos los servicios necesarios en el contenedor de dependencias
startup.ConfigureServices(builder.Services);

// Construye la aplicación basada en la configuración actual
// Este paso construye el objeto WebApplication que contiene los middleware y el pipeline de ejecución de solicitudes HTTP
var app = builder.Build();

// Llama a Configure de Startup para configurar los middleware y otros componentes del pipeline de solicitudes
// El entorno de la aplicación se pasa como parámetro para permitir configuraciones específicas de entorno
startup.Configure(app, app.Environment);

// Configuración dinámica del puerto para la aplicación
// Lee el valor del puerto desde la configuración de la aplicación (appsettings.json, variables de entorno, etc.)
var port = builder.Configuration.GetValue<string>("AppSettings:Port");

// Si el valor del puerto es nulo o vacío, se asigna un puerto por defecto (5002)
if (string.IsNullOrEmpty(port))
{
    port = "5002";  // Puerto por defecto si no se configura explícitamente en la configuración
}

// Inicia la aplicación en el puerto configurado o el puerto por defecto
// app.Run comienza a escuchar las solicitudes HTTP en el puerto especificado (por defecto: http://localhost:5002)
app.Run($"http://localhost:{port}");