using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GravitySDK.PC.Utils;
using GravitySDK.PC.GravityTurbo;
using System;
using GravitySDK.PC.Constant;
using GravityEngine.Utils;
using GravitySDK.PC.Main;


//#define GRAVITY_WECHAT_GAME_MODE
//#define ENABLE_TENCENT_SDK_TRACK
//#define ENABLE_AUTO_CHECK_DIRECT_AD_GAME_STATUS
//#define ENABLE_TENCENT_SDK_TRACK_ERROR

//#define GRAVITY_BILIBILI_GAME_MODE
//#define GRAVITY_MEITUAN_GAME_MODE


#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BILIBILI_GAME_MODE || GRAVITY_MEITUAN_GAME_MODE
using WeChatWASM;
#endif


#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
using System.Runtime.InteropServices;

/**
 * 小游戏 DN SDK 辅助类
 */
public class DnSdkHelper
{
    [DllImport("__Internal")]
    public static extern IntPtr onAppStart();

    [DllImport("__Internal")]
    public static extern IntPtr SetOpenId(string openid);

    [DllImport("__Internal")]
    public static extern IntPtr onPurchase(int purchaseValue);

    [DllImport("__Internal")]
    public static extern IntPtr onRegister();

    [DllImport("__Internal")]
    public static extern IntPtr onReActive(int backFlowDay);

    [DllImport("__Internal")]
    public static extern IntPtr onAddToWishlist(string type);

    [DllImport("__Internal")]
    public static extern IntPtr onShare(string target);

    [DllImport("__Internal")]
    public static extern IntPtr onCreateRole(string roleName);

    [DllImport("__Internal")]
    public static extern IntPtr onTutorialFinish();

    [DllImport("__Internal")]
    public static extern IntPtr onUpdateLevel(int level, int power);

    [DllImport("__Internal")]
    public static extern IntPtr onViewContent(string item);

    [DllImport("__Internal")]
    public static extern IntPtr onTrack(string eventName, string properties);
    
    [DllImport("__Internal")]
    public static extern void freeMemory(IntPtr ptr);
}

#endif


namespace GravityEngine.Platform
{
    public class GEMiniPlatformWechat : GEPlatformBase
    {

        public static bool activity()
        {
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BILIBILI_GAME_MODE || GRAVITY_MEITUAN_GAME_MODE
            return true;
#else
            return false;
#endif
        }

        public static bool IsWechat()
        {
#if GRAVITY_WECHAT_GAME_MODE
            return true;
#else
            return false;
#endif
        }
        public static bool IsBilibili()
        {
#if GRAVITY_BILIBILI_GAME_MODE
            return true;
#else
            return false;
#endif
        }

        public static bool canTrack()
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            return true;
#else
            return false;
#endif
        }

        public static bool NeedCheckWechatDirect()
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_AUTO_CHECK_DIRECT_AD_GAME_STATUS
            return true;
#else
            return false;
#endif
        }

        private static bool has_direct_callback = false;//防止重复回调
        public static void CheckDirect(Action callBack)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_AUTO_CHECK_DIRECT_AD_GAME_STATUS
            //查询只玩状态，如果非直玩，直接回调，如果正在直玩，添加监听，非直玩后再回调
            var directAdStatusInfo = WX.GetDirectAdStatusSync();
            if(directAdStatusInfo.isInMask)
            {
                has_direct_callback = false;
                WX.OnDirectAdStatusChange((result =>
                {
                    if (!result.isInMask && !has_direct_callback)
                    {
                        if (callBack != null)
                        {
                            callBack();
                            has_direct_callback = true;
                        }
                    }
                }));
            }
            else
            {
                if(callBack != null)
                {
                    callBack();
                }
            }
#else
            if (callBack != null)
            {
                callBack();
            }
