namespace Healthcare.Application.Exceptions
{
    public class UnauthorizedAccessException : AppException
    {
        public UnauthorizedAccessException() 
            : base("You are not authorized to perform this action.") { }
        
        public UnauthorizedAccessException(string resource) 
            : base($"You are not authorized to access {resource}.") { }
    }
} 