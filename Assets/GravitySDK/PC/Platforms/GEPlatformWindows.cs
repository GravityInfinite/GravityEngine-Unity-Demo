using System.Collections.Generic;
using UnityEngine;
using GravitySDK.PC.Utils;
using System;
using UnityEngine.SceneManagement;

//#define GRAVITY_WINDOWS_GAME_MODE

namespace GravityEngine.Platform
{
    public class GEPlatformWindows : GEPlatformBase
    {
        public static bool activity()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || GRAVITY_WINDOWS_GAME_MODE
            return true;
#else
            return false;
#endif
        }

        public override void SetDeviceInfo()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || GRAVITY_WINDOWS_GAME_MODE
            string networkTypeStr =
                Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork ? "WIFI" :
                Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork ? "Mobile" :
                "NULL";
            GravitySDKDeviceInfo.SetMiniGameDeviceInfo(new GEMiniGameDeviceInfo()
            {
                networkType = networkTypeStr,
                SDKVersion = "0",
                brand = SystemInfo.graphicsDeviceVendor,
                language = SystemLanguageStr(),
                model = SystemInfo.deviceModel,
                platform = "Windows",
                screenHeight = Screen.currentResolution.height,
                screenWidth = Screen.currentResolution.width,
                system = SystemInfo.operatingSystem,
                version = "0",
            });
#endif
        }


        public override void getDeviceInfo(Action<Dictionary<string, string>> callBack)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || GRAVITY_WINDOWS_GAME_MODE
            var deviceInfo = new Dictionary<string, string>()
            {
                {"os_name", "windows"},
                {"imei", SystemInfo.deviceUniqueIdentifier},
                {"system", SystemInfo.operatingSystem},
                {"brand", SystemInfo.graphicsDeviceVendor},
                {"model", SystemInfo.deviceModel},
            };
            callBack(deviceInfo);
#endif
        }

        private string SystemLanguageStr()
        {
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
                    return "fi";
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