#endif
        }




        public override void SetDeviceInfo()
        {
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BILIBILI_GAME_MODE || GRAVITY_MEITUAN_GAME_MODE
            // 从微信SDK获取属性信息
            var systemInfo = WX.GetSystemInfoSync();
            // 提前设置设备属性信息
            GravitySDKDeviceInfo.SetMiniGameDeviceInfo(new GEMiniGameDeviceInfo()
            {
                SDKVersion = systemInfo.SDKVersion, // 微信SDK版本号
                brand = systemInfo.brand,
                language = systemInfo.language,
                model = systemInfo.model,
                platform = systemInfo.platform,
                screenHeight = systemInfo.screenHeight,
                screenWidth = systemInfo.screenWidth,
                system = systemInfo.system,
                version = systemInfo.version, // 微信版本号
            });
#endif
        }

        public override void OnGravityStartEnd()
        {
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BILIBILI_GAME_MODE || GRAVITY_MEITUAN_GAME_MODE
            // 记录小程序/小游戏启动事件，在StartEngine并且获取network_type之后调用
            WX.GetNetworkType(new GetNetworkTypeOption()
            {
                success = (result) => { GravitySDKDeviceInfo.SetNetworkType(result.networkType); },
                fail = (result) => { GravitySDKDeviceInfo.SetNetworkType("error"); },
                complete = (result) =>
                {
                    LaunchOptionsGame launchOptionsSync = WX.GetLaunchOptionsSync();
                    GravityEngineAPI.TrackMPLaunch(launchOptionsSync.query, "" + launchOptionsSync.scene);
                    GravityEngineAPI.ReportSuperProperties("" + launchOptionsSync.scene);
                }
            });
#endif
        }



        public override string platformStr()
        {
#if GRAVITY_WECHAT_GAME_MODE
            return "wx";
#elif GRAVITY_BILIBILI_GAME_MODE
            return "bili";
#else
            return null;
#endif
        }


        public override void GetLaunchOptionsSync(Action<Dictionary<string, string>, string> callBack)
        {
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BILIBILI_GAME_MODE || GRAVITY_MEITUAN_GAME_MODE
            LaunchOptionsGame launchOptionsSync = WX.GetLaunchOptionsSync();
            var launchQuery = launchOptionsSync.query;
            var launchScene = "" + launchOptionsSync.scene;
            callBack(launchQuery,launchScene);
#endif
        }




        public override void addShowAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BILIBILI_GAME_MODE || GRAVITY_MEITUAN_GAME_MODE
            WX.OnShow((result =>
            {
                GravitySDKLogger.Print("wechat game on show");
                Dictionary<string, object> properties = propertiesCallback(AUTO_TRACK_EVENTS.MP_SHOW.ToString());
                GravityEngineAPI.TrackMPShow(result.query, "" + result.scene, properties);
            }));
#endif
        }

        public override void addHideAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BILIBILI_GAME_MODE || GRAVITY_MEITUAN_GAME_MODE
            WX.OnHide((result =>
            {
                GravitySDKLogger.Print("wechat game on hide");
                Dictionary<string, object> properties = propertiesCallback(AUTO_TRACK_EVENTS.MP_HIDE.ToString());
                GravityEngineAPI.TrackMPHide(properties);
            }));
