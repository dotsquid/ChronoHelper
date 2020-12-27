using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using dotsquid.ChronoHelper.Internal;

namespace dotsquid.ChronoHelper
{
    internal class ChronoHelper : EditorWindow, IHasCustomMenu
    {
        private static readonly Vector2 kBorderPositionOffset = Vector2.one;
        private static readonly Vector2 kBorderSizeOffset = kBorderPositionOffset * 2.0f;

        private class ChronoBack : IDisposable
        {
            private const float kTau = Mathf.PI * 2.0f;

            public enum Mode
            {
                Normal,
                Warning,
            }

            private ChronoHelper _owner;
            private Mode _mode;
            private double _modeSetTime;
            private float _blinkPhase;

            public Mode mode
            {
                get => _mode;
                set
                {
                    if (value != _mode)
                    {
                        _mode = value;
                        _modeSetTime = EditorApplication.timeSinceStartup;
                    }
                }
            }

            private Color color
            {
                get
                {
                    var setting = Settings.I;
                    var normalColor = setting.normalBackColor;
                    switch (_mode)
                    {
                        case Mode.Warning:
                            var warningColor = setting.warningBackColor;
                            return Color.Lerp(normalColor, warningColor, _blinkPhase);

                        case Mode.Normal:
                        default:
                            return normalColor;
                    }
                }
            }

            private TextureStorage textureStorage => _owner._textureStorage;

            public ChronoBack(ChronoHelper owner)
            {
                _owner = owner;
                EditorApplication.update += Update;
            }

            public void Dispose()
            {
                EditorApplication.update -= Update;
            }

            public void Draw(Rect windowRect)
            {
                using (GUIHelper.ReplaceColor.With(color))
                {
                    var backTexture = textureStorage.backTexture;
                    var backRect = windowRect;
                    backRect.position = kBorderPositionOffset;
                    backRect.size -= kBorderSizeOffset;
                    var uvRect = default(Rect);
                    uvRect.width = backRect.width / backTexture.width;
                    uvRect.height = backRect.height / backTexture.height;
                    GUI.DrawTextureWithTexCoords(backRect, backTexture, uvRect);
                }
            }

            private void Update()
            {
                if (_mode == Mode.Warning)
                {
                    var setting = Settings.I;
                    var time = EditorApplication.timeSinceStartup;
                    var warningPeriod = setting.warningBlinkPeriod;
                    var warningDuration = setting.warningBlinkDuration;
                    var endTime = _modeSetTime + warningDuration;
                    if (time > endTime)
                    {
                        mode = Mode.Normal;
                    }
                    else
                    {
                        var phase = (endTime - EditorApplication.timeSinceStartup) / warningPeriod;
                        _blinkPhase = (warningDuration > 0.0f)
                                    ? 0.5f - (float)Math.Sin(phase * kTau) * 0.5f
                                    : 1.0f;
                    }
                }
            }
        }

        private class ChronoButton
        {
            private ChronoHelper _owner;
            private GUIContent _editModeContent;
            private GUIContent _playModeContent;
            private GUIContent _editModePressedContent;
            private GUIContent _playModePressedContent;
            private float _value;
            private bool _currentState = false;
            private bool _oldState = false;

            public bool state
            {
                get => _currentState;
                set
                {
                    _currentState = value;
                    if (!_currentState && _owner._currentChronoButton == this)
                        _owner._currentChronoButton = null;
                    if (!_oldState && _currentState) // button was pressed in
                    {
                        if (null != _owner._currentChronoButton)
                            _owner._currentChronoButton.state = false;
                        _owner._currentChronoButton = this;
                        data.chronoScale = Mathf.Clamp(_value, kChronoMinScale, kChronoMaxScale);
                    }
                    _oldState = _currentState;
                }
            }

            public float value => _value;

            private Data data => _owner._data;
            private GUIContent normalContent => _owner._isPlayMode ? _playModeContent : _editModeContent;
            private GUIContent pressedContent => _owner._isPlayMode ? _playModePressedContent : _editModePressedContent;
            private GUIContent content => state ? pressedContent : normalContent;

