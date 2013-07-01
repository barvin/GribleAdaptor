using System;
using System.Collections.Generic;

namespace PineAdaptor
{
    /// <summary>
    /// Abstract template for descriptors objects. Contains methods for transforming data from HashMap to the descriptor.
    /// </summary>
    public abstract class BaseDescriptor
    {
        private Dictionary<string, string> data;

        /// <summary>
        /// Is the descriptor empty of not? Empty descriptor could be created by setting "0" value in Pine. All the properties of empty descriptor are null.
        /// </summary>
        public bool IsNotEmpty { get; private set; }

        public BaseDescriptor(Dictionary<string, string> data)
        {
            if ((data != null) && (data.Count == 0))
            {
                this.data = data;
                this.IsNotEmpty = true;
            }
            else
            {
                this.IsNotEmpty = false;
            }
        }

        public string GetString(string key)
        {
            if (data != null)
            {
                if (!data.ContainsKey(key))
                {
                    PineSettings.ErHandler.OnAdaptorFail(new Exception("Descriptor error: key '" + key + "' not found. HashMap: " + data + "."));
                }
                return data[key];
            }
            return null;
        }

        public bool GetBoolean(string key)
        {
            if (data != null)
            {
                if (!data.ContainsKey(key))
                {
                    PineSettings.ErHandler.OnAdaptorFail(new Exception("Descriptor error: key '" + key + "' not found. HashMap: " + data + "."));
                }
                return Boolean.Parse(data[key]);
            }
            return false;
        }

        public int GetInt(string key)
        {
            if (data != null)
            {
                if (!data.ContainsKey(key))
                {
                    PineSettings.ErHandler.OnAdaptorFail(new Exception("Descriptor error: key '" + key + "' not found. HashMap: " + data + "."));
                }
                return Convert.ToInt32(data[key]);
            }
            return 0;
        }
    }
}
