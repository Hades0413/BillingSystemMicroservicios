namespace AuthService.Response
{
    /// <summary>
    /// Representa una respuesta estándar para una solicitud exitosa en la API.
    /// Se utiliza para devolver detalles sobre una operación exitosa y los datos resultantes.
    /// </summary>
    public class SuccessResponse
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="SuccessResponse"/>.
        /// </summary>
        /// <param name="message">Mensaje principal que describe el éxito de la operación.</param>
        /// <param name="data">Datos opcionales relacionados con la respuesta (por defecto es null).</param>
        /// <param name="code">Código HTTP de la respuesta (por defecto es 200, OK).</param>
        /// <remarks>
        /// Este constructor se utiliza para crear una respuesta exitosa personalizada.
        /// El código de estado HTTP se puede modificar para adaptarse a diferentes tipos de éxito (por ejemplo, 201 para Creación).
        /// </remarks>
        public SuccessResponse(string message, object data = null, int code = 200)
        {
            Success = true;
            Message = message;
            Data = data;
            Code = code;
        }

        /// <summary>
        /// Obtiene o establece un valor que indica si la solicitud fue exitosa.
        /// Siempre es <c>true</c> en el caso de una respuesta exitosa.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Obtiene o establece el mensaje que describe el éxito ocurrido.
        /// Este mensaje es mostrado al usuario o utilizado en registros de éxito.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Obtiene o establece los datos relacionados con la respuesta exitosa.
        /// Este campo es opcional y puede contener cualquier objeto de datos que la operación haya devuelto.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Obtiene o establece el código HTTP que representa el tipo de respuesta exitosa.
        /// Los valores comunes incluyen 200 (OK), 201 (Created), 204 (No Content), etc.
        /// </summary>
        public int Code { get; set; }
    }
}
