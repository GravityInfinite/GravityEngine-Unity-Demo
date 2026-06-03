using System;
using GravitySDK.PC.Constant;
using GravitySDK.PC.Storage;
using UnityEngine;
using GravityEngine.Platform;

namespace GravitySDK.PC.Utils
{
    public class GEMiniGameDeviceInfo
    {
        /// <summary> 
        /// 需要基础库： `1.1.0`
        /// 客户端基础库版本
        /// </summary>
        public string SDKVersion;

        /// <summary> 
        /// 需要基础库： `2.6.0`
        /// 允许微信使用相册的开关（仅 iOS 有效）
        /// </summary>
        public bool albumAuthorized;

        /// <summary> 
        /// 需要基础库： `2.6.0`
        /// 蓝牙的系统开关
        /// </summary>
        public bool bluetoothEnabled;

        /// <summary> 
        /// 需要基础库： `1.5.0`
        /// 设备品牌
        /// </summary>
        public string brand;

        /// <summary> 
        /// 需要基础库： `2.6.0`
        /// 允许微信使用摄像头的开关
        /// </summary>
        public bool cameraAuthorized;

        /// <summary> 
        /// 需要基础库： `2.15.0`
        /// 是否已打开调试。可通过右上角菜单或 [wx.setEnableDebug](https://developers.weixin.qq.com/minigame/dev/api/base/debug/wx.setEnableDebug.html) 打开调试。
        /// </summary>
        public bool enableDebug;

        /// <summary> 
        /// 需要基础库： `1.5.0`
        /// 用户字体大小（单位px）。以微信客户端「我-设置-通用-字体大小」中的设置为准
        /// </summary>
        public double fontSizeSetting;

        /// <summary> 
        /// 微信设置的语言
        /// </summary>
        public string language;

        /// <summary> 
        /// 需要基础库： `2.6.0`
        /// 允许微信使用定位的开关
        /// </summary>
        public bool locationAuthorized;

        /// <summary> 
        /// 需要基础库： `2.6.0`
        /// 地理位置的系统开关
        /// </summary>
        public bool locationEnabled;

        /// <summary> 
        /// `true` 表示模糊定位，`false` 表示精确定位，仅 iOS 支持
        /// </summary>
        public bool locationReducedAccuracy;

        /// <summary> 
        /// 需要基础库： `2.6.0`
        /// 允许微信使用麦克风的开关
        /// </summary>
        public bool microphoneAuthorized;

        /// <summary> 
        /// 设备型号。新机型刚推出一段时间会显示unknown，微信会尽快进行适配。
        /// </summary>
        public string model;

        /// <summary> 
        /// 需要基础库： `2.6.0`
        /// 允许微信通知带有提醒的开关（仅 iOS 有效）
        /// </summary>
        public bool notificationAlertAuthorized;

        /// <summary> 
        /// 需要基础库： `2.6.0`
        /// 允许微信通知的开关
        /// </summary>
        public bool notificationAuthorized;

        /// <summary> 
        /// 需要基础库： `2.6.0`
        /// 允许微信通知带有标记的开关（仅 iOS 有效）
        /// </summary>
        public bool notificationBadgeAuthorized;

        /// <summary> 
        /// 需要基础库： `2.6.0`
        /// 允许微信通知带有声音的开关（仅 iOS 有效）
        /// </summary>
        public bool notificationSoundAuthorized;

        /// <summary> 
        /// 需要基础库： `2.19.3`
        /// 允许微信使用日历的开关
        /// </summary>
        public bool phoneCalendarAuthorized;

        /// <summary> 
        /// 设备像素比
        /// </summary>
        public double pixelRatio;

        /// <summary> 
        /// 客户端平台
        /// 可选值：
        /// - 'ios': iOS微信（包含 iPhone、iPad）;
        /// - 'android': Android微信;
        /// - 'windows': Windows微信;
        /// - 'mac': macOS微信;
        /// </summary>
        public string platform;

        /// <summary> 
        /// 需要基础库： `1.1.0`
        /// 屏幕高度，单位px
        /// </summary>
        public double screenHeight;

        /// <summary> 
        /// 需要基础库： `1.1.0`
        /// 屏幕宽度，单位px
        /// </summary>
        public double screenWidth;

        /// <summary> 
        /// 需要基础库： `1.9.0`
        /// 状态栏的高度，单位px
        /// </summary>
        public double statusBarHeight;

