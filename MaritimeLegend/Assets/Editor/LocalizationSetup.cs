using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;
using MaritimeLegend.Domain;

namespace MaritimeLegend.Editor {
public static class LocalizationSetup {
    public const string Root = "Assets/Localization";
    public const string Collection = "GameText";
    public static readonly string[] Codes = { "zh-CN", "en-US", "zh-TW", "ja-JP", "ko-KR" };
    static readonly string[] Names = { "简体中文", "English", "繁體中文", "日本語", "한국어" };

    [MenuItem("Maritime Legend/Localization/Create or repair five languages")]
    public static void Configure() {
        Directory.CreateDirectory(Root + "/Locales");
        Directory.CreateDirectory(Root + "/Tables");
        AssetDatabase.Refresh();
        var settings = LocalizationEditorSettings.ActiveLocalizationSettings;
        if (settings == null) {
            settings = ScriptableObject.CreateInstance<LocalizationSettings>();
            AssetDatabase.CreateAsset(settings, Root + "/LocalizationSettings.asset");
            LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            settings.GetStartupLocaleSelectors().Clear();
            settings.GetStartupLocaleSelectors().Add(new SystemLocaleSelector());
            settings.GetStartupLocaleSelectors().Add(new SpecificLocaleSelector { LocaleId = "zh-CN" });
        }
        var locales = new List<Locale>();
        for (int i = 0; i < Codes.Length; i++) {
            var locale = LocalizationEditorSettings.GetLocales().FirstOrDefault(l => l.Identifier.Code == Codes[i]);
            if (locale == null) {
                locale = Locale.CreateLocale(Codes[i]);
                locale.LocaleName = Names[i];
                AssetDatabase.CreateAsset(locale, Root + "/Locales/" + Codes[i] + ".asset");
                LocalizationEditorSettings.AddLocale(locale);
            }
            locales.Add(locale);
        }
        LocalizationSettings.ProjectLocale = locales[0];
        foreach (var locale in locales.Skip(1)) {
            if (locale.Metadata.GetMetadata<FallbackLocale>() == null)
                locale.Metadata.AddMetadata(new FallbackLocale(locales[0]));
            EditorUtility.SetDirty(locale);
        }
        var collection = LocalizationEditorSettings.GetStringTableCollection(Collection)
            ?? LocalizationEditorSettings.CreateStringTableCollection(Collection, Root + "/Tables", locales);
        foreach (var locale in locales) {
            var table = collection.GetTable(locale.Identifier) as UnityEngine.Localization.Tables.StringTable;
            if (table == null) table = (UnityEngine.Localization.Tables.StringTable)collection.AddNewTable(locale.Identifier);
            var source = JsonUtility.FromJson<LocaleTable>(File.ReadAllText("Assets/Resources/Localization/" + locale.Identifier.Code + ".json"));
            // Seed only missing keys: editor translations remain authoritative on subsequent runs.
            foreach (var entry in source.entries)
                if (table.GetEntry(entry.key) == null) table.AddEntry(entry.key, entry.text);
            EditorUtility.SetDirty(table);
        }
        collection.SetPreloadTableFlag(true);
        EditorUtility.SetDirty(collection.SharedData);
        EditorUtility.SetDirty(collection);
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        settings.ResetState();
        LocalizationChecks.Run();
    }

    [MenuItem("Maritime Legend/Localization/Open translation tables")]
    public static void OpenTables() {
        var collection = LocalizationEditorSettings.GetStringTableCollection(Collection);
        if (collection == null) { Configure(); collection = LocalizationEditorSettings.GetStringTableCollection(Collection); }
        EditorWindow.GetWindow<UnityEditor.Localization.UI.LocalizationTablesWindow>().EditCollection(collection);
    }
}
}