            public ChronoButton(ChronoHelper owner, float value, GUIContent normalContent, GUIContent pressedContent = null)
            {
                _owner = owner;
                _value = value;
                _playModeContent = new GUIContent(normalContent);
                _editModeContent = new GUIContent(normalContent);
                if (null == pressedContent)
                {
                    _playModePressedContent = new GUIContent(normalContent);
                    _editModePressedContent = new GUIContent(normalContent);
                }
                else
                {
                    _playModePressedContent = new GUIContent(pressedContent);
                    _editModePressedContent = new GUIContent(pressedContent);
                }
                _editModeContent.tooltip = kEditModeTooltip;
            }

            public void Update()
            {
                state = (Mathf.Abs(data.chronoScale - _value) <= Mathf.Min(_value * 0.05f, 0.05f));
                if (state)
                    data.chronoScale = _value;
            }

            public void Draw(string style = null)
            {
                if (string.IsNullOrEmpty(style))
                    style = kButtonStyle;
                state = GUILayout.Toggle(state, content, style, _owner._controlButtonWidth, kChronoButtonHeight);
            }
        }

        [Serializable]
        private class Data
        {
            private const string kPrefKey = Consts.kNamePrefix + "data";

            public float chronoScale = 1.0f;
            public bool canResetOnPlayEnd = true;
            public bool canSuppressTimeScale = false;

            public void Save()
            {
                var json = JsonUtility.ToJson(this);
                EditorPrefs.SetString(kPrefKey, json);
            }

            public void Load()
            {
                if (EditorPrefs.HasKey(kPrefKey))
                {
                    var json = EditorPrefs.GetString(kPrefKey);
                    JsonUtility.FromJsonOverwrite(json, this);
                }
            }
        }

        private class TextureStorage
        {
            public Texture2D backTexture;
            public Texture2D settingsIconDarkTexture;
            public Texture2D settingsIconLightTexture;
            public Texture2D resetIconDarkTexture;
            public Texture2D resetIconLightTexture;
            public Texture2D pauseIconDarkTexture;
            public Texture2D pauseIconLightTexture;
            public Texture2D lockedIconLightTexture;
            public Texture2D unlockedIconDarkTexture;
            public Texture2D unlockedIconLightTexture;
            public Texture2D autoResetOnIconLightTexture;
            public Texture2D autoResetOffIconDarkTexture;
            public Texture2D autoResetOffIconLightTexture;

            private List<Texture2D> _all = new List<Texture2D>();

            public void Load()
            {
                Load(out backTexture, Base64Image.Texture.Back, "CH_Texture_Back_Stripes");
                Load(out settingsIconDarkTexture, Base64Image.Icon.Settings_dark, "CH_Icon_Settings_Dark");
                Load(out settingsIconLightTexture, Base64Image.Icon.Settings_light, "CH_Icon_Settings_Light");
                Load(out resetIconDarkTexture, Base64Image.Icon.Reset_dark, "CH_Icon_Reset_Dark");
                Load(out resetIconLightTexture, Base64Image.Icon.Reset_light, "CH_Icon_Reset_Light");
                Load(out pauseIconDarkTexture, Base64Image.Icon.Pause_dark, "CH_Icon_Pause_Dark");
                Load(out pauseIconLightTexture, Base64Image.Icon.Pause_light, "CH_Icon_Pause_Light");
                Load(out lockedIconLightTexture, Base64Image.Icon.Locked_light, "CH_Icon_Locked_Light");
                Load(out unlockedIconDarkTexture, Base64Image.Icon.Unlocked_dark, "CH_Icon_Unlocked_Dark");
                Load(out unlockedIconLightTexture, Base64Image.Icon.Unlocked_light, "CH_Icon_Unlocked_Light");
                Load(out autoResetOnIconLightTexture, Base64Image.Icon.AutoResetOn_light, "CH_Icon_AutoResetOn_Light");
                Load(out autoResetOffIconDarkTexture, Base64Image.Icon.AutoResetOff_dark, "CH_Icon_AutoResetOff_Dark");
                Load(out autoResetOffIconLightTexture, Base64Image.Icon.AutoResetOff_light, "CH_Icon_AutoResetOff_Light");
            }

