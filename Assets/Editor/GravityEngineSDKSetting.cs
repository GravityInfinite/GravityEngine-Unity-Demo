using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class GravityEngineSDKSetting
{

    [MenuItem("引力引擎/使用国内底层SDK")]
    public static void UseGravityChinaSdk()
    {
        string filePath = "Assets/GravityNet/Android/Editor/Dependencies.xml";
        List<string> androidList = new List<string>();
        androidList.Add(androidPackagesStrForInfo("cn.gravity.android:GravityEngineSDK:+", new List<string>() { "https://nexus.gravity-engine.com/repository/maven-releases/", "https://nexus.gravity-engine.com/repository/maven-snapshots/"}));
        writeXmlContent(filePath, dependenciesXmlContentForAndroidPackages(androidList));

        filePath = "Assets/GravityNet/iOS/Editor/Dependencies.xml";
        writeXmlContent(filePath, dependenciesXmlContentForIosPackages(new List<string>() { "GravityEngineSDK" }));

        filePath = "Assets/GravityNet/GravityPlugins/Oaid/Editor/Dependencies.xml";
        List<string> oaidList = new List<string>();
        oaidList.Add(androidPackagesStrForInfo("com.huawei.hms:ads-identifier:3.4.62.300", new List<string>() { "https://developer.huawei.com/repo/" }));
        oaidList.Add(androidPackagesStrForInfo("com.hihonor.mcs:ads-identifier:1.0.2.301", new List<string>() { "https://developer.hihonor.com/repo" }));
        writeXmlContent(filePath, dependenciesXmlContentForAndroidPackages(oaidList));

        filePath = "Assets/GravityNet/GravityPlugins/Google/Editor/Dependencies.xml";
        writeXmlContent(filePath, dependenciesXmlContentForAndroidPackages());
    }

    [MenuItem("引力引擎/使用海外底层SDK")]
    public static void UseGravityOverseaSdk()
    {
        string filePath = "Assets/GravityNet/Android/Editor/Dependencies.xml";
        List<string> androidList = new List<string>();
        androidList.Add(androidPackagesStrForInfo("oversea.gravity.android:GravityEngineSDK:+", new List<string>() { "https://nexus.gravity-engine.com/repository/maven-releases/", "https://nexus.gravity-engine.com/repository/maven-snapshots/" }));
        writeXmlContent(filePath, dependenciesXmlContentForAndroidPackages(androidList));

        filePath = "Assets/GravityNet/iOS/Editor/Dependencies.xml";
        writeXmlContent(filePath, dependenciesXmlContentForIosPackages(new List<string>() { "GravityEngineOverseaSDK" }));

        filePath = "Assets/GravityNet/GravityPlugins/Oaid/Editor/Dependencies.xml";
        List<string> oaidList = new List<string>();
        oaidList.Add(androidPackagesStrForInfo("com.huawei.hms:ads-identifier:3.4.62.300", new List<string>() { "https://developer.huawei.com/repo/" }));
        oaidList.Add(androidPackagesStrForInfo("com.hihonor.mcs:ads-identifier:1.0.2.301", new List<string>() { "https://developer.hihonor.com/repo" }));
        writeXmlContent(filePath, dependenciesXmlContentForAndroidPackages(oaidList));

        filePath = "Assets/GravityNet/GravityPlugins/Google/Editor/Dependencies.xml";
        writeXmlContent(filePath, dependenciesXmlContentForAndroidPackages(new List<string>() { androidPackagesStrForInfo("com.android.installreferrer:installreferrer:+") }));
    }

    [MenuItem("引力引擎/不使用底层SDK")]
    public static void NotUseGravityNativeSdk()
    {
        string filePath = "Assets/GravityNet/Android/Editor/Dependencies.xml";
        writeXmlContent(filePath, dependenciesXmlContentForAndroidPackages());

        filePath = "Assets/GravityNet/iOS/Editor/Dependencies.xml";
        writeXmlContent(filePath, dependenciesXmlContentForIosPackages());

        filePath = "Assets/GravityNet/GravityPlugins/Oaid/Editor/Dependencies.xml";
        writeXmlContent(filePath, dependenciesXmlContentForAndroidPackages());

        filePath = "Assets/GravityNet/GravityPlugins/Google/Editor/Dependencies.xml";
        writeXmlContent(filePath, dependenciesXmlContentForAndroidPackages());
    }

    private static void writeXmlContent(string filePath, string newContent) {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        File.WriteAllText(filePath, newContent);
    }


    private static string androidPackagesStrForInfo(string androidPackage, List<string> urls = null)
    {
        string result =
          "  <androidPackages>" + "\n"
        + "    <androidPackage spec=\"" + androidPackage + "\">" + "\n";

        if (urls != null)
        {
            result = result
            + "      <repositories>" + "\n";
            foreach (string url in urls)
            {
                result = result
                + "        <repository>" + url + "</repository>" + "\n";
            }
            result = result
                + "      </repositories>" + "\n";
        }

        result = result
        + "    </androidPackage>" + "\n"
        + "  </androidPackages>";
        return result;
    }

    private static string dependenciesXmlContentForIosPackages(List<string> iosPackages = null)
    {
        string result = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + "\n"
        + "<dependencies>" + "\n";
        if (iosPackages != null)
        {
            result = result
                + "  <iosPods>" + "\n";

            foreach (string iosPackage in iosPackages)
            {
                result = result
                    + "    <iosPod name=\"" + iosPackage + "\" version=\"&gt;=0\" />" + "\n";
            }
            result = result
            + "  </iosPods>" + "\n";
        }
        result = result
            + "</dependencies>" + "\n";
        return result;
    }

    private static string dependenciesXmlContentForAndroidPackages(List<string> androidPackages = null)
    {
        string result = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + "\n"
        + "<dependencies>" + "\n";
        if (androidPackages != null)
        {
            foreach (string androidPackge in androidPackages)
            {
                result = result + androidPackge + "\n";
            }
        }
        result = result
        + "</dependencies>" + "\n";
        return result;
    }
}
