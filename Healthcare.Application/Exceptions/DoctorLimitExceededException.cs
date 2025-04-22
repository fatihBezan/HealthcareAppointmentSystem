namespace Healthcare.Application.Exceptions
{
    public class DoctorLimitExceededException : AppException
    {
        public DoctorLimitExceededException() 
            : base("The hospital has reached the maximum number of doctors for this specialty (10).") { }
        
        public DoctorLimitExceededException(string specialty, string hospitalName) 
            : base($"The hospital '{hospitalName}' has reached the maximum number of doctors (10) for specialty '{specialty}'.") { }
    }
} 