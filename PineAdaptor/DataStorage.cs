using System;
using System.Collections.Generic;

namespace PineAdaptor
{
    /// <summary>
    /// Class that contains methods for retrieving data from pine Data Storages and transform it to descriptors objects.
    /// </summary>
    public class DataStorage
    {

        public static List<T> GetDescriptors<T>()
        {
            List<Dictionary<string, string>> allParameters = new TestTable(typeof(T).Name).GetDataStorageValues();
            List<T> descriptors = new List<T>();
            for (int i = 0; i < allParameters.Count; i++)
            {
                descriptors.Add((T)Activator.CreateInstance(typeof(T), allParameters[i]));
            }
            return descriptors;
        }

        public static List<T> GetDescriptors<T>(string indexes)
        {
            return GetDescriptors<T>(indexes, false);
        }

        public static List<T> GetDescriptors<T>(string indexes, bool allowEmpty)
        {
            List<T> result = new List<T>();
            if (allowEmpty)
            {
                int[] iterationNumbers = { 0 };
                if (indexes != null)
                {
                    iterationNumbers = GetIntArrayFromString(indexes);
                }
                var rows = new TestTable(typeof(T).Name).GetDataStorageValues(iterationNumbers);
                for (int i = 0; i < iterationNumbers.Length; i++)
                {
                    object descriptor = null;
                    try
                    {
                        if (iterationNumbers[i] == 0)
                        {
                            descriptor = (T)Activator.CreateInstance(typeof(T), null);
                        }
                        else
                        {
                            descriptor = (T)Activator.CreateInstance(typeof(T), rows[iterationNumbers[i]]);
                        }
                    }
                    catch (Exception e)
                    {
                        PineSettings.ErHandler.OnAdaptorFail(new Exception("DataStorage exception: " + e.Message
                                    + ". Happened during creating a descriptor: " + typeof(T).Name + " # "
                                    + iterationNumbers[i]));
                    }
                    result.Add((T)descriptor);
                }
            }
            else
            {
                if ((!("0").Equals(indexes)) && (indexes != null))
                {
                    int[] iterationNumbers = GetIntArrayFromString(indexes);
                    var rows = new TestTable(typeof(T).Name).GetDataStorageValues(iterationNumbers);
                    for (int i = 0; i < iterationNumbers.Length; i++)
                    {
                        try
                        {
                            result.Add((T)Activator.CreateInstance(typeof(T), rows[iterationNumbers[i]]));
                        }
                        catch (Exception e)
                        {
                            PineSettings.ErHandler.OnAdaptorFail(new Exception("DataStorage exception: " + e.Message
                                    + ". Happened during creating a descriptor: " + typeof(T).Name + " # "
                                    + iterationNumbers[i]));
                        }
                    }
                }
            }
            return result;
        }

        public static T GetDescriptor<T>(string index)
        {
            List<T> descriptors = GetDescriptors<T>(index, true);
            if (descriptors.Count == 0)
            {
                return (T)Activator.CreateInstance(typeof(T), new Dictionary<string, string>());
            }
            return descriptors[0];
        }

        private static int[] GetIntArrayFromString(String allElements)
        {
            String[] tempArray = allElements.Split(';');
            int[] resultArray = new int[tempArray.Length];
            for (int i = 0; i < tempArray.Length; i++)
            {
                resultArray[i] = int.Parse(tempArray[i]);
            }
            return resultArray;
        }
    }
}
