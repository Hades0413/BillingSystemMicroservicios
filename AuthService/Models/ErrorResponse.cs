namespace AuthService.Models
{
    public class ErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorDetails { get; set; }
        public int Code { get; set; }

        public ErrorResponse(string message, string errorDetails = null, int code = 400)
        {
            Success = false;
            Message = message;
            ErrorDetails = errorDetails;
            Code = code;
        }
    }
}