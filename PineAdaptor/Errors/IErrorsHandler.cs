using System;

namespace PineAdaptor.Errors
{
    /// <summary>
    /// The interface for the handlers of exceptions that occur in the pine adaptor.
    /// </summary>
    public interface IErrorsHandler
    {
        void OnAdaptorFail(Exception e);
    }
}
