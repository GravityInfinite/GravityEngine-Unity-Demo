/*
 * 
    Copyright 2019, GravityEngine, Inc
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
 */

using System;
using System.Collections.Generic;
using GravitySDK.PC.GravityTurbo;
using GravityEngine.Utils;
using GravityEngine.Wrapper;
using UnityEngine;
using GravitySDK.PC.Constant;
using GravitySDK.PC.Storage;
using GravitySDK.PC.Time;
using GravitySDK.PC.Utils;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using GravityEngine.Platform;


namespace GravityEngine
{
    [DisallowMultipleComponent]
    public class GravityEngineAPI : MonoBehaviour
    {
        #region settings

        [System.Serializable]
        public struct Token
        {
            public string accessToken;
            public string channel;
            public string clientId;
            public SDKRunMode mode;
            public SDKTimeZone timeZone;
            public bool isChina; //是否中国国内，默认true
            public bool enableCrashToGe; //是否上报SDK报错信息到引力
            public bool enableCrashTo3D; //是否上报SDK报错信息到三方

            public string timeZoneId;

            // 加密 仅支持Android/iOS
            public bool enableEncrypt; // 开启加密传输，默认false
            public int encryptVersion; // 密钥版本号
            public string encryptPublicKey; // 加密公钥
            public SSLPinningMode pinningMode; // SSL证书验证模式，默认NONE
            public bool allowInvalidCertificates; // 是否允许自建证书或者过期SSL证书，默认false
            public bool validatesDomainName; // 是否验证证书域名，默认true
            private string instanceName; // 实例名

            // id 采集开关控制，只支持Android
            public bool enableIMEI;
            public bool enableOAID;
            public bool enableAndroidId;
            public bool enableMAC;

            public bool isGDPRArea;
            public bool isCoppaEnabled;
            public bool isKidsAppEnabled;
            public bool adPersonalizationEnabled;
            public bool adUserDataEnabled;
            public string fbAppID;

            public List<AndroidClientIdType> mClientIdPriorityOrder;

            //前台会话阀值（频繁切换前后台，AppEnd时间会很小，可以设置这个值忽略这种操作）,当AppEnd发生时，事件少于设置的time，会忽略计时
            public int foregroundSessionThreshold;


            public Token(string accessToken, string clientId, string channel)
            {
                this.accessToken = accessToken.Replace(" ", "");
                this.clientId = clientId.Replace(" ", "");
                this.channel = channel.Replace(" ", "");
                this.mode = SDKRunMode.NORMAL;
                this.timeZone = SDKTimeZone.Local;
                this.isChina = true;
                this.enableCrashToGe = false;
                this.enableCrashTo3D = false;

                this.timeZoneId = null;
                this.enableEncrypt = false;
                this.enableIMEI = true;
                this.enableOAID = true;
                this.enableAndroidId = true;
                this.enableMAC = true;
                this.encryptVersion = 0;
                this.encryptPublicKey = null;
                this.pinningMode = SSLPinningMode.NONE;
                this.allowInvalidCertificates = false;
                this.validatesDomainName = true;
                this.instanceName = null;

                this.isGDPRArea = false;
                this.isCoppaEnabled = false;
                this.isKidsAppEnabled = false;
                this.adPersonalizationEnabled = false;
                this.adUserDataEnabled = false;
                this.fbAppID = "";

                this.mClientIdPriorityOrder = null;

                this.foregroundSessionThreshold = 0;
            }

            public string GetInstanceName()
            {
                return this.instanceName;
            }

            public string getTimeZoneId()
            {
                switch (timeZone)
                {
                    case SDKTimeZone.UTC:
                        return "UTC";
                    case SDKTimeZone.Asia_Shanghai:
                        return "Asia/Shanghai";
                    case SDKTimeZone.Asia_Tokyo:
                        return "Asia/Tokyo";
                    case SDKTimeZone.America_Los_Angeles:
                        return "America/Los_Angeles";
                    case SDKTimeZone.America_New_York:
                        return "America/New_York";
                    case SDKTimeZone.Other:
                        return timeZoneId;
                    default:
                        break;
                }

                return null;
            }

            public List<int> mClientIdPriorityOrderInts()
            {
                List<int> result = new List<int>();
                if (this.mClientIdPriorityOrder != null)
                {
                    foreach (AndroidClientIdType type in this.mClientIdPriorityOrder)
                    {
                        result.Add((int)type);
                    }
                }
                return result;
            }
        }

        public enum SDKTimeZone
        {
            Local,
            UTC,
            Asia_Shanghai,
            Asia_Tokyo,
            America_Los_Angeles,
            America_New_York,
            Other = 100
        }

        public enum SDKRunMode
        {
            NORMAL = 0,
            DEBUG = 1
        }

        public enum NetworkType
        {
            DEFAULT = 0,
            WIFI = 1,
            ALL = 2
        }

        public enum AndroidClientIdType
        {
            OAID = 0,
            ANDROID_ID = 1
        }


        [Header("Configuration")] [Tooltip("是否手动初始化SDK")]
        public bool startManually = true;

        [Tooltip("设置网络类型")] public NetworkType networkType = NetworkType.DEFAULT;

        [Tooltip("项目相关配置, APP ID 会在项目申请时给出")] public Token[] tokens = new Token[1];

        #endregion

        /// <summary>
        /// 设置自定义访客 ID，用于替换系统生成的访客 ID
        /// </summary>
        /// <param name="firstId">访客 ID</param>
        private static void Identify(string firstId)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.Identify(firstId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {firstId};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        private static bool send_sdk_error_to_ge = false;
        private static bool send_sdk_error_to_3d = false;

        private static long last_sent_error_time = 0;
        private static int has_sent_error_count = 0;


        public static void TrackSdkError(string reasonStr, Dictionary<string, object> result)
        {
            long nowTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (nowTime - last_sent_error_time < 15)
            {
                return;
            }
            if (has_sent_error_count >= 10)
            {
                return;
            }
            last_sent_error_time = nowTime;
            has_sent_error_count += 1;


            if (result == null)
            {
                result = new Dictionary<string, object>();
            }
            if (send_sdk_error_to_ge)
            {
                try
                {
                    Dictionary<string, object> properties = new Dictionary<string, object>()
                        {
                            {GravitySDKConstant.CRASH_REASON, reasonStr + " "+ GravitySDKJSON.Serialize(result)}
                        };
                    GE_PropertiesChecker.MergeProperties(result, properties);
                    GravityEngineAPI.Track(GravitySDKConstant.CRASH_EVENT, properties);
                    GravityEngineAPI.Flush();
                }
                catch (Exception e)
                {

                }
            }
            if (send_sdk_error_to_3d)
            {
                try
                {
                    Dictionary<string, object> posthogChildProperties = new Dictionary<string, object>()
                        {
                            {"distinct_id", GetCurrentClientID()},
                            {"$lib", GravitySDKAppInfo.LibName()+"-"+GravitySDKAppInfo.LibVersion()},
                            {"GE_LIB", GravitySDKAppInfo.LibName()},
                            {"GE_LIB_VERSION", GravitySDKAppInfo.LibVersion()},
                            {"GE_TOKEN", GravitySDKUtil.MD5Str(GravityHelper.GetAccessToken())},
                        };
                    GE_PropertiesChecker.MergeProperties(result, posthogChildProperties);

                    Dictionary<string, object> posthogProperties = new Dictionary<string, object>()
                        {
                            {"api_key", "phc_tcHf8WBy2imjcXNmnnyqEBfayZHKLjuVra2nEUm98GqL"},
                            {"event", reasonStr},
                            {"timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")},
                            {"properties", posthogChildProperties}
                        };
                    UnityWebRequestMgr.Instance.SimplePost("https://us.i.posthog.com/capture/", posthogProperties,
                    (request =>
                    {
                        string responseText = request.downloadHandler.text;
                        Dictionary<string, object> res = GE_MiniJson.Deserialize(responseText);
                        Debug.Log("posthog response is " + responseText);
                    }));
                }
                catch (Exception e)
                {

                }
            }
        }

