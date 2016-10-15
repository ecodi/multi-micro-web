namespace CandidateDocuments.API.Core.Authorization
{
    public class AnyApiKeyRequirement : ApiKeyRequirement
    {
        public AnyApiKeyRequirement()
        {
            Filter = key => !string.IsNullOrWhiteSpace(key);
        }
    }
}
