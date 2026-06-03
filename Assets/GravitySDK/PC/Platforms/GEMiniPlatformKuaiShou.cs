using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GravitySDK.PC.Utils;
using System;

//#define GRAVITY_KUAISHOU_GAME_MODE
//#define KUAISHOU_BELOW_121_VERSION

#if GRAVITY_KUAISHOU_GAME_MODE
using com.kwai.mini.game;
using com.kwai.mini.game.config;
#endif

namespace GravityEngine.Platform
{
    public class GEMiniPlatformKuaiShou : GEPlatformBase
    {
        public static bool activity()
        {
#if GRAVITY_KUAISHOU_GAME_MODE
            return true;
#else
            return false;
#endif
        }


        public override void SetDeviceInfo()
        {
#if GRAVITY_KUAISHOU_GAME_MODE && KUAISHOU_BELOW_121_VERSION
            // 从快手SDK获取属性信息
            KSSystemInfo ks = KSConfig.kSSystemInfo;
            if (ks != null)
            {
                // 提前设置设备属性信息
                GravitySDKDeviceInfo.SetMiniGameDeviceInfo(new GEMiniGameDeviceInfo()
                {
                    version = ks.version, // 快手版本号
                });
            }
            else
            {
                Debug.Log("kuaishou system info is null");
            }
#endif
        }

        public override void OnGravityStartEnd()
        {
#if GRAVITY_KUAISHOU_GAME_MODE
            // 快手小游戏不需要提前获取网络状态，直接使用unity自带的即可
#if KUAISHOU_BELOW_121_VERSION
            KSOutLaunchOption launchOption = KSConfig.kSOutLaunchOption;
#else
            KSOutLaunchOption launchOption = KS.GetLaunchOptionSync();
#endif
            if (launchOption != null)
            {
                GravityEngineAPI.TrackMPLaunch(launchOption.query, launchOption.from);
                GravityEngineAPI.ReportSuperProperties(launchOption.from);
            }
            else
            {
                Debug.Log("kuaishou launch option is null");
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
#if GRAVITY_KUAISHOU_GAME_MODE
#if KUAISHOU_BELOW_121_VERSION
            KSOutLaunchOption launchOption = KSConfig.kSOutLaunchOption;
#else
            KSOutLaunchOption launchOption = KS.GetLaunchOptionSync();
#endif
            var launchQuery = launchOption.query;
            var launchScene = "" + launchOption.from;
            callBack(launchQuery,launchScene);
#endif
        }
    }
}