        /// <summary>
        /// 返回当前的访客 ID.
        /// </summary>
        /// <returns>访客 ID</returns>
        public static string GetDistinctId()
        {
            if (tracking_enabled)
            {
                return GravityWrapperManager.GetDistinctId();
            }

            return null;
        }

        /// <summary>
        /// 设置账号 ID. 该方法不会上传用户登录事件，仅仅会保存AccountID做后续的上报.
        /// </summary>
        /// <param name="account">账号 ID</param>
        public static void Login(string account)
        {
            // if (tracking_enabled)
            // {
            //     GravityWrapperManager.Login(account);
            // }
            // else
            // {
            //     System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
            //     object[] parameters = new object[] {account};
            //     eventCaches.Add(new Dictionary<string, object>()
            //     {
            //         {"method", method},
            //         {"parameters", parameters}
            //     });
            // }
        }

        /// <summary>
        /// 清空账号 ID. 该方法会上传用户登出事件.
        /// </summary>
        public static void Logout(ILogoutCallback logoutCallback)
        {
            // if (tracking_enabled)
            // {
            //     GravityWrapperManager.Logout(logoutCallback);
            // }
            // else
            // {
            //     System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
            //     object[] parameters = new object[] { };
            //     eventCaches.Add(new Dictionary<string, object>()
            //     {
            //         {"method", method},
            //         {"parameters", parameters}
            //     });
            // }
        }

