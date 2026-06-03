using System;
using System.Collections.Generic;
using GravityEngine.Utils;
using GravitySDK.PC.Main;
using GravitySDK.PC.Utils;
using GravitySDK.PC.DataModel;
using GravitySDK.PC.Config;
using GravitySDK.PC.Constant;
using GravitySDK.PC.GravityTurbo;
using UnityEngine.Networking;
using GravityEngine.Platform;
using UnityEngine;
using System.Runtime.InteropServices;

namespace GravityEngine.Wrapper
{
    public class GravityWrapperBase: IDynamicSuperProperties_PC, IAutoTrackEventCallback_PC
    {
        public static IDynamicSuperProperties mDynamicSuperProperties;

        private static IAutoTrackEventCallback mAutoTrackEventCallback;
        private static IInitializeCallback mInitializeCallback;
        private static ILogoutCallback mlogoutCallback;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //对应平台宏定义参考：https://docs.unity.cn/cn/tuanjiemanual/Manual/PlatformDependentCompilation.html

#if UNITY_IOS && !UNITY_EDITOR
        private static bool isNative()
        {
            return true;
        }
        //unity调底层iOS
        [DllImport("__Internal")]
        private static extern string ge_onUnityCallNative(string functionName, string dataString);

        //native回调
        [DllImport("__Internal")]
        public static extern void ge_initUnityNativeChannel(IntPtr geHandlerPointe);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate string GEiOSCallUnityHandler(string type, string jsonData);

        [AOT.MonoPInvokeCallback(typeof(GEiOSCallUnityHandler))]
        static string geOniOSCallUnityHandler(string type, string jsonData)
        {
            return nativeResultHandler(type, jsonData);
        }

        private static void registerNativeCallback()
        {
            GEiOSCallUnityHandler handler = new GEiOSCallUnityHandler(geOniOSCallUnityHandler);
            IntPtr geHandlerPointer = Marshal.GetFunctionPointerForDelegate(handler);
            ge_initUnityNativeChannel(geHandlerPointer);
        }
#elif UNITY_ANDROID && !UNITY_EDITOR && !GRAVITY_BYTEDANCE_CLOUD_MODE
//抖音云也可以导出成Android apk,所以 GRAVITY_BYTEDANCE_CLOUD_MODE 抖音云不能走Android逻辑
        private static bool isNative()
        {
            return true;
        }
        //unity调底层Android
        private static readonly AndroidJavaObject androidUnityAPIInstance = new AndroidJavaObject("cn.gravity.engine.GravityEngineUnityAPI");

        private static string ge_onUnityCallNative(string functionName, string dataString)
        {
            return androidUnityAPIInstance.CallStatic<string>("ge_onUnityCallNative", functionName, dataString);
        }

        //native回调
        private class AndroidCallUnityListenerAdapter : AndroidJavaProxy
        {
            public AndroidCallUnityListenerAdapter() : base(
                "cn.gravity.engine.GravityEngineUnityAPI$GEAndroidCallUnityListener")
            {
            }
            string onAndroidCallUnity(string type, string jsonData)
            {
                return nativeResultHandler(type, jsonData);
            }
        }
        private static void registerNativeCallback()
        {
            AndroidJavaObject context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"); //获得Context
            androidUnityAPIInstance.CallStatic("ge_initUnityNativeChannel", context, new AndroidCallUnityListenerAdapter());
        }
#elif UNITY_OPENHARMONY && !UNITY_EDITOR
        private static bool isNative()
        {
            return true;
        }
        //unity调底层鸿蒙
        private static readonly OpenHarmonyJSClass harmonyUnityAPIInstance = new OpenHarmonyJSClass("GravityEngineUnityChannel");

        private static string ge_onUnityCallNative(string functionName, string dataString)
        {
            return harmonyUnityAPIInstance.CallStatic<string>("ge_onUnityCallNative", functionName, dataString);
        }

