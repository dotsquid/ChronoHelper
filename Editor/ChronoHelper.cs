/*
    Copyright (c) 2016 Kyrylo Yakovliev
    https://twitter.com/dotsquid
    
    This software is provided 'as-is', without any express or implied
    warranty. In no event will the authors be held liable for any damages
    arising from the use of this software.
    
    Permission is granted to anyone to use this software for any purpose,
    including commercial applications, and to alter it and redistribute it
    freely, subject to the following restrictions:
    
    1. The origin of this software must not be misrepresented; you must not
       claim that you wrote the original software. If you use this software
       in a product, an acknowledgement in the product documentation would be
       appreciated but is not required.
    2. Altered source versions must be plainly marked as such, and must not be
       misrepresented as being the original software.
    3. This notice may not be removed or altered from any source distribution.
 */

using System;
using UnityEditor;
using UnityEngine;

public class ChronoHelper : EditorWindow
{
    private class ChronoButton
    {
        private ChronoHelper _owner;
        private GUIContent _editModeContent;
        private GUIContent _playModeContent;
        private float _value;
        private bool _currentState = false;
        private bool _oldState = false;

        public bool state
        {
            get { return _currentState; }

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
                    _owner._chronoScale = Mathf.Clamp(this._value, kChronoMinScale, kChronoMaxScale);
                }
                _oldState = _currentState;
            }
        }

        public GUIContent content
        {
            get
            {
                return _owner._isPlayMode ? _playModeContent : _editModeContent;
            }
        }

        public ChronoButton(ChronoHelper owner, float value, string title, string tooltip = "")
        {
            _owner = owner;
            _value = value;
            _playModeContent = new GUIContent(title);
            _editModeContent = new GUIContent(title, kEditModeTooltip);
        }

        public ChronoButton(ChronoHelper owner, float value, GUIContent content)
        {
            _owner = owner;
            _value = value;
            _playModeContent = new GUIContent(content);
            _editModeContent = new GUIContent(content);
            _editModeContent.tooltip = kEditModeTooltip;
        }

        public void Draw()
        {
            state = GUILayout.Toggle(state, content, kButtonStyle, kControlButtonWidth, kControlButtonHeight);
        }

        public void Update()
        {
            state = (Mathf.Abs(_owner._chronoScale - _value) <= Mathf.Min(_value * 0.05f, 0.05f));
            if (state)
                _owner._chronoScale = _value;
        }
    }

    #region Constants
    private const float kChronoMinScale = 0.0f;
    private const float kChronoMaxScale = 2.0f;
    private const float kHorizontalLayoutWidth = 600.0f;
    private const float kHorizontalLayoutHeight = 24.0f;
    private const float kVerticalLayoutHeight = 42.0f;
    private const float kWindowMinWidth = 342.0f;
    private const float kWindowMaxWidth = 8192.0f;

    private const string kChronoScalePrefKey = "ChronoHelper.chronoScale";
    private const string kCanResetChronoScalePrefKey = "ChronoHelper.canResetChronoScale";
    private const string kCanSuppressTimeScalePrefKey = "ChronoHelper.canSuppressTimeScale";
    private const float kChronoScalePrefDefault = 1.0f;
    private const bool kCanResetChronoScalePrefDefault = true;
    private const bool kCanSuppressTimeScalePrefDefault = false;

    private const string kGithubUrl = "https://github.com/dotsquid/ChronoHelper";
    private const string kAssetStoreUrl = "https://www.assetstore.unity3d.com";
    private const string kButtonStyle = "Button";
    private const string kOneEighthButtonTitle = "×⅛";
    private const string kOneFourthButtonTitle = "×¼";
    private const string kHalfButtonTitle = "×½";
    private const string kOneButtonTitle = "×1";
    private const string kOneAndHalfButtonTitle = "×1½";
    private const string kTwiceButtonTitle = "×2";
    private const string kEditModeTooltip = "Does not affect Time.timeScale while in EditorMode";
    private const string kResetIconBase64 = "iVBORw0KGgoAAAANSUhEUgAAAA0AAAAMCAYAAAC5tzfZAAAAXklEQVQoz52RwQ3AMAgDz1GGCfsPk3HcVyTUqmmoXzw4jGzZZikiDGjOyU7tBhypVQEAjTFKAKBGXe6AgOymHZCDUOk/20REvvQa+QqsAaSlI8e+hq9CH1C1pz+R6wKUNx2CpAeEkwAAAABJRU5ErkJggg==";
    private const string kPauseIconBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAKUlEQVQY02PU0ND4zwABjNevX2dgYGBg0NTU/M/AwMDAxIAHjEoSkgQANSEFj9cbB0UAAAAASUVORK5CYII=";

    private static readonly GUIContent kGithubMenuItemContent = new GUIContent("Github page");
    //private static readonly GUIContent kAssetStoreMenuItemContent = new GUIContent("AssetStore page");
    private static readonly GUIContent kResetButtonContent = new GUIContent(string.Empty, "Reset");
    private static readonly GUIContent kPauseButtonContent = new GUIContent(string.Empty, "Pause");
    private static readonly GUIContent kCanResetButtonContent = new GUIContent("R");
    private static readonly GUIContent kCanSuppressButtonContent = new GUIContent("S");
    private static readonly GUIContent kPlayModeTooltipContent = new GUIContent();
    private static readonly GUIContent kEditModeTooltipContent = new GUIContent("", "Does not affect Time.timeScale while in EditorMode");
    private static readonly GUIContent kCanResetToggleTooltipContent = new GUIContent("", "Auto-reset chronoScale to value set in EditorMode?");
    private static readonly GUIContent kCanSuppressTimeScaleToggleTooltipContent = new GUIContent("", "Suppress Time.timeScale changes from without?");
    private static readonly GUILayoutOption kControlButtonWidth = GUILayout.Width(38.0f);
    private static readonly GUILayoutOption kControlButtonHeight = GUILayout.Height(20.0f);
    private static readonly GUILayoutOption kControlToggleWidth = GUILayout.Width(20.0f);
    private static readonly GUILayoutOption kControlToggleExpandWidth = GUILayout.ExpandWidth(false);
    //private static readonly GUILayoutOption kChronoSliderMinWidth = GUILayout.MinWidth(256.0f);
    private static readonly GUILayoutOption kChronoSliderMaxWidth = GUILayout.MaxWidth(8192.0f);
    private static readonly GUILayoutOption kChronoSliderExpandWidth = GUILayout.ExpandWidth(true);
    #endregion

    private static Texture2D _resetIconTexture;
    private static Texture2D _pauseIconTexture;

    private bool _isPlayMode = false;
    private bool _canResetOnPlayEnd = true;
    private bool _canSuppressTimeScale = false;
    private float _chronoScale = 1.0f;
    private float _originalTimeScale = 1.0f;
    private float _originalChronoScale = 1.0f;
    private Rect _windowRect;
    private ChronoButton[] _chronoButtons;
    private ChronoButton _currentChronoButton = null;

    [MenuItem("Window/Chrono Helper")]
    private static void ShowWindow()
    {
        GetWindow<ChronoHelper>();
    }

    private void Awake()
    {
        LoadPrefs();
        UpdatePlayModeState();
    }

    private void OnEnable()
    {
        EditorApplication.playmodeStateChanged += OnApplicationStateChanged;
        titleContent = new GUIContent("Chrono Helper");
        InitTextureContent();
        CreateChronoButtons();
    }

    private void OnDisable()
    {
        EditorApplication.playmodeStateChanged -= OnApplicationStateChanged;
        DestroyImmediate(_resetIconTexture);
        DestroyImmediate(_pauseIconTexture);
    }

    private void OnDestroy()
    {
        SavePrefs();
        ResetTimeScale();
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        UpdateChronoScale();
        UpdateWindowRect();
        DrawContextMenu();
        DrawLayout();
        UpdateTimeScale();
    }

    private void InitTextureContent()
    {
        _resetIconTexture = CreateTextureFromBase64(kResetIconBase64, "CH_Icon_Reset");
        _pauseIconTexture = CreateTextureFromBase64(kPauseIconBase64, "CH_Icon_Pause");
        kResetButtonContent.image = _resetIconTexture;
        kPauseButtonContent.image = _pauseIconTexture;
    }

    private void CreateChronoButtons()
    {
        _chronoButtons = new ChronoButton[]{
            new ChronoButton(this, 0.000f, kPauseButtonContent),
            new ChronoButton(this, 0.125f, kOneEighthButtonTitle),
            new ChronoButton(this, 0.250f, kOneFourthButtonTitle),
            new ChronoButton(this, 0.500f, kHalfButtonTitle),
            new ChronoButton(this, 1.000f, kOneButtonTitle),
            new ChronoButton(this, 1.500f, kOneAndHalfButtonTitle),
            new ChronoButton(this, 2.000f, kTwiceButtonTitle)
        };
    }

    private void DrawLayout()
    {
        float viewWidth = EditorGUIUtility.currentViewWidth;
        if (viewWidth > kHorizontalLayoutWidth)
        {
            DrawHorizontalLayout();
        }
        else
        {
            DrawVerticalLayout();
        }
    }

    private void DrawHorizontalLayout()
    {
        minSize = new Vector2(kWindowMinWidth, kHorizontalLayoutHeight);
        maxSize = new Vector2(kWindowMaxWidth, kHorizontalLayoutHeight);

        using (new EditorGUILayout.HorizontalScope())
        {
            DrawControlButtons();
            DrawChronoSlider();
        }
    }

    private void DrawVerticalLayout()
    {
        minSize = new Vector2(kWindowMinWidth, kVerticalLayoutHeight);
        maxSize = new Vector2(kWindowMaxWidth, kVerticalLayoutHeight);

        using (new EditorGUILayout.VerticalScope())
        {
            DrawChronoSlider();
            DrawControlButtons();
        }
    }

    private void DrawChronoSlider()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            var oldChronoScale = _chronoScale;
            var content = _isPlayMode ? kPlayModeTooltipContent : kEditModeTooltipContent;
            _chronoScale = EditorGUILayout.Slider(_chronoScale, 0.0f, 2.0f, kChronoSliderExpandWidth, kChronoSliderMaxWidth);
            DrawTooltipOverLastRect(content);
            if (oldChronoScale != _chronoScale)
            {
                UpdateChronoButtons();
            }
            DrawControlToggles();
        }
    }

    private void DrawControlButtons()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(4.0f);
            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button(kResetButtonContent, kControlButtonWidth, kControlButtonHeight))
                    ResetTimeScale();
            }

            for (int i = 0; i < _chronoButtons.Length; ++i)
            {
                _chronoButtons[i].Draw();
            }
            UpdateChronoButtons();

            GUILayout.FlexibleSpace();
        }
    }

    private void DrawControlToggles()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            _canResetOnPlayEnd = GUILayout.Toggle(_canResetOnPlayEnd, kCanResetButtonContent, EditorStyles.miniButtonLeft, kControlToggleWidth, kControlToggleExpandWidth);
            DrawTooltipOverLastRect(kCanResetToggleTooltipContent);
            _canSuppressTimeScale = GUILayout.Toggle(_canSuppressTimeScale, kCanSuppressButtonContent, EditorStyles.miniButtonRight, kControlToggleWidth, kControlToggleExpandWidth);
            DrawTooltipOverLastRect(kCanSuppressTimeScaleToggleTooltipContent);
        }
    }

    private void DrawContextMenu()
    {
        Event evt = Event.current;
        if (evt.type == EventType.ContextClick)
        {
            Vector2 mousePos = evt.mousePosition;
            if (_windowRect.Contains(mousePos))
            {
                var menu = new GenericMenu();
                menu.AddItem(kGithubMenuItemContent, false, () => OpenURL(kGithubUrl));
                //menu.AddItem(kAssetStoreMenuItemContent, false, () => OpenURL(kAssetStoreUrl));
                menu.ShowAsContext();

                evt.Use();
            }
        }
    }

    private void DrawTooltipOverLastRect(GUIContent content)
    {
        var lastRect = GUILayoutUtility.GetLastRect();
        GUI.Label(lastRect, content);
    }

    private void UpdateWindowRect()
    {
        _windowRect = new Rect(Vector2.zero, position.size);
    }

    private void UpdateChronoScale()
    {
        if (_isPlayMode &&
            !_canSuppressTimeScale &&
            EditorApplication.isPlaying)
        {
            _chronoScale = Time.timeScale;
        }
    }

    private void UpdateTimeScale()
    {
        if (_isPlayMode)
            Time.timeScale = _chronoScale;
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
        _originalChronoScale = _chronoScale;
    }

    private void ResetTimeScale()
    {
        Time.timeScale = _originalTimeScale;
        if (_canResetOnPlayEnd)
        {
            _chronoScale = _originalChronoScale;
        }
    }

    private void OnApplicationStateChanged()
    {
        UpdatePlayModeState();
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

    private void LoadPrefs()
    {
        _chronoScale = EditorPrefs.GetFloat(kChronoScalePrefKey, kChronoScalePrefDefault);
        _canResetOnPlayEnd = EditorPrefs.GetBool(kCanResetChronoScalePrefKey, kCanResetChronoScalePrefDefault);
        _canSuppressTimeScale = EditorPrefs.GetBool(kCanSuppressTimeScalePrefKey, kCanSuppressTimeScalePrefDefault);
    }

    private void SavePrefs()
    {
        EditorPrefs.SetFloat(kChronoScalePrefKey, _chronoScale);
        EditorPrefs.SetBool(kCanResetChronoScalePrefKey, _canResetOnPlayEnd);
        EditorPrefs.SetBool(kCanSuppressTimeScalePrefKey, _canSuppressTimeScale);
    }

    private static void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    private static Texture2D CreateTextureFromBase64(string base64, string name = "")
    {
        byte[] data = Convert.FromBase64String(base64);
        var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false, true)
        {
            hideFlags = HideFlags.HideAndDontSave,
            name = name,
            filterMode = FilterMode.Bilinear
        };
        tex.LoadImage(data);
        return tex;
    }
}