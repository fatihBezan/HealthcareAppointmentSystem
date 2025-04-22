namespace Healthcare.Application.Exceptions
{
    public class AppointmentLimitExceededException : AppException
    {
        public AppointmentLimitExceededException() 
            : base("You can only book one appointment with a doctor per week.") { }
        
        public AppointmentLimitExceededException(string doctorName) 
            : base($"You already have an appointment with Dr. {doctorName} within the last week. Please wait at least one week between appointments with the same doctor.") { }
    }
} 