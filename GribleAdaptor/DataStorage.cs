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
        private static Dictionary<Type, SortedList<int, object>> descriptors = new Dictionary<Type, SortedList<int, object>>();

        /// <summary>
        /// Retrieves data from grible Data Storage and transforms it to descriptors objects.
        /// </summary>
        /// <typeparam name="T">class of the descriptor (i.e. UserInfo)</typeparam>
        /// <param name="indexes">rows indexes of the descriptors to retrieve (i.e. "5", "1;2;2;7"); "0" index in multiple indexes ("1;0;7") is not allowed</param>
        /// <param name="allowEmpty">specifies whether "0" index in multiple indexes (like "1;0;7") is allowed</param>
        /// <returns>List of the descriptors with specified row numbers found in the storage.</returns>
        public static List<T> GetDescriptors<T>(int[] indexes)
        {
            var result = new List<T>();

            if (indexes[0] == 0)
            {
                return result;
            }
            CreateDescriptorsEntryWithEmptyDescriptor<T>();

            SortedList<int, object> map = (SortedList<int, object>)descriptors[typeof(T)];

            var rows = new TestTable(typeof(T).Name).GetDataStorageValues(indexes);
            foreach (var i in indexes)
            {
                if (map.ContainsKey(i))
                {
                    result.Add((T)map[i]);
                    continue;
                }
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
                        + ". Happened during creating a descriptor: " + typeof(T).Name + " # " + i));
                }
                map.Add(i, descriptor);
                result.Add((T)descriptor);
            }
            return result;
        }

        /// <summary>
        /// Retrieves data from grible Data Storage and transforms it to descriptors objects.
        /// </summary>
        /// <typeparam name="T">class of the descriptor (i.e. UserInfo)</typeparam>
        /// <returns>List of all the specified descriptors found in the storage.</returns>
        public static List<T> GetDescriptors<T>()
        {
            int rowsCount = new TestTable(typeof(T).Name).GetDataStorageValues().Count;
            int[] indexes = new int[rowsCount];
            for (int i = 0; i < rowsCount; i++)
            {
                indexes[i] = i + 1;
            }
            return GetDescriptors<T>(indexes);
        }

        private static void CreateDescriptorsEntryWithEmptyDescriptor<T>()
        {
            if (!descriptors.ContainsKey(typeof(T)))
            {
                SortedList<int, object> map = new SortedList<int, object>();
                map.Add(0, CreateEmptyDescriptor<T>());
                descriptors.Add(typeof(T), map);
            }
        }

        private static T CreateEmptyDescriptor<T>()
        {
            object descriptor = null;
            try
            {
                descriptor = (T)Activator.CreateInstance(typeof(T), new Dictionary<string, string>());
            }
            catch (Exception e)
            {
                GribleSettings.ErHandler.OnAdaptorFail(new Exception("DataStorage exception: " + e.Message
                    + ". Happened during creating a descriptor: " + typeof(T).Name + "."));
            }
            return (T)descriptor;
        }

        /// <summary>
        /// Retrieves data from grible Data Storage and transforms it to descriptors objects.
        /// </summary>
        /// <typeparam name="T">class of the descriptor (i.e. UserInfo)</typeparam>
        /// <param name="indexes">rows indexes of the descriptors to retrieve (i.e. "5", "1;2;2;7"); "0" index in multiple indexes ("1;0;7") is not allowed</param>
        /// <param name="allowEmpty">specifies whether "0" index in multiple indexes (like "1;0;7") is allowed</param>
        /// <returns>List of the descriptors with specified row numbers found in the storage.</returns>
        public static List<T> GetDescriptors<T>(string indexes)
        {
            return GetDescriptors<T>(GetIntArrayFromString(indexes));
        }

        /// <summary>
        /// Retrieves data from grible Data Storage and transforms it to the single descriptor object.
        /// </summary>
        /// <typeparam name="T">class of the descriptor (i.e. UserInfo)</typeparam>
        /// <param name="index">row index of the descriptor to retrieve (i.e. "1", "5"); if "0", returns an empty descriptor.</param>
        /// <returns>Descriptor for specified row number found in the storage or an empty (which all properties are null) descriptor.</returns>
        public static T GetDescriptor<T>(string index)
        {
            List<T> list = GetDescriptors<T>(index);
            return (list.Count == 0) ? CreateEmptyDescriptor<T>() : list[0];
        }

        /// <summary>
        /// Retrieves data from grible Data Storage and transforms it to the single descriptor object.
        /// </summary>
        /// <typeparam name="T">class of the descriptor (i.e. UserInfo)</typeparam>
        /// <param name="index">row index of the descriptor to retrieve (i.e. 1, 5); if 0, returns an empty descriptor.</param>
        /// <returns>Descriptor for specified row number found in the storage or an empty (which all properties are null) descriptor.</returns>
        public static T GetDescriptor<T>(int index)
        {
            List<T> list = GetDescriptors<T>(index.ToString());
            return (list.Count == 0) ? CreateEmptyDescriptor<T>() : list[0];
        }

        private static int[] GetIntArrayFromString(string allElements)
        {
            if (("").Equals(allElements) || (allElements == null))
            {
                return new int[] { 0 };
            }
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
