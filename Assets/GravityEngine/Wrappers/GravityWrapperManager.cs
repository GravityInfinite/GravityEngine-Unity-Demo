using System;
using System.Collections.Generic;
using GravityEngine.Utils;
using GravitySDK.PC.Constant;
using UnityEngine;

namespace GravityEngine.Wrapper
{
    public class GravityWrapperManager
    {
        public static string GetTimeString(DateTime dateTime)
        {
            return GravityWrapperBase.getTimeString(dateTime);
        }

        public static void ShareInstance(GravityEngineAPI.Token token, MonoBehaviour mono, bool initRequired = true)
        {
            if (initRequired) GravityWrapperBase.init(token, mono);
        }

        public static void EnableLog(bool enable)
        {
            GravityWrapperBase.enableLog(enable);
        }

        public static void SetVersionInfo(string version)
        {
            GravityWrapperBase.setVersionInfo("Unity", version);
        }

        public static void Identify(string uniqueId)
        {
            GravityWrapperBase.identify(uniqueId);
        }

        public static string GetDistinctId()
        {
            return GravityWrapperBase.getDistinctId();
        }

        public static void Login(string accountId)
        {
            GravityWrapperBase.login(accountId);
        }

        public static void Logout(ILogoutCallback logoutCallback)
        {
            GravityWrapperBase.logout(logoutCallback);
        }

