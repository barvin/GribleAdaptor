using System;
using System.Collections.Generic;

namespace GribleAdaptor
{
    /// <summary>
    /// Abstract template for descriptors objects. Contains methods for transforming data from HashMap to the descriptor.
    /// </summary>
    public abstract class BaseDescriptor
    {
        private Dictionary<string, string> _data;

        /// <summary>
        /// Is the descriptor empty of not? Empty descriptor could be created by setting "0" value in Grible. All the properties of empty descriptor are null.
        /// </summary>
        public bool IsNotEmpty { get; private set; }

        public BaseDescriptor(Dictionary<string, string> data)
        {
            if ((data != null) && (data.Count > 0))
            {
                _data = data;
                IsNotEmpty = true;
            }
            else
            {
                IsNotEmpty = false;
            }
        }

        public string GetString(string key)
        {
            if (_data == null) return null;
            if (!_data.ContainsKey(key))
            {
                GribleSettings.ErHandler.OnAdaptorFail(new Exception("Descriptor error: key '" + key + "' not found. Dictionary: " + _data + "."));
            }
            return _data[key];
        }

        public bool GetBoolean(string key)
        {
            if (_data == null) return false;
            if (!_data.ContainsKey(key))
            {
                GribleSettings.ErHandler.OnAdaptorFail(new Exception("Descriptor error: key '" + key + "' not found. Dictionary: " + _data + "."));
            }
            return Boolean.Parse(_data[key]);
        }

        public int GetInt(string key)
        {
            if (_data == null) return 0;
            if (!_data.ContainsKey(key))
            {
                GribleSettings.ErHandler.OnAdaptorFail(new Exception("Descriptor error: key '" + key + "' not found. Dictionary: " + _data + "."));
            }
            return Convert.ToInt32(_data[key]);
        }

        public T GetEnum<T>(string key)
        {
            if (_data == null) return default(T);
            if (!_data.ContainsKey(key))
            {
                GribleSettings.ErHandler.OnAdaptorFail(new Exception("Descriptor error: key '" + key + "' not found. Dictionary: " + _data + "."));
            }
            return (T)Enum.Parse(typeof(T), _data[key]);
        }
    }
}
