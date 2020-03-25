using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.EncryptedFileStorage.Client.Models.Requests;

namespace Lykke.Service.EncryptedFileStorage.Validation
{
    [UsedImplicitly]
    public class PaginationModelRequestValidator : AbstractValidator<PaginationModelRequest>
    {
        public PaginationModelRequestValidator()
        {
            RuleFor(x => x.CurrentPage)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Current page cannot be less than 1 and cannot exceed more than 10000")
                .LessThanOrEqualTo(10_000)
                .WithMessage("Current page cannot be less than 1 and cannot exceed more than 10000");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page Size can't be less than 1 and cannot exceed more then 500")
                .LessThanOrEqualTo(500)
                .WithMessage("Page Size can't be less than 1 and cannot exceed more then 500");
        }
    }
}