#endif
        }


        public override void addFavoriteAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {
//#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BILIBILI_GAME_MODE || GRAVITY_MEITUAN_GAME_MODE
//            WX.OnAddToFavorites((action =>
//            {
//                GravitySDKLogger.Print("wechat game on add to favorites");
//                Dictionary<string, object> properties = propertiesCallback(AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES.ToString());
//                GravityEngineAPI.TrackMPAddFavorites(properties);
//            }));
//#endif
        }

        public override void addShareAutoTrack(Func<string, Dictionary<string, object>> propertiesCallback)
        {
//#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BILIBILI_GAME_MODE || GRAVITY_MEITUAN_GAME_MODE
//            WXShareAppMessageParam param = new WXShareAppMessageParam();
//            WX.OnShareAppMessage(param, action =>
//            {
//                action.Invoke(param);
//                Dictionary<string, object> properties = propertiesCallback(AUTO_TRACK_EVENTS.MP_SHARE.ToString());
//                GravityEngineAPI.TrackMPShare("APP_MESSAGE", properties);
//            });
//            WX.OnShareTimeline(action =>
//            {
//                Dictionary<string, object> properties = propertiesCallback(AUTO_TRACK_EVENTS.MP_SHARE.ToString());
//                GravityEngineAPI.TrackMPShare("TIME_LINE", properties);
//            });
//#endif
        }

        private void trackTencentLog(string functionStr,string action_param, Dictionary<string, object> resultDic)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK && ENABLE_TENCENT_SDK_TRACK_ERROR
            if (resultDic.TryGetValue("code", out var codeStr))
            {
                int code = Convert.ToInt32(codeStr);
                if (code != 0)
                {
                    if (resultDic.TryGetValue("message", out object error_msg))
                    {
                        Dictionary<string, object> body = new Dictionary<string, object>()
                        {
                            {"$code", code},
                            {"$error_msg", error_msg},
                            {"$action_type", functionStr},
                            {"$action_param", action_param},
                            {"$action_time", GravityPCSDK.TimeString(DateTime.Now)},
                        };
                        GravityEngineAPI.Track("$TencentSDKLog", body);
                        GravityEngineAPI.Flush();
                    }
                }
            }
#endif
        }


#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
        private object autoTrackJsResult(IntPtr ptr,string action_type,string action_param)
        {
            string resultStr = Marshal.PtrToStringUTF8(ptr);
            Dictionary<string, object> resultDic = GE_MiniJson.Deserialize(resultStr);
            DnSdkHelper.freeMemory(ptr);
            trackTencentLog(action_type, action_param, resultDic);
            return resultDic;
        }
#endif


        public override object SetOpenId(string openId)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            IntPtr ptr = DnSdkHelper.SetOpenId(openId);
            TryQueryAppConfigInfo();
            return autoTrackJsResult(ptr, openId, "setOpenId");
#else

            return null;
#endif
        }


        public static void TryQueryAppConfigInfo()
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            int hasInit = UnityEngine.PlayerPrefs.HasKey("gravity_sdk_has_initialize") ? UnityEngine.PlayerPrefs.GetInt("gravity_sdk_has_initialize") : 0;
            if (hasInit == 1)
            {
                //没初始化过，不请求演练模式（SetOpenId 先于 initiate）
                GravityHelper.QueryAppConfigInfo(GravityHelper.UseParams());
            }
#endif
        }

        /// <summary>
        /// 在用户分享小游戏的时候上报，需区分是【转发给朋友】还是【分享到朋友圈】
        /// </summary>
        /// <param name="target">shareType 分享类型：APP_MESSAGE / TIME_LINE</param>
        /// <param name="properties">默认传null</param>
        public override object TrackMPShare(string target, Dictionary<string, object> properties = null)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk onShare " + target);
            IntPtr ptr = DnSdkHelper.onShare(target);
            return autoTrackJsResult(ptr, target, "onShare");
#else
            return null;
#endif
        }

        /// <summary>
        /// 在用户收藏小游戏的时候上报，包括收藏、添加到我的小程序、添加到桌面以及小游戏自己定义的收藏逻辑。
        /// </summary>
        /// <param name="wishType">wishType 类型枚举值：普通收藏（default）/添加到我的小程序（my）/添加到桌面（desktop）/其他</param>
        public override object TrackMPAddFavorites(string wishType = "default")
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk onAddToWishlist " + wishType);
            IntPtr ptr = DnSdkHelper.onAddToWishlist(wishType);
            return autoTrackJsResult(ptr, wishType, "onAddToWishlist");
#else
            return null;
#endif
        }

        /// <summary>
        /// 用户在小游戏内创建角色成功后上报 CREATE_ROLE 行为，可添加创建角色相关的自定义参数，如角色名等。
        /// </summary>
        /// <param name="roleName">角色名</param>
        public override object TrackMPCreateRole(string roleName)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk onCreateRole " + roleName);
            IntPtr ptr = DnSdkHelper.onCreateRole(roleName);
            return autoTrackJsResult(ptr, roleName, "onCreateRole");
