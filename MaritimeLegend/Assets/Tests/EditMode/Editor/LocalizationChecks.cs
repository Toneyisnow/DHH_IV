#if UNITY_EDITOR
using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using MaritimeLegend.Domain;
namespace MaritimeLegend.Editor {
public static class LocalizationChecks {
    static void Require(bool ok, string message) { if (!ok) throw new Exception("LOCALIZATION: " + message); }
    static string Slots(string text) => string.Join(",", Regex.Matches(text, @"\{(\d+)(?:[^}]*)\}").Cast<Match>().Select(m => m.Groups[1].Value).OrderBy(x => x));
    [MenuItem("Maritime Legend/Localization/Validate five languages")]
    public static void Run() {
        var collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationSetup.Collection);
        Require(collection != null, "GameText missing");
        var source = (StringTable)collection.GetTable("zh-CN");
        var keys = source.Values.Select(e => e.Key).OrderBy(k => k).ToArray();
        var plan = new LocalizationPlan { locales = LocalizationSetup.Codes, fallbackLocale = "zh-CN", sourceLocale = "zh-CN" };
        var service = new LocalizationService(plan);
        foreach (var code in LocalizationSetup.Codes) {
            Require(LocalizationEditorSettings.GetLocales().Any(l => l.Identifier.Code == code), "Locale " + code);
            var table = (StringTable)collection.GetTable(code);
            Require(table != null, "Table " + code);
            Require(keys.SequenceEqual(table.Values.Select(e => e.Key).OrderBy(k => k)), "Keys " + code);
            foreach (var key in keys) {
                var value = table.GetEntry(key).LocalizedValue;
                Require(!string.IsNullOrWhiteSpace(value), code + "/" + key);
                Require(Slots(value) == Slots(source.GetEntry(key).LocalizedValue), "Placeholders " + code + "/" + key);
                string.Format(System.Globalization.CultureInfo.GetCultureInfo(code), value, "Tea", 2, 3);
                Require(service.Text(code, key, "Tea", 2, 3) == string.Format(System.Globalization.CultureInfo.GetCultureInfo(code), value, "Tea", 2, 3), "Runtime " + code + "/" + key);
            }
        }
        Require(service.Text("invalid", "ui.sailing.accept") == source.GetEntry("ui.sailing.accept").LocalizedValue, "Unsupported locale fallback");
        Require(service.Text("en-US", "test.absent") == "[test.absent]", "Missing key marker");
        // Prove edits in the native table reach the existing runtime service immediately.
        var english = (StringTable)collection.GetTable("en-US");
        var entry = english.GetEntry("ui.sailing.accept");
        var original = entry.Value;
        try {
            entry.Value = "Native table edit";
            Require(service.Text("en-US", entry.Key) == entry.Value, "Native table authority");
            entry.Value = "";
            Require(service.Text("en-US", entry.Key) == source.GetEntry(entry.Key).Value, "Empty translation falls back to source");
        }
        finally { entry.Value = original; }
        var report = "PASS: 5 locales; " + keys.Length + " keys per locale; placeholders, runtime reads, native edits, unknown locale/key fallback.\nFull-game translation and font coverage remain incomplete.\n";
        Directory.CreateDirectory("Documentation");
        File.WriteAllText("Documentation/LocalizationChecks.txt", report);
        Debug.Log(report);
    }
}
}
#endif
