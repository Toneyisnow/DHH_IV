using System;
using System.IO;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using MaritimeLegend.Domain;

namespace MaritimeLegend {
public partial class MaritimeGame {
    LocalizationService uiLocalization;
    static readonly string[] LanguageCodes = { "zh-CN", "en-US", "zh-TW", "ja-JP", "ko-KR" };
    static readonly string[] LanguageNames = { "简体中文", "English", "繁體中文", "日本語", "한국어" };
    string CurrentLanguage => LocalizationSettings.SelectedLocale?.Identifier.Code ?? "zh-CN";
    string L(string key, params object[] args) {
        if (uiLocalization == null) uiLocalization = new LocalizationService(c.localizationPlan);
        return uiLocalization.Text(CurrentLanguage, key, args);
    }
    void InitializeLanguages() {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        // Existing global preferences win. On first launch Unity selects the system language.
        if (File.Exists(SettingsStore.PathName)) {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(menuSettings.locale ?? "zh-CN");
            LocalizationSettings.SelectedLocale = locale ?? LocalizationSettings.AvailableLocales.GetLocale("zh-CN");
        }
        menuSettings.locale = CurrentLanguage;
        LocalizationSettings.SelectedLocaleChanged += LanguageChanged;
    }
    void LanguageChanged(Locale locale) {
        if (locale == null) return;
        menuSettings.locale = locale.Identifier.Code;
        if (s != null) s.settings.locale = locale.Identifier.Code;
    }
    void DisposeLanguages() { LocalizationSettings.SelectedLocaleChanged -= LanguageChanged; }
    void LanguagePicker() {
        Label(510, 890, 150, 40, L("ui.settings.language"), small);
        for (int i = 0; i < LanguageCodes.Length; i++) {
            if (!Btn(665 + i * 148, 888, 140, 42, LanguageNames[i], CurrentLanguage != LanguageCodes[i])) continue;
            var set = s == null ? menuSettings : s.settings;
            var old = set.locale;
            try {
                set.locale = LanguageCodes[i];
                SettingsStore.Save(set);
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(set.locale);
            } catch (Exception e) { set.locale = old; Notify(L("ui.settings.save_failed", e.Message)); }
        }
    }
}
}
