using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GravityEngine.Utils;
using GravitySDK.PC.Utils;
using System;


//#define GRAVITY_OPPO_GAME_MODE

#if GRAVITY_OPPO_GAME_MODE
using QGMiniGame;
#endif

namespace GravityEngine.Platform
{
    public class GEMiniPlatformOppo : GEPlatformBase
    {

        public static bool activity()
        {
#if GRAVITY_OPPO_GAME_MODE
            return true;
#else
            return false;
#endif
        }

        public override void SetDeviceInfo()
        {
#if GRAVITY_OPPO_GAME_MODE
            // 从OPPO快游戏SDK获取属性信息
            QG.GetSystemInfo((systemInfo) =>
            {
                GravitySDKDeviceInfo.SetMiniGameDeviceInfo(new GEMiniGameDeviceInfo()
                {
                    SDKVersion = systemInfo.data.COREVersion, // 抖音SDK版本号
                        brand = systemInfo.data.brand,
                    language = systemInfo.data.language,
                    model = systemInfo.data.model,
                    platform = systemInfo.data.platformVersionName,
                        // screenHeight = systemInfo.data.screenHeight,
                        // screenWidth = systemInfo.data.screenWidth,
                        system = systemInfo.data.system,
                });
            });
#endif
        }

        public override void OnGravityStartEnd()
        {
#if GRAVITY_OPPO_GAME_MODE
            var launchStr = QG.GetEnterOptionsSync();
            Debug.Log("oppo quickgame launch info is " + launchStr);
            if (launchStr != null)
            {
                Dictionary<string, object> launchOptionDict = GE_MiniJson.Deserialize(launchStr);
                if (launchOptionDict.TryGetValue("query", out var queryStr))
                {
                    Dictionary<string, object> queryDict = (Dictionary<string, object>)queryStr;
                    GravityEngineAPI.TrackMPLaunch(GravityEngineAPI.ConvertToDictionary(queryDict), "");
                    GravityEngineAPI.ReportSuperProperties("");
                    Debug.Log("oppo quickgame query got");
                }
                else
                {
                    queryStr = "{}";
                    GravityEngineAPI.TrackMPLaunch(new Dictionary<string, string>(), "");
                    Debug.Log("oppo quickgame query is null");
                }
            }
            else
            {
                Debug.Log("oppo quickgame launch option is null");
                GravityEngineAPI.TrackMPLaunch(new Dictionary<string, string>(), "fake");
            }
#endif
        }


        public override void GetLaunchOptionsSync(Action<Dictionary<string, string>, string> callBack)
        {
#if GRAVITY_OPPO_GAME_MODE
            Dictionary<string, string> launchQuery = new Dictionary<string, string>();
            var launchStr = QG.GetEnterOptionsSync();
            if (launchStr != null)
            {
                Dictionary<string, object> launchOptionDict = GE_MiniJson.Deserialize(launchStr);
                if (launchOptionDict.TryGetValue("query", out var queryStr))
                {
                    Dictionary<string, object> queryDict = (Dictionary<string, object>)queryStr;
                    launchQuery = GravityEngineAPI.ConvertToDictionary(queryDict);
                    Debug.Log("oppo quickgame query got");
                }
                else
                {
                    Debug.Log("oppo quickgame query is null");
                }
            }
            callBack(launchQuery,"");
#endif
        }



        public override void getDeviceInfo(Action<Dictionary<string, string>> callBack)
        {
#if GRAVITY_OPPO_GAME_MODE
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
                            string platformVersionCode = msg.data.platformVersionCode; // Version
                            string screenHeight = msg.data.screenHeight; // 屏幕高度
                            string screenWidth = msg.data.screenWidth; // 屏幕宽度
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
#if GRAVITY_OPPO_GAME_MODE
            QG.StorageSetItem(key, (string)value);
#endif
        }

        public override object GetData(string key, Type type)
        {
#if GRAVITY_OPPO_GAME_MODE
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
#if GRAVITY_OPPO_GAME_MODE
            QG.StorageRemoveItem(key);
#endif
        }
        public override bool HasKey(string key)
        {
#if GRAVITY_OPPO_GAME_MODE
            return true;
#else
            return false;
#endif
        }
    }
}