namespace AuthService.Response
{
    /// <summary>
    /// Representa una respuesta de error estándar para las API.
    /// Se utiliza para devolver detalles sobre errores que ocurren durante el procesamiento de una solicitud.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ErrorResponse"/>.
        /// </summary>
        /// <param name="message">Mensaje principal que describe el error.</param>
        /// <param name="errorDetails">Detalles adicionales sobre el error (opcional).</param>
        /// <param name="code">Código HTTP del error (por defecto es 400, Bad Request).</param>
        /// <remarks>
        /// Este constructor se utiliza para crear una respuesta de error personalizada.
        /// El código de error se puede modificar para adaptarse a diferentes tipos de errores HTTP.
        /// </remarks>
        public ErrorResponse(string message, string errorDetails = null, int code = 400)
        {
            Success = false;
            Message = message;
            ErrorDetails = errorDetails;
            Code = code;
        }

        /// <summary>
        /// Obtiene o establece un valor que indica si la solicitud fue exitosa.
        /// Siempre es <c>false</c> en el caso de una respuesta de error.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Obtiene o establece el mensaje que describe el error ocurrido.
        /// Este mensaje es mostrado al usuario o utilizado en registros de error.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Obtiene o establece detalles adicionales sobre el error.
        /// Este campo es opcional y puede incluir información técnica o mensajes detallados del error.
        /// </summary>
        public string ErrorDetails { get; set; }

        /// <summary>
        /// Obtiene o establece el código HTTP que representa el tipo de error.
        /// Los valores comunes incluyen 400 (Bad Request), 401 (Unauthorized), 404 (Not Found), 500 (Internal Server Error), etc.
        /// </summary>
        public int Code { get; set; }
    }
}
