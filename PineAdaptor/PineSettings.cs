using PineAdaptor.Errors;
using System;
using System.Xml;

namespace PineAdaptor
{
    /// <summary>
    /// Class that holds pine adaptor settings.
    /// </summary>
    public class PineSettings
    {
        private static IErrorsHandler handler;

        /// <summary>
        /// The handler for the errors that occur in the pine adaptor.
        /// </summary>
        public static IErrorsHandler ErHandler
        {
            get
            {
                if (handler == null)
                {
                    handler = new SimpleErrorsHandler();
                }
                return handler;
            }
            set
            {
                handler = value;
            }
        }

        public static string ProductName { get; set; }
        internal static string Dbhost { get; set; }
        internal static string Dbport { get; set; }
        internal static string Dbname { get; set; }
        internal static string Dblogin { get; set; }
        internal static string Dbpswd { get; set; }

        public static void Init(string configFilePath)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(configFilePath);
                if (doc != null)
                {
                    XmlNode database = doc.DocumentElement.GetElementsByTagName("pinedb").Item(0);
                    Dbhost = database.ChildNodes.Item(0).InnerText;
                    Dbport = database.ChildNodes.Item(1).InnerText;
                    Dbname = database.ChildNodes.Item(2).InnerText;
                    Dblogin = database.ChildNodes.Item(3).InnerText;
                    Dbpswd = database.ChildNodes.Item(4).InnerText;
                }
            }
            catch (Exception e)
            {
                ErHandler.OnAdaptorFail(e);
            }
        }
    }
}
