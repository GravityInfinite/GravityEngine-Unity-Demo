using UnityEngine;
using System.Collections;

public static class GravityEngineAutoConfig
{
    public const bool isChina = true;

    public const string accessToken = "";
    public const string channel = "base_channel";

    public const bool debugMode = false;

    public const bool enableIMEI = true;
    public const bool enableOAID = true;
    public const bool enableAndroidId = true;
    public const bool enableMAC = true;

    public const bool isGDPRArea = false;
    public const bool isCoppaEnabled = false;
    public const bool isKidsAppEnabled = false;

    public const string fbAppID = "";
    public const string foregroundSessionThreshold = "0";

    public static string[] mClientIdPriorityOrder = {"OAID","ANDROID_ID"};

}
