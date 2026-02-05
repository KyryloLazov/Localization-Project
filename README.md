# Unity Remote Localization System

A Zenject-based localization system for Unity that supports **remote updates** via JSON (Firebase/REST), **Addressables** for asset management, and **dynamic font switching** based on the selected language.

## Features
- **Remote & Local Hybrid:** Loads localization from remote sources and falls back to local.
- **Dynamic Font Switching:** Automatically changes TMP Fonts for specific languages.
- **Addressables Integration:** Handles dynamic assets for text & fonts.
- **Zenject Architecture:** Clean and testable dependency injection.
- **UniTask Async Flow:** Lightweight async operations.
- **Editor Mode Preview:** Supports localization preview in Editor without entering Play Mode.

---

## 📦 Dependencies
Make sure you have:
- [Zenject (Extenject)](https://assetstore.unity.com/packages/tools/utilities/extenject-dependency-injection-ioc-157735)
- `Cysharp UniTask`
- `Addressables`
- `TextMeshPro`
- `Newtonsoft.Json`

---

## Setup Guide

### 1. Zenject Configuration
1. Create prefab **ProjectContext** at  
   `Assets/Resources/ProjectContext.prefab`
2. Create installer:  
   `Create -> Installers -> ProjectGlobalInstaller`
3. Set these fields in the inspector:
   - **Localization Url:**  
     `https://your-project.web.app/localization/localization.config.json`
   - **Fonts Url:**  
     `https://your-project.web.app/fonts/fonts.config.json`
4. Add your installer into  
   **ProjectContext → Scriptable Object Installers**
5. Add a **SceneContext** object on your scene

---

### 2. Addressables
1. Mark all TMP Font Assets as **Addressable**
   - Address name should match your `fonts.config.json`
   - In the TMP Font Asset:
     - Set **Atlas Population Mode = Dynamic**
2. Optionally: add a local fallback `LocalizationDatabase` asset  
   and mark it Addressable with the key `LocalizationDatabase`.

---

### 3. Remote Config Formats

#### ✅ Localization JSON
Example:  
```json
{
  "en": {
    "Menu": {
      "Play": "Play Game",
      "Settings": "Settings"
    }
  },
  "ua": {
    "Menu": {
      "Play": "Грати",
      "Settings": "Налаштування"
    }
  }
}
```
Usage:
```csharp
LocalizationManager.Get("Menu/Play");
```

#### ✅ Fonts JSON
Example:
```json
{
  "FontsConfig.EN_Text": "OpenSans_SDF",
  "FontsConfig.EN_Header": "Oswald_SDF",
  "FontsConfig.UA_Text": "OpenSans_Cyrillic_SDF",
  "FontsConfig.UA_Header": "Oswald_SDF"
}
```

---

## 🧩 Usage

### UI Text
Add `LocalizedTextUI` component to your TMP Text object.  
- Set **Key**, for example: `Menu/Play`  
- Choose **Font Type** — `Text` or `Header`  
Texts and fonts will update automatically on language change.

### Language Dropdown
Attach `LanguageDropdown` to a `TMP_Dropdown` component.  
It fills options automatically from available languages and updates selection.

### Code API
```csharp
// Get translation
string title = LocalizationManager.Get("Menu/Play");

// With formatting
string welcome = LocalizationManager.Get("Game/Welcome", "PlayerOne");

// Change current language
LocalizationManager.CurrentLanguage = "ua";

// Listen to events
LocalizationManager.OnLanguageChanged += () => Debug.Log("Language switched!");
LocalizationManager.OnContentChanged += () => Debug.Log("Localization updated!");
```

---

## ❓ Troubleshooting

**🧩 Text shows only keys ([Menu/Play])**  
- Verify your remote JSON has correct keys.  
- Check Console: `[Localization] Remote loaded` should appear.  
- Keys are **case-insensitive**, but `/` separators matter.  

**🧩 TMP Warning: Unable to add the requested character**  
- Font Asset atlas must be **Dynamic**.
  - Select Font Asset → *Atlas Settings* → `Atlas Population Mode = Dynamic`.

**🧩 Dropdown is empty**  
- Ensure your remote JSON loaded successfully.  
- Check Console for `[Localization] Remote loaded. Languages: X`.  
- Verify CORS headers if serving from Firebase (for WebGL builds).

---

## 📂 Code Structure

| File | Description |
|------|--------------|
| `LocalizationService.cs` | Handles remote & local loading. |
| `LocalizationManager.cs` | Static interface to the current language & text. |
| `LocalizationFontService.cs` | Dynamically loads Addressable TMP fonts. |
| `LocalizedTextUI.cs` | UI behaviour to auto-update localized texts. |
| `LanguageDropdown.cs` | Language selector UI. |
| `ProjectGlobalInstaller.cs` | Zenject DI bindings. |

---
