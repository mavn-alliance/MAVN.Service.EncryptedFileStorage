using System;
using FluentValidation;
using JetBrains.Annotations;
using MAVN.Service.EncryptedFileStorage.Client.Models.Requests;
using MAVN.Service.EncryptedFileStorage.Domain.Services;

namespace MAVN.Service.EncryptedFileStorage.Validation
{
    [UsedImplicitly]
    public class CreateFileInfoRequestValidator : AbstractValidator<CreateFileInfoRequest>
    {
        private readonly IEncryptedFileService _encryptedFileService;

        public CreateFileInfoRequestValidator(IEncryptedFileService encryptedFileService)
        {
            _encryptedFileService = encryptedFileService;

            RuleFor(x => x.Origin)
                .NotNull()
                .NotEmpty()
                .WithMessage("Origin is required")
                .MinimumLength(3)
                .WithMessage("Origin must not be less then 3 characters in length")
                .MaximumLength(63)
                .WithMessage("Origin must not be more then 63 characters in length")
                .Custom((origin, context) =>
                {
                    if (string.IsNullOrEmpty(origin))
                    {
                        context.AddFailure("Origin is required");
                        return;
                    }

                    var preparedName = _encryptedFileService.PrepareContainerName(origin);

                    if (preparedName.Length < 3)
                    {
                        context.AddFailure("Origin, when prepared, must not be less then 3 characters in length");
                    }
                    else if (preparedName.Length > 63)
                    {
                        context.AddFailure("Origin, when prepared, must not be more then 63 characters in length");
                    }
                });

            RuleFor(x => x.FileName)
                .NotNull()
                .NotEmpty()
                .WithMessage("File name is required")
                .MaximumLength(255)
                .WithMessage("File name must not be more then 255 characters in length")
                .Custom((fileName, context) =>
                {
                    if (string.IsNullOrEmpty(fileName))
                    {
                        context.AddFailure("File name is required");
                        return;
                    }

                    //We can validate with and empty GUID, cause we just need GUID chars
                    var preparedName = _encryptedFileService.PrepareBlobName(fileName, Guid.Empty);

                    if (preparedName.Length > 1024)
                    {
                        context.AddFailure("File name, when prepared, must not be more then 1024 characters in length");
                    }
                });
        }
    }
}
