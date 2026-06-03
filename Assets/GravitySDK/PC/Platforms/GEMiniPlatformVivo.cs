using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GravityEngine.Utils;
using GravitySDK.PC.Utils;
using System;
using GravitySDK.PC.Constant;
using System.Runtime.InteropServices;


//#define GRAVITY_VIVO_GAME_MODE

#if GRAVITY_VIVO_GAME_MODE
using QGMiniGame;
#endif

namespace GravityEngine.Platform
{
    public class GEMiniPlatformVivo : GEPlatformBase
    {

        public static bool activity()
        {
#if GRAVITY_VIVO_GAME_MODE
            return true;
#else
            return false;
#endif
        }

        public override void SetDeviceInfo()
        {
#if GRAVITY_VIVO_GAME_MODE
            // 从VIVO快游戏SDK获取属性信息
            QG.GetSystemInfo(
                (systemInfo) => {
                    GravitySDKDeviceInfo.SetMiniGameDeviceInfo(new GEMiniGameDeviceInfo()
                    {
                        SDKVersion = systemInfo.data.COREVersion,
                        brand = systemInfo.data.brand,
                        language = systemInfo.data.language,
                        model = systemInfo.data.model,
                        platform = systemInfo.data.platformVersionName,
                        screenHeight = systemInfo.data.screenHeight,
                        screenWidth = systemInfo.data.screenWidth,
                        system = systemInfo.data.system,
                    });
                },
                (msg) => { Debug.Log("QG.GetSystemInfo fail = " + msg.errMsg); }
            );
#endif
        }

        public override void OnGravityStartEnd()
        {
#if GRAVITY_VIVO_GAME_MODE
            GravityEngineAPI.TrackMPLaunch(new Dictionary<string, string>(), "");
#endif
        }


        public override void GetLaunchOptionsSync(Action<Dictionary<string, string>, string> callBack)
        {
#if GRAVITY_VIVO_GAME_MODE
            Dictionary<string, string> launchQuery = new Dictionary<string, string>();
            callBack(launchQuery,"");
#endif
        }





        public override void addShowAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {
#if GRAVITY_VIVO_GAME_MODE
            QG.OnShow((result =>
            {
                GravitySDKLogger.Print("VIVO quickgame on show");
                Dictionary<string, object> properties = propertiesCallback(AUTO_TRACK_EVENTS.MP_SHOW.ToString());
                GravityEngineAPI.TrackMPShow(new Dictionary<string, string> { { "userEntry",result.query.userEntry } }, "" + result.type, properties);
            }));
#endif
        }

        public override void addHideAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {
#if GRAVITY_VIVO_GAME_MODE
            QG.OnHide((result =>
            {
                GravitySDKLogger.Print("VIVO quickgame on hide");
                Dictionary<string, object> properties = propertiesCallback(AUTO_TRACK_EVENTS.MP_HIDE.ToString());
                GravityEngineAPI.TrackMPHide(properties);
            }));
#endif
        }

        public override void getDeviceInfo(Action<Dictionary<string, string>> callBack)
        {
#if GRAVITY_VIVO_GAME_MODE
            string deviceId = "";
            QG.GetDeviceId(
                (success) =>
                {
                    deviceId = success.data.deviceId; //设备唯一标识
                },
                (fail) =>
                {
                    Debug.Log("QG.GetDeviceId fail = " + JsonUtility.ToJson(fail));
                },
                (complete) =>
                {
                    Debug.Log("gravity-engine current deviceId is " + deviceId);
                    QG.GetSystemInfo((msg) =>
                        {
                            string brand = msg.data.brand; // 手机品牌
                            string language = msg.data.language; // 系统语言
                            string model = msg.data.model; // 手机型号
                            string platformVersionName = msg.data.platformVersionName; // 客户端平台
                            string platformVersionCode = msg.data.platformVersionCode+""; // Version
                            string screenHeight = msg.data.screenHeight+""; // 屏幕高度
                            string screenWidth = msg.data.screenWidth + ""; // 屏幕宽度
                            string system = msg.data.system; // 系统版本
                            string COREVersion = msg.data.COREVersion; // 版本号
                            
                            var deviceInfo = new Dictionary<string, string>()
                            {
                                {"os_name", "android"},
                                {"android_id", deviceId},
                                {"imei", deviceId},
                                {"oaid", deviceId},
                                {"rom", platformVersionName},
                                {"rom_version", platformVersionCode},
                                {"brand", brand},
                                {"model", model},
                                {"android_version", COREVersion}
                            };
                            callBack(deviceInfo);
                        },
                        (err) =>
                        {
                            Debug.Log("QG.GetSystemInfo fail = " + JsonUtility.ToJson(err));
                            var deviceInfo = new Dictionary<string, string>()
                            {
                                {"os_name", "android"},
                                {"android_id", deviceId},
                                {"imei", deviceId},
                                {"oaid", deviceId},
                            };
                            callBack(deviceInfo);
                        });
                });
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
#if GRAVITY_VIVO_GAME_MODE
            QG.StorageSetItem(key, (string)value);
#endif
        }

        public override object GetData(string key, Type type)
        {
#if GRAVITY_VIVO_GAME_MODE
            if (!string.IsNullOrEmpty(key))
            {
                var v = QG.StorageGetItem(key);
                if (type == typeof(string))
                {
                    return v;
                }

                if (string.IsNullOrEmpty(v))
                {
                    return null;
                }

                if (type == typeof(int))
                {
                    return int.Parse(v);
                }

                if (type == typeof(double))
                {
                    return double.Parse(v);
                }

                if (type == typeof(float))
                {
                    return float.Parse(v);
                }

                if (type == typeof(bool))
                {
                    return bool.Parse(v);
                }
                return v;
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
#if GRAVITY_VIVO_GAME_MODE
            QG.StorageRemoveItem(key);
#endif
        }
        public override bool HasKey(string key)
        {
#if GRAVITY_VIVO_GAME_MODE
            return true;
#else
            return false;
#endif
        }
    }
}