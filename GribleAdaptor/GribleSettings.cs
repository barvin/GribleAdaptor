using GribleAdaptor.Errors;
using System;
using System.Xml;

namespace GribleAdaptor
{
    /// <summary>
    /// Class that holds grible adaptor settings.
    /// </summary>
    public class GribleSettings
    {
        private static IErrorsHandler _handler;

        /// <summary>
        /// The handler for the errors that occur in the grible adaptor.
        /// </summary>
        public static IErrorsHandler ErHandler
        {
            get { return _handler ?? (_handler = new SimpleErrorsHandler()); }
            set
            {
                _handler = value;
            }
        }

        public static string ProductName { get; set; }
        public static string ProductPath { get; set; }
        public static AppTypes AppType { get; set; }
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
                var database = doc.DocumentElement.GetElementsByTagName("gribledb").Item(0);
                Dbhost = database.ChildNodes.Item(0).InnerText;
                Dbport = database.ChildNodes.Item(1).InnerText;
                Dbname = database.ChildNodes.Item(2).InnerText;
                Dblogin = database.ChildNodes.Item(3).InnerText;
                Dbpswd = database.ChildNodes.Item(4).InnerText;
            }
            catch (Exception e)
            {
                ErHandler.OnAdaptorFail(e);
            }
        }
    }
}
