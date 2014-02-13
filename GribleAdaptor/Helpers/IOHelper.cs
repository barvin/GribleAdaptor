using GribleAdaptor.Json;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace GribleAdaptor.Helpers
{
    static class IOHelper
    {
        public static string SearchFile(string dirPath, string fileName)
        {
            var searchResults = Directory.GetFiles(dirPath, fileName, SearchOption.AllDirectories);
            if (searchResults.Length == 0)
            {
                return null;
            }
            else
            {
                return searchResults[0];
            }
        }


        public static string SearchFileByClassName(string dirPath, string className)
        {
            var searchResults = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
            foreach (var path in searchResults)
            {
                TableJson tableJson = ParseTableJson(path);
                if (tableJson.ClassName.Equals(className))
                {
                    return path;
                }
            }
            return null;
        }

        public static TableJson ParseTableJson(string filePath)
        {
            var fileText = System.IO.File.ReadAllText(filePath);
            var tableJson = JsonConvert.DeserializeObject<TableJson>(fileText);
            return tableJson;
        }

    }
}
