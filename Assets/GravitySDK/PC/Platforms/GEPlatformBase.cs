using UnityEngine;
using System.Collections.Generic;
using System;

namespace GravityEngine.Platform
{
    public class GEPlatformBase
    {
        #region 通用
        public virtual void SetDeviceInfo()
        {
        }

        public virtual void OnGravityStartEnd()
        {
        }

        public virtual string platformStr()
        {
            return null;
        }

        public virtual void GetLaunchOptionsSync(Action<Dictionary<string, string>, string> callBack)
        {

        }
        public virtual void getDeviceInfo(Action<Dictionary<string,string>> callBack)
        {

        }

        public virtual void addShowAutoTrack(Func<string,Dictionary<string, object>> propertiesCallback)
        {

        }

        public virtual void addHideAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {

        }

        public virtual void addFavoriteAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {

        }

        public virtual void addShareAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {

        }
        #endregion
        #region 数据管理
        public virtual void SaveData(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            if (value == null)
            {
                return;
            }
            if (value.GetType() == typeof(int))
            {
                UnityEngine.PlayerPrefs.SetInt(key, (int)value);
            }
            else if (value.GetType() == typeof(float))
            {
                UnityEngine.PlayerPrefs.SetFloat(key, (float)value);
            }
            else if (value.GetType() == typeof(string))
            {
                UnityEngine.PlayerPrefs.SetString(key, (string)value);
            }
            UnityEngine.PlayerPrefs.Save();
        }

        public virtual object GetData(string key, Type type)
        {
            if (!string.IsNullOrEmpty(key) && UnityEngine.PlayerPrefs.HasKey(key))
            {
                if (type == typeof(int))
                {
                    return UnityEngine.PlayerPrefs.GetInt(key);
                }
                else if (type == typeof(float))
                {
                    return UnityEngine.PlayerPrefs.GetFloat(key);
                }
                else if (type == typeof(string))
                {
                    return UnityEngine.PlayerPrefs.GetString(key);
                }
                UnityEngine.PlayerPrefs.Save();
            }
            return null;
        }
        public virtual void DeleteData(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            if (UnityEngine.PlayerPrefs.HasKey(key))
            {
                UnityEngine.PlayerPrefs.DeleteKey(key);
            }
        }
        public virtual bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            return UnityEngine.PlayerPrefs.HasKey(key);
        }
        #endregion

        #region 腾讯SDK专用

        public virtual object SetOpenId(string openId)
        {
            return null;
        }

        public virtual object TrackMPShare(string target, Dictionary<string, object> properties = null)
        {
            return null;
        }

        public virtual object TrackMPAddFavorites(string wishType = "default")
        {
            return null;
        }

        public virtual object TrackMPCreateRole(string roleName)
        {
            return null;
        }

        public virtual object TrackMPTutorialFinish()
        {
            return null;
        }

        public virtual object TrackMPUpdateLevel(int level, int power)
        {
            return null;
        }

        public virtual object TrackMPViewMallContent()
        {
            return null;
        }

        public virtual object TrackMPViewActivityContent()
        {
            return null;
        }


        public virtual object onPurchase(int amount)
        {
            return null;
        }

        public virtual object onRegister()
        {
            return null;
        }

        public virtual object onReActive(int backFlowDay)
        {
            return null;
        }


        public virtual object TrackMPAppStart()
        {
            return null;
        }

        public virtual object TrackMPLoadFinish()
        {
            return null;
        }

        public virtual object TrackMPSubscribe()
        {
            return null;
        }

        public virtual object TrackMPTutorialStart()
        {
            return null;
        }


        public virtual object TrackMPLevelEnter(Dictionary<string, object> properties)
        {
            return null;
        }

        public virtual object TrackMPLevelExit(Dictionary<string, object> properties)
        {
            return null;
        }

        public virtual object TrackMPLevelClose(Dictionary<string, object> properties)
        {
            return null;
        }

        public virtual object TrackMPLevelPass(Dictionary<string, object> properties)
        {
            return null;
        }


        //激励点位出现 
        //ad_placement_type 激励视频广告行为事件;例如1代表复活，2代表体力，当复活广告位出现被点击时上报1枚举见:https://doc.weixin.qq.com/sheet/e3_AQAA_gboAOACNEgqhOXynRwqq0gNv?scode=AJEAIQdfAAo7woEvatAQAA_gboAOA&tab=BB08J2
        public virtual object TrackMPAdPlacementShow(int ad_placement_type)
        {
            return null;
        }

        //广告位点击 
        //ad_placement_type 激励视频广告行为事件;例如1代表复活，2代表体力，当复活广告位出现被点击时上报1枚举见:https://doc.weixin.qq.com/sheet/e3_AQAA_gboAOACNEgqhOXynRwqq0gNv?scode=AJEAIQdfAAo7woEvatAQAA_gboAOA&tab=BB08J2
        public virtual object TrackMPAdPlacementClick(int ad_placement_type)
        {
            return null;
        }

        //广告播放结束
        //ad_placement_type 激励视频广告行为事件;例如1代表复活，2代表体力，当复活广告位出现被点击时上报1枚举见:https://doc.weixin.qq.com/sheet/e3_AQAA_gboAOACNEgqhOXynRwqq0gNv?scode=AJEAIQdfAAo7woEvatAQAA_gboAOA&tab=BB08J2
        public virtual object TrackMPAdPlacementVideoFinish(int ad_placement_type)
        {
            return null;
        }

        //广告曝光
        //ad_type 广告位类型; 1-激励视频，2-插屏，3-格子，4-banner
        public virtual object TrackMPAdPlacementVideoShow(int ad_type)
        {
            return null;
        }

        //事件上报
        public virtual object TrackMPEvent(string eventName, Dictionary<string, object> properties = null)
        {
            return null;
        }
        #endregion
    }
}
