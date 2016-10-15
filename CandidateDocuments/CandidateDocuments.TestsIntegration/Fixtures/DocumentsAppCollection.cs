using Xunit;

namespace CandidateDocuments.Tests.Integration.Fixtures
{
    public static partial class CollectionNames
    {
        public const string DocumentsApp = "DocumentsApp";
    }

    [CollectionDefinition(CollectionNames.DocumentsApp)]
    public class DocumentsAppCollection :
        ICollectionFixture<TestAppFixture>,
        ICollectionFixture<TestSettingsFixture>,
        ICollectionFixture<ModulesWebHostFixture>
    {
    }
}