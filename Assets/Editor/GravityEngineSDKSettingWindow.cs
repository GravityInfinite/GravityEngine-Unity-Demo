using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;

public class GravityEngineSDKSettingWindow : EditorWindow
{
    [MenuItem("引力引擎/SDK参数配置")]
    public static void ShowWindow()
    {
        GetWindow<GravityEngineSDKSettingWindow>("GravityEngineSDKSettingWindow");
    }


    private void OnEnable()
    {
        Load();
    }


    #region 自动配置宏参数

    List<string> gravity_plaforms = new List<string>() {
            "原生APP",
            "微信小游戏",
            "抖音小游戏/Tiktok",
            "抖音小游戏 TT SDK 模式",
            "抖音云游戏",
            "快手小游戏",
            "快手小游戏 WEBGL",
            "支付宝小游戏",
            "美团小游戏",
            "Bilibili 小游戏",
            "华为快游戏",
            "OPPO 快游戏",
            "小米快游戏",
            "TapTap小游戏",
            "Vivo快游戏",
            "Windows应用/游戏"
        };

    List<string> gravity_defines = new List<string>() {
            "",
            "GRAVITY_WECHAT_GAME_MODE",
            "GRAVITY_BYTEDANCE_GAME_MODE",
            "GRAVITY_BYTEDANCE_TT_GAME_MODE",
            "GRAVITY_BYTEDANCE_CLOUD_MODE",
            "GRAVITY_KUAISHOU_GAME_MODE",
            "GRAVITY_KUAISHOU_WEBGL_GAME_MODE",
            "GRAVITY_ALIPAY_GAME_MODE",
            "GRAVITY_MEITUAN_GAME_MODE",
            "GRAVITY_BILIBILI_GAME_MODE",
            "GRAVITY_HUAWEI_GAME_MODE",
            "GRAVITY_OPPO_GAME_MODE",
            "GRAVITY_XIAOMI_GAME_MODE",
            "GRAVITY_TAPTAP_GAME_MODE",
            "GRAVITY_VIVO_GAME_MODE",
            "GRAVITY_WINDOWS_GAME_MODE"
        };

#if GRAVITY_WECHAT_GAME_MODE
    int miniGameHongIndex = 1;
#elif GRAVITY_BYTEDANCE_GAME_MODE
    int miniGameHongIndex = 2;
#elif GRAVITY_BYTEDANCE_TT_GAME_MODE
    int miniGameHongIndex = 3;
#elif GRAVITY_BYTEDANCE_CLOUD_MODE
    int miniGameHongIndex = 4;
#elif GRAVITY_KUAISHOU_GAME_MODE
    int miniGameHongIndex = 5;
#elif GRAVITY_KUAISHOU_WEBGL_GAME_MODE
    int miniGameHongIndex = 6;
#elif GRAVITY_ALIPAY_GAME_MODE
    int miniGameHongIndex = 7;
#elif GRAVITY_MEITUAN_GAME_MODE
    int miniGameHongIndex = 8;
#elif GRAVITY_BILIBILI_GAME_MODE
    int miniGameHongIndex = 9;
#elif GRAVITY_HUAWEI_GAME_MODE
    int miniGameHongIndex = 10;
#elif GRAVITY_OPPO_GAME_MODE
    int miniGameHongIndex = 11;
#elif GRAVITY_XIAOMI_GAME_MODE
    int miniGameHongIndex = 12;
#elif GRAVITY_TAPTAP_GAME_MODE
    int miniGameHongIndex = 13;
#elif GRAVITY_VIVO_GAME_MODE
    int miniGameHongIndex = 14;
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || GRAVITY_WINDOWS_GAME_MODE
    int miniGameHongIndex = 15;
#else
    int miniGameHongIndex = 0;
#endif


#if ENABLE_TENCENT_SDK_TRACK
    bool enable_tencent_sdk_track = true;
#else
    bool enable_tencent_sdk_track = false;
#endif

#if ENABLE_TENCENT_SDK_TRACK_ERROR
    bool enable_tencent_sdk_track_error = true;
#else
    bool enable_tencent_sdk_track_error = false;
#endif

#if ENABLE_AUTO_CHECK_DIRECT_AD_GAME_STATUS
    bool enable_auto_check_direct_ad_game_status = true;
#else
    bool enable_auto_check_direct_ad_game_status = false;
#endif
    

#if KUAISHOU_BELOW_121_VERSION
    bool kuaishou_below_121_verson = true;
#else
    bool kuaishou_below_121_verson = false;
#endif

#if KUAISHOU_WEBGL_BELOW_TUANJIE_VERSION
    bool kuaishou_webgl_below_tuanjie_verson = true;
#else
    bool kuaishou_webgl_below_tuanjie_verson = false;
#endif

#if GRAVITY_CONFIG_XML_READER_CLOSE
    bool close_gravity_xml_reader = true;
#else
    bool close_gravity_xml_reader = false;
#endif

