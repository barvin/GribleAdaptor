using System;

namespace GribleAdaptor.Errors
{
    /// <summary>
    /// The interface for the handlers of exceptions that occur in the grible adaptor.
    /// </summary>
    public interface IErrorsHandler
    {
        void OnAdaptorFail(Exception e);
    }
}
