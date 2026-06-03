using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GravitySDK.PC.Utils;
using GravitySDK.PC.GravityTurbo;
using System;
using GravitySDK.PC.Constant;


//#define GRAVITY_TAPTAP_GAME_MODE


#if GRAVITY_TAPTAP_GAME_MODE
using TapTapMiniGame;
#endif

namespace GravityEngine.Platform
{
    public class GEMiniPlatformTapTap : GEPlatformBase
    {
        public static bool activity()
        {
#if GRAVITY_TAPTAP_GAME_MODE
            return true;
#else
            return false;
#endif
        }

        public override void SetDeviceInfo()
        {
#if GRAVITY_TAPTAP_GAME_MODE
            // 从支taptapSDK获取属性信息
            // 使用GetSystemInfo获取平台信息
            GetSystemInfoOption option = new GetSystemInfoOption();
            option.success = (res) =>
            {
                GravitySDKDeviceInfo.SetMiniGameDeviceInfo(new GEMiniGameDeviceInfo()
                {
                    SDKVersion = res.SDKVersion, // 客户端基础库版本
                    language = res.language,
                    model = res.model,
                    platform = res.platform,
                    screenHeight = res.screenHeight,
                    screenWidth = res.screenWidth,
                    system = res.system,
                    version = res.version // TapTap 版本号
                });
            };
            Tap.GetSystemInfo(option);
#endif
        }

        public override void OnGravityStartEnd()
        {
#if GRAVITY_TAPTAP_GAME_MODE
            //https://developer.taptap.cn/minigameapidoc/dev/api/base/app/life-cycle/tap.getEnterOptionsSync/
            EnterOptionsGame launchOptionsSync = Tap.GetEnterOptionsSync();
            if (launchOptionsSync == null)
            {
                GravityEngineAPI.TrackMPLaunch(new Dictionary<string, string>(), "");
                GravityEngineAPI.ReportSuperProperties("");
            }
            else
            {
                GravityEngineAPI.TrackMPLaunch(launchOptionsSync.query, "" + launchOptionsSync.scene);
                GravityEngineAPI.ReportSuperProperties(""+launchOptionsSync.scene);
            }
#endif
        }

        public override void GetLaunchOptionsSync(Action<Dictionary<string, string>, string> callBack)
        {
#if GRAVITY_TAPTAP_GAME_MODE
            var launchQuery = new Dictionary<string, string>();
            var launchScene = "";

            EnterOptionsGame launchOptionsSync = Tap.GetEnterOptionsSync();
            // 从 1.7.0 版本开始支持
            if (launchOptionsSync.query != null) {
                launchQuery = launchOptionsSync.query;
                launchScene = "" + launchOptionsSync.scene;
            }
            callBack(launchQuery, launchScene);
#endif
        }


        public override void addShowAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {
#if GRAVITY_TAPTAP_GAME_MODE
            Tap.OnShow((res) => {
                // 从 1.7.0 版本开始支持
                if (res.query != null) {
                    GravitySDKLogger.Print("taptap game on show");
                    Dictionary<string, object> properties = propertiesCallback(AUTO_TRACK_EVENTS.MP_SHOW.ToString());
                    GravityEngineAPI.TrackMPShow(res.query, "" + res.scene,properties);
                }
            });
#endif
        }

        public override void addHideAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {
#if GRAVITY_TAPTAP_GAME_MODE
            Tap.OnHide((res) => {
                GravitySDKLogger.Print("taptap game on hide");
                Dictionary<string, object> properties = propertiesCallback(AUTO_TRACK_EVENTS.MP_HIDE.ToString());
                GravityEngineAPI.TrackMPHide(properties);
            });
#endif
        }
        /*
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
#if GRAVITY_TAPTAP_GAME_MODE
          Tap.SetStorageSync(key, value);
#endif
      }

      public override object GetData(string key, Type type)
      {
#if GRAVITY_TAPTAP_GAME_MODE
            if (!string.IsNullOrEmpty(key))
                  {
      目前这个方法获取到的没有具体的数据，没找到对应文档，demo也没有说明,返回的GetStorageInfoSyncOption类型定义如下

public class GetStorageInfoSyncOption
{
  public double currentSize;

  public string[] keys;

  public double limitSize;
}

        //return Tap.GetStorageInfoSync();
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
#if GRAVITY_TAPTAP_GAME_MODE
                    Tap.RemoveStorageSync(key);
#endif
        }
        public override bool HasKey(string key)
        {
#if GRAVITY_TAPTAP_GAME_MODE
                    return true;
#else
            return false;
#endif
        }
        */
    }
}