    #endregion

    #region 自动启动配置
    private string path = "Assets/GravityEngine/GravityEngineAutoConfig.cs";

    bool isChina = true;

    string accessToken = "";
    string channel = "";

    bool debugMode = false;

    bool enableIMEI = true;
    bool enableOAID = true;
    bool enableAndroidId = true;
    bool enableMAC = true;

    bool isGDPRArea = false;
    bool isCoppaEnabled = false;
    bool isKidsAppEnabled = false;

    string fbAppID = "";

    string foregroundSessionThreshold = "0";

    ReorderableList mClientIdPriorityOrderList;
    List<string> mClientIdPriorityOrderItems;


    #endregion



    string UpdateDefines(string currentDefines, bool needDefine, string defineStr)
    {
        currentDefines = currentDefines.Replace(" ", "");
        if (currentDefines.Equals(defineStr))
        {
            //就这一个
            if (!needDefine)
            {
                currentDefines = "";
                Debug.Log(defineStr + " removed.");
            }
        }
        else if (currentDefines.StartsWith(defineStr + ";"))
        {
            //第一个
            if (!needDefine)
            {
                currentDefines = currentDefines.Replace((defineStr + ";"), "");
                Debug.Log(defineStr + " removed.");
            }
        }
        else if (currentDefines.Contains(";" + defineStr + ";") || currentDefines.EndsWith(";" + defineStr))
        {
            //中间或最后一个
            if (!needDefine)
            {
                currentDefines = currentDefines.Replace((";" + defineStr), "");
                Debug.Log(defineStr + " removed.");
            }
        }
        else
        {
            //没有
            if (needDefine)
            {
                if (currentDefines.Length > 0)
                {
                    currentDefines = currentDefines + ";" + defineStr;
                }
                else
                {
                    currentDefines = defineStr;
                }
                Debug.Log(defineStr + " added.");
            }
        }
        return currentDefines;
    }


