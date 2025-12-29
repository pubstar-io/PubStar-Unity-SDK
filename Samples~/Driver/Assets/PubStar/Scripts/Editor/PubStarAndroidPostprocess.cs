#if UNITY_ANDROID
using System;
using System.IO;
using System.Text;
using UnityEditor.Android;

namespace PubStar.Editor
{
    public class PubStarAndroidPostprocess : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 0;

        // ===== Config =====
        private const string AppodealMavenRepoLine = "maven { url \"https://artifactory.appodeal.com/appodeal\" }";
        private const string AdmobAppId = "ca-app-pub-3940256099942544~3347511713";

        private static UTF8Encoding utf8NoBom = new UTF8Encoding(false);

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            try
            {
                var gradleRoot = ResolveGradleRoot(path);
                UnityEngine.Debug.LogError("[PubStar][AndroidPostprocess] OnPostGenerateGradleAndroidProject: " + gradleRoot);
                PatchSettingsGradle(gradleRoot);
                PatchAndroidManifest(gradleRoot);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("[PubStar][AndroidPostprocess] Failed: " + e);
            }
        }

        private static string ResolveGradleRoot(string exportedPath)
        {
            // exportedPath có thể là:
            // - <root> (đúng)
            // - <root>/unityLibrary (Unity hay trả về cái này)
            // - <root>/launcher
            // => đi lên vài cấp để tìm settings.gradle(.kts)

            var dir = new DirectoryInfo(exportedPath);

            for (int i = 0; i < 5 && dir != null; i++)
            {
                var settingsGradle = Path.Combine(dir.FullName, "settings.gradle");
                var settingsKts = Path.Combine(dir.FullName, "settings.gradle.kts");

                if (File.Exists(settingsGradle) || File.Exists(settingsKts))
                {
                    return dir.FullName; // đây là root
                }

                dir = dir.Parent;
            }

            // fallback: nếu không tìm được, cứ dùng path hiện tại
            return exportedPath;
        }

        // =========================
        // 1) settings.gradle
        // =========================
        private static void PatchSettingsGradle(string gradleProjectPath)
        {
            var settingsPath = FindFirstExistingFile(
                gradleProjectPath,
                "settings.gradle",
                "settings.gradle.kts"
            );

            if (settingsPath == null)
            {
                UnityEngine.Debug.LogWarning("[PubStar][AndroidPostprocess] settings.gradle not found.");
                return;
            }

            var text = File.ReadAllText(settingsPath, Encoding.UTF8);
            if (text.Contains(AppodealMavenRepoLine))
            {
                UnityEngine.Debug.Log("[PubStar][AndroidPostprocess] Appodeal repo already exists in settings.gradle");
                return;
            }

            // Prefer to inject inside:
            // dependencyResolutionManagement { repositories { ... } }
            // If not found, we fall back to injecting into an existing repositories { } (best effort).
            var updated = TryInsertIntoDependencyResolutionManagementRepositories(text, AppodealMavenRepoLine);
            if (updated == null)
            {
                updated = TryInsertIntoAnyRepositoriesBlock(text, AppodealMavenRepoLine);
            }

            if (updated == null)
            {
                // Last resort: append a proper block to the end (still valid, but may not be desired)
                updated = text.TrimEnd() +
                          "\n\ndependencyResolutionManagement {\n" +
                          "    repositories {\n" +
                          "        " + AppodealMavenRepoLine + "\n" +
                          "    }\n" +
                          "}\n";
            }

            File.WriteAllText(settingsPath, updated, utf8NoBom);
            UnityEngine.Debug.Log("[PubStar][AndroidPostprocess] Patched settings.gradle: added Appodeal repo");
        }

        private static string TryInsertIntoDependencyResolutionManagementRepositories(string text, string lineToInsert)
        {
            var drmIdx = text.IndexOf("dependencyResolutionManagement", StringComparison.Ordinal);
            if (drmIdx < 0) return null;

            // Find repositories { after drmIdx
            var repoIdx = text.IndexOf("repositories", drmIdx, StringComparison.Ordinal);
            if (repoIdx < 0) return null;

            var braceOpenIdx = text.IndexOf("{", repoIdx, StringComparison.Ordinal);
            if (braceOpenIdx < 0) return null;

            // Insert right after the repositories {
            var insertAt = braceOpenIdx + 1;
            var indent = DetectIndentBefore(text, braceOpenIdx) + "    ";
            return text.Insert(insertAt, "\n" + indent + lineToInsert);
        }

        private static string TryInsertIntoAnyRepositoriesBlock(string text, string lineToInsert)
        {
            // Find first "repositories {"
            var repoIdx = text.IndexOf("repositories", StringComparison.Ordinal);
            if (repoIdx < 0) return null;

            var braceOpenIdx = text.IndexOf("{", repoIdx, StringComparison.Ordinal);
            if (braceOpenIdx < 0) return null;

            var insertAt = braceOpenIdx + 1;
            var indent = DetectIndentBefore(text, braceOpenIdx) + "    ";
            return text.Insert(insertAt, "\n" + indent + lineToInsert);
        }

        // =========================
        // 3) AndroidManifest.xml
        // =========================
        private static void PatchAndroidManifest(string gradleProjectPath)
        {
            var manifestPath = FindFirstExistingFile(
                gradleProjectPath,
                Path.Combine("launcher", "src", "main", "AndroidManifest.xml"),
                Path.Combine("src", "main", "AndroidManifest.xml"),
                Path.Combine("unityLibrary", "src", "main", "AndroidManifest.xml")
            );

            if (manifestPath == null)
            {
                UnityEngine.Debug.LogWarning("[PubStar][AndroidPostprocess] AndroidManifest.xml not found.");
                return;
            }

            var xml = File.ReadAllText(manifestPath, utf8NoBom);

            // (optional) strip BOM if any
            xml = xml.TrimStart('\uFEFF', '\u200B');

            // idempotent
            if (xml.Contains("com.google.android.gms.ads.APPLICATION_ID"))
            {
                UnityEngine.Debug.Log("[PubStar][AndroidPostprocess] AdMob APPLICATION_ID meta-data already exists.");
                return;
            }

            // Find <application ...>
            var appIdx = xml.IndexOf("<application", StringComparison.Ordinal);
            if (appIdx < 0)
            {
                UnityEngine.Debug.LogWarning("[PubStar][AndroidPostprocess] <application> tag not found.");
                return;
            }

            // Find end of the <application ...> tag (the first '>' after <application)
            var tagEnd = xml.IndexOf(">", appIdx, StringComparison.Ordinal);
            if (tagEnd < 0)
            {
                UnityEngine.Debug.LogWarning("[PubStar][AndroidPostprocess] <application> tag malformed.");
                return;
            }

            // Detect whether it's self-closing: "... />"
            bool isSelfClosing = false;
            {
                var j = tagEnd - 1;
                while (j > appIdx && char.IsWhiteSpace(xml[j])) j--;
                if (j > appIdx && xml[j] == '/') isSelfClosing = true;
            }

            // Build meta-data line (single line, as you requested)
            var metaLine = $"<meta-data android:name=\"com.google.android.gms.ads.APPLICATION_ID\" android:value=\"{AdmobAppId}\"/>\n" 
            + "<meta-data android:name=\"io.pubstar.key\" android:value=\"pub-app-id-1692\" />";

            string updated;

            if (isSelfClosing)
            {
                // Convert:
                // <application ... />
                // to:
                // <application ...>
                //   <meta-data .../>
                // </application>

                // Remove the "/>" and replace with ">"
                // We'll keep everything up to just before "/>"
                var tagText = xml.Substring(appIdx, tagEnd - appIdx + 1); // includes '>'
                                                                          // Replace ending "/>" (with optional spaces) inside tagText
                                                                          // simplest: remove the "/" right before ">"
                var fixedTagText = tagText;
                {
                    var k = fixedTagText.Length - 2; // char before '>'
                    while (k >= 0 && char.IsWhiteSpace(fixedTagText[k])) k--;
                    if (k >= 0 && fixedTagText[k] == '/')
                    {
                        fixedTagText = fixedTagText.Remove(k, 1);
                    }
                }

                // Indent: try to reuse indent of the <application ...> line
                var baseIndent = DetectIndentBefore(xml, appIdx);
                var childIndent = baseIndent + "    ";

                updated =
                    xml.Substring(0, appIdx) +
                    fixedTagText + "\n" +
                    childIndent + metaLine + "\n" +
                    baseIndent + "</application>" +
                    xml.Substring(tagEnd + 1);
            }
            else
            {
                // Normal open tag: <application ...>
                // Insert meta-data right after the opening tag
                var baseIndent = DetectIndentAfterNewline(xml, tagEnd);
                var childIndent = baseIndent + "    ";

                var insert = "\n" + childIndent + metaLine;
                updated = xml.Insert(tagEnd + 1, insert);
            }

            File.WriteAllText(manifestPath, updated, utf8NoBom);
            UnityEngine.Debug.Log("[PubStar][AndroidPostprocess] Patched AndroidManifest.xml: added AdMob APPLICATION_ID");
        }

        // =========================
        // Helpers
        // =========================
        private static string FindFirstExistingFile(string root, params string[] relativeOrFileNames)
        {
            foreach (var rel in relativeOrFileNames)
            {
                var p = Path.IsPathRooted(rel) ? rel : Path.Combine(root, rel);
                if (File.Exists(p)) return p;
            }
            return null;
        }

        private static string DetectIndentBefore(string text, int index)
        {
            // Walk backwards to start of line
            var i = index;
            while (i > 0 && text[i - 1] != '\n' && text[i - 1] != '\r') i--;
            // Count spaces/tabs forward
            var start = i;
            while (i < text.Length && (text[i] == ' ' || text[i] == '\t')) i++;
            return text.Substring(start, i - start);
        }

        private static string DetectIndentAfterNewline(string text, int index)
        {
            // Find next line start after index
            var i = index;
            while (i < text.Length && text[i] != '\n') i++;
            if (i >= text.Length) return "";

            i++; // move past '\n'
            var start = i;
            while (i < text.Length && (text[i] == ' ' || text[i] == '\t')) i++;
            return text.Substring(start, i - start);
        }
    }
}
#endif
