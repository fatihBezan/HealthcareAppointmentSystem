using FluentValidation;
using Healthcare.Application.DTOs;

namespace Healthcare.Application.Validators
{
    public class CreateDoctorValidator : AbstractValidator<CreateDoctorDto>
    {
        public CreateDoctorValidator()
        {
            RuleFor(d => d.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(d => d.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(d => d.Specialty)
                .NotEmpty().WithMessage("Specialty is required.")
                .MaximumLength(100).WithMessage("Specialty cannot exceed 100 characters.");

            RuleFor(d => d.HospitalId)
                .GreaterThan(0).WithMessage("A valid hospital must be selected.");
        }
    }

    public class UpdateDoctorValidator : AbstractValidator<UpdateDoctorDto>
    {
        public UpdateDoctorValidator()
        {
            RuleFor(d => d.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(d => d.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(d => d.Specialty)
                .NotEmpty().WithMessage("Specialty is required.")
                .MaximumLength(100).WithMessage("Specialty cannot exceed 100 characters.");

            RuleFor(d => d.HospitalId)
                .GreaterThan(0).WithMessage("A valid hospital must be selected.");
        }
    }
} 