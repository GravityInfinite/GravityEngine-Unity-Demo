using System;
using GravityEngine.Platform;


namespace GravitySDK.PC.Storage
{
    public class GravitySDKFile
    {
        private static string GetKey(string key)
        {
            return "gravity_" + key;
        }
        
        public static void SaveData(string key, object value)
        {
            if (value == null)
            {
                return;
            }
            key = GetKey(key);// modified by gravity
            if (!string.IsNullOrEmpty(key))
            {
                if (GEPlatformManager.IsGePlatform())
                {
                    GEPlatformManager.SaveData(key, value);
                }
                else
                {
                    if (value.GetType() == typeof(int))
                    {
                        UnityEngine.PlayerPrefs.SetInt(key, (int)value);
                    }
                    else if (value.GetType() == typeof(float))
                    {
                        UnityEngine.PlayerPrefs.SetFloat(key, (float)value);
                    }
                    else if (value.GetType() == typeof(string))
                    {
                        UnityEngine.PlayerPrefs.SetString(key, (string)value);
                    }
                    UnityEngine.PlayerPrefs.Save();
                }
            }
        }

        public static object GetData(string key, Type type)
        {
            key = GetKey(key);// modified by gravity
            if (GEPlatformManager.IsGePlatform())
            {
                return GEPlatformManager.GetData(key, type);
            }
            else
            {
                if (!string.IsNullOrEmpty(key) && UnityEngine.PlayerPrefs.HasKey(key))
                {
                    if (type == typeof(int))
                    {
                        return UnityEngine.PlayerPrefs.GetInt(key);
                    }
                    else if (type == typeof(float))
                    {
                        return UnityEngine.PlayerPrefs.GetFloat(key);
                    }
                    else if (type == typeof(string))
                    {
                        return UnityEngine.PlayerPrefs.GetString(key);
                    }
                    UnityEngine.PlayerPrefs.Save();
                }
            }
            return null;
        }

        public static void DeleteData(string key)
        {
            key = GetKey(key);// modified by gravity
            if (!string.IsNullOrEmpty(key))
            {
                if (GEPlatformManager.IsGePlatform())
                {
                    GEPlatformManager.DeleteData(key);
                }
                else
                {
                    if (UnityEngine.PlayerPrefs.HasKey(key))
                    {
                        UnityEngine.PlayerPrefs.DeleteKey(key);
                    }
                }
            }
        }
        
        public static bool HasKey(string key)
        {
            key = GetKey(key);// modified by gravity
            if (GEPlatformManager.IsGePlatform())
            {
                return GEPlatformManager.HasKey(key);
            }
            else
            {
                return UnityEngine.PlayerPrefs.HasKey(key);
            }
        }
    }
}