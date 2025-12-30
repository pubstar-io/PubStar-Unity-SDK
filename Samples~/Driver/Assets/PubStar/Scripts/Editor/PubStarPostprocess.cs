#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEngine;
using UnityEditor.iOS.Xcode;

public static class PubStarPodPostprocess
{
    private const string PodName = "Pubstar";
    private const string PodVersion = "'~> 1.3.1'";
    private static readonly string[] SkAdNetworkIds =
    {
        "cstr6suwn9.skadnetwork",
        "4fzdc2evr5.skadnetwork",
        "2fnua5tdw4.skadnetwork",
        "ydx93a7ass.skadnetwork",
        "p78axxw29g.skadnetwork",
        "v72qych5uu.skadnetwork",
        "ludvb6z3bs.skadnetwork",
        "cp8zw746q7.skadnetwork",
        "3sh42y64q3.skadnetwork",
        "c6k4g5qg8m.skadnetwork",
        "s39g8k73mm.skadnetwork",
        "3qy4746246.skadnetwork",
        "f38h382jlk.skadnetwork",
        "hs6bdukanm.skadnetwork",
        "mlmmfzh3r3.skadnetwork",
        "v4nxqhlyqp.skadnetwork",
        "wzmmz9fp6w.skadnetwork",
        "su67r6k2v3.skadnetwork",
        "yclnxrl5pm.skadnetwork",
        "t38b2kh725.skadnetwork",
        "7ug5zh24hu.skadnetwork",
        "gta9lk7p23.skadnetwork",
        "vutu7akeur.skadnetwork",
        "y5ghdn5j9k.skadnetwork",
        "v9wttpbfk9.skadnetwork",
        "n38lu8286q.skadnetwork",
        "47vhws6wlr.skadnetwork",
        "kbd757ywx3.skadnetwork",
        "9t245vhmpl.skadnetwork",
        "a2p9lx4jpn.skadnetwork",
        "22mmun2rn5.skadnetwork",
        "44jx6755aq.skadnetwork",
        "k674qkevps.skadnetwork",
        "4468km3ulz.skadnetwork",
        "2u9pt9hc89.skadnetwork",
        "8s468mfl3y.skadnetwork",
        "klf5c3l5u5.skadnetwork",
        "ppxm28t8ap.skadnetwork",
        "kbmxgpxpgc.skadnetwork",
        "uw77j35x4d.skadnetwork",
        "578prtvx9j.skadnetwork",
        "4dzt52r2t5.skadnetwork",
        "tl55sbb4fm.skadnetwork",
        "c3frkrj4fj.skadnetwork",
        "e5fvkxwrpn.skadnetwork",
        "8c4e2ghe7u.skadnetwork",
        "3rd42ekr43.skadnetwork",
        "97r2b46745.skadnetwork",
        "3qcr597p9d.skadnetwork"
    };

    [PostProcessBuild(40)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        UpdatePodfileOrCreatePodfileIfNeeded(pathToBuiltProject);
        UpdateInfoIfNeeded(pathToBuiltProject);
    }

    private static void UpdateInfoIfNeeded(string pathToBuiltProject)
    {
        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");

        if (!File.Exists(plistPath))
        {
            Debug.LogError($"[PubStarPostprocess] Info.plist not found at: {plistPath}");
            return;
        }

        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict root = plist.root;

        // 1. Các key string đơn
        root.SetString(
            "NSUserTrackingUsageDescription",
            "We use your data to show personalized ads and improve your experience."
        );

        root.SetString("io.pubstar.key", "Your PubStar app ID");

        root.SetString(
            "GADApplicationIdentifier",
            "ca-app-pub-3940256099942544~1458002511"
        );

        // 2. SKAdNetworkItems
        PlistElement skAdElement;
        PlistElementArray skAdArray;

        if (root.values.TryGetValue("SKAdNetworkItems", out skAdElement)
            && skAdElement is PlistElementArray existingArray)
        {
            skAdArray = existingArray;
        }
        else
        {
            skAdArray = root.CreateArray("SKAdNetworkItems");
        }

        // Lấy các ID đã tồn tại để tránh add trùng
        var existingIds = new System.Collections.Generic.HashSet<string>();
        foreach (var element in skAdArray.values)
        {
            if (element is PlistElementDict dict &&
                dict.values.TryGetValue("SKAdNetworkIdentifier", out var idElem))
            {
                var id = idElem.AsString();
                if (!string.IsNullOrEmpty(id))
                {
                    existingIds.Add(id);
                }
            }
        }

        // Thêm các ID còn thiếu
        foreach (var id in SkAdNetworkIds)
        {
            if (existingIds.Contains(id)) continue;

            var dict = skAdArray.AddDict();
            dict.SetString("SKAdNetworkIdentifier", id);
        }

        // Ghi lại file
        File.WriteAllText(plistPath, plist.WriteToString());

        Debug.Log("[PubStarPostprocess] Info.plist updated with NSUserTrackingUsageDescription, io.pubstar.key, GADApplicationIdentifier, and SKAdNetworkItems.");
    }

    private static void UpdatePodfileOrCreatePodfileIfNeeded(string pathToBuiltProject)
    {
        string podfilePath = Path.Combine(pathToBuiltProject, "Podfile");

        if (!File.Exists(podfilePath))
        {
            Debug.Log("[PubStar] Podfile not found. Creating a default Podfile...");

            string defaultPodfile = @"
platform :ios, '13.0'

target 'Unity-iPhone' do
  use_frameworks!
end

target 'UnityFramework' do
  use_frameworks!
end
";

            File.WriteAllText(podfilePath, defaultPodfile);
        }

        string podfile = File.ReadAllText(podfilePath);
        bool modified = false;

        if (!podfile.Contains("pod 'Pubstar'"))
        {
            string podLine = $"pod '{PodName}', {PodVersion}";

            modified |= InsertPodIntoTarget(ref podfile, "Unity-iPhone", podLine);
            modified |= InsertPodIntoTarget(ref podfile, "UnityFramework", podLine);
        }
        else
        {
            Debug.Log("[PubStar] Podfile already contains 'Pubstar'.");
        }

        if (modified)
        {
            File.WriteAllText(podfilePath, podfile);
            Debug.Log("[PubStar] Podfile updated with Pubstar pod.");
        }
    }

    private static bool InsertPodIntoTarget(ref string podfile, string targetName, string podLine)
    {
        string targetMarker = $"target '{targetName}' do";
        int index = podfile.IndexOf(targetMarker, System.StringComparison.Ordinal);
        if (index < 0)
        {
            Debug.LogWarning($"[PubStar] Could not find target '{targetName}' in Podfile.");
            return false;
        }

        int insertPos = podfile.IndexOf('\n', index + targetMarker.Length);
        if (insertPos < 0)
            insertPos = podfile.Length;

        string insertText = $"\n  {podLine}\n";
        podfile = podfile.Insert(insertPos, insertText);

        Debug.Log($"[PubStar] Added '{podLine}' to target '{targetName}' in Podfile.");
        return true;
    }
}

#endif
