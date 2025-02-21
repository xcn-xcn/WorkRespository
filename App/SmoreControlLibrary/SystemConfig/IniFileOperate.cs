﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SmoreControlLibrary
{
    class IniFileOperate
    {
        public const int MaxSectionSize = 1310680; 
        private string m_path; 
        private static Dictionary<string, object> _locks = new Dictionary<string, object>();

        public IniFileOperate(string path)
        {
            m_path = System.IO.Path.GetFullPath(path);
            if (!_locks.ContainsKey(m_path))
            {
                _locks.Add(m_path, new object());
            }
        }

        public string Path
        {
            get
            {
                return m_path;
            }
        }

        [System.Security.SuppressUnmanagedCodeSecurity] 
        private static class NativeMethods 
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern int GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer,
                                                                   uint nSize,
                                                                   string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern uint GetPrivateProfileString(string lpAppName,
                                                              string lpKeyName,
                                                              string lpDefault,
                                                              StringBuilder lpReturnedString,
                                                              int nSize,
                                                              string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern uint GetPrivateProfileString(string lpAppName,
                                                              string lpKeyName,
                                                              string lpDefault,
                                                              [In, Out] char[] lpReturnedString,
                                                              int nSize,
                                                              string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern int GetPrivateProfileString(string lpAppName,
                                                             string lpKeyName,
                                                             string lpDefault,
                                                             IntPtr lpReturnedString,
                                                             uint nSize,
                                                             string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern int GetPrivateProfileInt(string lpAppName,
                                                          string lpKeyName,
                                                          int lpDefault,
                                                          string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern int GetPrivateProfileSection(string lpAppName,
                                                              IntPtr lpReturnedString,
                                                              uint nSize,
                                                              string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool WritePrivateProfileString(string lpAppName,
                                                                string lpKeyName,
                                                                string lpString,
                                                                string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool WritePrivateProfileSection(string lpAppName,
                                                                 string lpstring,
                                                                 string lpFileName);

        }

        public string GetString(string sectionName, string keyName, string defaultValue)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");
            if (keyName == null)
                throw new ArgumentNullException("keyName");
            lock (_locks[m_path])
            {
                StringBuilder retval = new StringBuilder(IniFileOperate.MaxSectionSize);

                NativeMethods.GetPrivateProfileString(sectionName,
                                                      keyName,
                                                      defaultValue,
                                                      retval,
                                                      IniFileOperate.MaxSectionSize,
                                                      m_path);
                return retval.ToString();
            }
        }

        public int GetInt16(string sectionName, string keyName, short defaultValue)
        {
            int retval = GetInt32(sectionName, keyName, defaultValue);

            return Convert.ToInt16(retval);
        }

        public int GetInt32(string sectionName, string keyName, int defaultValue)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");
            if (keyName == null)
                throw new ArgumentNullException("keyName");
            lock (_locks[m_path])
            {
                return NativeMethods.GetPrivateProfileInt(sectionName, keyName, defaultValue, m_path);
            }
        }

        public double GetDouble(string sectionName, string keyName, double defaultValue)
        {
            string retval = GetString(sectionName, keyName, "");

            if (retval == null || retval.Length == 0)
            {
                return defaultValue;
            }

            return Convert.ToDouble(retval, CultureInfo.InvariantCulture);
        }

        public List<KeyValuePair<string, string>> GetSectionValuesAsList(string sectionName)
        {
            List<KeyValuePair<string, string>> retval;
            string[] keyValuePairs;
            string key, value;
            int equalSignPos;
            if (sectionName == null) throw new ArgumentNullException("sectionName"); 
            IntPtr ptr = Marshal.AllocCoTaskMem(IniFileOperate.MaxSectionSize); 
            try
            {
                lock (_locks[m_path])
                {
                    int len = NativeMethods.GetPrivateProfileSection(sectionName, ptr, IniFileOperate.MaxSectionSize, m_path);
                    keyValuePairs = ConvertNullSeperatedStringToStringArray(ptr, len); 
                }
            }
            finally
            {    
                Marshal.FreeCoTaskMem(ptr); 
            }
            retval = new List<KeyValuePair<string, string>>(keyValuePairs.Length);
            for (int i = 0; i < keyValuePairs.Length; ++i)
            {
                equalSignPos = keyValuePairs[i].IndexOf('='); 
                key = keyValuePairs[i].Substring(0, equalSignPos);
                value = keyValuePairs[i].Substring(equalSignPos + 1, keyValuePairs[i].Length - equalSignPos - 1);
                retval.Add(new KeyValuePair<string, string>(key, value));
            }
            return retval;
        }

        public Dictionary<string, string> GetSectionValues(string sectionName)
        {
            List<KeyValuePair<string, string>> keyValuePairs;
            Dictionary<string, string> retval;
            keyValuePairs = GetSectionValuesAsList(sectionName);
            retval = new Dictionary<string, string>(keyValuePairs.Count);
            foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
            {
                if (!retval.ContainsKey(keyValuePair.Key)) 
                {
                    retval.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            return retval;
        }

        public List<string> GetValues(string sectionName)
        {
            List<KeyValuePair<string, string>> keyValuePairs;
            Dictionary<string, string> retval;
            List<string> values=new List<string>();
            keyValuePairs = GetSectionValuesAsList(sectionName);
            retval = new Dictionary<string, string>(keyValuePairs.Count);
            foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
            {
                {
                    values.Add(keyValuePair.Value);
                }
            }
            return values;
        }

        public string[] GetKeyNames(string sectionName)
        {
            int len;
            string[] retval;
            if (sectionName == null) throw new ArgumentNullException("sectionName");
            IntPtr ptr = Marshal.AllocCoTaskMem(IniFileOperate.MaxSectionSize);
            try
            {
                lock (_locks[m_path])
                {
                    len = NativeMethods.GetPrivateProfileString(sectionName, null, null, ptr, IniFileOperate.MaxSectionSize, m_path);
                    retval = ConvertNullSeperatedStringToStringArray(ptr, len);
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(ptr);
            }
            return retval;
        }

        public string[] GetSectionNames()
        {
            string[] retval;
            int len;
            IntPtr ptr = Marshal.AllocCoTaskMem(IniFileOperate.MaxSectionSize);
            try
            {
                lock (_locks[m_path])
                {
                    len = NativeMethods.GetPrivateProfileSectionNames(ptr, IniFileOperate.MaxSectionSize, m_path);
                    retval = ConvertNullSeperatedStringToStringArray(ptr, len);
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(ptr);
            }
            return retval;
        }

        private static string[] ConvertNullSeperatedStringToStringArray(IntPtr ptr, int valLength)
        {
            string[] retval;
            if (valLength == 0)
            {
                retval = new string[0];
            }
            else
            {
                string buff = Marshal.PtrToStringAuto(ptr, valLength - 1);
                retval = buff.Split('\0');
            }
            return retval;
        }

        private void WriteValueInternal(string sectionName, string keyName, string value)
        {
            lock (_locks[m_path])
            {
                if (!NativeMethods.WritePrivateProfileString(sectionName, keyName, value, m_path))
                {
                    throw new System.ComponentModel.Win32Exception();
                }
            }
        }

        public void WriteValue(string sectionName)
        {
            WriteValueInternal(sectionName, "a", "a");
            WriteValueInternal(sectionName, "a", null);
        }

        public void WriteValue(string sectionName, string keyName, string value)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");

            if (keyName == null)
                throw new ArgumentNullException("keyName");

            if (value == null)
                throw new ArgumentNullException("value");

            WriteValueInternal(sectionName, keyName, value);
        }

        public void WriteValue(string sectionName, string keyName, short value)
        {
            WriteValue(sectionName, keyName, (int)value);
        }

        public void WriteValue(string sectionName, string keyName, int value)
        {
            WriteValue(sectionName, keyName, value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteValue(string sectionName, string keyName, float value)
        {
            WriteValue(sectionName, keyName, value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteValue(string sectionName, string keyName, double value)
        {
            WriteValue(sectionName, keyName, value.ToString(CultureInfo.InvariantCulture));
        }

        public void DeleteKey(string sectionName, string keyName)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");
            if (keyName == null)
                throw new ArgumentNullException("keyName");

            ArrayList list = ArrayList.Adapter(GetSectionNames());
            if (!list.Contains(sectionName)) 
            {
                return;
            }
            list = ArrayList.Adapter(GetKeyNames(sectionName));
            if (!list.Contains(keyName))
            {
                return;
            }
            WriteValueInternal(sectionName, keyName, null);
        }

        public void DeleteSection(string sectionName)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");

            ArrayList list = ArrayList.Adapter(GetSectionNames());
            if (list.Contains(sectionName))
            {
                WriteValueInternal(sectionName, null, null);
            }
        }
    }
}
