namespace AuthService.Models
{
    public class SuccessResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public int Code { get; set; }

        public SuccessResponse(string message, object data = null, int code = 200)
        {
            Success = true;
            Message = message;
            Data = data;
            Code = code;
        }
    }
}