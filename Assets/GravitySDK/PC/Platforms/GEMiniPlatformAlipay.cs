using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GravitySDK.PC.Utils;
using GravityEngine.Utils;
using System;
using GravitySDK.PC.Storage;


//#define GRAVITY_ALIPAY_GAME_MODE


#if GRAVITY_ALIPAY_GAME_MODE
using AlipaySdk;
#endif

namespace GravityEngine.Platform
{
    public class GEMiniPlatformAlipay : GEPlatformBase
    {
        public static bool activity()
        {
#if GRAVITY_ALIPAY_GAME_MODE
            return true;
#else
            return false;
#endif
        }

        public override void SetDeviceInfo()
        {
#if GRAVITY_ALIPAY_GAME_MODE
            // 从支付宝SDK获取属性信息
            var systemInfoStr = AlipaySDK.API.GetSystemInfoSync();
            Dictionary<string, object> systemInfoDict = GE_MiniJson.Deserialize(systemInfoStr);
            var envInfoStr = AlipaySDK.API.GetAlipayEnv();
            Dictionary<string, object> envInfoDict = GE_MiniJson.Deserialize(envInfoStr);
            // 提前设置设备属性信息
            GravitySDKDeviceInfo.SetMiniGameDeviceInfo(new GEMiniGameDeviceInfo()
            {
                SDKVersion = envInfoDict["version"] as string, // 微信SDK版本号
                language = envInfoDict["language"] as string,
                model = systemInfoDict["model"] as string,
                platform = envInfoDict["platform"] as string,
                screenHeight = Convert.ToDouble(envInfoDict["screenHeight"]),
                screenWidth = Convert.ToDouble(envInfoDict["screenWidth"]),
                system = systemInfoDict["system"] as string,
                version = envInfoDict["clientVersion"] as string // 支付宝版本号
            });
#endif
        }

        public override void OnGravityStartEnd()
        {
#if GRAVITY_ALIPAY_GAME_MODE
            // 支付宝小游戏不需要提前获取网络状态，直接使用unity自带的即可
            AlipaySDK.API.GetLaunchOptions((result) =>
            {
                // TEST  {"query":{"key1":"value1","key2":"value2"},"referrerInfo":{}}
                Debug.Log("alipay launch info is " + result);
                if (result != null)
                {
                    Dictionary<string, object> launchOptionDict = GE_MiniJson.Deserialize(result);
                    if (launchOptionDict.TryGetValue("query", out var queryStr))
                    {
                        Dictionary<string, object> queryDict = (Dictionary<string, object>)queryStr;
                        GravityEngineAPI.TrackMPLaunch(GravityEngineAPI.ConvertToDictionary(queryDict), "");
                        GravityEngineAPI.ReportSuperProperties("");
                        Debug.Log("alipay query got");
                    }
                    else
                    {
                        queryStr = "{}";
                        GravityEngineAPI.TrackMPLaunch(new Dictionary<string, string>(), "");
                        Debug.Log("alipay query is null");
                    }
                }
                else
                {
                    Debug.Log("alipay launch option is null");
                    GravityEngineAPI.TrackMPLaunch(new Dictionary<string, string>(), "fake");
                }
            });
#endif
        }

        public override void GetLaunchOptionsSync(Action<Dictionary<string, string>, string> callBack)
        {
#if GRAVITY_ALIPAY_GAME_MODE
            Dictionary<string, string> launchQuery = new Dictionary<string, string>();
            var launchScene = "";
            AlipaySDK.API.GetLaunchOptions((result) =>
            {
                // TEST  {"query":{"key1":"value1","key2":"value2"},"referrerInfo":{}}
                // result = "{\"query\":{\"key1\":\"value1\",\"key2\":\"value2\"},\"referrerInfo\":{}}";
                Debug.Log("alipay launch info is " + result);
                if (result != null)
                {
                    Dictionary<string, object> launchOptionDict = GE_MiniJson.Deserialize(result);
                    if (launchOptionDict.TryGetValue("query", out var queryStr))
                    {
                        Dictionary<string, object> queryDict = (Dictionary<string, object>)queryStr;
                        launchQuery = GravityEngineAPI.ConvertToDictionary(queryDict);
                        Debug.Log("alipay query got");
                        // finally
                        var launchQueryStr = GE_MiniJson.Serialize(launchQuery);
                        GravitySDKFile.SaveData("query_cache", "" + launchQueryStr);
                        GravitySDKFile.SaveData("scene_cache", "" + launchScene);
                        long currentTimestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                        GravitySDKFile.SaveData("latest_cache_timestamp", "" + currentTimestamp);
                    }
                    else
                    {
                        Debug.Log("alipay query is null");
                    }
                }
                else
                {
                    Debug.Log("alipay launch option is null");
                }
                // 异步的需要return，针对支付宝小游戏，暂时没有改造
                return;
            });
            callBack(launchQuery, launchScene);
#endif
        }

    }
}