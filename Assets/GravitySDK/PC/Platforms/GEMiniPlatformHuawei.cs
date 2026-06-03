using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GravitySDK.PC.Utils;
using GravityEngine.Utils;
using System;

//#define GRAVITY_HUAWEI_GAME_MODE

#if GRAVITY_HUAWEI_GAME_MODE
using HWWASM;
#endif

namespace GravityEngine.Platform
{
    public class GEMiniPlatformHuawei : GEPlatformBase
    {
        public static bool activity()
        {
#if GRAVITY_HUAWEI_GAME_MODE
            return true;
#else
            return false;
#endif
        }

        public override void SetDeviceInfo()
        {
#if GRAVITY_HUAWEI_GAME_MODE
            GetSystemInfoSuccessResult systemInfo = QG.GetSystemInfoSync();
            GravitySDKDeviceInfo.SetMiniGameDeviceInfo(new GEMiniGameDeviceInfo()
            {
                SDKVersion = systemInfo.COREVersion, // 渲染引擎版本号
                brand = systemInfo.brand,
                language = systemInfo.language,
                model = systemInfo.model,
                platform = systemInfo.platform,
                screenHeight = systemInfo.screenHeight,
                screenWidth = systemInfo.screenWidth,
                system = systemInfo.system,
                version = systemInfo.version,
            });
#endif
        }

        public override void OnGravityStartEnd()
        {
#if GRAVITY_HUAWEI_GAME_MODE
            GetLaunchOptionsSyncResult result = QG.GetLaunchOptionsSync();
            if (result != null)
            {
                Dictionary<string, object> queryDict = GE_MiniJson.Deserialize(result.query);
                GravityEngineAPI.TrackMPLaunch(GravityEngineAPI.ConvertToDictionary(queryDict), "");
                GravityEngineAPI.ReportSuperProperties("");
                Debug.Log("huawei quickgame query got");
            }
            else
            {
                Debug.Log("huawei quickgame launch option is null");
                GravityEngineAPI.TrackMPLaunch(new Dictionary<string, string>(), "fake");
            }
#endif
        }


        public override void GetLaunchOptionsSync(Action<Dictionary<string, string>, string> callBack)
        {
#if GRAVITY_HUAWEI_GAME_MODE
            Dictionary<string, string> launchQuery = new Dictionary<string, string>();
            GetLaunchOptionsSyncResult result = QG.GetLaunchOptionsSync();
            Debug.Log("huawei quickgame launch info is " + result);
            if (result != null)
            {
                Dictionary<string, object> queryDict = GE_MiniJson.Deserialize(result.query);
                launchQuery = GravityEngineAPI.ConvertToDictionary(queryDict);
                Debug.Log("huawei quickgame query got");
            }
            callBack(launchQuery,"");
#endif
        }


        public override void getDeviceInfo(Action<Dictionary<string, string>> callBack)
        {
#if GRAVITY_HUAWEI_GAME_MODE
            string deviceId = "";
            QG.GetOAID(new GetOAIDOption()
            {
                success = res =>
                {
                    // 成功获取oaid
                    deviceId = res.oaid; //设备唯一标识
                },
                fail = () =>
                {
                    Debug.Log("QG.GetOAID failed");
                },
                complete = () =>
                {
                    QG.GetSystemInfo(new GetSystemInfoOption()
                    {
                        success = msg =>
                        {
                            string brand = msg.brand; // 手机品牌
                            string model = msg.model; // 手机型号
                            string platformVersionName = msg.platformVersionName; // 客户端平台
                            string platformVersionCode = Convert.ToString(msg.platformVersionCode); // Version
                            string COREVersion = msg.COREVersion; // 版本号

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
                        fail = () =>
                        {
                            Debug.Log("QG.GetSystemInfo fail");
                            var deviceInfo = new Dictionary<string, string>()
                            {
                                {"os_name", "android"},
                                {"android_id", deviceId},
                                {"imei", deviceId},
                                {"oaid", deviceId},
                            };
                            callBack(deviceInfo);
                        },
                        complete = () =>
                        {

                        }
                    });
                }
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
#if GRAVITY_HUAWEI_GAME_MODE
            QG.LocalStorage.SetItem(key, value.ToString());
#endif
        }

        public override object GetData(string key, Type type)
        {
#if GRAVITY_HUAWEI_GAME_MODE
            if (!string.IsNullOrEmpty(key))
            {
                return QG.LocalStorage.GetItem(key);
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
#if GRAVITY_HUAWEI_GAME_MODE
            QG.LocalStorage.RemoveItem(key);
#endif
        }
        public override bool HasKey(string key)
        {
#if GRAVITY_HUAWEI_GAME_MODE
            return true;
#else
            return false;
#endif
        }
    }
}