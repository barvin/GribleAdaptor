using System;
using System.Collections.Generic;
using System.Linq;

namespace GribleAdaptor
{
    /// <summary>
    /// Class that contains methods for retrieving data from grible Data Storages and transform it to descriptors objects.
    /// </summary>
    public class DataStorage
    {
        /// <summary>
        /// Retrieves data from grible Data Storage and transforms it to descriptors objects.
        /// </summary>
        /// <typeparam name="T">class of the descriptor (i.e. UserInfo)</typeparam>
        /// <returns>List of all the specified descriptors found in the storage.</returns>
        public static List<T> GetDescriptors<T>()
        {
            var allParameters = new TestTable(typeof(T).Name).GetDataStorageValues();
            return allParameters.Select(t => (T) Activator.CreateInstance(typeof (T), t)).ToList();
        }

        /// <summary>
        /// Retrieves data from grible Data Storage and transforms it to descriptors objects.
        /// </summary>
        /// <typeparam name="T">class of the descriptor (i.e. UserInfo)</typeparam>
        /// <param name="indexes">rows indexes of the descriptors to retrieve (i.e. "5", "1;2;2;7"); "0" index in multiple indexes ("1;0;7") is not allowed</param>
        /// <returns>List of the descriptors with specified row numbers found in the storage.</returns>
        public static List<T> GetDescriptors<T>(string indexes)
        {
            return GetDescriptors<T>(indexes, false);
        }

        /// <summary>
        /// Retrieves data from grible Data Storage and transforms it to descriptors objects.
        /// </summary>
        /// <typeparam name="T">class of the descriptor (i.e. UserInfo)</typeparam>
        /// <param name="indexes">rows indexes of the descriptors to retrieve (i.e. "5", "1;2;2;7"); "0" index in multiple indexes ("1;0;7") is not allowed</param>
        /// <param name="allowEmpty">specifies whether "0" index in multiple indexes (like "1;0;7") is allowed</param>
        /// <returns>List of the descriptors with specified row numbers found in the storage.</returns>
        public static List<T> GetDescriptors<T>(string indexes, bool allowEmpty)
        {
            var result = new List<T>();
            if (allowEmpty)
            {
                int[] iterationNumbers = { 0 };
                if (indexes != null)
                {
                    iterationNumbers = GetIntArrayFromString(indexes);
                }
                var rows = new TestTable(typeof(T).Name).GetDataStorageValues(iterationNumbers);
                foreach (var i in iterationNumbers)
                {
                    object descriptor = null;
                    try
                    {
                        if (i == 0)
                        {
                            descriptor = (T)Activator.CreateInstance(typeof(T), new Dictionary<string, string>());
                        }
                        else
                        {
                            descriptor = (T)Activator.CreateInstance(typeof(T), rows[i]);
                        }
                    }
                    catch (Exception e)
                    {
                        GribleSettings.ErHandler.OnAdaptorFail(new Exception("DataStorage exception: " + e.Message
                                                                             + ". Happened during creating a descriptor: " + typeof(T).Name + " # "
                                                                             + i));
                    }
                    result.Add((T)descriptor);
                }
            }
            else
            {
                if ((("0").Equals(indexes)) || (indexes == null)) return result;
                var iterationNumbers = GetIntArrayFromString(indexes);
                var rows = new TestTable(typeof(T).Name).GetDataStorageValues(iterationNumbers);
                foreach (var i in iterationNumbers)
                {
                    try
                    {
                        result.Add((T)Activator.CreateInstance(typeof(T), rows[i]));
                    }
                    catch (Exception e)
                    {
                        GribleSettings.ErHandler.OnAdaptorFail(new Exception("DataStorage exception: " + e.Message
                                                                             + ". Happened during creating a descriptor: " + typeof(T).Name + " # "
                                                                             + i));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves data from grible Data Storage and transforms it to the single descriptor object.
        /// </summary>
        /// <typeparam name="T">class of the descriptor (i.e. UserInfo)</typeparam>
        /// <param name="index">row index of the descriptor to retrieve (i.e. "1", "5"); if "0", returns an empty descriptor.</param>
        /// <returns>Descriptor for specified row number found in the storage or an empty (which all properties are null) descriptor.</returns>
        public static T GetDescriptor<T>(string index)
        {
            var descriptors = GetDescriptors<T>(index, true);
            if (descriptors.Count == 0)
            {
                return (T)Activator.CreateInstance(typeof(T), new Dictionary<string, string>());
            }
            return descriptors[0];
        }

        private static int[] GetIntArrayFromString(String allElements)
        {
            var tempArray = allElements.Split(';');
            var resultArray = new int[tempArray.Length];
            for (var i = 0; i < tempArray.Length; i++)
            {
                resultArray[i] = int.Parse(tempArray[i]);
            }
            return resultArray;
        }
    }
}
