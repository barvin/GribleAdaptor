
using System;
namespace PineAdaptor.Errors
{
    public class SimpleErrorsHandler : IErrorsHandler
    {
        public void OnAdaptorFail(Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
