using FluentValidation;
using Healthcare.Application.DTOs;

namespace Healthcare.Application.Validators
{
    public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentDto>
    {
        public CreateAppointmentValidator()
        {
            RuleFor(a => a.DoctorId)
                .GreaterThan(0).WithMessage("A valid doctor must be selected.");

            RuleFor(a => a.AppointmentDate)
                .NotEmpty().WithMessage("Appointment date is required.")
                .GreaterThan(DateTime.Now).WithMessage("Appointment date must be in the future.");

            RuleFor(a => a.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
        }
    }

    public class UpdateAppointmentValidator : AbstractValidator<UpdateAppointmentDto>
    {
        public UpdateAppointmentValidator()
        {
            RuleFor(a => a.AppointmentDate)
                .NotEmpty().WithMessage("Appointment date is required.")
                .GreaterThan(DateTime.Now).WithMessage("Appointment date must be in the future.");

            RuleFor(a => a.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
        }
    }
} 