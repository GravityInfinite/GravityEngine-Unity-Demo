using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GravitySDK.PC.Utils;
using System;


//#define GRAVITY_KUAISHOU_WEBGL_GAME_MODE
//#define KUAISHOU_WEBGL_BELOW_TUANJIE_VERSION

#if GRAVITY_KUAISHOU_WEBGL_GAME_MODE
using KSWASM;
#endif

namespace GravityEngine.Platform
{
    public class GEMiniPlatformKuaiShouWebGl : GEPlatformBase
    {
        public static bool activity()
        {
#if GRAVITY_KUAISHOU_WEBGL_GAME_MODE
            return true;
#else
            return false;
#endif
        }

        public override void SetDeviceInfo()
        {
#if GRAVITY_KUAISHOU_WEBGL_GAME_MODE
            // 从快手SDK获取属性信息
            var systemInfoStr = KS.GetSystemInfoSync();
            // 提前设置设备属性信息
            GravitySDKDeviceInfo.SetMiniGameDeviceInfo(new GEMiniGameDeviceInfo()
            {
                SDKVersion = systemInfoStr.version,
                language = systemInfoStr.language,
                model = systemInfoStr.model,
                platform = systemInfoStr.platform,
                screenHeight = systemInfoStr.screenHeight,
                screenWidth = systemInfoStr.screenWidth,
                system = systemInfoStr.system,
            });
#endif
        }

        public override void OnGravityStartEnd()
        {
#if GRAVITY_KUAISHOU_WEBGL_GAME_MODE
#if KUAISHOU_WEBGL_BELOW_TUANJIE_VERSION
            LaunchOptions launchOptions = KS.GetLaunchOptionSync();
#else
            LaunchOptionsGame launchOptions = KS.GetLaunchOptionsSync();
#endif
            if (launchOptions != null)
            {
                GravityEngineAPI.TrackMPLaunch(launchOptions.query, launchOptions.from);
                GravityEngineAPI.ReportSuperProperties(launchOptions.from);
            }
            else
            {
                Debug.Log("kuaishou webgl launch option is null");
                GravityEngineAPI.TrackMPLaunch(new Dictionary<string, string>(), "fake");
            }
#endif
        }

        public override string platformStr()
        {
            return "ks";
        }

        public override void GetLaunchOptionsSync(Action<Dictionary<string, string>, string> callBack)
        {
#if GRAVITY_KUAISHOU_WEBGL_GAME_MODE
#if KUAISHOU_WEBGL_BELOW_TUANJIE_VERSION
            LaunchOptions launchOption = KS.GetLaunchOptionSync();
#else
            LaunchOptionsGame launchOption = KS.GetLaunchOptionsSync();
#endif
            var launchQuery = launchOption.query;
            var launchScene = "" + launchOption.from;
            callBack(launchQuery,launchScene);
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
#if GRAVITY_KUAISHOU_WEBGL_GAME_MODE
            if (value.GetType() == typeof(int))
            {
                KS.StorageSetIntSync(key, (int)value);
            }
            else if (value.GetType() == typeof(float))
            {
                KS.StorageSetFloatSync(key, (float)value);
            }
            else if (value.GetType() == typeof(string))
            {
                KS.StorageSetStringSync(key, (string)value);
            }
#endif
        }

        public override object GetData(string key, Type type)
        {
#if GRAVITY_KUAISHOU_WEBGL_GAME_MODE
            if (!string.IsNullOrEmpty(key) && KS.StorageHasKeySync(key))
            {
                if (type == typeof(int))
                {
                    return KS.StorageGetIntSync(key, 0);
                }
                else if (type == typeof(float))
                {
                    return KS.StorageGetFloatSync(key, 0);
                }
                else if (type == typeof(string))
                {
                    return KS.StorageGetStringSync(key, "");
                }
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
#if GRAVITY_KUAISHOU_WEBGL_GAME_MODE
            if (KS.StorageHasKeySync(key))
            {
                KS.StorageDeleteKeySync(key);
            }
#endif
        }
        public override bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
#if GRAVITY_KUAISHOU_WEBGL_GAME_MODE
            return KS.StorageHasKeySync(key);
#else
            return false;
#endif
        }
    }
}