        /// <summary>
        /// 开启自动采集功能.
        /// </summary>
        /// <param name="events">自动采集事件</param>
        /// <param name="properties">自动采集事件扩展属性(可选)</param>
        public static void EnableAutoTrack(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties = null)
        {
            if (tracking_enabled)
            {
                if (properties == null)
                {
                    properties = new Dictionary<string, object>();
                }

                GravityWrapperManager.EnableAutoTrack(events, properties);
                // C#异常捕获提前，包含所有端
                if ((events & AUTO_TRACK_EVENTS.APP_CRASH) != 0 && !GE_PublicConfig.DisableCSharpException)
                {
                    GravityEngineExceptionHandler.RegisterGEExceptionHandler(properties);
                }

                if ((events & AUTO_TRACK_EVENTS.APP_SCENE_LOAD) != 0)
                {
                    SceneManager.sceneLoaded += GravityEngineAPI.OnSceneLoaded;
                }

                if ((events & AUTO_TRACK_EVENTS.APP_SCENE_UNLOAD) != 0)
                {
                    SceneManager.sceneUnloaded += GravityEngineAPI.OnSceneUnloaded;
                }
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {events, properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置自动采集扩展属性.
        /// </summary>
        /// <param name="events">自动采集事件</param>
        /// <param name="properties">自动采集事件扩展属性</param>
        public static void SetAutoTrackProperties(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.SetAutoTrackProperties(events, properties);
                // C#异常捕获提前，包含所有端
                if ((events & AUTO_TRACK_EVENTS.APP_CRASH) != 0 && !GE_PublicConfig.DisableCSharpException)
                {
                    GravityEngineExceptionHandler.SetAutoTrackProperties(properties);
                }
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {events, properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 上报微信小游戏事件：MPLaunch、MPShow
        /// </summary>
        /// <param name="query"></param>
        /// <param name="scene"></param>
        private static void TrackMPEvent(string eventName, Dictionary<string, string> query, string scene,
            Dictionary<string, object> properties)
        {
            // join query's key and value with =, and join all query with &, ignore the last &
            string queryStr = "";
            Dictionary<string, string> modified_query = GravityHelper.ReSetAdParams(query);
            foreach (KeyValuePair<string, string> kv in modified_query)
            {
                queryStr += Uri.EscapeDataString(kv.Key) + "=" + Uri.EscapeDataString(kv.Value) + "&";
            }

            queryStr = queryStr.Length > 0 ? queryStr.Substring(0, queryStr.Length - 1) : "";
            if (properties == null)
            {
                properties = new Dictionary<string, object>();
            }

            properties.Add("$url_query", queryStr);
            properties.Add(GravitySDKConstant.SCENE, "" + scene);
            Track(eventName, properties);
        }

        /// <summary>
        /// 上报小游戏启动事件 MPLaunch
        /// </summary>
        /// <param name="query"></param>
        /// <param name="scene"></param>
        public static void TrackMPLaunch(Dictionary<string, string> query, string scene)
        {
            TrackMPEvent(GravitySDKConstant.MP_LAUNCH, query, scene, null);
            Flush();
        }

        /// <summary>
        /// 上报微信小游戏显示事件 MPShow
        /// </summary>
        /// <param name="query"></param>
        /// <param name="scene"></param>
        public static void TrackMPShow(Dictionary<string, string> query, string scene,
            Dictionary<string, object> properties)
        {
            TrackMPEvent(GravitySDKConstant.MP_SHOW, query, scene, properties);
            // $MPHide开始计时
            TimeEvent(GravitySDKConstant.MP_HIDE);
        }

        /// <summary>
        /// 上报微信小游戏退出事件 MPHide
        /// </summary>
        public static void TrackMPHide(Dictionary<string, object> properties)
        {
            Track(GravitySDKConstant.MP_HIDE, properties);
        }

        /// <summary>
        /// 在用户分享小游戏的时候上报，需区分是【转发给朋友】还是【分享到朋友圈】
        /// </summary>
        /// <param name="target">shareType 分享类型：APP_MESSAGE / TIME_LINE</param>
        /// <param name="properties">默认传null</param>
        public static object TrackMPShare(string target, Dictionary<string, object> properties = null)
        {
            object result = GEPlatformManager.TrackMPShare(target, properties);
            if (properties == null)
            {
                properties = new Dictionary<string, object>();
                properties["$share_depth"] = 1;
                properties["$share_method"] = "转发消息卡片";
            }
            properties["$share_target"] = target;
            Track(GravitySDKConstant.MP_SHARE, properties);
            return result;
        }

        /// <summary>
        /// 在用户收藏小游戏的时候上报，包括收藏、添加到我的小程序、添加到桌面以及小游戏自己定义的收藏逻辑。
        /// </summary>
        /// <param name="wishType">wishType 类型枚举值：普通收藏（default）/添加到我的小程序（my）/添加到桌面（desktop）/其他</param>
        /// <returns>当前调用腾讯sdk track 事件的结果（如果有），没有会返回null
        public static object TrackMPAddFavorites(string wishType = "default")
        {
            object result = GEPlatformManager.TrackMPAddFavorites(wishType);
            Track(GravitySDKConstant.MP_ADD_FAVORITES);
            Flush();
            return result;
        }
        
        /// <summary>
        /// 用户在小游戏内创建角色成功后上报 CREATE_ROLE 行为，可添加创建角色相关的自定义参数，如角色名等。
        /// </summary>
        /// <param name="roleName">角色名</param>
        /// <returns>当前调用腾讯sdk track 事件的结果（如果有），没有会返回null
        public static object TrackMPCreateRole(string roleName)
        {
            object result = GEPlatformManager.TrackMPCreateRole(roleName);
            var properties = new Dictionary<string, object>()
            {
                {"$role_name", roleName},
            };
            Track("$CreateRole", properties);
            Flush();
            return result;
        }
        
        public static void TryCreateRoleEventDryRun(string roleName)
        {
            if (GravityHelper.IsDryRunModeEnabled())
            {
                GravityHelper.TryDryRunMode(specificAction: "create_role");
            }
            else
            {
                TrackMPCreateRole(roleName);
            }
        }
        
        /// <summary>
        /// 在用户完成游戏新手指引教程或者完成教程关卡后上报 TUTORIAL_FINISH 行为。
        /// </summary>
        /// <returns>当前调用腾讯sdk track 事件的结果（如果有），没有会返回null
        public static object TrackMPTutorialFinish()
        {
            object result = GEPlatformManager.TrackMPTutorialFinish();
            Track("$TutorialFinish");
            Flush();
            return result;
        }
        
        public static void TryTutorialFinishEventDryRun()
        {
            if (GravityHelper.IsDryRunModeEnabled())
            {
                GravityHelper.TryDryRunMode(specificAction:"tutorial_finish");
            }
            else
            {
                TrackMPTutorialFinish();
            }
        }
        
        public static void TryPayEventDryRun(int amount)
        {
            if (GravityHelper.IsDryRunModeEnabled())
            {
                GravityHelper.TryDryRunMode(specificAction:"pay");
            }
            else
            {
                GEPlatformManager.onPurchase(amount);
            }
        }
        
        /// <summary>
        /// 用户在小游戏内的完成游戏等级提升时上报 UPDATE_LEVEL 行为，可添加自定义参数，如当前游戏等级、游戏能量等。
        /// </summary>
        /// <param name="level">游戏等级</param>
        /// <param name="power">游戏能量</param>
        public static object TrackMPUpdateLevel(int level, int power)
        {
            object result = GEPlatformManager.TrackMPUpdateLevel(level, power);
            var properties = new Dictionary<string, object>()
            {
                {"$user_level", level},
                {"$user_power", power},
            };
            Track("$UpdateLevel", properties);
            Flush();
            return result;
        }
        
        /// <summary>
        /// 用户在小游戏内浏览商城页面时上报。
        /// </summary>
        public static object TrackMPViewMallContent()
        {
            object result = GEPlatformManager.TrackMPViewMallContent();
            Track("$ViewMallContent");
            Flush();
            return result;
        }     
        
        /// <summary>
        /// 用户在小游戏内浏览活动页面时上报。
        /// </summary>
        public static object TrackMPViewActivityContent()
        {
            object result = GEPlatformManager.TrackMPViewActivityContent();
            Track("$ViewActivityContent");
            Flush();
            return result;
        }

        /// <summary>
        /// 绑定数数账号
        /// </summary>
        /// <param name="taAccountId"></param>      当前用户的数数账户 ID (#account_id)
        /// <param name="taDistinctId"></param>     当前用户的数数访客 ID (#distinct_id)
        public static void BindTAThirdPlatform(string taAccountId, string taDistinctId)
        {
            if (GravitySDKUtil.IsEmptyString(taAccountId) && GravitySDKUtil.IsEmptyString(taDistinctId))
            {
                GE_Log.e("taAccountId or taDistinctId must be required");
                return;
            }
            GravityWrapperManager.BindTAThirdPlatform(taAccountId, taDistinctId);
        }

        /// <summary>
        /// 仅上报付费事件到引力后端
        /// </summary>
        /// <param name="payAmount"></param>            付费金额 单位为分
        /// <param name="payType"></param>              付费类型 按照国际标准组织ISO 4217中规范的3位字母，例如CNY人民币、USD美金等
        /// <param name="orderId"></param>              订单号
        /// <param name="payReason"></param>            付费原因 例如：购买钻石、办理月卡
        /// <param name="payMethod"></param>            付费方式 例如：支付宝、微信、银联等
        /// <returns>当前事件的traceId，当使用演练模式时，需要保存下来以供演练模式使用</returns>
        public static string TrackPayEvent(int payAmount, string payType, string orderId, string payReason,
            string payMethod)
        {
            return GravityWrapperManager.TrackPayEvent(payAmount, payType, orderId, payReason, payMethod);
        }
        
        /// <summary>
        /// 仅上报付费事件到腾讯SDK
        /// </summary>
        /// <param name="payAmount">付费金额 单位为分</param>
        /// <returns>当前调用腾讯sdk track 事件的结果（如果有），没有会返回null
        public static object TrackPayEventToTencent(int payAmount)
        {
            Debug.Log("only to Tencent pay " + payAmount);
            return GEPlatformManager.onPurchase(payAmount);
        }
        
        /// <summary>
        /// 腾讯小游戏自定义付费回传
        /// </summary>
        /// <param name="payAmount">付费金额 单位为分</param>
        /// <param name="traceId">订单TrackPayEvent返回的的traceId</param>
        public static void FilterAndReportPayEventToTencent(int payAmount, string traceId)
        {
            if (GEPlatformManager.CanFilterAndReportPayEventToTencent())
            {
                GravityWrapperManager.FilterAndReportPayEventToTencent(payAmount, traceId);
            }
        }

        /// <summary>
        /// 上报原生App广告观看事件 AdShow
        /// </summary>
        /// <param name="adUnionType"></param>          广告聚合平台类型  （取值为：topon、gromore、admore、self，分别对应Topon、Gromore、Admore、自建聚合）
        /// <param name="adPlacementId"></param>        广告瀑布流ID
        /// <param name="adSourceId"></param>           广告源ID
        /// <param name="adType"></param>               广告类型 （取值为：reward、banner、 native 、interstitial、 splash ，分别对应激励视频广告、横幅广告、信息流广告、插屏广告、开屏广告）
        /// <param name="adnType"></param>              广告平台类型（取值为：csj、gdt、ks、 mint 、baidu，分别对应为穿山甲、优量汇、快手联盟、Mintegral、百度联盟）
        /// <param name="ecpm"></param>                 预估ECPM价格（单位为元）
        public static void TrackNativeAppAdShowEvent(string adUnionType, string adPlacementId, string adSourceId,
            string adType, string adnType, float ecpm)
        {
            GravityWrapperManager.TrackNativeAppAdShowEvent(adUnionType, adPlacementId, adSourceId, adType, adnType,
                ecpm);
        }

        /// <summary>
        /// 上报微信小游戏广告观看事件 AdShow
        /// </summary>
        /// <param name="adType"></param>               广告类型 取值为：reward、banner、native、interstitial、video_feed、video_begin，分别对应：激励视频广告、Banner广告、原生模板广告、插屏广告、视频广告、视频贴片广告
        /// <param name="adUnitId"></param>             广告位ID
        /// <param name="otherProperties"></param>      其他需要携带的自定义参数
        ///
        public static void TrackWechatAdShowEvent(string adType, string adUnitId,
            Dictionary<string, object> otherProperties = null)
        {
            var properties = new Dictionary<string, object>()
            {
                {"$ad_type", adType},
                {"$ad_unit_id", adUnitId},
                {"$adn_type", "wechat"}
            };
            GE_PropertiesChecker.MergeProperties(otherProperties, properties);
            Track("$AdShow", properties);
            Flush();
        }
        
        /// <summary>
        /// 上报小游戏、快游戏广告观看事件 AdShow
        /// </summary>
        /// <param name="adType"></param>               广告类型 取值为：reward、banner、native、interstitial、video_feed、video_begin，分别对应：激励视频广告、Banner广告、原生模板广告、插屏广告、视频广告、视频贴片广告
        /// <param name="adUnitId"></param>             广告位ID
        /// <param name="otherProperties"></param>      其他需要携带的自定义参数
        ///
        public static void TrackMiniGameAdShowEvent(string adType, string adUnitId,
            Dictionary<string, object> otherProperties = null)
        {
            if (GEPlatformManager.CanTrackMiniGameAdShowEvent()) {
                var properties = new Dictionary<string, object>()
                {
                    {"$ad_type", adType},
                    {"$ad_unit_id", adUnitId},
                    {"$adn_type", GEPlatformManager.GetTrackMiniGameAdShowType()}
                };
                GE_PropertiesChecker.MergeProperties(otherProperties, properties);
                Track("$AdShow", properties);
                Flush();
            }
        }
        
        /// <summary>
        /// 上报微信小游戏登录事件 $MPLogin
        /// </summary>
        public static void TrackMPLogin()
        {
            Track("$MPLogin");
            Flush();
        }
        
        /// <summary>
        /// 上报微信小游戏注册事件 $MPRegister
        /// </summary>
        public static void TrackMPRegister()
        {
            Track("$MPRegister");
            Flush();
        }
        
        /// <summary>
        /// 上报App注册事件 $AppRegister
        /// </summary>
        public static void TrackAppRegister()
        {
            Track("$AppRegister");
            Flush();
        }

        /// <summary>
        /// 上报微信小游戏退出登录事件 $MPLogout
        /// </summary>
        public static void TrackMPLogout()
        {
            Track("$MPLogout");
            Flush();
        }

        /// <summary>
        /// 上报微信AppStart：
        /// </summary>
        public static object TrackMPAppStart()
        {
            return GEPlatformManager.TrackMPAppStart();
        }

        /// <summary>
        /// 上报微信LoadFinish：
        /// </summary>
        public static object TrackMPLoadFinish()
        {
            object result = GEPlatformManager.TrackMPLoadFinish();
            Track("$LoadFinish");
            Flush();
            return result;
        }


        /// <summary>
        /// 上报微信Subscribe：
        /// </summary>
        public static object TrackMPSubscribe()
        {
            object result = GEPlatformManager.TrackMPSubscribe();
            Track("$Subscribe");
            Flush();
            return result;
        }

        /// <summary>
        /// 上报微信TutorialStart：
        /// </summary>
        public static object TrackMPTutorialStart()
        {
            object result = GEPlatformManager.TrackMPTutorialStart();
            Track("$TutorialStart");
            Flush();
            return result;
        }

        /// <summary>
        /// 上报微信 进入关卡：
        //level_id 必须 进入的关卡ID
        //enter_level_name 游戏关卡名称
        //game_mode 进入的游戏模式（名称）
        //enter_level_id 进入的关卡进度（0-1之间，保留两位小数）
        //coin_amount 进入关卡时的金币数
        //stamina_value 进入关卡时的体力值
        //level_value 进入关卡时的等级值
        /// </summary>
        public static object TrackMPLevelEnter(int level_id, string enter_level_name = "", string game_mode = "", float enter_level_id = -1.0f, int chapter_id = -1, int coin_amount = -1, int stamina_value
= -1, int level_value = -1)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
                        {
                            {"level_id", level_id},
                        };
            Dictionary<string, object> geproperties = new Dictionary<string, object>()
                        {
                            {"$level_id", level_id},
                        };
            if (enter_level_name.Length > 0)
            {
                properties.Add("enter_level_name", enter_level_name);
                geproperties.Add("$enter_level_name", enter_level_name);
            }
            if (game_mode.Length > 0)
            {
                properties.Add("game_mode", game_mode);
                geproperties.Add("$game_mode", game_mode);
            }
            if (enter_level_id >= 0)
            {
                properties.Add("enter_level_id", enter_level_id);
                geproperties.Add("$enter_level_id", enter_level_id);
            }
            if (chapter_id >= 0)
            {
                properties.Add("chapter_id", chapter_id);
                geproperties.Add("$chapter_id", chapter_id);
            }
            if (coin_amount >= 0)
            {
                properties.Add("coin_amount", coin_amount);
                geproperties.Add("$coin_amount", coin_amount);
            }
            if (stamina_value >= 0)
            {
                properties.Add("stamina_value", stamina_value);
                geproperties.Add("$stamina_value", stamina_value);
            }
            if (level_value >= 0)
            {
                properties.Add("level_value", level_value);
                geproperties.Add("$level_value", level_value);
            }
            object result = GEPlatformManager.TrackMPLevelEnter(properties);
            Track("$LevelStart", geproperties);
            Flush();
            return result;
        }

        /// <summary>
        /// 上报微信 关卡退出：
        //level_id 必须 中途退出关卡时的关卡ID
        //ad_cnt 必须  中途退出关卡时，用户在关卡中点击、完成广告观看的次数
        //items 使用道具信息，包括道具类型和使用数量  按照最小循环关卡*计数；道具类型枚举：例如道具是炸弹，在以下枚举列表里，对应是4，则上报4，道具是重置，则报22（https://doc.weixin.qq.com/sheet/e3_AXsA9gYhADkCNqA5dxnhFTsi1GSAA?scode=AJEAIQdfAAoiOxtxQKAXsA9gYhADk&tab=BB08J2）
        //game_mode 进入的游戏模式（名称）
        //enter_level_id 进入的关卡进度（0-1之间，保留两位小数）
        //duration 中途退出关卡时的用户时长
        //chapter_id 中途退出关卡时的章节ID
        //coin_amount 中途退出关卡时的金币数
        //stamina_value 中途退出关卡时的体力值
        //level_value 中途退出关卡时的等级值
        public static object TrackMPLevelExit(int level_id, int ad_cnt, string[] items, string game_mode = "", float enter_level_id = -1.0f, int duration = -1, int chapter_id = -1, int coin_amount
= -1, int stamina_value = -1, int level_value = -1)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
                        {
                            {"level_id", level_id},
                            {"ad_cnt", ad_cnt},
                        };
            Dictionary<string, object> geproperties = new Dictionary<string, object>()
                        {
                            {"$level_id", level_id},
                            {"$ad_cnt", ad_cnt},
                        };
            if (items.Length > 0)
            {
                properties.Add("items", items);
                geproperties.Add("$items", items);
            }
            if (game_mode.Length > 0)
            {
                properties.Add("game_mode", game_mode);
                geproperties.Add("$game_mode", game_mode);
            }
            if (enter_level_id >= 0)
            {
                properties.Add("enter_level_id", enter_level_id);
                geproperties.Add("$enter_level_id", enter_level_id);
            }
            if (duration >= 0)
            {
                properties.Add("duration", duration);
                geproperties.Add("$duration", duration);
            }
            if (chapter_id >= 0)
            {
                properties.Add("chapter_id", chapter_id);
                geproperties.Add("$chapter_id", chapter_id);
            }
            if (coin_amount >= 0)
            {
                properties.Add("coin_amount", coin_amount);
                geproperties.Add("$coin_amount", coin_amount);
            }
            if (stamina_value >= 0)
            {
                properties.Add("stamina_value", stamina_value);
                geproperties.Add("$stamina_value", stamina_value);
            }
            if (level_value >= 0)
            {
                properties.Add("level_value", level_value);
                geproperties.Add("$level_value", level_value);
            }
            object result = GEPlatformManager.TrackMPLevelExit(properties);
            Track("$LevelExit", geproperties);
            Flush();
            return result;
        }

        /// <summary>
        /// 上报微信 关卡失败
        //level_id 必须 关卡失败时的关卡ID
        //ad_cnt 必须  关卡失败时，用户在关卡中点击、完成广告观看的次数
        //items 使用道具信息，包括道具类型和使用数量  按照最小循环关卡*计数；道具类型枚举：例如道具是炸弹，在以下枚举列表里，对应是4，则上报4，道具是重置，则报22（https://doc.weixin.qq.com/sheet/e3_AXsA9gYhADkCNqA5dxnhFTsi1GSAA?scode=AJEAIQdfAAoiOxtxQKAXsA9gYhADk&tab=BB08J2）
        //game_mode 进入的游戏模式（名称）
        //enter_level_id 进入的关卡进度（0-1之间，保留两位小数）
        //duration 关卡失败时的用户时长
        //chapter_id 关卡失败时的章节ID
        //coin_amount 关卡失败时的金币数
        //stamina_value 关卡失败时的体力值
        //level_value 关卡失败时的等级值
        public static object TrackMPLevelFail(int level_id, int ad_cnt, string[] items, string game_mode = "", float enter_level_id = -1.0f, int duration = -1, int chapter_id = -1, int coin_amount
= -1, int stamina_value = -1, int level_value = -1)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
                        {
                            {"level_id", level_id},
                            {"ad_cnt", ad_cnt},
                        };
            Dictionary<string, object> geproperties = new Dictionary<string, object>()
                        {
                            {"$level_id", level_id},
                            {"$ad_cnt", ad_cnt},
                        };
            if (items.Length > 0)
            {
                properties.Add("items", items);
                geproperties.Add("$items", items);
            }
            if (game_mode.Length > 0)
            {
                properties.Add("game_mode", game_mode);
                geproperties.Add("$game_mode", game_mode);
            }
            if (enter_level_id >= 0)
            {
                properties.Add("enter_level_id", enter_level_id);
                geproperties.Add("$enter_level_id", enter_level_id);
            }
            if (duration >= 0)
            {
                properties.Add("duration", duration);
                geproperties.Add("$duration", duration);
            }
            if (chapter_id >= 0)
            {
                properties.Add("chapter_id", chapter_id);
                geproperties.Add("$chapter_id", chapter_id);
            }
            if (coin_amount >= 0)
            {
                properties.Add("coin_amount", coin_amount);
                geproperties.Add("$coin_amount", coin_amount);
            }
            if (stamina_value >= 0)
            {
                properties.Add("stamina_value", stamina_value);
                geproperties.Add("$stamina_value", stamina_value);
            }
            if (level_value >= 0)
            {
                properties.Add("level_value", level_value);
                geproperties.Add("$level_value", level_value);
            }

            object result = GEPlatformManager.TrackMPLevelClose(properties);
            Track("$LevelFail", geproperties);
            Flush();
            return result;
        }



        /// <summary>
        /// 上报微信 关卡通过
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
        public static object TrackMPLevelPass(int level_id, int ad_cnt, string[] items, string game_mode = "", float enter_level_id = -1.0f, int duration = -1, int chapter_id = -1, int coin_amount
= -1, int stamina_value = -1, int level_value = -1)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
                        {
                            {"level_id", level_id},
                            {"ad_cnt", ad_cnt},
                        };
            Dictionary<string, object> geproperties = new Dictionary<string, object>()
                        {
                            {"$level_id", level_id},
                            {"$ad_cnt", ad_cnt},
                        };
            if (items.Length > 0)
            {
                properties.Add("items", items);
                geproperties.Add("$items", items);
            }
            if (game_mode.Length > 0)
            {
                properties.Add("game_mode", game_mode);
                geproperties.Add("$game_mode", game_mode);
            }
            if (enter_level_id >= 0)
            {
                properties.Add("enter_level_id", enter_level_id);
                geproperties.Add("$enter_level_id", enter_level_id);
            }
            if (duration >= 0)
            {
                properties.Add("duration", duration);
                geproperties.Add("$duration", duration);
            }
            if (chapter_id >= 0)
            {
                properties.Add("chapter_id", chapter_id);
                geproperties.Add("$chapter_id", chapter_id);
            }
            if (coin_amount >= 0)
            {
                properties.Add("coin_amount", coin_amount);
                geproperties.Add("$coin_amount", coin_amount);
            }
            if (stamina_value >= 0)
            {
                properties.Add("stamina_value", stamina_value);
                geproperties.Add("$stamina_value", stamina_value);
            }
            if (level_value >= 0)
            {
                properties.Add("level_value", level_value);
                geproperties.Add("$level_value", level_value);
            }

            object result = GEPlatformManager.TrackMPLevelPass(properties);
            Track("$LevelPass", geproperties);
            Flush();
            return result;
        }

        /// <summary>
        /// 上报微信 广告激励点位出现 
        //ad_placement_type 激励视频广告行为事件;例如1代表复活，2代表体力，当复活广告位出现被点击时上报1枚举见:https://doc.weixin.qq.com/sheet/e3_AQAA_gboAOACNEgqhOXynRwqq0gNv?scode=AJEAIQdfAAo7woEvatAQAA_gboAOA&tab=BB08J2
        public static object TrackMPAdPlacementShow(int ad_placement_type)
        {
            object result = GEPlatformManager.TrackMPAdPlacementShow(ad_placement_type);
            Dictionary<string, object> geproperties = new Dictionary<string, object>()
                        {
                            {"$ad_placement_type", ad_placement_type},
                        };
            Track("$IAAAdShow", geproperties);
            Flush();
            return result;
        }

        /// <summary>
        /// 上报微信 广告位点击 
        //ad_placement_type 激励视频广告行为事件;例如1代表复活，2代表体力，当复活广告位出现被点击时上报1枚举见:https://doc.weixin.qq.com/sheet/e3_AQAA_gboAOACNEgqhOXynRwqq0gNv?scode=AJEAIQdfAAo7woEvatAQAA_gboAOA&tab=BB08J2
        public static object TrackMPAdPlacementClick(int ad_placement_type)
        {
            object result = GEPlatformManager.TrackMPAdPlacementClick(ad_placement_type);
            Dictionary<string, object> geproperties = new Dictionary<string, object>()
                        {
                            {"$ad_placement_type", ad_placement_type},
                        };
            Track("$IAAAdClick", geproperties);
            Flush();
            return result;
        }

        /// <summary>
        /// 上报微信 广告播放结束
        //ad_placement_type 激励视频广告行为事件;例如1代表复活，2代表体力，当复活广告位出现被点击时上报1枚举见:https://doc.weixin.qq.com/sheet/e3_AQAA_gboAOACNEgqhOXynRwqq0gNv?scode=AJEAIQdfAAo7woEvatAQAA_gboAOA&tab=BB08J2
        public static object TrackMPAdPlacementVideoFinish(int ad_placement_type)
        {
            object result = GEPlatformManager.TrackMPAdPlacementVideoFinish(ad_placement_type);
            Dictionary<string, object> geproperties = new Dictionary<string, object>()
                        {
                            {"$ad_placement_type", ad_placement_type},
                        };
            Track("$IAAAdFinish", geproperties);
            Flush();
            return result;
        }


        /// <summary>
        /// 上报微信 广告曝光
        //ad_type 广告位类型; 1-激励视频，2-插屏，3-格子，4-banner
        public static object TrackMPAdPlacementVideoShow(int ad_type)
        {
            object result = GEPlatformManager.TrackMPAdPlacementVideoShow(ad_type);
            Dictionary<string, object> geproperties = new Dictionary<string, object>()
                        {
                            {"$ad_type", ad_type+""},
                        };
            Track("$IAAAdPlay", geproperties);
            Flush();
            return result;
        }


        /// <summary>
        /// 上报其他微信自定义事件：
        /// </summary>
        public static object TrackMPEvent(string eventName, Dictionary<string, object> properties = null)
        {
            return GEPlatformManager.TrackMPEvent(eventName, properties);
        }



        /// <summary>
        /// track 简单事件. 该事件会先缓存在本地，达到触发上报条件或者主动调用 Flush 时会上报到服务器.
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public static string Track(string eventName)
        {
            return Track(eventName, null);
        }

        /// <summary>
        /// track 事件及事件属性. 该事件会先缓存在本地，达到触发上报条件或者主动调用 Flush 时会上报到服务器.
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="properties">Properties</param>
        public static string Track(string eventName, Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                return GravityWrapperManager.Track(eventName, properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {eventName, properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
                return "";
            }
        }

        /// <summary>
        /// track 事件及事件属性，并指定 $event_time #zone_offset 属性. 该事件会先缓存在本地，达到触发上报条件或者主动调用 Flush 时会上报到服务器.
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="properties">事件属性</param>
        /// <param name="date">事件时间</param>
        /// <param name="timeZone">事件时区</param>
        public static string Track(string eventName, Dictionary<string, object> properties, DateTime date,
            TimeZoneInfo timeZone)
        {
            if (tracking_enabled)
            {
                return GravityWrapperManager.Track(eventName, properties, date, timeZone);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {eventName, properties, date, timeZone};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
                return "";
            }
        }

        /// <summary>
        /// 主动触发上报缓存事件到服务器. 
        /// </summary>
        public static void Flush()
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.Flush();
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { };
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 注册场景加载监听
        /// </summary>
        /// <param name="scene">场景对象</param>
        /// <param name="mode">场景加载模式</param>
        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {"$scene_name", scene.name},
                {"$scene_path", scene.path}
            };
            Track("$SceneLoaded", properties, DateTime.Now, null);
            TimeEvent("$SceneUnloaded");
        }