        //native回调
        private static string ge_onHarmonyCallUnity(params OpenHarmonyJSObject[] args)
        {
            string type = args[0].As<string>();
            string jsonData = args[1].As<string>();
            return nativeResultHandler(type, jsonData);
        }

        private static void registerNativeCallback()
        {
            OpenHarmonyJSCallback harmonyUnityCalllback = new OpenHarmonyJSCallback(ge_onHarmonyCallUnity);
            harmonyUnityAPIInstance.CallStatic("ge_initUnityNativeChannel", harmonyUnityCalllback);
        }
#else
        private static bool isNative()
        {
            if (!GEPlatformManager.IsGePlatform())
            {
                //如果非Native,Windows，那一定是某个小游戏，需要配置对应的宏参数
                Debug.LogError("平台类型错误，请检查是否正确添加全局宏参数：https://help.gravity-engine.com/docs/Unity#1.4_添加全局宏参数");
            }
            return false;
        }

        private static string ge_onUnityCallNative(string functionName, string dataString)
        {
            return "";
        }

        private static void registerNativeCallback()
        {

        }
#endif

        public static string getTimeString(DateTime dateTime)
        {
            if (isNative())
            {
                long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;
                DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;

                return ge_onUnityCallNative("ge_getTimeString",""+currentMillis);
            }
            return GravityPCSDK.TimeString(dateTime);
        }


