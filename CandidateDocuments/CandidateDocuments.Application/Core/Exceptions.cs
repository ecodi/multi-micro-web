using System;

namespace CandidateDocuments.Application.Core
{
    public class UniqnessViolation : Exception
    {
        public UniqnessViolation(string message) : base(message)
        {
        }
    }

    public class SaveDataError : Exception
    {
        public SaveDataError(string message) : base(message)
        {
        }
    }

    public class IncompleteRequest : Exception
    {
        public IncompleteRequest(string message) : base(message)
        {
        }
    }

    public class FailureHandledError : Exception
    {
        public FailureHandledError(string message) : base(message)
        {
        }
    }
}
