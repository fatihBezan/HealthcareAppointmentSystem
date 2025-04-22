using FluentValidation;
using Healthcare.Application.DTOs;

namespace Healthcare.Application.Validators
{
    public class CreateHospitalValidator : AbstractValidator<CreateHospitalDto>
    {
        public CreateHospitalValidator()
        {
            RuleFor(h => h.Name)
                .NotEmpty().WithMessage("Hospital name is required.")
                .MaximumLength(100).WithMessage("Hospital name cannot exceed 100 characters.");

            RuleFor(h => h.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(200).WithMessage("Address cannot exceed 200 characters.");

            RuleFor(h => h.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(50).WithMessage("City cannot exceed 50 characters.");
        }
    }

    public class UpdateHospitalValidator : AbstractValidator<UpdateHospitalDto>
    {
        public UpdateHospitalValidator()
        {
            RuleFor(h => h.Name)
                .NotEmpty().WithMessage("Hospital name is required.")
                .MaximumLength(100).WithMessage("Hospital name cannot exceed 100 characters.");

            RuleFor(h => h.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(200).WithMessage("Address cannot exceed 200 characters.");

            RuleFor(h => h.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(50).WithMessage("City cannot exceed 50 characters.");
        }
    }
} 