        public static void EnableAutoTrack(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
        {
            GravityWrapperBase.enableAutoTrack(events, properties);
        }

        public static void EnableAutoTrack(AUTO_TRACK_EVENTS events, IAutoTrackEventCallback eventCallback)
        {
            GravityWrapperBase.enableAutoTrack(events, eventCallback);
        }

        public static void SetAutoTrackProperties(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
        {
            GravityWrapperBase.setAutoTrackProperties(events, properties);
        }


        public static string Track(string eventName, Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckString(eventName);
            return GravityWrapperBase.track(eventName, properties);
        }

        public static string Track(string eventName, Dictionary<string, object> properties, DateTime datetime)
        {
            GE_PropertiesChecker.CheckString(eventName);
            return GravityWrapperBase.track(eventName, properties, datetime);
        }

        public static string Track(string eventName, Dictionary<string, object> properties, DateTime datetime,
            TimeZoneInfo timeZone)
        {
            GE_PropertiesChecker.CheckString(eventName);
            return GravityWrapperBase.track(eventName, properties, datetime, timeZone);
        }

        public static void SetSuperProperties(Dictionary<string, object> superProperties)
        {
            GE_PropertiesChecker.CheckProperties(superProperties);
            GravityWrapperBase.setSuperProperties(superProperties);
        }

        public static void UnsetSuperProperty(string superPropertyName)
        {
            GE_PropertiesChecker.CheckString(superPropertyName);
            GravityWrapperBase.unsetSuperProperty(superPropertyName);
        }

        public static void ClearSuperProperty()
        {
            GravityWrapperBase.clearSuperProperty();
        }

        public static void TimeEvent(string eventName)
        {
            GE_PropertiesChecker.CheckString(eventName);
            GravityWrapperBase.timeEvent(eventName);
        }

        public static Dictionary<string, object> GetSuperProperties()
        {
            return GravityWrapperBase.getSuperProperties();
        }

        public static Dictionary<string, object> GetCurrentPresetProperties()
        {
            return GravityWrapperBase.getCurrentPresetProperties();
        }

        public static void UserSet(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userSet(properties);
        }

        public static void UserSet(Dictionary<string, object> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userSet(properties, dateTime);
        }

        public static void UserSetOnce(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userSetOnce(properties);
        }

        public static void UserSetOnce(Dictionary<string, object> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userSetOnce(properties, dateTime);
        }

        public static void UserUnset(List<string> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userUnset(properties);
        }

        public static void UserUnset(List<string> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userUnset(properties, dateTime);
        }

        public static void UserAdd(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userAdd(properties);
        }

        public static void UserAdd(Dictionary<string, object> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userAdd(properties, dateTime);
        }

        public static void UserNumberMin(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userNumberMin(properties);
        }

        public static void UserNumberMin(Dictionary<string, object> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userNumberMin(properties, dateTime);
        }

        public static void UserNumberMax(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userNumberMax(properties);
        }

        public static void UserNumberMax(Dictionary<string, object> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userNumberMax(properties, dateTime);
        }

        public static void UserAppend(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userAppend(properties);
        }

        public static void UserAppend(Dictionary<string, object> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userAppend(properties, dateTime);
        }

        public static void UserUniqAppend(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userUniqAppend(properties);
        }

        public static void UserUniqAppend(Dictionary<string, object> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            GravityWrapperBase.userUniqAppend(properties, dateTime);
        }

        public static void UserDelete()
        {
            GravityWrapperBase.userDelete();
        }

        public static void UserDelete(DateTime dateTime)
        {
            GravityWrapperBase.userDelete(dateTime);
        }

        public static void Flush()
        {
            GravityWrapperBase.flush();
        }

        public static void SetNetworkType(GravityEngineAPI.NetworkType networkType)
        {
            GravityWrapperBase.setNetworkType(networkType);
        }

        public static string GetDeviceId()
        {
            return GravityWrapperBase.getDeviceId();
        }

        public static void SetDynamicSuperProperties(IDynamicSuperProperties dynamicSuperProperties)
        {
            if (!GE_PropertiesChecker.CheckProperties(dynamicSuperProperties.GetDynamicSuperProperties()))
            {
                GE_Log.d("GE.Wrapper - Cannot set dynamic super properties due to invalid properties.");
            }
            GravityWrapperBase.setDynamicSuperProperties(dynamicSuperProperties);
        }

        public static void SetTrackStatus(GE_TRACK_STATUS status)
        {
            GravityWrapperBase.setTrackStatus(status);
        }

        public static void CalibrateTime(long timestamp)
        {
            GravityWrapperBase.calibrateTime(timestamp);
        }

        public static void CalibrateTimeWithNtp(string ntpServer)
        {
            GravityWrapperBase.calibrateTimeWithNtp(ntpServer);
        }

        public static void Initialize(string clientId, string name, int version, string wxOpenId, bool enableSyncAttribution, IInitializeCallback initializeCallback)
        {
            GravityWrapperBase.initialize(clientId, name, version, wxOpenId, enableSyncAttribution, initializeCallback);
        }

        public static void InitializeIOS(bool enableAsa, string clientId, string caid1_md5, string caid2_md5, bool enableSyncAttribution, string channel, IInitializeCallback initializeCallback)
        {
            GravityWrapperBase.initializeIOS(enableAsa, clientId, caid1_md5, caid2_md5, enableSyncAttribution, channel, initializeCallback);
        }
        public static void InitializeIOS(bool enableAsa, string clientId, string caid, bool enableSyncAttribution, string channel, IInitializeCallback initializeCallback)
        {
            GravityWrapperBase.initializeIOS(enableAsa, clientId, caid, enableSyncAttribution, channel, initializeCallback);
        }

        public static void UpdateUserInfo(Dictionary<string, object> properties)
        {
            GravityWrapperBase.UpdateUserInfo(properties);
        }

        public static void QueryDryRunInfo(string traceId, Dictionary<string, object> otherParams, IQueryDryRunCallback queryDryRunCallback)
        {
            GravityWrapperBase.queryDryRunInfo(traceId, otherParams, queryDryRunCallback);
        }

        public static string GetCurrentClientID()
        {
            return GravityWrapperBase.getCurrentClientID();
        }

        public static string TrackPayEvent(int payAmount, string payType, string orderId, string payReason,
            string payMethod)
        {
            return GravityWrapperBase.trackPayEvent(payAmount, payType, orderId, payReason, payMethod);
        }

        public static void TrackNativeAppAdShowEvent(string adUnionType, string adPlacementId, string adSourceId,
            string adType, string adnType, float ecpm)
        {
            GravityWrapperBase.trackNativeAppAdShowEvent(adUnionType, adPlacementId, adSourceId, adType, adnType,
                ecpm);
        }

        public static void BindTAThirdPlatform(string taAccountId, string taDistinctId)
        {
            GravityWrapperBase.bindTAThirdPlatform(taAccountId, taDistinctId);
        }

        public static void FilterAndReportPayEventToTencent(int payAmount, string traceId)
        {
            GravityWrapperBase.filterAndReportPayEventToTencent(payAmount, traceId);
        }
    }

}