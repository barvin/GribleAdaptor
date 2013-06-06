using System;
using System.Collections.Generic;

namespace PineAdaptor
{
    /// <summary>
    /// Class that contains methods for retrieving data from pine Data Storages and transform it to descriptors objects.
    /// </summary>
    public class DataStorage
    {
        /// <summary>
        /// Retrieves data from pine Data Storage and transforms it to descriptors objects.
        /// </summary>
        /// <typeparam name="T">class of the descriptor (i.e. UserInfo)</typeparam>
        /// <returns>List of all the specified descriptors found in the storage.</returns>
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

        /// <summary>
        /// Retrieves data from pine Data Storage and transforms it to descriptors objects.
        /// </summary>
        /// <typeparam name="T">class of the descriptor (i.e. UserInfo)</typeparam>
        /// <param name="indexes">rows indexes of the descriptors to retrieve (i.e. "5", "1;2;2;7"); "0" index in multiple indexes ("1;0;7") is not allowed</param>
        /// <returns>List of the descriptors with specified row numbers found in the storage.</returns>
        public static List<T> GetDescriptors<T>(string indexes)
        {
            return GetDescriptors<T>(indexes, false);
        }

        /// <summary>
        /// Retrieves data from pine Data Storage and transforms it to descriptors objects.
        /// </summary>
        /// <typeparam name="T">class of the descriptor (i.e. UserInfo)</typeparam>
        /// <param name="indexes">rows indexes of the descriptors to retrieve (i.e. "5", "1;2;2;7"); "0" index in multiple indexes ("1;0;7") is not allowed</param>
        /// <param name="allowEmpty">specifies whether "0" index in multiple indexes (like "1;0;7") is allowed</param>
        /// <returns>List of the descriptors with specified row numbers found in the storage.</returns>
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

        /// <summary>
        /// Retrieves data from pine Data Storage and transforms it to the single descriptor object.
        /// </summary>
        /// <typeparam name="T">class of the descriptor (i.e. UserInfo)</typeparam>
        /// <param name="index">row index of the descriptor to retrieve (i.e. "1", "5"); if "0", returns an empty descriptor.</param>
        /// <returns>Descriptor for specified row number found in the storage or an empty (which all properties are null) descriptor.</returns>
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