        /// <summary> 
        /// 操作系统及版本
        /// </summary>
        public string system;

        /// <summary> 
        /// 微信版本号
        /// </summary>
        public string version;

        /// <summary> 
        /// 需要基础库： `2.6.0`
        /// Wi-Fi 的系统开关
        /// </summary>
        public bool wifiEnabled;

        /// <summary> 
        /// 可使用窗口高度，单位px
        /// </summary>
        public double windowHeight;

        /// <summary> 
        /// 可使用窗口宽度，单位px
        /// </summary>
        public double windowWidth;

        /// <summary> 
        /// 需要基础库： `2.11.0`
        /// 系统当前主题，取值为`light`或`dark`，全局配置`"darkmode":true`时才能获取，否则为 undefined （不支持小游戏）
        /// 可选值：
        /// - 'dark': 深色主题;
        /// - 'light': 浅色主题;
        /// </summary>
        public string theme;

        public string networkType;
    }

    public class GravitySDKDeviceInfo
    {
        private static GEMiniGameDeviceInfo _miniGameDeviceInfo;

        public static void SetMiniGameDeviceInfo(GEMiniGameDeviceInfo miniGameDeviceInfo)
        {
            _miniGameDeviceInfo = miniGameDeviceInfo;
        }

        public static void SetNetworkType(string networkType)
        {
            if (_miniGameDeviceInfo == null)
            {
                _miniGameDeviceInfo = new GEMiniGameDeviceInfo();
            }
            _miniGameDeviceInfo.networkType = networkType;
        }

        //设备ID
        public static string DeviceID()
        {
            if (GravitySDKUtil.DisPresetProperties.Contains(GravitySDKConstant.DEVICE_ID))
            {
                return "";
            }
            if (GEPlatformManager.isMiniGame())
            {
                return RandomDeviceID();
            }
            else
            {
                return SystemInfo.deviceUniqueIdentifier;
            }
        }

        //随机数持久化,作为设备ID的备选(WebGL获取不到设备ID)
        private static string RandomDeviceID()
        {
            string randomID = (string) GravitySDKFile.GetData(GravitySDKConstant.RANDOM_DEVICE_ID, typeof(string));
            if (string.IsNullOrEmpty(randomID))
            {
                randomID = System.Guid.NewGuid().ToString("N");
                GravitySDKFile.SaveData(GravitySDKConstant.RANDOM_DEVICE_ID, randomID);
            }

            return randomID;
        }

        //网络类型
        public static string NetworkType()
        {
            if (GravitySDKUtil.DisPresetProperties.Contains(GravitySDKConstant.NETWORK_TYPE))
            {
                return "";
            }
            if (GEPlatformManager.IsWechatMiniGame())
            {
                return _miniGameDeviceInfo != null ? _miniGameDeviceInfo.networkType : "";
            }
            string networkType = "NULL";
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                networkType = "Mobile";
            }
            else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                networkType = "WIFI";
            }