            public void Clear()
            {
                foreach (var texture in _all)
                {
                    DestroyImmediate(texture);
                }
                _all.Clear();
            }

            private void Load(out Texture2D texture, string base64, string name)
            {
                texture = Helper.CreateTextureFromBase64(base64, name);
                _all.Add(texture);
            }
        }

        #region Constants
        private const float kChronoMinScale = 0.0f;
        private const float kChronoMaxScale = 100.0f;
        private const float kChronoButtonDefaultWidth = 38.0f;
        private const float kHorizontalLayoutThresholdFactor = 1.2f;
        private const float kHorizontalLayoutThresholdExtra = 128.0f;
        private const float kHorizontalLayoutHeight = 24.0f;
        private const float kVerticalLayoutUndockedHeight = 40.0f;
        private const float kVerticalLayoutDockedHeight = 44.0f;
        private const float kWindowMinWidth = 156.0f;
        private const float kWindowMaxWidth = 8192.0f;

        private const string kButtonStyle = "Button";
        private const string kButtonMidStyle = "ButtonMid";
        private const string kButtonLeftStyle = "ButtonLeft";
        private const string kButtonRightStyle = "ButtonRight";
        private const string kEditModeTooltip = "Does not affect Time.timeScale while in EditorMode";
        private const string kMenuPath = "Window/ChronoHelper";

        private static readonly GUIContent kSettingsMenuItemContent = new GUIContent("Settings");
        private static readonly GUIContent kGitHubMenuItemContent = new GUIContent("GitHub");
        private static readonly GUIContent kHomepageMenuItemContent = new GUIContent("Home page");
        private static readonly GUIContent kDotsquidDotComMenuItemContent = new GUIContent("dotsquid.com");
        private static readonly GUIContent kResetButtonContent = new GUIContent(string.Empty, "Reset");
        private static readonly GUIContent kPauseButtonNormalContent = new GUIContent(string.Empty, "Pause");
        private static readonly GUIContent kPauseButtonPressedContent = new GUIContent(string.Empty, "Pause");
        private static readonly GUIContent kSettingsButtonContent = new GUIContent(string.Empty, "Settings");
        private static readonly GUIContent kLockedButtonContent = new GUIContent(string.Empty, "Suppress Time.timeScale changes from without");
        private static readonly GUIContent kUnlockedButtonContent = new GUIContent(string.Empty, "Allow Time.timeScale changes from without");
        private static readonly GUIContent kAutoResetOnButtonContent = new GUIContent(string.Empty, "Auto-reset chronoScale to value set in EditorMode");
        private static readonly GUIContent kAutoResetOffButtonContent = new GUIContent(string.Empty, "Don't auto-reset chronoScale to value set in EditorMode");
        private static readonly GUIContent kPlayModeTooltipContent = new GUIContent();
        private static readonly GUIContent kEditModeTooltipContent = new GUIContent("", "Does not affect Time.timeScale while in EditorMode");
        private static readonly GUILayoutOption kChronoButtonHeight = GUILayout.Height(20.0f);
        private static readonly GUILayoutOption kControlToggleWidth = GUILayout.Width(24.0f);
        private static readonly GUILayoutOption kControlToggleHeight = GUILayout.Height(16.0f);
        private static readonly GUILayoutOption kControlToggleExpandWidth = GUILayout.ExpandWidth(false);
        private static readonly GUILayoutOption kChronoSliderValueWidth = GUILayout.Width(40.0f);
        private static readonly GUILayoutOption kChronoSliderMaxWidth = GUILayout.MaxWidth(8192.0f);
        private static readonly GUILayoutOption kChronoSliderExpandWidth = GUILayout.ExpandWidth(true);
        private static readonly GUILayoutOption[] kControlToggleOptions = new GUILayoutOption[] { kControlToggleWidth, kControlToggleHeight, kControlToggleExpandWidth };
        #endregion

