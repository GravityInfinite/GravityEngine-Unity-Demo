using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GravitySDK.PC.Utils;
using System;
using GravitySDK.PC.Constant;


//#define GRAVITY_BYTEDANCE_TT_GAME_MODE


#if GRAVITY_BYTEDANCE_TT_GAME_MODE
using TTSDK;
#endif

namespace GravityEngine.Platform
{
    public class GEMiniPlatformByteDanceTT : GEPlatformBase
    {
        public static bool activity()
        {
#if GRAVITY_BYTEDANCE_TT_GAME_MODE
            return true;
#else
            return false;
#endif
        }

        public override void SetDeviceInfo()
        {
#if GRAVITY_BYTEDANCE_TT_GAME_MODE
            // 从抖音SDK获取属性信息
            var systemInfo = TT.GetSystemInfo();
            // 提前设置设备属性信息
            GravitySDKDeviceInfo.SetMiniGameDeviceInfo(new GEMiniGameDeviceInfo()
            {
                SDKVersion = systemInfo.sdkVersion, // 客户端基础库版本
                brand = systemInfo.brand,
                language = systemInfo.language,
                model = systemInfo.model,
                platform = systemInfo.platform,
                screenHeight = systemInfo.screenHeight,
                screenWidth = systemInfo.screenWidth,
                system = systemInfo.system,
                version = systemInfo.hostVersion, // 宿主版本号
            });
#endif
        }

        public override void OnGravityStartEnd()
        {
#if GRAVITY_BYTEDANCE_TT_GAME_MODE
            // 抖音小游戏不需要提前获取网络状态，直接使用unity自带的即可
            LaunchOption launchOptionsSync = TT.GetLaunchOptionsSync();
            if (launchOptionsSync == null)
            {
                GravityEngineAPI.TrackMPLaunch(new Dictionary<string, string>(), "");
                GravityEngineAPI.ReportSuperProperties("");
            }
            else
            {
                GravityEngineAPI.TrackMPLaunch(launchOptionsSync.Query, launchOptionsSync.Scene);
                GravityEngineAPI.ReportSuperProperties(launchOptionsSync.Scene);
            }
#endif
        }

        public override string platformStr()
        {
            return "dy";
        }

        public override void GetLaunchOptionsSync(Action<Dictionary<string, string>, string> callBack)
        {
#if GRAVITY_BYTEDANCE_TT_GAME_MODE
            LaunchOption launchOptionsSync = TT.GetLaunchOptionsSync();
            var launchQuery = new Dictionary<string, string>();
            var launchScene = "";
            if (launchOptionsSync != null)
            {
                launchQuery = launchOptionsSync.Query;
                launchScene = "" + launchOptionsSync.Scene;
            }
            callBack(launchQuery, launchScene);
#endif
        }


        public override void addShowAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {
#if GRAVITY_BYTEDANCE_TT_GAME_MODE
            TT.GetAppLifeCycle().OnShow += objects =>
            {
                GravitySDKLogger.Print("bytedance game on show");
                var query = objects["query"] ?? new Dictionary<string, object>();
                var scene = objects["scene"];
                Dictionary<string, object> properties = propertiesCallback(AUTO_TRACK_EVENTS.MP_SHOW.ToString());
                GravityEngineAPI.TrackMPShow((Dictionary<string, string>)query, "" + scene, properties);
            };
#endif
        }

        public override void addHideAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {
#if GRAVITY_BYTEDANCE_TT_GAME_MODE
            TT.GetAppLifeCycle().OnHide += () =>
            {
                GravitySDKLogger.Print("bytedance game on hide");
                Dictionary<string, object> properties = propertiesCallback(AUTO_TRACK_EVENTS.MP_HIDE.ToString());
                GravityEngineAPI.TrackMPHide(properties);
            };
#endif
        }


        public override void SaveData(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            if (value == null)
            {
                return;
            }
#if GRAVITY_BYTEDANCE_TT_GAME_MODE
            if (value.GetType() == typeof(int))
            {
                TT.PlayerPrefs.SetInt(key, (int)value);
            }
            else if (value.GetType() == typeof(float))
            {
                TT.PlayerPrefs.SetFloat(key, (float)value);
            }
            else if (value.GetType() == typeof(string))
            {
                TT.PlayerPrefs.SetString(key, (string)value);
            }
            TT.PlayerPrefs.Save();
#endif
        }

        public override object GetData(string key, Type type)
        {
#if GRAVITY_BYTEDANCE_TT_GAME_MODE
            if (!string.IsNullOrEmpty(key) && TT.PlayerPrefs.HasKey(key))
            {
                if (type == typeof(int))
                {
                    return TT.PlayerPrefs.GetInt(key);
                }
                else if (type == typeof(float))
                {
                    return TT.PlayerPrefs.GetFloat(key);
                }
                else if (type == typeof(string))
                {
                    return TT.PlayerPrefs.GetString(key);
                }
                TT.PlayerPrefs.Save();
            }
#endif
            return null;
        }
        public override void DeleteData(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
#if GRAVITY_BYTEDANCE_TT_GAME_MODE
            if (TT.PlayerPrefs.HasKey(key))
            {
                TT.PlayerPrefs.DeleteKey(key);
            }
#endif
        }
        public override bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
#if GRAVITY_BYTEDANCE_TT_GAME_MODE
            return TT.PlayerPrefs.HasKey(key);
#else
            return false;
#endif
        }
    }
}