            return networkType;
        }

        //os类型
        public static string OS()
        {
            if (GravitySDKUtil.DisPresetProperties.Contains(GravitySDKConstant.OS))
            {
                return "";
            }
            if (GEPlatformManager.IsGePlatform())
            {
                return _miniGameDeviceInfo != null ? _miniGameDeviceInfo.platform : "";
            }
            string os = "other";
            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Linux)
            {
                os = "Linux";
            }
            else if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
            {
                os = "MacOSX";
            }
            else if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
            {
                os = "Windows";
            }
            return os;
        }

        //OS版本信息
        public static string OSVersion()
        {
            if (GravitySDKUtil.DisPresetProperties.Contains(GravitySDKConstant.OS_VERSION))
            {
                return "";
            }
            if (GEPlatformManager.IsGePlatform())
            {
                return _miniGameDeviceInfo != null ? _miniGameDeviceInfo.system : "";
            }
            return SystemInfo.operatingSystem;
        }

        //屏幕宽度
        public static int ScreenWidth()
        {
            if (GravitySDKUtil.DisPresetProperties.Contains(GravitySDKConstant.SCREEN_WIDTH))
            {
                return 0;
            }
            if (GEPlatformManager.IsGePlatform())
            {
                return (int)(_miniGameDeviceInfo?.screenWidth ?? 0);
            }
            return (int)(UnityEngine.Screen.currentResolution.width);
        }

        //屏幕高度
        public static int ScreenHeight()
        {
            if (GravitySDKUtil.DisPresetProperties.Contains(GravitySDKConstant.SCREEN_HEIGHT))
            {
                return 0;
            }
            if (GEPlatformManager.IsGePlatform())
            {
                return (int)(_miniGameDeviceInfo?.screenHeight ?? 0);
            }
            return (int)(UnityEngine.Screen.currentResolution.height);
        }

        //显卡厂商名称
        public static string Manufacture()
        {
            if (GravitySDKUtil.DisPresetProperties.Contains(GravitySDKConstant.MANUFACTURE))
            {
                return "";
            }
            if (GEPlatformManager.IsGePlatform())
            {
                return (_miniGameDeviceInfo != null && _miniGameDeviceInfo.brand != null) ? _miniGameDeviceInfo.brand : "";
            }
            return SystemInfo.graphicsDeviceVendor;
        }

        //设备型号
        public static string DeviceModel()
        {
            if (GravitySDKUtil.DisPresetProperties.Contains(GravitySDKConstant.DEVICE_MODEL))
            {
                return "";
            }
            if (GEPlatformManager.IsGePlatform())
            {
                return _miniGameDeviceInfo != null ? _miniGameDeviceInfo.model : "";
            }
            return SystemInfo.deviceModel;
        }

        //本机语言
        public static string MachineLanguage()
        {
            if (GravitySDKUtil.DisPresetProperties.Contains(GravitySDKConstant.SYSTEM_LANGUAGE))
            {
                return "";
            }
            if (GEPlatformManager.IsGePlatform())
            {
                return _miniGameDeviceInfo != null ? _miniGameDeviceInfo.language : "";
            }
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Afrikaans:
                    return "af";
                case SystemLanguage.Arabic:
                    return "ar";
                case SystemLanguage.Basque:
                    return "eu";
                case SystemLanguage.Belarusian:
                    return "be";
                case SystemLanguage.Bulgarian:
                    return "bg";
                case SystemLanguage.Catalan:
                    return "ca";
                case SystemLanguage.Chinese:
                    return "zh";
                case SystemLanguage.Czech:
                    return "cs";
                case SystemLanguage.Danish:
                    return "da";
                case SystemLanguage.Dutch:
                    return "nl";
                case SystemLanguage.English:
                    return "en";
                case SystemLanguage.Estonian:
                    return "et";
                case SystemLanguage.Faroese:
                    return "fo";
                case SystemLanguage.Finnish:
                    return "fu";
                case SystemLanguage.French:
                    return "fr";
                case SystemLanguage.German:
                    return "de";
                case SystemLanguage.Greek:
                    return "el";
                case SystemLanguage.Hebrew:
                    return "he";
                case SystemLanguage.Icelandic:
                    return "is";
                case SystemLanguage.Indonesian:
                    return "id";
                case SystemLanguage.Italian:
                    return "it";
                case SystemLanguage.Japanese:
                    return "ja";
                case SystemLanguage.Korean:
                    return "ko";
                case SystemLanguage.Latvian:
                    return "lv";
                case SystemLanguage.Lithuanian:
                    return "lt";
                case SystemLanguage.Norwegian:
                    return "nn";
                case SystemLanguage.Polish:
                    return "pl";
                case SystemLanguage.Portuguese:
                    return "pt";
                case SystemLanguage.Romanian:
                    return "ro";
                case SystemLanguage.Russian:
                    return "ru";
                case SystemLanguage.SerboCroatian:
                    return "sr";
                case SystemLanguage.Slovak:
                    return "sk";
                case SystemLanguage.Slovenian:
                    return "sl";
                case SystemLanguage.Spanish:
                    return "es";
                case SystemLanguage.Swedish:
                    return "sv";
                case SystemLanguage.Thai:
                    return "th";
                case SystemLanguage.Turkish:
                    return "tr";
                case SystemLanguage.Ukrainian:
                    return "uk";
                case SystemLanguage.Vietnamese:
                    return "vi";
                case SystemLanguage.ChineseSimplified:
                    return "zh";
                case SystemLanguage.ChineseTraditional:
                    return "zh";
                case SystemLanguage.Hungarian:
                    return "hu";
                case SystemLanguage.Unknown:
                    return "unknown";

            };
            return "";
        }
    }
}