#else
            return null;
#endif
        }

        /// <summary>
        /// 在用户完成游戏新手指引教程或者完成教程关卡后上报 TUTORIAL_FINISH 行为。
        /// </summary>
        public override object TrackMPTutorialFinish()
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk onTutorialFinish");
            IntPtr ptr = DnSdkHelper.onTutorialFinish();
            return autoTrackJsResult(ptr,"", "onTutorialFinish");
#else
            return null;
#endif
        }


        /// <summary>
        /// 用户在小游戏内的完成游戏等级提升时上报 UPDATE_LEVEL 行为，可添加自定义参数，如当前游戏等级、游戏能量等。
        /// </summary>
        /// <param name="level">游戏等级</param>
        /// <param name="power">游戏能量</param>
        public override object TrackMPUpdateLevel(int level, int power)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk onUpdateLevel " + level + " " + power);
            IntPtr ptr = DnSdkHelper.onUpdateLevel(level, power);
            return autoTrackJsResult(ptr, level + "-" + power , "onUpdateLevel");
#else
            return null;
#endif
        }


        /// <summary>
        /// 用户在小游戏内浏览商城页面时上报。
        /// </summary>
        public override object TrackMPViewMallContent()
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk onViewContentMall");
            IntPtr ptr = DnSdkHelper.onViewContent("Mall");
            return autoTrackJsResult(ptr,"","onViewContentMall");
#else
            return null;
#endif
        }

        /// <summary>
        /// 用户在小游戏内浏览活动页面时上报。
        /// </summary>
        public override object TrackMPViewActivityContent()
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk onViewContentActivity");
            IntPtr ptr = DnSdkHelper.onViewContent("Activity");
            return autoTrackJsResult(ptr, "", "onViewContentActivity");
#else
            return null;
#endif
        }

        /// <summary>
        /// 上报付费事件到腾讯SDK
        /// </summary>
        /// <param name="amount">付费金额 单位为分</param>
        public override object onPurchase(int amount)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            // 付费
            Debug.Log("tencent sdk onPurchase " + amount);
            IntPtr ptr = DnSdkHelper.onPurchase(amount);
            return autoTrackJsResult(ptr, ""+ amount, "onPurchase");
#else
            return null;
#endif
        }

        public override object onRegister()
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk onRegister");
            IntPtr ptr = DnSdkHelper.onRegister();
            return autoTrackJsResult(ptr, "", "onRegister");
#else
            return null;
#endif
        }

        public override object onReActive(int backFlowDay)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk onReActive " + backFlowDay);
            IntPtr ptr = DnSdkHelper.onReActive(backFlowDay);
            return autoTrackJsResult(ptr, "" + backFlowDay, "onReActive");
#else
            return null;
#endif
        }



        public override object TrackMPAppStart()
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk TrackMPAppStart ");
            IntPtr ptr = DnSdkHelper.onAppStart();
            return autoTrackJsResult(ptr, "", "onAppStart");
#else
            return null;
#endif
        }

        public override object TrackMPLoadFinish()
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk TrackMPLoadFinish ");
            IntPtr ptr = DnSdkHelper.onTrack("LOAD_FINISH","{}");
            return autoTrackJsResult(ptr, "{}", "onLoadFinish");
#else
            return null;
#endif
        }

        public override object TrackMPSubscribe()
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk TrackMPSubscribe ");
            IntPtr ptr = DnSdkHelper.onTrack("SUBSCRIBE", "{}");
            return autoTrackJsResult(ptr, "{}", "onSubscribe");
#else
            return null;
#endif
        }

        public override object TrackMPTutorialStart()
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk TrackMPTutorialStart ");
            IntPtr ptr = DnSdkHelper.onTrack("TUTORIAL_START", "{}");
            return autoTrackJsResult(ptr, "{}", "onTutorialStart");
#else
            return null;
