using System;
using System.Collections;
using System.Collections.Generic;
using GravitySDK.PC.Utils;
using UnityEngine;


//#define GRAVITY_XIAOMI_GAME_MODE

#if GRAVITY_XIAOMI_GAME_MODE
using mi;
#endif

namespace GravityEngine.Platform
{
    public class GEMiniPlatformXiaomi : GEPlatformBase
    {
        public static bool activity()
        {
#if GRAVITY_XIAOMI_GAME_MODE
            return true;
#else
            return false;
#endif
        }

        public override void SetDeviceInfo()
        {
#if GRAVITY_XIAOMI_GAME_MODE
            bool successSystemInfo;
            if (SyncHelper.GetSystemInfoSync(out successSystemInfo, out outData))
            {
                if (successSystemInfo)
                {
                    GravitySDKDeviceInfo.SetMiniGameDeviceInfo(new GEMiniGameDeviceInfo()
                    {
                        brand = outData.brand,
                        language = outData.language,
                        model = outData.model,
                        screenHeight = outData.screenHeight,
                        screenWidth = outData.screenWidth,
                        system = outData.system,
                        version = outData.platformVersionName,
                    });
                }
            }
            else
            {
                Debug.Log("Failed to get system info.");
            }
#endif
        }

        public override void OnGravityStartEnd()
        {
#if GRAVITY_XIAOMI_GAME_MODE
            Dictionary<string, string> launchQuery = new Dictionary<string, string>();
            QGLaunchInfo launchInfo = MiGetOptions.Instance.GetLaunchOptionsSync();
            var launchScene = "" + launchInfo.scene;
            if (launchInfo.query.type != null)
            {
                launchQuery["type"] = launchInfo.query.type;
            }

            GravityEngineAPI.TrackMPLaunch(launchQuery, launchScene);
            GravityEngineAPI.ReportSuperProperties(launchScene);
#endif
        }


        public override void GetLaunchOptionsSync(Action<Dictionary<string, string>, string> callBack)
        {
#if GRAVITY_XIAOMI_GAME_MODE
            Dictionary<string, string> launchQuery = new Dictionary<string, string>();
            QGLaunchInfo launchInfo = MiGetOptions.Instance.GetLaunchOptionsSync();
            var launchScene = "" + launchInfo.scene;
            if (launchInfo.query.type != null)
            {
                launchQuery["type"] = launchInfo.query.type;
            }
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
#if GRAVITY_XIAOMI_GAME_MODE
            if (value.GetType() == typeof(int))
            {
                MiBridge.Instance.SetKVInt(key, (int)value);
            }
            else if (value.GetType() == typeof(float))
            {
                MiBridge.Instance.SetKVFloat(key, (float)value);
            }
            else if (value.GetType() == typeof(string))
            {
                MiBridge.Instance.SetKVString(key, (string)value);
            }
#endif
        }

        public override object GetData(string key, Type type)
        {
#if GRAVITY_XIAOMI_GAME_MODE
            if (!string.IsNullOrEmpty(key) && MiBridge.Instance.HasKV(key))
            {
                if (type == typeof(int))
                {
                    return MiBridge.Instance.GetKVInt(key, 0);
                }
                else if (type == typeof(float))
                {
                    return MiBridge.Instance.GetKVFloat(key, 0);
                }
                else if (type == typeof(string))
                {
                    return MiBridge.Instance.GetKVString(key, "");
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
#if GRAVITY_XIAOMI_GAME_MODE
            MiBridge.Instance.DeleteKV(key);
#endif
        }

        public override bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
#if GRAVITY_XIAOMI_GAME_MODE
            return MiBridge.Instance.HasKV(key);
#else
            return false;
#endif

        }

    }

#if GRAVITY_XIAOMI_GAME_MODE
    public static class SyncHelper
    {
        // 将小米快游戏的系统参数获取改为同步
        public static bool GetSystemInfoSync(out bool success, out mi.SystemInfo outData)
        {
            // 初始化 out 参数
            success = false;
            outData = new mi.SystemInfo();

            // 使用局部变量存储 lambda 内部的结果
            bool localSuccess = false;
            mi.SystemInfo localData = null;
            AutoResetEvent waitHandle = new AutoResetEvent(false);

            // 在 lambda 中修改局部变量
            MiBridge.Instance.GetSystemInfo((success, systemData) =>
            {
                localSuccess = success;
                if (success)
                {
                    localData = systemData;
                }
                waitHandle.Set(); // 通知主线程继续
            });

            // 阻塞等待回调完成，设置超时（例如 5 秒）
            bool completed = waitHandle.WaitOne(3000);

            // 将局部变量赋值给 out 参数
            if (completed && localSuccess)
            {
                success = localSuccess;
                outData = localData;
                return true;
            }
            return false;
        }
    }
#endif

}