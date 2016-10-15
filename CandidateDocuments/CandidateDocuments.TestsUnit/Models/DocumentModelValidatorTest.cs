using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using CandidateDocuments.Application.Models;
using CandidateDocuments.Application.Services;
using CandidateDocuments.API.ViewModels;
using CandidateDocuments.API.Validators;

namespace CandidateDocuments.Tests.Unit.Models
{
    public class DocumentModelValidatorTest
    {
        private readonly Mock<IModulesService> _modulesServiceMock;
        private readonly DocumentModelValidator _documentValidator;
        private readonly DocumentSaveModel _documentSm;

        public DocumentModelValidatorTest()
        {
            _modulesServiceMock = new Mock<IModulesService>();
            _documentValidator = new DocumentModelValidator(_modulesServiceMock.Object);
            _documentSm = new DocumentSaveModel
            {
                CandidateId = Guid.NewGuid(),
                Filename = "MyCV.pdf",
                DocumentType = (int)DocumentType.BusinessPlan,
                ReviewerId = Guid.NewGuid(),
            };

        }

        public class ValidateMethod : DocumentModelValidatorTest
        {
            [Fact]
            public void ShouldPassOnCorrectViewModel()
            {
                var result = _documentValidator.Validate(_documentSm);
                Assert.True(result.IsValid);
            }
        }

        public class CandidateIdProperty : DocumentModelValidatorTest
        {
            [Fact]
            public void DisallowedEmpty()
            {
                _documentSm.CandidateId = Guid.Empty;
                var result = _documentValidator.Validate(_documentSm);
                Assert.False(result.IsValid);
            }
        }

        public class FilenameProperty : DocumentModelValidatorTest
        {
            [Fact]
            public void DisallowedEmpty()
            {
                _documentSm.Filename = "";
                var result = _documentValidator.Validate(_documentSm);
                Assert.False(result.IsValid);
            }
        }

        public class ReviewerIdProperty : DocumentModelValidatorTest
        {
            [Theory,
                InlineData(true, false),
                InlineData(false, true)]
            public void EmptyAllowanceDependentOnAccountManagementModuleActivation(bool moduleActive, bool allowEmpty)
            {
                _modulesServiceMock.Setup(x => x.IsActive(Modules.AccountManagement)).Returns(Task.FromResult(moduleActive));
                _documentSm.ReviewerId = Guid.Empty;
                var result = _documentValidator.Validate(_documentSm);
                Assert.True(result.IsValid == allowEmpty);
            }
        }
    }
}