#endif
        }


        public override object TrackMPLevelEnter(Dictionary<string, object> properties)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk TrackMPLevelEnter ");

            string action_param = GE_MiniJson.Serialize(properties);
            IntPtr ptr = DnSdkHelper.onTrack("LEVEL_ENTER", action_param);
            return autoTrackJsResult(ptr, action_param, "onLevelEnter");
#else
            return null;
#endif
        }

        public override object TrackMPLevelExit(Dictionary<string, object> properties)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk TrackMPLevelExit ");

            string action_param = GE_MiniJson.Serialize(properties);
            IntPtr ptr = DnSdkHelper.onTrack("LEVEL_EXIT", action_param);
            return autoTrackJsResult(ptr, action_param, "onLevelExit");
#else
            return null;
#endif
        }

        public override object TrackMPLevelClose(Dictionary<string, object> properties)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk TrackMPLevelClose ");

            string action_param = GE_MiniJson.Serialize(properties);
            IntPtr ptr = DnSdkHelper.onTrack("LEVEL_LOSE", action_param);
            return autoTrackJsResult(ptr, action_param, "onLevelClose");
#else
            return null;
#endif
        }

        //level_id 必须 关卡通关时的关卡ID
        //ad_cnt 必须  关关卡通关时，用户在关卡中点击、完成广告观看的次数
        //items 使用道具信息，包括道具类型和使用数量  按照最小循环关卡*计数；道具类型枚举：例如道具是炸弹，在以下枚举列表里，对应是4，则上报4，道具是重置，则报22（https://doc.weixin.qq.com/sheet/e3_AXsA9gYhADkCNqA5dxnhFTsi1GSAA?scode=AJEAIQdfAAoiOxtxQKAXsA9gYhADk&tab=BB08J2）
        //game_mode 进入的游戏模式（名称）
        //enter_level_id 进入的关卡进度（0-1之间，保留两位小数）
        //duration 关卡通关时的用户时长
        //chapter_id 关卡通关时的章节ID
        //coin_amount 关卡通关时的金币数
        //stamina_value 关卡通关时的体力值
        //level_value 关卡通关时的等级值
        public override object TrackMPLevelPass(Dictionary<string, object> properties)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk TrackMPLevelPass ");

            string action_param = GE_MiniJson.Serialize(properties);
            IntPtr ptr = DnSdkHelper.onTrack("LEVEL_PASS", action_param);
            return autoTrackJsResult(ptr, action_param, "onLevelPass");
#else
            return null;
#endif
        }


        //激励点位出现 
        //ad_placement_type 激励视频广告行为事件;例如1代表复活，2代表体力，当复活广告位出现被点击时上报1枚举见:https://doc.weixin.qq.com/sheet/e3_AQAA_gboAOACNEgqhOXynRwqq0gNv?scode=AJEAIQdfAAo7woEvatAQAA_gboAOA&tab=BB08J2
        public override object TrackMPAdPlacementShow(int ad_placement_type)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk TrackMPAdPlacementShow ");

            Dictionary<string, object> properties = new Dictionary<string, object>()
                        {
                            {"ad_placement_name", ad_placement_type},
                        };
            string action_param = GE_MiniJson.Serialize(properties);
            IntPtr ptr = DnSdkHelper.onTrack("AD_PLACEMENT_SHOW", action_param);
            return autoTrackJsResult(ptr, action_param, "onAdPlacementShow");
#else
            return null;
#endif
        }

        //广告位点击 
        //ad_placement_type 激励视频广告行为事件;例如1代表复活，2代表体力，当复活广告位出现被点击时上报1枚举见:https://doc.weixin.qq.com/sheet/e3_AQAA_gboAOACNEgqhOXynRwqq0gNv?scode=AJEAIQdfAAo7woEvatAQAA_gboAOA&tab=BB08J2
        public override object TrackMPAdPlacementClick(int ad_placement_type)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk TrackMPAdPlacementClick ");

            Dictionary<string, object> properties = new Dictionary<string, object>()
                        {
                            {"ad_placement_name", ad_placement_type},
                        };
            string action_param = GE_MiniJson.Serialize(properties);
            IntPtr ptr = DnSdkHelper.onTrack("AD_CLICK", action_param);
            return autoTrackJsResult(ptr, action_param, "onAdPlacementClick");