    void Load()
    {
        #region 自动启动配置
        string text = File.ReadAllText(path);

        isChina = Regex.Match(text, "isChina\\s*=\\s*(true|false)").Value.Contains("true");

        accessToken = Regex.Match(text, "accessToken\\s*=\\s*\"(.*?)\"").Groups[1].Value;
        channel = Regex.Match(text, "channel\\s*=\\s*\"(.*?)\"").Groups[1].Value;

        debugMode = Regex.Match(text, "debugMode\\s*=\\s*(true|false)").Value.Contains("true");

        enableIMEI = Regex.Match(text, "enableIMEI\\s*=\\s*(true|false)").Value.Contains("true");
        enableOAID = Regex.Match(text, "enableOAID\\s*=\\s*(true|false)").Value.Contains("true");
        enableAndroidId = Regex.Match(text, "enableAndroidId\\s*=\\s*(true|false)").Value.Contains("true");
        enableMAC = Regex.Match(text, "enableMAC\\s*=\\s*(true|false)").Value.Contains("true");

        isGDPRArea = Regex.Match(text, "isGDPRArea\\s*=\\s*(true|false)").Value.Contains("true");
        isCoppaEnabled = Regex.Match(text, "isCoppaEnabled\\s*=\\s*(true|false)").Value.Contains("true");
        isKidsAppEnabled = Regex.Match(text, "isKidsAppEnabled\\s*=\\s*(true|false)").Value.Contains("true");

        fbAppID = Regex.Match(text, "fbAppID\\s*=\\s*\"(.*?)\"").Groups[1].Value;
        foregroundSessionThreshold = Regex.Match(text, "foregroundSessionThreshold\\s*=\\s*\"(.*?)\"").Groups[1].Value;


        string mClientIdPriorityOrderStr = Regex.Match(text, "mClientIdPriorityOrder\\s*=\\s*{(.*?)}").Groups[1].Value;

        mClientIdPriorityOrderStr = mClientIdPriorityOrderStr.Replace("{", "");
        mClientIdPriorityOrderStr = mClientIdPriorityOrderStr.Replace("}", "");
        mClientIdPriorityOrderStr = mClientIdPriorityOrderStr.Replace("\"", "");
        mClientIdPriorityOrderStr = mClientIdPriorityOrderStr.Replace(" ", "");
        string[] mClientIdPriorityOrderItemsTemp = mClientIdPriorityOrderStr.Split(",");
        mClientIdPriorityOrderItems = new List<string>(mClientIdPriorityOrderItemsTemp);

        mClientIdPriorityOrderList = new ReorderableList(mClientIdPriorityOrderItems, typeof(string), true, true, false, false);//可拖拽，显示标题，不允许新增，不允许删除
        mClientIdPriorityOrderList.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "安卓/鸿蒙 自动ClientId 取值顺序(拖拽排序}");
        };

        mClientIdPriorityOrderList.drawElementCallback = (rect, index, active, focused) =>
        {
            EditorGUI.LabelField(rect, mClientIdPriorityOrderItems[index]);
        };
        #endregion
    }


    private void OnGUI()
    {
        #region 自动配置宏参数
        EditorGUILayout.LabelField("===============================Gravity-宏定义================================", EditorStyles.boldLabel);


        EditorGUILayout.LabelField("===============================打包渠道选择================================", EditorStyles.boldLabel);
        for (int i = 0; i < gravity_plaforms.Count; i++)
        {
            if (GUILayout.Toggle(miniGameHongIndex == i, gravity_plaforms[i], "Radio"))
            {
                miniGameHongIndex = i;
                if (miniGameHongIndex == 1)
                {
                    //微信小游戏
                    enable_tencent_sdk_track = EditorGUILayout.Toggle("开启腾讯DN统计", enable_tencent_sdk_track);
                    enable_auto_check_direct_ad_game_status = EditorGUILayout.Toggle("开启腾讯直玩状态检测", enable_auto_check_direct_ad_game_status);
                    enable_tencent_sdk_track_error = EditorGUILayout.Toggle("开启腾讯DN统计失败上报", enable_tencent_sdk_track_error);
                }
                else
                {
                    enable_tencent_sdk_track = false;
                    enable_auto_check_direct_ad_game_status = false;
                    enable_tencent_sdk_track_error = false;
                }
                if (miniGameHongIndex == 5)
                {
                    //快手小游戏
                    kuaishou_below_121_verson = EditorGUILayout.Toggle("低于1.2.1的快手SDK", kuaishou_below_121_verson);
                }
                else
                {
                    kuaishou_below_121_verson = false;
                }
                if (miniGameHongIndex == 6)
                {
                    //快手小游戏 WEBGL
                    kuaishou_webgl_below_tuanjie_verson = EditorGUILayout.Toggle("不兼容团结引擎的快手SDK", kuaishou_webgl_below_tuanjie_verson);
                }
                else
                {
                    kuaishou_webgl_below_tuanjie_verson = false;
                }
            }
        }
        EditorGUILayout.LabelField("===============================其他开关控制================================", EditorStyles.boldLabel);
        close_gravity_xml_reader = EditorGUILayout.Toggle("是否关闭xml读取", close_gravity_xml_reader);


        if (GUILayout.Button("应用宏设置"))
        {
            Apply();
        }
        GUILayout.Space(40);

        #endregion

        #region 自动启动配置

        EditorGUILayout.LabelField("========================引力引擎SDK自动启动参数================================", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("========需要调用GravityEngineAPI.AutoStartGravityEngine()才会使用此处设置的配置========", EditorStyles.boldLabel);
        
        GUILayout.Space(10);

        EditorGUILayout.LabelField("===============================通用========================================", EditorStyles.boldLabel);

        isChina = EditorGUILayout.Toggle("是否中国地区", isChina);

        accessToken = EditorGUILayout.TextField("AccessToken", accessToken);
        channel = EditorGUILayout.TextField("Channel", channel);

        debugMode = EditorGUILayout.Toggle("Debug模式", debugMode);
        foregroundSessionThreshold = EditorGUILayout.TextField("前台会话阀值", foregroundSessionThreshold);

        mClientIdPriorityOrderList.DoLayoutList();

        EditorGUILayout.LabelField("===============================国内========================================", EditorStyles.boldLabel);

        enableIMEI = EditorGUILayout.Toggle("收集IMEI", enableIMEI);
        enableOAID = EditorGUILayout.Toggle("收集OAID", enableOAID);
        enableAndroidId = EditorGUILayout.Toggle("收集AndroidId", enableAndroidId);
        enableMAC = EditorGUILayout.Toggle("收集MAC地址", enableMAC);

        EditorGUILayout.LabelField("===============================海外========================================", EditorStyles.boldLabel);

        isGDPRArea = EditorGUILayout.Toggle("是否欧盟地区", isGDPRArea);
        isCoppaEnabled = EditorGUILayout.Toggle("需符合[儿童隐私权保护法]", isCoppaEnabled);
        isKidsAppEnabled = EditorGUILayout.Toggle("是否是儿童应用", isKidsAppEnabled);

        fbAppID = EditorGUILayout.TextField("FacebookId", fbAppID);

        EditorGUILayout.LabelField("==========================================================================", EditorStyles.boldLabel);

        if (GUILayout.Button("保存 引力引擎SDK自动启动参数"))
        {
            Save();
        }
        #endregion
    }


    void Apply()
    {
        BuildTargetGroup target = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
        // 从 i=1 开始遍历，跳过 i=0（原生APP,原生APP不需要宏定义）
        for (int i = 1; i < gravity_plaforms.Count; i++)
        {
            currentDefines = UpdateDefines(currentDefines, miniGameHongIndex == i, gravity_defines[i]);
        }
        currentDefines = UpdateDefines(currentDefines, enable_tencent_sdk_track, "ENABLE_TENCENT_SDK_TRACK");
        currentDefines = UpdateDefines(currentDefines, enable_auto_check_direct_ad_game_status, "ENABLE_AUTO_CHECK_DIRECT_AD_GAME_STATUS");
        currentDefines = UpdateDefines(currentDefines, enable_tencent_sdk_track_error, "ENABLE_TENCENT_SDK_TRACK_ERROR");
        currentDefines = UpdateDefines(currentDefines, kuaishou_below_121_verson, "KUAISHOU_BELOW_121_VERSION");
        currentDefines = UpdateDefines(currentDefines, kuaishou_webgl_below_tuanjie_verson, "KUAISHOU_WEBGL_BELOW_TUANJIE_VERSION");
        currentDefines = UpdateDefines(currentDefines, close_gravity_xml_reader, "GRAVITY_CONFIG_XML_READER_CLOSE");

        PlayerSettings.SetScriptingDefineSymbolsForGroup(target, currentDefines);
        AssetDatabase.Refresh();
        Close();

        Debug.Log("引力引擎SDK 宏参数配置成功，Unity 将重新加载。");
    }

    void Save()
    {
        string text = File.ReadAllText(path);

        text = Regex.Replace(text, "isChina\\s*=\\s*(true|false)", $"isChina = {isChina.ToString().ToLower()}");

        text = Regex.Replace(text, "accessToken\\s*=\\s*\"(.*?)\"", $"accessToken = \"{accessToken}\"");
        text = Regex.Replace(text, "channel\\s*=\\s*\"(.*?)\"", $"channel = \"{channel}\"");

        text = Regex.Replace(text, "debugMode\\s*=\\s*(true|false)", $"debugMode = {debugMode.ToString().ToLower()}");

        text = Regex.Replace(text, "enableIMEI\\s*=\\s*(true|false)", $"enableIMEI = {enableIMEI.ToString().ToLower()}");
        text = Regex.Replace(text, "enableOAID\\s*=\\s*(true|false)", $"enableOAID = {enableOAID.ToString().ToLower()}");
        text = Regex.Replace(text, "enableAndroidId\\s*=\\s*(true|false)", $"enableAndroidId = {enableAndroidId.ToString().ToLower()}");
        text = Regex.Replace(text, "enableMAC\\s*=\\s*(true|false)", $"enableMAC = {enableMAC.ToString().ToLower()}");


        text = Regex.Replace(text, "isGDPRArea\\s*=\\s*(true|false)", $"isGDPRArea = {isGDPRArea.ToString().ToLower()}");
        text = Regex.Replace(text, "isCoppaEnabled\\s*=\\s*(true|false)", $"isCoppaEnabled = {isCoppaEnabled.ToString().ToLower()}");
        text = Regex.Replace(text, "isKidsAppEnabled\\s*=\\s*(true|false)", $"isKidsAppEnabled = {isKidsAppEnabled.ToString().ToLower()}");

        text = Regex.Replace(text, "fbAppID\\s*=\\s*\"(.*?)\"", $"fbAppID = \"{fbAppID}\"");
        text = Regex.Replace(text, "foregroundSessionThreshold\\s*=\\s*\"(.*?)\"", $"foregroundSessionThreshold = \"{foregroundSessionThreshold}\"");

        string mClientIdPriorityOrderItemsStr = string.Join("\",\"", mClientIdPriorityOrderItems);
        mClientIdPriorityOrderItemsStr = "{\"" + mClientIdPriorityOrderItemsStr + "\"}";
        text = Regex.Replace(text, "mClientIdPriorityOrder\\s*=\\s*{(.*?)}", $"mClientIdPriorityOrder = {mClientIdPriorityOrderItemsStr}");

        File.WriteAllText(path, text);

        if (isChina)
        {
            GravityEngineSDKSetting.UseGravityChinaSdk();
        }
        else
        {
            GravityEngineSDKSetting.UseGravityOverseaSdk();
        }

        AssetDatabase.Refresh();
        Close();

        Debug.Log("引力引擎SDK 启动参数配置 更新完成，Unity 将重新加载。");
    }

}
