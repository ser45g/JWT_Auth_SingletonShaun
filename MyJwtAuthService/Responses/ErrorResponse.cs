namespace MyJwtAuthService.Responses
{
    public class ErrorResponse
    {
        public IEnumerable<string> ErrorMessages { get; set; }

        public ErrorResponse() : this(new List<string>()) { }

        public ErrorResponse(string errorMessage) : this(new List<string>() { errorMessage }) { }

        public ErrorResponse(IEnumerable<string> errorMessages)
        {
            ErrorMessages = errorMessages;
        }
    }
}