#else
            return null;
#endif
        }

        //广告播放结束
        //ad_placement_type 激励视频广告行为事件;例如1代表复活，2代表体力，当复活广告位出现被点击时上报1枚举见:https://doc.weixin.qq.com/sheet/e3_AQAA_gboAOACNEgqhOXynRwqq0gNv?scode=AJEAIQdfAAo7woEvatAQAA_gboAOA&tab=BB08J2
        public override object TrackMPAdPlacementVideoFinish(int ad_placement_type)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk TrackMPAdPlacementVideoFinish");

            Dictionary<string, object> properties = new Dictionary<string, object>()
                        {
                            {"ad_placement_name", ad_placement_type},
                        };
            string action_param = GE_MiniJson.Serialize(properties);
            IntPtr ptr = DnSdkHelper.onTrack("AD_VIDEO_FINISH", action_param);
            return autoTrackJsResult(ptr, action_param, "onAdPlacementVideoFinish");
#else
            return null;
#endif
        }

        //广告曝光
        //ad_type 广告位类型; 1-激励视频，2-插屏，3-格子，4-banner
        public override object TrackMPAdPlacementVideoShow(int ad_type)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            Debug.Log("tencent sdk TrackMPAdPlacementVideoShow ");

            Dictionary<string, object> properties = new Dictionary<string, object>()
                        {
                            {"ad_type", ad_type},
                        };
            string action_param = GE_MiniJson.Serialize(properties);
            IntPtr ptr = DnSdkHelper.onTrack("AD_IMPRESSION", action_param);
            return autoTrackJsResult(ptr, action_param, "onAdPlacementVideoShow");
#else
            return null;
#endif
        }

        //其他事件上报
        public override object TrackMPEvent(string eventName, Dictionary<string, object> properties = null)
        {
#if GRAVITY_WECHAT_GAME_MODE && ENABLE_TENCENT_SDK_TRACK
            if (properties == null)
            {
                properties = new Dictionary<string, object>();
            }
            string action_param = GE_MiniJson.Serialize(properties);
            IntPtr ptr = DnSdkHelper.onTrack(eventName, action_param);
            return autoTrackJsResult(ptr, action_param, "onTrackMPEvent-" + eventName);
#else
            return null;
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
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BILIBILI_GAME_MODE || GRAVITY_MEITUAN_GAME_MODE
            if (value.GetType() == typeof(int))
            {
                PlayerPrefs.SetInt(key, (int)value);
            }
            else if (value.GetType() == typeof(float))
            {
                PlayerPrefs.SetFloat(key, (float)value);
            }
            else if (value.GetType() == typeof(string))
            {
                PlayerPrefs.SetString(key, (string)value);
            }
            PlayerPrefs.Save();
#endif
        }

        public override object GetData(string key, Type type)
        {
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BILIBILI_GAME_MODE || GRAVITY_MEITUAN_GAME_MODE
            if (!string.IsNullOrEmpty(key) && PlayerPrefs.HasKey(key))
            {
                if (type == typeof(int))
                {
                    return PlayerPrefs.GetInt(key);
                }
                else if (type == typeof(float))
                {
                    return PlayerPrefs.GetFloat(key);
                }
                else if (type == typeof(string))
                {
                    return PlayerPrefs.GetString(key);
                }
                PlayerPrefs.Save();
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
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BILIBILI_GAME_MODE || GRAVITY_MEITUAN_GAME_MODE
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
            }
#endif
        }
        public override bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BILIBILI_GAME_MODE || GRAVITY_MEITUAN_GAME_MODE
            return PlayerPrefs.HasKey(key);
#else
            return false;
#endif
        }
    }
}