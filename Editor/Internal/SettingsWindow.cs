using System;
using UnityEditor;
using UnityEngine;

namespace dotsquid.ChronoHelper.Internal
{
    internal class SettingsWindow : EditorWindow
    {
        public static event Action onChronoButtonsDirty;
        public static event Action onChronoWarningTest;

        private class Styles
        {
            public GUIStyle urlLabel;
        }

        private static readonly GUILayoutOption kMidButtonMaxWidth = GUILayout.MaxWidth(120.0f);
        private static readonly Vector2 kWinSize = new Vector2(360.0f, 680.0f);
        private const string kVersionLabel = "Version: ";
        private const string kDialogueResetSettingsTitle = "Reset settings";
        private const string kDialogueResetSettingsMessage = "Are you sure want to reset settings?";
        private const string kDialogueResetSettingsYes = "Yes";
        private const string kDialogueResetSettingsNo = "No";
        private const float kWarningBlinkMinPeriod = 0.25f;
        private const float kWarningBlinkMaxPeriod = 5.0f;
        private const float kWarningBlinkMinDuration = 0.5f;
        private const float kWarningBlinkMaxDuration = 10.0f;

        private static SettingsWindow _window;
        private static Texture2D _logoTexture;
        private Styles _styles;
        private Vector2 _scrollPosition;

        private void OnEnable()
        {
            titleContent = new GUIContent("Settings");
            InitTextureContent();
        }

        private void OnDisable()
        {
            ReleaseTextureContent();
        }

        private void OnDestroy()
        {
            Settings.I.Save();
        }

        private void OnGUI()
        {
            EnsureStyles();
            DrawLayout();
        }

        private void InitTextureContent()
        {
            _logoTexture = Helper.CreateTextureFromBase64(Base64Image.Icon.Logo, "CH_Icon_Logo");
        }

        private void ReleaseTextureContent()
        {
            DestroyImmediate(_logoTexture);
        }

        private void EnsureStyles()
        {
            if (_styles == null)
            {
                _styles = new Styles();
                _styles.urlLabel = new GUIStyle(EditorStyles.label);
                _styles.urlLabel.fontSize = 16;
            }
        }

        private void DrawLayout()
        {
            using (var vertical = new EditorGUILayout.VerticalScope())
            {
                GUILayout.FlexibleSpace();
                DrawInfo();
                GUILayout.Space(8.0f);
                using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPosition))
                {
                    _scrollPosition = scroll.scrollPosition;
                    DrawBackgroundSettings();
                    GUILayout.Space(8.0f);
                    DrawButtonsSettings();
                }
                GUILayout.Space(4.0f);
                DrawResetButton();
                GUILayout.FlexibleSpace();
            }
        }

        private void DrawBackgroundSettings()
        {
            var settings = Settings.I;
            using (var group = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Background", EditorStyles.centeredGreyMiniLabel);
                settings.normalBackColor = EditorGUILayout.ColorField("Normal color", settings.normalBackColor);
                settings.warningBackColor = EditorGUILayout.ColorField("Warning color", settings.warningBackColor);
                settings.warningBlinkPeriod = EditorGUILayout.Slider("Warning blink period", settings.warningBlinkPeriod, kWarningBlinkMinPeriod, kWarningBlinkMaxPeriod);
                settings.warningBlinkDuration = EditorGUILayout.Slider("Warning blink duration", settings.warningBlinkDuration, kWarningBlinkMinDuration, kWarningBlinkMaxDuration);
                settings.warningMode = (WarningMode)EditorGUILayout.EnumPopup("Warning mode", settings.warningMode);

                GUILayout.Space(2.0f);
                using (var center = new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Test warning", EditorStyles.miniButton, kMidButtonMaxWidth))
                    {
                        onChronoWarningTest?.Invoke();
                    }
                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void DrawButtonsSettings()
        {
            var settings = Settings.I;
            using (var group = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Buttons", EditorStyles.centeredGreyMiniLabel);
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    settings.showResetButton = EditorGUILayout.Toggle("Show reset button", settings.showResetButton);
                    settings.layout = (Layout)EditorGUILayout.EnumPopup("Window layout", settings.layout);
                    settings.blockOrder = (BlockOrder)EditorGUILayout.EnumPopup("Button block order", settings.blockOrder);
                    settings.buttonWidth = (ButtonWidth)EditorGUILayout.EnumPopup("Button width", settings.buttonWidth);
                    settings.buttonFormat = (Format)EditorGUILayout.EnumPopup("Button format", settings.buttonFormat);
                    EditorGUILayout.PropertyField(settings.chronoPointsListProp);

                    if (check.changed)
                    {
                        if (null != onChronoButtonsDirty)
                            onChronoButtonsDirty.Invoke();
                    }
                }
            }
        }

        private void DrawResetButton()
        {
            using (var center = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reset settings", EditorStyles.miniButton, kMidButtonMaxWidth))
                {
                    if (EditorUtility.DisplayDialog(kDialogueResetSettingsTitle, kDialogueResetSettingsMessage, kDialogueResetSettingsYes, kDialogueResetSettingsNo))
                    {
                        Settings.I.ResetToDefault();
                        if (null != onChronoButtonsDirty)
                            onChronoButtonsDirty.Invoke();
                    }
                }
                GUILayout.FlexibleSpace();
            }
        }

        private void DrawInfo()
        {
            GUILayout.Space(16.0f);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Box(_logoTexture, GUIStyle.none);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Space(8.0f);
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();
                    DrawLinkButton("GitHub", Consts.URL.Github);
                    DrawLinkButton("Home page", Consts.URL.Homepage);
                    DrawLinkButton("dotsquid.com", Consts.URL.DotsquidDotCom);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.LabelField(kVersionLabel + Helper.version, EditorStyles.centeredGreyMiniLabel);
        }

        private bool DrawLinkButton(string title, string url = null)
        {
            bool result = GUILayout.Button(title, _styles.urlLabel);
            var rect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (result && !string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
            }
            return result;
        }

        public static SettingsWindow Open()
        {
            _window = GetWindow<SettingsWindow>(true, "Settings", true);
            _window.minSize = kWinSize;
            _window.maxSize = kWinSize;
            return _window;
        }
    }
}
