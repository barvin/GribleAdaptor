
using System;
namespace GribleAdaptor.Errors
{
    public class SimpleErrorsHandler : IErrorsHandler
    {
        public void OnAdaptorFail(Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