        private Data _data = new Data();
        private TextureStorage _textureStorage = new TextureStorage();
        private SettingsWindow _settingsWindow;
        private bool _isPlayMode = false;
        private bool _areChronoButtonDirty = true;
        private float _originalTimeScale = 1.0f;
        private float _originalChronoScale = 1.0f;
        private float _chronoButtonWidth = kChronoButtonDefaultWidth;
        private float _chronoButtonsTotalWidth = 0.0f;
        private Layout _currentLayout;
        private Rect _windowRect;
        private ChronoBack _chronoBack;
        private ChronoButton[] _chronoButtons;
        private ChronoButton _currentChronoButton = null;
        private GUILayoutOption _controlButtonWidth = GUILayout.Width(kChronoButtonDefaultWidth);

        private float maxChronoValue
        {
            get
            {
                float result = 1.0f;
                if (null != _chronoButtons)
                {
                    var count = _chronoButtons.Length;
                    if (count > 0)
                    {
                        result = _chronoButtons[count - 1].value;
                    }
                }
                return result;
            }
        }

        [MenuItem(kMenuPath, false, 25000)]
        private static void ShowWindow()
        {
            GetWindow<ChronoHelper>();
        }

        private void Awake()
        {
            _data.Load();
            UpdatePlayModeState();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("ChronoHelper");
            _textureStorage.Load();
            UpdateButtonsContent();
            InitChronoBack();
            ScheduleChronoButtonsRecreation();
            Subscribe();
        }

        private void OnDisable()
        {
            _chronoBack.Dispose();
            _textureStorage.Clear();
            CloseSettingsWindow();
            Unsubscribe();
        }

        private void OnDestroy()
        {
            _data.Save();
            ResetTimeScale();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            RecreateChronoButtonsIfRequired();
            CheckChronoScaleIntegrity();
            UpdateChronoScale();
            UpdateWindowRect();
            DrawBackground();
            DrawContextMenu();
            DrawLayout();
            UpdateTimeScale();
        }

        private void InitChronoBack()
        {
            _chronoBack = new ChronoBack(this);
        }

        private void UpdateButtonsContent()
        {
            if (EditorGUIUtility.isProSkin)
            {
                kSettingsButtonContent.image = _textureStorage.settingsIconLightTexture;
                kResetButtonContent.image = _textureStorage.resetIconLightTexture;
                kPauseButtonNormalContent.image = _textureStorage.pauseIconLightTexture;
                kPauseButtonPressedContent.image = _textureStorage.pauseIconLightTexture;
                kLockedButtonContent.image = _textureStorage.lockedIconLightTexture;
                kUnlockedButtonContent.image = _textureStorage.unlockedIconLightTexture;
                kAutoResetOnButtonContent.image = _textureStorage.autoResetOnIconLightTexture;
                kAutoResetOffButtonContent.image = _textureStorage.autoResetOffIconLightTexture;
            }
            else
            {
                kSettingsButtonContent.image = _textureStorage.settingsIconDarkTexture;
                kResetButtonContent.image = _textureStorage.resetIconDarkTexture;
                kPauseButtonNormalContent.image = _textureStorage.pauseIconDarkTexture;
                kPauseButtonPressedContent.image = _textureStorage.pauseIconLightTexture;
                kLockedButtonContent.image = _textureStorage.lockedIconLightTexture;
                kUnlockedButtonContent.image = _textureStorage.unlockedIconDarkTexture;
                kAutoResetOnButtonContent.image = _textureStorage.autoResetOnIconLightTexture;
                kAutoResetOffButtonContent.image = _textureStorage.autoResetOffIconDarkTexture;
            }
        }

        private void ResetChronoButtonWidth()
        {
            _chronoButtonWidth = kChronoButtonDefaultWidth;
        }

        private void UpdateChronoButtonWidth(GUIContent buttonContent)
        {
            GUIStyle buttonStyle = kButtonStyle;
            buttonStyle.CalcMinMaxWidth(buttonContent, out var minWidth, out var maxWidth);
            _chronoButtonWidth = Mathf.Max(_chronoButtonWidth, maxWidth);
        }

        private void ApplyChronoButtonWidth()
        {
            var buttonsWidth = Settings.I.buttonWidth;
            switch (buttonsWidth)
            {
                case ButtonWidth.Equal:
                    _controlButtonWidth = GUILayout.Width(_chronoButtonWidth);
                    break;

                case ButtonWidth.AsIs:
                    _controlButtonWidth = GUILayout.MaxWidth(8192.0f);
                    break;
            }
        }