        /// <summary>
        /// 注册场景卸载监听
        /// </summary>
        /// <param name="scene">场景对象</param>
        public static void OnSceneUnloaded(Scene scene)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {"$scene_name", scene.name},
                {"$scene_path", scene.path}
            };
            Track("$SceneUnloaded", properties, DateTime.Now, null);
        }

        /// <summary>
        /// 设置公共事件属性. 公共事件属性指的就是每个事件都会带有的属性.
        /// </summary>
        /// <param name="superProperties">公共事件属性</param>
        public static void SetSuperProperties(Dictionary<string, object> superProperties)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.SetSuperProperties(superProperties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {superProperties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 删除某个公共事件属性.
        /// </summary>
        /// <param name="property">属性名称</param>
        public static void UnsetSuperProperty(string property)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UnsetSuperProperty(property);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {property};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 返回当前公共事件属性.
        /// </summary>
        /// <returns>公共事件属性</returns>
        public static Dictionary<string, object> GetSuperProperties()
        {
            if (tracking_enabled)
            {
                return GravityWrapperManager.GetSuperProperties();
            }

            return null;
        }
        
        /// <summary>
        /// 返回当前预置事件属性.
        /// </summary>
        /// <returns>预置事件属性</returns>
        public static Dictionary<string, object> GetCurrentPresetProperties()
        {
            if (tracking_enabled)
            {
                return GravityWrapperManager.GetCurrentPresetProperties();
            }

            return null;
        }

        /// <summary>
        /// 清空公共事件属性.
        /// </summary>
        public static void ClearSuperProperties()
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.ClearSuperProperty();
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { };
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// Sets the dynamic super properties.
        /// </summary>
        /// <param name="dynamicSuperProperties">Dynamic super properties interface.</param>
        public static void SetDynamicSuperProperties(IDynamicSuperProperties dynamicSuperProperties)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.SetDynamicSuperProperties(dynamicSuperProperties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {dynamicSuperProperties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 记录事件时长. 调用 TimeEvent 为某事件开始计时，当 track 传该事件时，SDK 会在在事件属性中加入 #duration 这一属性来表示事件时长，单位为秒.
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public static void TimeEvent(string eventName)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.TimeEvent(eventName);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {eventName};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置用户属性. 该接口上传的属性将会覆盖原有的属性值.
        /// </summary>
        /// <param name="properties">用户属性</param>
        public static void UserSet(Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserSet(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置用户属性. 该接口上传的属性将会覆盖原有的属性值，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">用户属性设置的时间</param>
        public static void UserSet(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserSet(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 重置一个用户属性.
        /// </summary>
        /// <param name="property">用户属性名称</param>
        public static void UserUnset(string property)
        {
            List<string> properties = new List<string>();
            properties.Add(property);
            UserUnset(properties);
        }


        /// <summary>
        /// 重置一组用户属性
        /// </summary>
        /// <param name="properties">用户属性列表</param>
        public static void UserUnset(List<string> properties)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserUnset(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 重置一组用户属性, 并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性列表</param>
        /// <param name="dateTime">操作时间</param>
        public static void UserUnset(List<string> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserUnset(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置用户属性. 当该属性之前已经有值的时候，将会忽略这条信息.
        /// </summary>
        /// <param name="properties">用户属性</param>
        public static void UserSetOnce(Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserSetOnce(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置用户属性. 当该属性之前已经有值的时候，将会忽略这条信息，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        public static void UserSetOnce(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserSetOnce(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性进行累加. 如果该属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="property">属性名称</param>
        /// <param name="value">数值</param>
        public static void UserAdd(string property, object value)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {property, value}
            };
            UserAdd(properties);
        }

        /// <summary>
        /// 对数值类用户属性进行累加. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        public static void UserAdd(Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserAdd(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性进行累加. 如果属性还未被设置，则会赋值 0 后再进行计算，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        public static void UserAdd(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserAdd(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性取最小值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="property">属性名称</param>
        /// <param name="value">数值</param>
        public static void UserNumberMin(string property, object value)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {property, value}
            };
            UserNumberMin(properties);
        }

        /// <summary>
        /// 对数值类用户属性取最小值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        public static void UserNumberMin(Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserNumberMin(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性取最小值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        public static void UserNumberMin(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserNumberMin(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性取最大值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="property">属性名称</param>
        /// <param name="value">数值</param>
        public static void UserNumberMax(string property, object value)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {property, value}
            };
            UserNumberMax(properties);
        }

        /// <summary>
        /// 对数值类用户属性取最大值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        public static void UserNumberMax(Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserNumberMax(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性取最大值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        public static void UserNumberMax(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserNumberMax(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对 List 类型的用户属性进行追加.
        /// </summary>
        /// <param name="properties">用户属性</param>
        public static void UserAppend(Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserAppend(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对 List 类型的用户属性进行追加，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        public static void UserAppend(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserAppend(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对 List 类型的用户属性进行去重追加.
        /// </summary>
        /// <param name="properties">用户属性</param>
        public static void UserUniqAppend(Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserUniqAppend(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对 List 类型的用户属性进行去重追加，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        public static void UserUniqAppend(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserUniqAppend(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 删除用户数据. 之后再查询该名用户的用户属性，将为空字典，但该用户产生的事件仍然可以被查询到
        /// </summary>
        public static void UserDelete()
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserDelete();
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { };
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 删除用户数据并指定操作时间.
        /// </summary>
        public static void UserDelete(DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.UserDelete(dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置允许上报数据到服务器的网络类型.（只支持Android/iOS）暂时不支持
        /// </summary>
        /// <param name="networkType">网络类型</param>
        private static void SetNetworkType(NetworkType networkType)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.SetNetworkType(networkType);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {networkType};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        /// <returns>The device identifier.</returns>
        public static string GetDeviceId()
        {
            if (tracking_enabled)
            {
                return GravityWrapperManager.GetDeviceId();
            }

            return null;
        }

        /// <summary>
        /// 设置数据上报状态
        /// </summary>
        /// <param name="status">上报状态，详见 GE_TRACK_STATUS 定义</param>
        public static void SetTrackStatus(GE_TRACK_STATUS status)
        {
            if (tracking_enabled)
            {
                GravityWrapperManager.SetTrackStatus(status);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {status};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 传入时间戳校准 SDK 时间.
        /// </summary>
        /// <param name="timestamp">当前 Unix timestamp, 单位 毫秒</param>
        public static void CalibrateTime(long timestamp)
        {
            GravityWrapperManager.CalibrateTime(timestamp);
        }

        /// <summary>
        /// 传入 NTP Server 地址校准 SDK 时间.
        /// 您可以根据您用户所在地传入访问速度较快的 NTP Server 地址, 例如 time.asia.apple.com
        /// SDK 默认情况下会等待 3 秒，去获取时间偏移数据，并用该偏移校准之后的数据.
        /// 如果在 3 秒内未因网络原因未获得正确的时间偏移，本次应用运行期间将不会再校准时间.
        /// </summary>
        /// <param name="timestamp">可用的 NTP 服务器地址</param>
        public static void CalibrateTimeWithNtp(string ntpServer)
        {
            GravityWrapperManager.CalibrateTimeWithNtp(ntpServer);
        }

        /// <summary>
        /// 获取当前国家/地区代码
        /// 在 ISO 3166 中定义的由两个字母组成的国家/地区代码
        /// </summary>
        /// <returns>国家/地区代码</returns>
        public static string GetLocalRegion()
        {
            return System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName;
        }

        /// <summary>
        /// 手动初始化 Gravity Engine SDK
        /// </summary>
        /// <param name="accessToken">项目token</param>
        /// <param name="clientId">用户唯一ID，iOS产品若采用引力自动获取 idfv 的方案，则这里传固定值default_placeholder即可；否则传入您自定义的 clientId</param>
        /// <param name="mode">SDK运行模式（默认为正常模式）</param>
        /// <param name="channel">用户渠道（选填）</param>
        /// <param name="enableImei">是否采集 imei，仅支持 Android，默认true（选填）</param>
        /// <param name="enableOaid">是否采集 oaid，仅支持 Android，默认true（选填）</param>
        /// <param name="enableAndroidId">是否采集 android_id，仅支持 Android，默认true（选填）</param>
        /// <param name="enableMAC">是否采集 mac，仅支持 Android，默认true（选填）</param>
        public static void StartGravityEngine(string accessToken, string clientId="default_placeholder", SDKRunMode mode = SDKRunMode.NORMAL,
            string channel = "base_channel", bool enableImei = true, bool enableOaid = true, bool enableAndroidId = true, bool enableMAC = true)
        {
            Token token = new Token(accessToken, clientId, channel);
            token.mode = mode;
            token.enableIMEI = enableImei;
            token.enableOAID = enableOaid;
            token.enableAndroidId = enableAndroidId;
            token.enableMAC = enableMAC;

            StartGravityEngine(token);
        }



        /// <summary>
        /// 海外 手动初始化 Gravity Engine SDK
        /// </summary>
        /// <param name="accessToken">项目token</param>
        /// <param name="clientId">用户唯一ID，iOS产品若采用引力自动获取 idfv 的方案，则这里传固定值default_placeholder即可；否则传入您自定义的 clientId</param>
        /// <param name="mode">SDK运行模式（默认为正常模式）</param>
        /// <param name="channel">用户渠道（选填）</param>
        ///
        /// <param name="isGDPRArea">是否欧盟地区（选填）如果你的应用在欧盟地区运营，则需要符合欧盟隐私保护法律的规定（关于GDPR），请务必在用户拒绝采集设备敏感信息时设置</param>
        /// <param name="isCoppaEnabled">如果您的应用需要符合《儿童在线隐私权保护法》(COPPA) 规定，设置 isCoppaEnabled = true（选填）</param>
        /// <param name="isKidsAppEnabled">如果您的应用会定向到不满 13 周岁的儿童，则需要将其标记为儿童应用 (Kids App)，设置 isKidsAppEnabled = true（选填）</param>
        /// <param name="adPersonalizationEnabled">如果你的应用在欧盟地区运营并且在Google投放您的应用，请务必将用户是否允许Google将其数据用于个性化广告的意见结果传入该属性，以确保您符合Google对欧盟用户意见征求政策的新政策仅支持 （选填）Android</param>
        /// <param name="adUserDataEnabled">如果你的应用在欧盟地区运营并且在Google投放您的应用，请务必将用户是否同意将其数据发送到Google的意见结果传入该属性，以确保您符合Google对欧盟用户意见征求政策的新政策（选填）仅支持 Android</param>
        /// <param name="fbAppID">如果你的应用需要在Facebook平台推广，需要进行Facebook归因，需要设置对应Facebook APP ID（选填）仅支持 Android</param>
        public static void StartOverseaGravityEngine(string accessToken, string clientId = "default_placeholder", SDKRunMode mode = SDKRunMode.NORMAL,
            string channel = "base_channel", bool isGDPRArea = false, bool isCoppaEnabled = false, bool isKidsAppEnabled = false, bool adPersonalizationEnabled = false, bool adUserDataEnabled = false, string fbAppID = "")
        {
            Token token = new Token(accessToken, clientId, channel);
            token.mode = mode;
            token.isGDPRArea = isGDPRArea;
            token.isCoppaEnabled = isCoppaEnabled;
            token.isKidsAppEnabled = isKidsAppEnabled;
            token.adPersonalizationEnabled = adPersonalizationEnabled;
            token.adUserDataEnabled = adUserDataEnabled;
            token.fbAppID = fbAppID;

            StartGravityEngine(token);
        }

        /// <summary>
        /// 使用可视化配置 初始化 Gravity Engine SDK
        /// <param name="clientId">用户唯一ID，iOS产品若采用引力自动获取 idfv 的方案，则这里传固定值default_placeholder即可；否则传入您自定义的 clientId</param>
        /// <param name="adPersonalizationEnabled">如果你的应用在欧盟地区运营并且在Google投放您的应用，请务必将用户是否允许Google将其数据用于个性化广告的意见结果传入该属性，以确保您符合Google对欧盟用户意见征求政策的新政策仅支持 （选填）Android</param>
        /// <param name="adUserDataEnabled">如果你的应用在欧盟地区运营并且在Google投放您的应用，请务必将用户是否同意将其数据发送到Google的意见结果传入该属性，以确保您符合Google对欧盟用户意见征求政策的新政策（选填）仅支持 Android</param>
        /// </summary>
        /// 
        public static void AutoStartGravityEngine(string clientId = "default_placeholder", bool adPersonalizationEnabled = false,bool adUserDataEnabled = false)
        {
            if (string.IsNullOrEmpty(GravityEngineAutoConfig.accessToken))
            {
                throw new ApplicationException("[GravityEngineSDK] 检测到自动初始化配置未设置，请先在《引力引擎/SDK启动参数配置》进行设置！！！！！！！");
            }

            //读取配置文件，创建token
            Token token = new Token(GravityEngineAutoConfig.accessToken,clientId, GravityEngineAutoConfig.channel);
            if (GravityEngineAutoConfig.debugMode)
            {
                token.mode = SDKRunMode.DEBUG;
            }
            if (GravityEngineAutoConfig.isChina)
            {
                token.enableIMEI = GravityEngineAutoConfig.enableIMEI;
                token.enableOAID = GravityEngineAutoConfig.enableOAID;
                token.enableAndroidId = GravityEngineAutoConfig.enableAndroidId;
                token.enableMAC = GravityEngineAutoConfig.enableMAC;
            }
            else
            {
                token.isGDPRArea = GravityEngineAutoConfig.isGDPRArea;
                token.isCoppaEnabled = GravityEngineAutoConfig.isCoppaEnabled;
                token.isKidsAppEnabled = GravityEngineAutoConfig.isKidsAppEnabled;
                token.adPersonalizationEnabled = adPersonalizationEnabled;
                token.adUserDataEnabled = adUserDataEnabled;
                token.fbAppID = GravityEngineAutoConfig.fbAppID;
            }
            token.isChina = GravityEngineAutoConfig.isChina;
            token.foregroundSessionThreshold = int.Parse(GravityEngineAutoConfig.foregroundSessionThreshold);

            List<AndroidClientIdType> mClientIdPriorityOrder = new List<AndroidClientIdType>();
            foreach(string typeStr in GravityEngineAutoConfig.mClientIdPriorityOrder)
            {
                if(typeStr == "OAID")
                {
                    mClientIdPriorityOrder.Add(AndroidClientIdType.OAID);
                }
                else if (typeStr == "ANDROID_ID")
                {
                    mClientIdPriorityOrder.Add(AndroidClientIdType.ANDROID_ID);
                }
            }
            token.mClientIdPriorityOrder = mClientIdPriorityOrder;


            StartGravityEngine(token);
        }

        /// <summary>
        /// 初始化 Gravity Engine SDK
        /// </summary>
        /// <param name="token">多项目配置，详情参见 GravityEngineAPI.Token</param>
        public static void StartGravityEngine(Token token)
        {
            Token[] tokens = new Token[1];
            tokens[0] = token;
            StartGravityEngine(tokens);
        }

        private static void StartGravityEngine(Token[] tokens = null)
        {
            start_called = true;
            if (GEMiniPlatformWechat.NeedCheckWechatDirect())
            {
                GEMiniPlatformWechat.CheckDirect(() =>
                {
                    RealStartGravityEngine(tokens);
                });
            }
            else
            {
                RealStartGravityEngine(tokens);
            }
        }

        /// <summary>
        /// 初始化 Gravity Engine SDK，仅内部调用
        /// </summary>
        /// <param name="token">多项目配置，详情参见 GravityEngineAPI.Token</param>
        private static void RealStartGravityEngine(Token[] tokens = null)
        {
            tracking_enabled = true;

            if(_sGravityEngineAPI == null)
            {
                new GameObject("GravityEngine", typeof(GravityEngineAPI));
            }
            if (tracking_enabled)
            {
                GEPlatformManager.SetDeviceInfo();

                GE_PublicConfig.GetPublicConfig();
                GravityWrapperManager.SetVersionInfo(GE_PublicConfig.LIB_VERSION);
                if (tokens == null)
                {
                    tokens = _sGravityEngineAPI.tokens;
                }

                try
                {
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        Token token = tokens[i];
                        if(token.mode == SDKRunMode.DEBUG)
                        {
                            GE_Log.EnableLog(true);
                            GravityWrapperManager.EnableLog(true);
                        }
                        send_sdk_error_to_ge = token.enableCrashToGe;
                        send_sdk_error_to_3d = token.enableCrashTo3D;

                        GravitySDKConstant.SET_SERVER_URL(token.isChina);
                        GravityHelper.GravityHost = GravitySDKConstant.SERVER_URL;

                        GravityHelper.InitSDK(token.accessToken, token.clientId, token.channel);
                        GravityWrapperManager.ShareInstance(token, _sGravityEngineAPI);
                        GravityWrapperManager.SetNetworkType(_sGravityEngineAPI.networkType);

                        GEPlatformManager.SetOpenId(token.clientId);
                        GE_Log.d("GravityEngine start with SERVER_URL: " + GravitySDKConstant.SERVER_URL +
                                 ", MODE: " + token.mode);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError("gravity GravityEngine error:" + e);
                    // ignored
                }

                GEPlatformManager.OnGravityStartEnd();
            }

            //上报缓存事件
            FlushEventCaches();
            if (GEPlatformManager.isMiniGame())
            {
                EnableAutoTrack(AUTO_TRACK_EVENTS.MINI_GAME_ALL);
            }
            else
            {
                EnableAutoTrack(AUTO_TRACK_EVENTS.APP_ALL);
            }
        }

        public static Dictionary<string, string> ConvertToDictionary(Dictionary<string, object> sourceDict)
        {
            var targetDict = new Dictionary<string, string>();

            if (sourceDict == null) return targetDict;
            foreach (var kvp in sourceDict)
            {
                // 尝试将值转换为字符串
                string stringValue = kvp.Value?.ToString();
                targetDict[kvp.Key] = stringValue;
            }

            return targetDict;
        }

        // 统计启动场景
        public static void ReportSuperProperties(string scene)
        {
            string latestStartupTimestamp;
            
            if (GravitySDKUtil.IsFirstLaunchToday())
            {
                GravitySDKFile.SaveData(GravitySDKConstant.TODAY_FIRST_SCENE_KEY, scene);
                latestStartupTimestamp = scene;
            }
            else
            {
                latestStartupTimestamp = (string) GravitySDKFile.GetData(GravitySDKConstant.TODAY_FIRST_SCENE_KEY, typeof(string));
            }
            
            Dictionary<string, object> superProperties = new Dictionary<string, object>()
            {
                {GravitySDKConstant.SCENE, scene},
                {GravitySDKConstant.TODAY_FIRST_SCENE, latestStartupTimestamp}
            };
            SetSuperProperties(superProperties);
        }

        #region turbo

        /// <summary>
        /// 在引力引擎初始化，其他方法均需在本方法回调成功之后才可正常使用
        /// </summary>
        /// <param name="clientId"></param>                 用户唯一标识，如产品为小游戏，则必须填用户openid(如传空，则使用调用StartGravityEngine时传入的clientID；如传，则会使用当前传入的clientID)
        /// <param name="nickname"></param>                 用户昵称
        /// <param name="version"></param>                  用户注册的程序版本，比如当前小游戏的版本号
        /// <param name="openId"></param>                   open id (小程序/小游戏必填)
        /// <param name="enableSyncAttribution"></param>    是否开启同步获取归因信息，具体请参考同步归因：https://doc.gravity-engine.com/turbo-integrated/sync_attribution.html
        /// <param name="initializeCallback"></param>       网络回调，其他方法均需在回调成功之后才可正常使用
        /// <exception cref="ArgumentException"></exception>
        public static void Initialize(string clientId, string nickname, int version, string openId,
            bool enableSyncAttribution, IInitializeCallback initializeCallback)
        {
            if(start_called && !tracking_enabled && GEMiniPlatformWechat.NeedCheckWechatDirect())
            {
                //如果微信小游戏且开启了直玩检测，且调用了StartGravityEngine，但没StartGravityEngine完成，稍后调用Initialize
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { clientId, nickname, version, openId, enableSyncAttribution,
                    initializeCallback };
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
            else
            {
                GravityWrapperManager.Initialize(clientId, nickname, version, openId, enableSyncAttribution,
                    initializeCallback);
            }
        }

        /// <summary>
        /// 在引力引擎注册，其他方法均需在本方法回调成功之后才可正常使用（iOS专用）
        /// </summary>
        /// <param name="enableAsa"></param>                是否开启asa归因
        /// <param name="clientId"></param>                 用户在引力系统中的唯一标识，如果传空字符串，则引力 sdk 内部会采集一个稳定的 idfv 作为用户唯一标识（推荐传空字符串）
        /// <param name="caid"></param>                     当前用户中广协 ID 返回的 的 json 数组 string 
        /// <param name="enableSyncAttribution"></param>    是否开启同步获取归因信息，具体请参考同步归因：https://doc.gravity-engine.com/turbo-integrated/sync_attribution.html
        /// <param name="channel"></param>                  当前包渠道
        /// <param name="initializeCallback"></param>       网络回调，其他方法均需在回调成功之后才可正常使用
        /// <exception cref="ArgumentException"></exception>
        public static void InitializeIOS(bool enableAsa, string clientId, string caid, bool enableSyncAttribution, string channel, IInitializeCallback initializeCallback)
        {
            GravityWrapperManager.InitializeIOS(enableAsa, clientId, caid, enableSyncAttribution, channel, initializeCallback);
        }

        public static void InitializeIOS(bool enableAsa, string clientId, string caid1MD5, string caid2MD5, bool enableSyncAttribution, string channel, IInitializeCallback initializeCallback)
        {
            GravityWrapperManager.InitializeIOS(enableAsa, clientId, caid1MD5, caid2MD5, enableSyncAttribution, channel, initializeCallback);
        }

        //更新用户基础信息
        //channel str 用户初始渠道 不传视为不更新
        //name str 用户初始名称 不传视为不更新
        //version int 用户初始应用版本，不传视为不更新
        //openid str 用户openid 不传视为不更新
        //unionid str 用户unionid 不传视为不更新
        public static void UpdateUserInfo(Dictionary<string, object> properties)
        {
            GravityWrapperManager.UpdateUserInfo(properties);
        }

        public static string GetCurrentClientID()
        {
            return GravityWrapperManager.GetCurrentClientID();
        }


        public enum DryRunEventEmpty
        {
            NOT_ENABLE,
            NO_IN_POSTBACK_WINDOW,
            SKIP
        }
        /// <summary>
        /// 演练模式查询接口，查询当前traceId对应的演练结果
        /// </summary>
        /// <param name="traceId"></param>
        /// <param name="otherParams"></param>
        /// <param name="queryDryRunCallback"></param>
        public static void DryRunEventWithCallback(string traceId, Dictionary<string, object> otherParams, IQueryDryRunCallback queryDryRunCallback)
        {
            GravityWrapperManager.QueryDryRunInfo(traceId, otherParams, queryDryRunCallback);
        }
        
        public static void test()
        {
            // 方便调用
        }

        #endregion

        #region internal

        private static void FlushEventCaches()
        {
            List<Dictionary<string, object>> tmpEventCaches = new List<Dictionary<string, object>>(eventCaches);
            eventCaches.Clear();
            foreach (Dictionary<string, object> eventCache in tmpEventCaches)
            {
                if (eventCache.ContainsKey("method") && eventCache.ContainsKey("parameters"))
                {
                    System.Reflection.MethodBase method = (System.Reflection.MethodBase) eventCache["method"];
                    object[] parameters = (object[]) eventCache["parameters"];
                    method.Invoke(null, parameters);
                }
            }
        }

        private void Awake()
        {
            if (_sGravityEngineAPI == null)
            {
                _sGravityEngineAPI = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (this.startManually == false)
            {
                GravityEngineAPI.StartGravityEngine();
            }
        }

        private void Start()
        {
        }

        private void OnApplicationQuit()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene != null)
            {
                OnSceneUnloaded(scene);
            }
        }

        private static GravityEngineAPI _sGravityEngineAPI;
        private static bool tracking_enabled = false;
        private static bool start_called = false;
        private static List<Dictionary<string, object>> eventCaches = new List<Dictionary<string, object>>();

        #endregion
    }
}