        //调用底层
        private static string commonUnityCallNative(string functionName, Dictionary<string, object> properties = null)
        {
            string dataString = "{}";
            if (properties != null)
            {
                dataString = GE_MiniJson.Serialize(properties, getTimeString);
            }
            return ge_onUnityCallNative(functionName, dataString);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //底层回调回来
        static string nativeResultHandler(string type, string jsonData)
        {
            if (type == "AutoTrackProperties")
            {
                if (mAutoTrackEventCallback != null)
                {
                    Dictionary<string, object> properties = GE_MiniJson.Deserialize(jsonData);
                    int eventType = Convert.ToInt32(properties["EventType"]);
                    properties.Remove("EventType");
                    Dictionary<string, object> autoTrackProperties =
                        mAutoTrackEventCallback.AutoTrackEventCallback(eventType, properties);
                    return GE_MiniJson.Serialize(autoTrackProperties);
                }
            }
            else if (type == "DynamicSuperProperties")
            {
                if (mDynamicSuperProperties != null)
                {
                    Dictionary<string, object> dynamicSuperProperties =
                        mDynamicSuperProperties.GetDynamicSuperProperties();
                    return GE_MiniJson.Serialize(dynamicSuperProperties);
                }
            }
            else if (type == "LogoutCallback")
            {
                if (mlogoutCallback != null)
                {
                    mlogoutCallback.onCompleted();
                    mlogoutCallback = null;
                }

                GE_Log.d("logout callback");
            }
            else if (type == "InitializeCallbackSuccess")
            {
                if (mInitializeCallback != null)
                {
                    Dictionary<string, object> jsonResponse = GE_MiniJson.Deserialize(jsonData);
                    mInitializeCallback.onSuccess(jsonResponse);
                    mInitializeCallback = null;
                }
                //第一次一定会调用init
                //Native回调init成功，更新unity的clientid
                GravityHelper.SetClientId(getCurrentClientID());
                //如果初始化时传入的clientid为空，底层会自动生成lientId,后续演练模式需要用到clientid,所以这里需要更新

                GE_Log.d("initialize callback success");
            }
            else if (type == "InitializeCallbackFailed")
            {
                if (mInitializeCallback != null)
                {
                    mInitializeCallback.onFailed("initialize failed, :"+ jsonData);
                    mInitializeCallback = null;
                }
                GE_Log.d("initialize callback failed");
            }
            return "{}";
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public Dictionary<string, object> GetDynamicSuperProperties_PC()
        {
            if (mDynamicSuperProperties != null)
            {
                return mDynamicSuperProperties.GetDynamicSuperProperties();
            }
            else
            {
                return new Dictionary<string, object>();
            }
        }

        public Dictionary<string, object> AutoTrackEventCallback_PC(int type, Dictionary<string, object> properties)
        {
            if (mAutoTrackEventCallback != null)
            {
                return mAutoTrackEventCallback.AutoTrackEventCallback(type, properties);
            }
            else
            {
                return new Dictionary<string, object>();
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        

        private static Dictionary<string, object> functionProperties(object properties)
        {
            if (properties != null)
            {
            return new Dictionary<string, object>()
                    {
                        {"properties", properties},
                    };
            }
            return new Dictionary<string, object>() { };
        }

        private static Dictionary<string, object> functionProperties(object properties, DateTime dateTime)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;

            Dictionary<string, object> finalProperties = new Dictionary<string, object>()
                    {
                        {"time", currentMillis},
                    };
            if (properties != null)
            {
                finalProperties.Add("properties", properties);
            }
            return finalProperties;
        }

        //////////////////////////////////////
        public static void init(GravityEngineAPI.Token token,MonoBehaviour sMono)
        {
            if (isNative())
            {
                registerNativeCallback();

                Dictionary<string, object> configDic = new Dictionary<string, object>();
                configDic["accessToken"] = token.accessToken;
                configDic["clientId"] = token.clientId;
                configDic["mode"] = (int)token.mode;
                configDic["enableIMEI"] = token.enableIMEI;
                configDic["enableOAID"] = token.enableOAID;
                configDic["enableAndroidId"] = token.enableAndroidId;
                configDic["enableMAC"] = token.enableMAC;

                configDic["isGDPRArea"] = token.isGDPRArea;
                configDic["isCoppaEnabled"] = token.isCoppaEnabled;
                configDic["isKidsAppEnabled"] = token.isKidsAppEnabled;
                configDic["adPersonalizationEnabled"] = token.adPersonalizationEnabled;
                configDic["adUserDataEnabled"] = token.adUserDataEnabled;
                configDic["fbAppID"] = token.fbAppID;

                if (token.mClientIdPriorityOrder != null)
                {
                    configDic["mClientIdPriorityOrder"] = token.mClientIdPriorityOrderInts();
                }
                configDic["foregroundSessionThreshold"] = token.foregroundSessionThreshold;


                string timeZoneId = token.getTimeZoneId();
                if (null != timeZoneId && timeZoneId.Length > 0)
                {
                    configDic["timeZone"] = timeZoneId;
                }

                if (token.enableEncrypt)
                {
                    configDic["enableEncrypt"] = true;
                    configDic["secretKey"] = new Dictionary<string, object>()
                    {
                        {"publicKey", token.encryptPublicKey},
                        {"version", token.encryptVersion},
                        {"symmetricEncryption", "AES"},
                        {"asymmetricEncryption", "RSA"},
                    };
                }
                commonUnityCallNative("ge_setupAndStart", configDic);


                //后续启动可能不会调用init，但底层本地会缓存ClientID
                //Native回调init成功，更新unity的clientid
                GravityHelper.SetClientId(getCurrentClientID());
                //如果初始化时传入的clientid为空，底层会自动生成lientId,后续演练模式需要用到clientid,所以这里需要更新
            }
            else
            {
                GravitySDKConfig config = GravitySDKConfig.GetInstance(GravitySDKConstant.SERVER_URL, token.GetInstanceName());
                if (!string.IsNullOrEmpty(token.getTimeZoneId()))
                {
                    try
                    {
                        config.SetTimeZone(TimeZoneInfo.FindSystemTimeZoneById(token.getTimeZoneId()));
                    }
                    catch (Exception e)
                    {
                        //GravitySDKLogger.Print("TimeZoneInfo set failed : " + e.Message);
                    }
                }

                if (token.mode == GravityEngineAPI.SDKRunMode.DEBUG)
                {
                    config.SetMode(Mode.DEBUG);
                }
                GravityPCSDK.Init(GravitySDKConstant.SERVER_URL, token.GetInstanceName(), config, sMono);
            }
        }


        public static void identify(string uniqueId)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"distinctId", uniqueId},
                    };
                commonUnityCallNative("ge_identify", funcProperties);
            }
            else
            {
                GravityPCSDK.Identifiy(uniqueId);
            }
        }