        private void ScheduleChronoButtonsRecreation()
        {
            _areChronoButtonDirty = true;
        }

        private void RecreateChronoButtonsIfRequired()
        {
            if (!_areChronoButtonDirty)
                return;

            ResetChronoButtonWidth();

            var settings = Settings.I;
            var points = settings.chronoPointList.array;
            var mode = settings.buttonFormat;
            int count = points.Length;

            _chronoButtons = new ChronoButton[count];
            for (int i = 0; i < count; ++i)
            {
                var point = points[i];
                var value = point.value;
                GUIContent normalContent = null;
                GUIContent pressedContent = null;
                if (Mathf.Approximately(value, 0.0f))
                {
                    normalContent = kPauseButtonNormalContent;
                    pressedContent = kPauseButtonPressedContent;
                }
                else
                {
                    var display = ChronoValueFormatter.Nicify(value, mode);
                    normalContent = new GUIContent(display);
                }
                _chronoButtons[i] = new ChronoButton(this, value, normalContent, pressedContent);
                UpdateChronoButtonWidth(normalContent);
            }
            UpdateChronoButtonWidth(kResetButtonContent);
            ApplyChronoButtonWidth();
            _areChronoButtonDirty = false;
        }

        private void DrawBackground()
        {
            _chronoBack.Draw(_windowRect);
        }

        private void DrawLayout()
        {
            float viewWidth = EditorGUIUtility.currentViewWidth;
            switch (Settings.I.layout)
            {
                case Layout.Auto:
                    if (viewWidth > _chronoButtonsTotalWidth * kHorizontalLayoutThresholdFactor + kHorizontalLayoutThresholdExtra)
                    {
                        DrawHorizontalLayout();
                    }
                    else
                    {
                        DrawVerticalLayout();
                    }
                    break;

                case Layout.Vertical:
                    DrawVerticalLayout();
                    break;

                case Layout.Horizontal:
                    DrawHorizontalLayout();
                    break;
            }
        }

        private void DrawHorizontalLayout()
        {
            _currentLayout = Layout.Horizontal;
            minSize = new Vector2(kWindowMinWidth, kHorizontalLayoutHeight);
            maxSize = new Vector2(kWindowMaxWidth, kHorizontalLayoutHeight);

            using (new EditorGUILayout.HorizontalScope())
            {
                switch (Settings.I.blockOrder)
                {
                    case BlockOrder.Reversed:
                        DrawChronoSliderInHorizontalLayout();
                        DrawControlButtons();
                        break;

                    case BlockOrder.Normal:
                    default:
                        DrawControlButtons();
                        DrawChronoSliderInHorizontalLayout();
                        break;
                }
            }
        }

        private void DrawVerticalLayout()
        {
            _currentLayout = Layout.Vertical;

            bool isDocked = true;
#if UNITY_2020_1_OR_NEWER
            isDocked = docked;
#endif
            if (isDocked)
            {
                minSize = new Vector2(kWindowMinWidth, kVerticalLayoutDockedHeight);
                maxSize = new Vector2(kWindowMaxWidth, kVerticalLayoutDockedHeight);
            }
            else
            {
                minSize = new Vector2(kWindowMinWidth, kVerticalLayoutUndockedHeight);
                maxSize = new Vector2(kWindowMaxWidth, kVerticalLayoutUndockedHeight);
            }

            using (new EditorGUILayout.VerticalScope())
            {
                switch (Settings.I.blockOrder)
                {
                    case BlockOrder.Reversed:
                        DrawControlButtons();
                        DrawChronoSlider();
                        break;

                    case BlockOrder.Normal:
                    default:
                        DrawChronoSlider();
                        GUILayout.Space(-1.0f);
                        DrawControlButtons();
                        break;
                }
            }
        }

