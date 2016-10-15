using System;
using CandidateDocuments.API.Core;
using CandidateDocuments.API.ViewModels;
using CandidateDocuments.Application.Core;
using CandidateDocuments.Application.Services;
using FluentValidation;
using FluentValidation.Results;

namespace CandidateDocuments.API.Validators
{
    public class DocumentModelValidator : AbstractValidator<DocumentSaveModel>
    {
        public DocumentModelValidator(IModulesService modulesService)
        {
            RuleFor(doc => doc.CandidateId).NotEmpty().WithMessage(ResponseMessageCodes.EmptyRequiredAttribute);
            RuleFor(doc => doc.Filename).NotEmpty().WithMessage(ResponseMessageCodes.EmptyRequiredAttribute);
            CustomAsync(async doc =>
            {
                try
                {
                    if ((doc.ReviewerId == null || doc.ReviewerId == default(Guid)) && await modulesService.IsActive(Modules.AccountManagement))
                        return new ValidationFailure("ReviewerId", ResponseMessageCodes.EmptyRequiredAttribute);
                }
                catch (IncompleteRequest)
                {
                    return new ValidationFailure("", ResponseMessageCodes.IncompleteRequest);
                }
                return null;
            });
        }
    }
}