        public static string getDistinctId()
        {
            if (isNative())
            {
                return commonUnityCallNative("ge_getDistinctId");
            }
            else
            {
                return GravityPCSDK.DistinctId();
            }
        }

        public static void login(string accountId)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"loginId", accountId},
                    };
                commonUnityCallNative("ge_login", funcProperties);
            }
            else
            {
                GravityPCSDK.Login(accountId);
            }
        }

        public static void logout(ILogoutCallback logoutCallback)
        {
            mlogoutCallback = logoutCallback;
            if (isNative())
            {
                commonUnityCallNative("ge_logout");
            }
            else
            {
                GravityPCSDK.Logout(logoutCallback);
            }
        }

        public static void flush()
        {
            if (isNative())
            {
                commonUnityCallNative("ge_flush");
            }
            else
            {
                GravityPCSDK.Flush();
            }
        }

        public static void setVersionInfo(string lib_name, string lib_version)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"libName", lib_name},
                        {"libVersion", lib_version},
                    };
                commonUnityCallNative("ge_setCustomerLibInfo", funcProperties);
            }
            else
            {
                GravityPCSDK.SetLibName(lib_name);
                GravityPCSDK.SetLibVersion(lib_version);
            }
        }

        private static Dictionary<string, object> getFinalEventProperties(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);

            if (null != GravityWrapperBase.mDynamicSuperProperties)
            {
                Dictionary<string, object> finalProperties = new Dictionary<string, object>();
                GE_PropertiesChecker.MergeProperties(GravityWrapperBase.mDynamicSuperProperties.GetDynamicSuperProperties(),
                    finalProperties);
                GE_PropertiesChecker.MergeProperties(properties, finalProperties);
                return finalProperties;
            }
            else
            {
                return properties;
            }
        }

        public static string track(string eventName, Dictionary<string, object> properties)
        {
            Dictionary<string, object> finalProperties = getFinalEventProperties(properties);
            if (isNative())
            {
                Dictionary<string, object> trackProperties = functionProperties(finalProperties);
                trackProperties.Add("eventName", eventName);
                return commonUnityCallNative("ge_track",trackProperties);
            }
            else
            {
                GravityPCSDK.Track(eventName, finalProperties);
                return "";
            }
        }

        public static string track(string eventName, Dictionary<string, object> properties, DateTime dateTime)
        {
            if (isNative())
            {
                return track(eventName, properties, dateTime, null);
            }
            else
            {
                Dictionary<string, object> finalProperties = getFinalEventProperties(properties);
                GravityPCSDK.Track(eventName, finalProperties, dateTime);
                return "";
            }
        }

        public static string track(string eventName, Dictionary<string, object> properties, DateTime dateTime, TimeZoneInfo timeZone)
        {
            Dictionary<string, object> finalProperties = getFinalEventProperties(properties);
            if (isNative())
            {
                Dictionary<string, object> trackProperties = functionProperties(finalProperties, dateTime);
                trackProperties.Add("eventName", eventName);
                if(timeZone != null)
                {
                    trackProperties.Add("timeZone", timeZone.Id);
                }
                return commonUnityCallNative("ge_track", trackProperties);
            }
            else
            {
                GravityPCSDK.Track(eventName, finalProperties, dateTime, timeZone);
                return "";
            }
        }

        public static void setSuperProperties(Dictionary<string, object> superProperties)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(superProperties);
                commonUnityCallNative("ge_setSuperProperties", funcProperties);
            }
            else
            {
                GravityPCSDK.SetSuperProperties(superProperties);
            }
        }

        public static void unsetSuperProperty(string superPropertyName)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"property", superPropertyName},
                    };
                commonUnityCallNative("ge_unsetSuperProperty", funcProperties);
            }
            else
            {
                GravityPCSDK.UnsetSuperProperty(superPropertyName);
            }
        }

        public static void clearSuperProperty()
        {
            if (isNative())
            {
                commonUnityCallNative("ge_clearSuperProperties");
            }
            else
            {
                GravityPCSDK.ClearSuperProperties();
            }
        }

        public static Dictionary<string, object> getSuperProperties()
        {
            if (isNative())
            {
                string resultStr = commonUnityCallNative("ge_getSuperProperties");
                return GE_MiniJson.Deserialize(resultStr);
            }
            else
            {
                return GravityPCSDK.SuperProperties();
            }
        }
        
        public static Dictionary<string, object> getCurrentPresetProperties()
        {
            if (isNative())
            {
                string resultStr = commonUnityCallNative("ge_getPresetProperties");
                return GE_MiniJson.Deserialize(resultStr);
            }
            else
            {
                return new Dictionary<string, object>();
            }
        }

        public static void timeEvent(string eventName)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"eventName", eventName},
                    };
                commonUnityCallNative("ge_timeEvent", funcProperties);
            }
            else
            {
                GravityPCSDK.TimeEvent(eventName);
            }
        }

        public static void userSet(Dictionary<string, object> properties)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties);
                commonUnityCallNative("ge_userSet", funcProperties);
            }
            else
            {
                GravityPCSDK.UserSet(properties);
            }
        }

        public static void userSet(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties, dateTime);
                commonUnityCallNative("ge_userSet", funcProperties);
            }
            else
            {
                GravityPCSDK.UserSet(properties, dateTime);
            }
        }

        public static void userUnset(List<string> properties)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties);
                commonUnityCallNative("ge_userUnset", funcProperties);
            }
            else
            {
                GravityPCSDK.UserUnset(properties);
            }
        }

        public static void userUnset(List<string> properties, DateTime dateTime)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties, dateTime);
                commonUnityCallNative("ge_userUnset", funcProperties);
            }
            else
            {
                GravityPCSDK.UserUnset(properties, dateTime);
            }
        }

        public static void userSetOnce(Dictionary<string, object> properties)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties);
                commonUnityCallNative("ge_userSetOnce", funcProperties);
            }
            else
            {
                GravityPCSDK.UserSetOnce(properties);
            }
        }

        public static void userSetOnce(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties, dateTime);
                commonUnityCallNative("ge_userSetOnce", funcProperties);
            }
            else
            {
                GravityPCSDK.UserSetOnce(properties, dateTime);
            }
        }

        public static void userAdd(Dictionary<string, object> properties)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties);
                commonUnityCallNative("ge_userAdd", funcProperties);
            }
            else
            {
                GravityPCSDK.UserAdd(properties);
            }
        }

        public static void userAdd(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties, dateTime);
                commonUnityCallNative("ge_userAdd", funcProperties);
            }
            else
            {
                GravityPCSDK.UserAdd(properties, dateTime);
            }
        }

        public static void userNumberMin(Dictionary<string, object> properties)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties);
                commonUnityCallNative("ge_userNumberMin", funcProperties);
            }
            else
            {
                GravityPCSDK.UserNumberMin(properties);
            }
        }

        public static void userNumberMin(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties, dateTime);
                commonUnityCallNative("ge_userNumberMin", funcProperties);
            }
            else
            {
                GravityPCSDK.UserNumberMin(properties, dateTime);
            }
        }

        public static void userNumberMax(Dictionary<string, object> properties)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties);
                commonUnityCallNative("ge_userNumberMax", funcProperties);
            }
            else
            {
                GravityPCSDK.UserNumberMax(properties);
            }
        }

        public static void userNumberMax(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties, dateTime);
                commonUnityCallNative("ge_userNumberMax", funcProperties);
            }
            else
            {
                GravityPCSDK.UserNumberMax(properties, dateTime);
            }
        }

        public static void userDelete()
        {
            if (isNative())
            {
                commonUnityCallNative("ge_userDel");
            }
            else
            {
                GravityPCSDK.UserDelete();
            }
        }

        public static void userDelete(DateTime dateTime)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(null, dateTime);
                commonUnityCallNative("ge_userDel", funcProperties);
            }
            else
            {
                GravityPCSDK.UserDelete(dateTime);
            }
        }

        public static void userAppend(Dictionary<string, object> properties)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties);
                commonUnityCallNative("ge_userAppend", funcProperties);
            }
            else
            {
                GravityPCSDK.UserAppend(properties);
            }
        }

        public static void userAppend(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties, dateTime);
                commonUnityCallNative("ge_userAppend", funcProperties);
            }
            else
            {
                GravityPCSDK.UserAppend(properties, dateTime);
            }
        }

        public static void userUniqAppend(Dictionary<string, object> properties)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties);
                commonUnityCallNative("ge_userUniqAppend", funcProperties);
            }
            else
            {
                GravityPCSDK.UserUniqAppend(properties);
            }
        }

        public static void userUniqAppend(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties, dateTime);
                commonUnityCallNative("ge_userUniqAppend", funcProperties);
            }
            else
            {
                GravityPCSDK.UserUniqAppend(properties, dateTime);
            }
        }

        public static void setNetworkType(GravityEngineAPI.NetworkType networkType)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"network_type", networkType},
                    };
                commonUnityCallNative("ge_setNetworkType", funcProperties);
            }
        }

        public static string getDeviceId()
        {
            if (isNative())
            {
                return commonUnityCallNative("ge_getDeviceId");
            }
            else
            {
                return GravityPCSDK.GetDeviceId();
            }
        }

        public static void setDynamicSuperProperties(IDynamicSuperProperties dynamicSuperProperties)
        {
            mDynamicSuperProperties = dynamicSuperProperties;
            if (isNative())
            {
                commonUnityCallNative("ge_setDynamicSuperProperties");
            }
            else
            {
                GravityPCSDK.SetDynamicSuperProperties(new GravityWrapperBase());
            }
        }

        public static void setTrackStatus(GE_TRACK_STATUS status)
        {
            if (isNative())
            {
                string statusStr = "normal";
                switch (status)
                {
                    case GE_TRACK_STATUS.PAUSE:
                        statusStr = "pause";
                        break;
                    case GE_TRACK_STATUS.STOP:
                        statusStr = "stop";
                        break;
                    case GE_TRACK_STATUS.SAVE_ONLY:
                        statusStr = "saveOnly";
                        break;
                    case GE_TRACK_STATUS.NORMAL:
                        statusStr = "normal";
                        break;
                    default:
                        break;
                }
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"status",statusStr},
                    };
                commonUnityCallNative("ge_setTrackStatus", funcProperties);
            }
            else
            {
                GravityPCSDK.SetTrackStatus((GE_TRACK_STATUS)status);
            }
        }

        public static void enableAutoTrack(AUTO_TRACK_EVENTS autoTrackEvents, Dictionary<string, object> properties)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties);
                funcProperties.Add("autoTrackType", (int)autoTrackEvents);
                commonUnityCallNative("ge_enableAutoTrack", funcProperties);
            }
            else
            {
                AUTO_TRACK_EVENTS pcAutoTrackEvents = AUTO_TRACK_EVENTS.NONE;
                if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_INSTALL) != 0)
                {
                    pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_INSTALL;
                }

                if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_START) != 0)
                {
                    pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_START;
                }

                if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_END) != 0)
                {
                    pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_END;
                }

                if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_CRASH) != 0)
                {
                    pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_CRASH;
                }
                if (GEPlatformManager.isMiniGame())
                {
                    if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_SHOW) != 0)
                    {
                        pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_SHOW;
                    }

                    if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_HIDE) != 0)
                    {
                        pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_HIDE;
                    }

                    if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_SHARE) != 0)
                    {
                        pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_SHARE;
                    }

                    if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES) != 0)
                    {
                        pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES;
                    }
                }
                GravityPCSDK.EnableAutoTrack(pcAutoTrackEvents, properties);
            }
        }

        public static void enableAutoTrack(AUTO_TRACK_EVENTS autoTrackEvents, IAutoTrackEventCallback eventCallback)
        {
            mAutoTrackEventCallback = eventCallback;
            if (isNative())
            {
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"autoTrackType", (int)autoTrackEvents},
                    };
                commonUnityCallNative("ge_enableAutoTrack", funcProperties);
            }
            else
            {
                AUTO_TRACK_EVENTS pcAutoTrackEvents = AUTO_TRACK_EVENTS.NONE;
                if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_INSTALL) != 0)
                {
                    pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_INSTALL;
                }

                if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_START) != 0)
                {
                    pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_START;
                }

                if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_END) != 0)
                {
                    pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_END;
                }

                if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_CRASH) != 0)
                {
                    pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_CRASH;
                }

                if (GEPlatformManager.isMiniGame())
                {
                    if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_SHOW) != 0)
                    {
                        pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_SHOW;
                    }

                    if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_HIDE) != 0)
                    {
                        pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_HIDE;
                    }

                    if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_SHARE) != 0)
                    {
                        pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_SHARE;
                    }

                    if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES) != 0)
                    {
                        pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES;
                    }
                }
                GravityPCSDK.EnableAutoTrack(pcAutoTrackEvents, new GravityWrapperBase());
            }
        }

        public static void setAutoTrackProperties(AUTO_TRACK_EVENTS autoTrackEvents, Dictionary<string, object> properties)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = functionProperties(properties);
                funcProperties.Add("autoTrackType", (int)autoTrackEvents);
                commonUnityCallNative("ge_setAutoTrackProperties", funcProperties);
            }
            else
            {
                if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_INSTALL) != 0)
                {
                    GravityPCSDK.SetAutoTrackProperties(AUTO_TRACK_EVENTS.APP_INSTALL, properties);
                }

                if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_START) != 0)
                {
                    GravityPCSDK.SetAutoTrackProperties(AUTO_TRACK_EVENTS.APP_START, properties);
                }

                if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_END) != 0)
                {
                    GravityPCSDK.SetAutoTrackProperties(AUTO_TRACK_EVENTS.APP_END, properties);
                }

                if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_CRASH) != 0)
                {
                    GravityPCSDK.SetAutoTrackProperties(AUTO_TRACK_EVENTS.APP_CRASH, properties);
                }
            }
        }

        public static void enableLog(bool enable)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"enable",enable},
                    };
                commonUnityCallNative("ge_enableLog", funcProperties);
            }
            else
            {
                GravityPCSDK.EnableLog(enable);
            }
        }

        public static void calibrateTime(long timestamp)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"time",timestamp},
                    };
                commonUnityCallNative("ge_calibrateTime", funcProperties);
            }
            else
            {
                GravityPCSDK.CalibrateTime(timestamp);
            }
        }

        public static void calibrateTimeWithNtp(string ntpServer)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"ntpServer",ntpServer},
                    };
                commonUnityCallNative("ge_calibrateTime", funcProperties);
            }
            else
            {
                GravityPCSDK.CalibrateTimeWithNtp(ntpServer);
            }
        }

        public static void initialize(string clientId, string name, int version, string openid,
            bool enableSyncAttribution, IInitializeCallback initializeCallback)
        {
            mInitializeCallback = initializeCallback;
            if (isNative())
            {
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"access_token",GravityHelper.GetAccessToken()},
                        {"channel",GravityHelper.GetChannel()},
                        {"clientId",GravityHelper.SetOrGetClientId(clientId)},
                        {"name",name},
                        {"needReturnAttribution",enableSyncAttribution},
                    };
                commonUnityCallNative("ge_initialize", funcProperties);
            }
            else
            {
                GravityPCSDK.Initialize(clientId, name, version, openid, enableSyncAttribution, initializeCallback);
            }
        }
        
        public static void initializeIOS(bool enableAsa, string clientId, string caid1_md5, string caid2_md5, bool enableSyncAttribution, string channel, IInitializeCallback initializeCallback)
        {
            mInitializeCallback = initializeCallback;
            if (isNative())
            {
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"channel",channel},
                        {"clientId",clientId},
                        {"enableAsa",enableAsa},
                        {"caid1",caid1_md5},
                        {"caid2",caid2_md5},
                        {"needReturnAttribution",enableSyncAttribution},
                    };
                commonUnityCallNative("ge_initialize", funcProperties);
            }
            else
            {
                GravitySDKLogger.Print("not support initializeIOS!");
            }
        }
        
        public static void initializeIOS(bool enableAsa, string clientId, string caid, bool enableSyncAttribution, string channel, IInitializeCallback initializeCallback)
        {
            mInitializeCallback = initializeCallback;
            if (isNative())
            {
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"channel",channel},
                        {"clientId",clientId},
                        {"enableAsa",enableAsa},
                        {"caid",caid},
                        {"needReturnAttribution",enableSyncAttribution},
                    };
                commonUnityCallNative("ge_initialize", funcProperties);
            }
            else
            {
                GravitySDKLogger.Print("not support initializeIOS!");
            }
        }

        public static void UpdateUserInfo(Dictionary<string, object> properties)
        {
            if (isNative())
            {
                commonUnityCallNative("ge_updateUserInfo", properties);
            }
            else
            {
                GravityPCSDK.UpdateUserInfo(properties);
            }
        }

        public static void queryDryRunInfo(string traceId, Dictionary<string, object> otherParams, IQueryDryRunCallback queryDryRunCallback)
        {
            if (isNative())
            {
                GravityHelper.QueryDryRunInfo(traceId, otherParams, queryDryRunCallback);
            }
            else
            {
                GravitySDKLogger.Print("not support queryDryRunInfo!");
                //GravityPCSDK.QueryDryRunInfo(traceId, otherParams, queryDryRunCallback);
            }
        }
        
        public static string getCurrentClientID()
        {
            if (isNative())
            {
                return commonUnityCallNative("ge_getCurrentClientID");
            }
            return GravityHelper.GetClientId();
        }
        
        public static string trackPayEvent(int payAmount, string payType, string orderId, string payReason,
            string payMethod)
        {
            track("$PayEvent", new Dictionary<string, object>()
                    {
                        {"$pay_amount", payAmount},
                        {"$pay_type", payType},
                        {"$order_id", orderId},
                        {"$pay_reason", payReason},
                        {"$pay_method", payMethod}
                    });
            flush();
            return orderId;
        }
        
        public static void trackNativeAppAdShowEvent(string adUnionType, string adPlacementId, string adSourceId,
            string adType, string adnType, float ecpm)
        {
            if (isNative())
            {
                track("$AdShow", new Dictionary<string, object>()
                    {
                        {"$ad_through",adUnionType},
                        {"$ad_placement_id",adPlacementId},
                        {"$ad_source_id",adSourceId},
                        {"$ad_type",adType},
                        {"$adn_type",adnType},
                        {"$ecpm",ecpm},
                    });
                flush();
            }
            else
            {
                GravitySDKLogger.Print("not support trackNativeAppAdShowEvent!");
            }
        }

        public static void bindTAThirdPlatform(string taAccountId, string taDistinctId)
        {
            if (isNative())
            {
                Dictionary<string, object> funcProperties = new Dictionary<string, object>()
                    {
                        {"taAccountId", taAccountId},
                        {"taDistinctId", taDistinctId},
                    };
                commonUnityCallNative("ge_bindTAThirdPlatform", funcProperties);
                return;
            }
            GravityPCSDK.BindTAThirdPlatform(taAccountId, taDistinctId);
        }

        public static void filterAndReportPayEventToTencent(int payAmount, string traceId)
        {
            if (isNative())
            {
                GE_Log.d("native not support filterAndReportPayEventToTencent");
                return;
            }
            GravityHelper.FilterAndReportPayEventToTencent(payAmount, traceId);
        }
    }
}