        private void DrawChronoSliderInHorizontalLayout()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Space(5.0f);
                DrawChronoSlider();
            }
        }

        private void DrawChronoSlider()
        {
            const float kMinLinearValue = 0.0f;
            const float kMaxLinearValue = 2.0f;
            float maxChronoValue = this.maxChronoValue;

            using (new EditorGUILayout.HorizontalScope())
            {
                var oldChronoScale = _data.chronoScale;
                var content = _isPlayMode ? kPlayModeTooltipContent : kEditModeTooltipContent;

                var linear = ChronoScaleToLinear(_data.chronoScale, maxChronoValue);
                linear = GUILayout.HorizontalSlider(linear, kMinLinearValue, kMaxLinearValue, kChronoSliderExpandWidth, kChronoSliderMaxWidth);
                _data.chronoScale = ChronoLinearToScale(linear, maxChronoValue);

                float roundChronoScale = (float)Math.Round(_data.chronoScale, 3);
                _data.chronoScale = EditorGUILayout.FloatField(roundChronoScale, kChronoSliderValueWidth);

                DrawTooltipOverLastRect(content);
                if (oldChronoScale != _data.chronoScale)
                {
                    UpdateChronoButtons();
                }
                DrawControlToggles();
            }
        }

        private void DrawControlButtons()
        {
            var settings = Settings.I;
            using (new EditorGUILayout.HorizontalScope())
            {
                if (settings.blockOrder == BlockOrder.Normal ||
                    _currentLayout == Layout.Vertical)
                {
                    GUILayout.Space(4.0f);
                }
                GUILayout.FlexibleSpace();

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (settings.showResetButton)
                    {
                        using (new EditorGUI.DisabledScope(!Application.isPlaying))
                        {
                            if (GUILayout.Button(kResetButtonContent, kButtonLeftStyle, _controlButtonWidth, kChronoButtonHeight))
                                ResetTimeScale();
                        }
                    }

                    for (int i = 0, count = _chronoButtons.Length; i < count; ++i)
                    {
                        string style = kButtonMidStyle;
                        if (i == 0 && !Settings.I.showResetButton)
                            style = kButtonLeftStyle;
                        else if (i == count - 1)
                            style = kButtonRightStyle;

                        _chronoButtons[i].Draw(style);
                    }
                }
                UpdateChronoButtonsTotalWidth();
                UpdateChronoButtons();

                GUILayout.FlexibleSpace();
            }
        }

        private void DrawControlToggles()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                {
                    var autoResetButtonContent = _data.canResetOnPlayEnd
                                               ? kAutoResetOnButtonContent
                                               : kAutoResetOffButtonContent;
                    _data.canResetOnPlayEnd = GUILayout.Toggle(_data.canResetOnPlayEnd, autoResetButtonContent, EditorStyles.miniButtonLeft, kControlToggleOptions);
                }

                {
                    var suppressButtonContent = _data.canSuppressTimeScale
                                              ? kLockedButtonContent
                                              : kUnlockedButtonContent;
                    _data.canSuppressTimeScale = GUILayout.Toggle(_data.canSuppressTimeScale, suppressButtonContent, EditorStyles.miniButtonMid, kControlToggleOptions);
                }

                if (GUILayout.Button(kSettingsButtonContent, EditorStyles.miniButtonRight, kControlToggleOptions))
                {
                    OpenSettingsWindow();
                }
            }
        }

        private void DrawContextMenu()
        {
            var evt = Event.current;
            if (evt.type == EventType.ContextClick)
            {
                var mousePos = evt.mousePosition;
                if (_windowRect.Contains(mousePos))
                {
                    var menu = new GenericMenu();
                    PopulateMenu(menu);
                    menu.ShowAsContext();
                    evt.Use();
                }
            }
        }

        private void OpenSettingsWindow()
        {
            _settingsWindow = SettingsWindow.Open();
        }

        private void CloseSettingsWindow()
        {
            if (_settingsWindow != null)
                _settingsWindow.Close();
        }

        private void UpdateChronoButtonsTotalWidth()
        {
            float lastWidth = GUILayoutUtility.GetLastRect().width;
            if (lastWidth > 1.0f)
                _chronoButtonsTotalWidth = lastWidth;
        }

        private void UpdateWindowRect()
        {
            _windowRect = new Rect(Vector2.zero, position.size);
        }

        private void CheckChronoScaleIntegrity()
        {
            if (_isPlayMode && EditorApplication.isPlaying)
            {
                var settings = Settings.I;
                var warningMode = settings.warningMode;
                bool isIntegrityViolated = (_data.chronoScale != Time.timeScale);
                bool showWarning = false;
                switch (warningMode)
                {
                    case WarningMode.WhenNotSuppressing:
                        if (!_data.canSuppressTimeScale)
                            showWarning = true;
                        break;

                    case WarningMode.Always:
                        showWarning = true;
                        break;
                }
                if (showWarning && isIntegrityViolated)
                {
                    _chronoBack.mode = ChronoBack.Mode.Warning;
                }
            }
        }

        private void UpdateChronoScale()
        {
            if (_isPlayMode &&
                !_data.canSuppressTimeScale &&
                EditorApplication.isPlaying)
            {
                _data.chronoScale = Time.timeScale;
            }
        }

        private void UpdateTimeScale()
        {
            if (_isPlayMode)
                Time.timeScale = _data.chronoScale;
        }

        private void UpdateChronoButtons()
        {
            for (int i = 0; i < _chronoButtons.Length; ++i)
            {
                _chronoButtons[i].Update();
            }
        }

        private void StoreTimeScale()
        {
            _originalTimeScale = Time.timeScale;
            _originalChronoScale = _data.chronoScale;
        }

        private void ResetTimeScale()
        {
            Time.timeScale = _originalTimeScale;
            if (_data.canResetOnPlayEnd)
            {
                _data.chronoScale = _originalChronoScale;
            }
        }

        private void UpdatePlayModeState()
        {
            var oldPlayMode = _isPlayMode;
            _isPlayMode = EditorApplication.isPlaying;
            if (_isPlayMode && !oldPlayMode) // EditMode -> PlayMode
            {
                StoreTimeScale();
                UpdateTimeScale();
            }
            else if (!_isPlayMode && oldPlayMode) // PlayMode -> EditMode
            {
                ResetTimeScale();
            }
        }

        private void PopulateMenu(GenericMenu menu)
        {
            menu.AddItem(kSettingsMenuItemContent, false, OpenSettingsWindow);
            menu.AddItem(kGitHubMenuItemContent, false, () => Application.OpenURL(Consts.URL.Github));
            menu.AddItem(kHomepageMenuItemContent, false, () => Application.OpenURL(Consts.URL.Homepage));
            menu.AddItem(kDotsquidDotComMenuItemContent, false, () => Application.OpenURL(Consts.URL.DotsquidDotCom));
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            PopulateMenu(menu);
        }

        private void OnApplicationStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    _isPlayMode = true;
                    StoreTimeScale();
                    UpdateTimeScale();
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    _isPlayMode = false;
                    ResetTimeScale();
                    break;
            }
        }

        private void OnChronoButtonsDirty()
        {
            ScheduleChronoButtonsRecreation();
        }

        private void OnChronoWarningTest()
        {
            _chronoBack.mode = ChronoBack.Mode.Warning;
        }

        private void Subscribe()
        {
            SettingsWindow.onChronoButtonsDirty += OnChronoButtonsDirty;
            SettingsWindow.onChronoWarningTest += OnChronoWarningTest;
            EditorApplication.playModeStateChanged += OnApplicationStateChanged;
        }

        private void Unsubscribe()
        {
            SettingsWindow.onChronoButtonsDirty -= OnChronoButtonsDirty;
            SettingsWindow.onChronoWarningTest -= OnChronoWarningTest;
            EditorApplication.playModeStateChanged -= OnApplicationStateChanged;
        }

        private static void DrawTooltipOverLastRect(GUIContent content)
        {
            var lastRect = GUILayoutUtility.GetLastRect();
            GUI.Label(lastRect, content);
        }

        private static float ChronoScaleToLinear(float scale, float max)
        {
            if (scale <= 1.0f)
                return scale;
            else
                return Mathf.InverseLerp(1.0f, max, scale) + 1.0f;
        }

        private static float ChronoLinearToScale(float linear, float max)
        {
            if (linear <= 1.0f)
                return linear;
            else
                return Mathf.Lerp(1.0f, max, linear - 1.0f);
        }
    }
}
