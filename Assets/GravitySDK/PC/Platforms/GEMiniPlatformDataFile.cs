using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using GravityEngine.Utils;

namespace GravityEngine.Platform
{
    public class GEMiniPlatformDataFile
    {
        //云游戏SDK没有本地缓存的api，需要自定义实现本地缓存
        private string dataInfoFileName = "/GravityEngine.json";
        private Dictionary<string, object> dataInfoDic = new Dictionary<string, object>();

        private string dataInfoFilePath()
        {
#if UNITY_EDITOR
            return Application.persistentDataPath + dataInfoFileName;
#endif
            return "data/data/" + Application.identifier + "/files" + dataInfoFileName;
        }

        private void ReadDataCacheDic()
        {
            string path = dataInfoFilePath();
            if (File.Exists(path))
            {
                string jsonString = File.ReadAllText(path);
                Dictionary<string, object> loadedDic = GE_MiniJson.Deserialize(jsonString);
                if (loadedDic != null)
                {
                    foreach (var entry in loadedDic)
                    {
                        dataInfoDic[entry.Key] = entry.Value;
                    }
                }
            }
        }

        private void SaveDataToPath()
        {
            try
            {
                string jsonString = GE_MiniJson.Serialize(dataInfoDic);
                File.WriteAllText(dataInfoFilePath(), jsonString);
            }
            catch (Exception e)
            {

            }
        }

        GEMiniPlatformDataFile()
        {
            ReadDataCacheDic();
        }

        private static GEMiniPlatformDataFile mMiniCloudFlile = new GEMiniPlatformDataFile();


        public static void SaveData(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            if (value == null)
            {
                return;
            }
            mMiniCloudFlile.dataInfoDic[key] = value;
            mMiniCloudFlile.SaveDataToPath();
        }

        public static object GetData(string key, Type type)
        {
            if (!string.IsNullOrEmpty(key) && mMiniCloudFlile.dataInfoDic.ContainsKey(key))
            {
                if (type == typeof(int))
                {
                    return int.Parse(mMiniCloudFlile.dataInfoDic[key].ToString());
                }
                else if (type == typeof(float))
                {
                    return float.Parse(mMiniCloudFlile.dataInfoDic[key].ToString());
                }
                else if (type == typeof(string))
                {
                    return mMiniCloudFlile.dataInfoDic[key].ToString();
                }
            }
            return null;
        }
        public static void DeleteData(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            if (mMiniCloudFlile.dataInfoDic.ContainsKey(key))
            {
                mMiniCloudFlile.dataInfoDic.Remove(key);
                mMiniCloudFlile.SaveDataToPath();
            }
        }
        public static bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            return mMiniCloudFlile.dataInfoDic.ContainsKey(key);
        }
    }
}
