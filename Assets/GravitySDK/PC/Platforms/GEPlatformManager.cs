using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GravityEngine.Platform
{
    public class GEPlatformManager : MonoBehaviour
    {
        public static bool CanTrackMiniGameAdShowEvent()
        {
            return !GEMiniPlatformByteDance.activity() && !GEMiniPlatformKuaiShou.activity() && !GEMiniPlatformWechat.IsBilibili();
        }
        public static string GetTrackMiniGameAdShowType()
        {
            if (GEMiniPlatformWechat.IsWechat())
            {
                return "wechat";
            }
            return "media";
        }
        public static bool CanFilterAndReportPayEventToTencent()
        {
            return GEMiniPlatformWechat.canTrack();
        }
        public static bool IsAliPayMiniGame()
        {
            return GEMiniPlatformAlipay.activity();
        }
        public static bool IsWechatMiniGame()
        {
            return GEMiniPlatformWechat.activity();
        }

        public static bool NeedGetDeviceInfoBeforeInit()
        {
            return GEMiniPlatformOppo.activity() || GEMiniPlatformHuawei.activity() || GEMiniPlatformVivo.activity() || GEPlatformWindows.activity();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public static bool IsGePlatform()
        {
            if (mGEPlatform != null)
            {
                return true;
            }
            return false;
        }


        public static bool isMiniGame()
        {
            if (mGEPlatform != null)
            {
                return !GEPlatformWindows.activity();
            }
            return false;
        }


        public static void SetDeviceInfo()
        {
            if (mGEPlatform != null)
            {
                mGEPlatform.SetDeviceInfo();
            }
        }

        public static void OnGravityStartEnd()
        {
            if (mGEPlatform != null)
            {
                mGEPlatform.OnGravityStartEnd();
            }
        }

        public static void GetLaunchOptionsSync(Action<Dictionary<string,string>,string> callBack)
        {
            if (mGEPlatform != null)
            {
                mGEPlatform.GetLaunchOptionsSync(callBack);
            }
        }


        public static void addShowAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {
            if (mGEPlatform != null)
            {
                mGEPlatform.addShowAutoTrack(propertiesCallback);
            }
        }

        public static void addHideAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {
            if (mGEPlatform != null)
            {
                mGEPlatform.addHideAutoTrack(propertiesCallback);
            }
        }


        public static void addFavoriteAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {
            if (mGEPlatform != null)
            {
                mGEPlatform.addFavoriteAutoTrack(propertiesCallback);
            }
        }

        public static void addShareAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {
            if (mGEPlatform != null)
            {
                mGEPlatform.addShareAutoTrack(propertiesCallback);
            }
        }

        public static string platformStr()
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.platformStr();
            }
            return null;
        }


        public static object SetOpenId(string openId)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.SetOpenId(openId);
            }
            return null;
        }
        public static object TrackMPShare(string target, Dictionary<string, object> properties = null)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPShare(target, properties);
            }
            return null;
        }
        public static object TrackMPAddFavorites(string wishType = "default")
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPAddFavorites(wishType);
            }
            return null;
        }
        public static object TrackMPCreateRole(string roleName)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPCreateRole(roleName);
            }
            return null;
        }
        public static object TrackMPTutorialFinish()
        {
            if (mGEPlatform != null)
            {
               return mGEPlatform.TrackMPTutorialFinish();
            }
            return null;
        }
        public static object TrackMPUpdateLevel(int level, int power)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPUpdateLevel(level, power);
            }
            return null;
        }
        public static object TrackMPViewMallContent()
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPViewMallContent();
            }
            return null;
        }
        public static object TrackMPViewActivityContent()
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPViewActivityContent();
            }
            return null;
        }

        public static object onRegister()
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.onRegister();
            }
            return null;
        }
        public static object onReActive(int backFlowDay)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.onReActive(backFlowDay);
            }
            return null;
        }
        public static object onPurchase(int amount)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.onPurchase(amount);
            }
            return null;
        }



        public static object TrackMPAppStart()
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPAppStart();
            }
            return null;
        }

        public static object TrackMPLoadFinish()
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPLoadFinish();
            }
            return null;
        }

        public static object TrackMPSubscribe()
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPSubscribe();
            }
            return null;
        }

        public static object TrackMPTutorialStart()
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPTutorialStart();
            }
            return null;
        }


        public static object TrackMPLevelEnter(Dictionary<string, object> properties)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPLevelEnter(properties);
            }
            return null;
        }

        public static object TrackMPLevelExit(Dictionary<string, object> properties)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPLevelExit(properties);
            }
            return null;
        }

        public static object TrackMPLevelClose(Dictionary<string, object> properties)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPLevelClose(properties);
            }
            return null;
        }


        public static object TrackMPLevelPass(Dictionary<string, object> properties)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPLevelPass(properties);
            }
            return null;
        }


        //激励点位出现 
        //ad_placement_type 激励视频广告行为事件;例如1代表复活，2代表体力，当复活广告位出现被点击时上报1枚举见:https://doc.weixin.qq.com/sheet/e3_AQAA_gboAOACNEgqhOXynRwqq0gNv?scode=AJEAIQdfAAo7woEvatAQAA_gboAOA&tab=BB08J2
        public static object TrackMPAdPlacementShow(int ad_placement_type)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPAdPlacementShow(ad_placement_type);
            }
            return null;
        }

        //广告位点击 
        //ad_placement_type 激励视频广告行为事件;例如1代表复活，2代表体力，当复活广告位出现被点击时上报1枚举见:https://doc.weixin.qq.com/sheet/e3_AQAA_gboAOACNEgqhOXynRwqq0gNv?scode=AJEAIQdfAAo7woEvatAQAA_gboAOA&tab=BB08J2
        public static object TrackMPAdPlacementClick(int ad_placement_type)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPAdPlacementClick(ad_placement_type);
            }
            return null;
        }

        //广告播放结束
        //ad_placement_type 激励视频广告行为事件;例如1代表复活，2代表体力，当复活广告位出现被点击时上报1枚举见:https://doc.weixin.qq.com/sheet/e3_AQAA_gboAOACNEgqhOXynRwqq0gNv?scode=AJEAIQdfAAo7woEvatAQAA_gboAOA&tab=BB08J2
        public static object TrackMPAdPlacementVideoFinish(int ad_placement_type)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPAdPlacementVideoFinish(ad_placement_type);
            }
            return null;
        }

        //广告曝光
        //ad_type 广告位类型; 1-激励视频，2-插屏，3-格子，4-banner
        public static object TrackMPAdPlacementVideoShow(int ad_type)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPAdPlacementVideoShow(ad_type);
            }
            return null;
        }

        //事件上报
        public static object TrackMPEvent(string eventName, Dictionary<string, object> properties = null)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.TrackMPEvent(eventName, properties);
            }
            return null;
        }


        public static void getDeviceInfo(Action<Dictionary<string, string>> callBack)
        {
            if (mGEPlatform != null)
            {
                mGEPlatform.getDeviceInfo(callBack);
            }
        }


        public static void SaveData(string key, object value)
        {
            if (mGEPlatform != null)
            {
                mGEPlatform.SaveData(key, value);
            }
        }

        public static object GetData(string key, Type type)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.GetData(key, type);
            }
            return null;
        }
        public static void DeleteData(string key)
        {
            if (mGEPlatform != null)
            {
                mGEPlatform.DeleteData(key);
            }
        }
        public static bool HasKey(string key)
        {
            if (mGEPlatform != null)
            {
                return mGEPlatform.HasKey(key);
            }
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //返回有定义宏定义接入的SDK数量
        private static int hasDefineSdkNumber()
        {
            int hasSdk = 0;
            if (GEMiniPlatformAlipay.activity())
            {
                Debug.Log("[GravityEngineSDK] 有接入 Alipay 小游戏");
                hasSdk++;
            }
            if (GEMiniPlatformByteDance.activity())
            {
                Debug.Log("[GravityEngineSDK] 有接入 ByteDance 小游戏");
                hasSdk++;
            }
            if (GEMiniPlatformByteDanceTT.activity())
            {
                Debug.Log("[GravityEngineSDK] 有接入 ByteDanceTT 小游戏");
                hasSdk++;
            }
            if (GEMiniPlatformHuawei.activity())
            {
                Debug.Log("[GravityEngineSDK] 有接入 Huawei 小游戏");
                hasSdk++;
            }
            if (GEMiniPlatformKuaiShou.activity())
            {
                Debug.Log("[GravityEngineSDK] 有接入 KuaiShou 小游戏");
                hasSdk++;
            }
            if (GEMiniPlatformKuaiShouWebGl.activity())
            {
                Debug.Log("[GravityEngineSDK] 有接入 KuaiShouWebGl 小游戏");
                hasSdk++;
            }
            if (GEMiniPlatformOppo.activity())
            {
                Debug.Log("[GravityEngineSDK] 有接入 Oppo 小游戏");
                hasSdk++;
            }
            if (GEMiniPlatformVivo.activity())
            {
                Debug.Log("[GravityEngineSDK] 有接入 Vivo 小游戏");
                hasSdk++;
            }
            if (GEMiniPlatformWechat.activity())
            {
                Debug.Log("[GravityEngineSDK] 有接入 Wechat 小游戏");
                hasSdk++;
            }
            if (GEMiniPlatformXiaomi.activity())
            {
                Debug.Log("[GravityEngineSDK] 有接入 Xiaomi 小游戏");
                hasSdk++;
            }
            if (GEMiniPlatformTapTap.activity())
            {
                Debug.Log("[GravityEngineSDK] 有接入 TapTap 小游戏");
                hasSdk++;
            }
            
            if (GEPlatformWindows.activity())
            {
                Debug.Log("[GravityEngineSDK] 有接入 Windows");
                hasSdk++;
            }

            return hasSdk;
        }

        private static GEPlatformBase mGEPlatform = getGEPlatForm();

        private static GEPlatformBase getGEPlatForm()
        {
            int sdkNumber = hasDefineSdkNumber();
            if (sdkNumber > 1)
            {
                throw new ApplicationException("[GravityEngineSDK] 检测到" + sdkNumber + "个平台接入（应最多为1个），请检查宏设置是否正确！！！！！！！");
            }
            if (GEMiniPlatformAlipay.activity())
            {
                return new GEMiniPlatformAlipay();
            }
            else if (GEMiniPlatformByteDance.activity())
            {
                return new GEMiniPlatformByteDance();
            }
            else if (GEMiniPlatformByteDanceTT.activity())
            {
                return new GEMiniPlatformByteDanceTT();
            }
            else if (GEMiniPlatformHuawei.activity())
            {
                return new GEMiniPlatformHuawei();
            }
            else if (GEMiniPlatformKuaiShou.activity())
            {
                return new GEMiniPlatformKuaiShou();
            }
            else if (GEMiniPlatformKuaiShouWebGl.activity())
            {
                return new GEMiniPlatformKuaiShouWebGl();
            }
            else if (GEMiniPlatformOppo.activity())
            {
                return new GEMiniPlatformOppo();
            }
            else if (GEMiniPlatformVivo.activity())
            {
                return new GEMiniPlatformVivo();
            }
            else if (GEMiniPlatformWechat.activity())
            {
                return new GEMiniPlatformWechat();
            }
            else if (GEMiniPlatformXiaomi.activity())
            {
                return new GEMiniPlatformXiaomi();
            }
            else if (GEMiniPlatformTapTap.activity())
            {
                return new GEMiniPlatformTapTap();
            }
            else if (GEPlatformWindows.activity())
            {
                return new GEPlatformWindows();
            }
            
            return null;
        }
    }
}