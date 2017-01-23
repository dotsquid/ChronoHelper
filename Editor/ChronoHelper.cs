/*
    Copyright (c) 2016 Kyrylo Iakovliev
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

using UnityEngine;
using UnityEditor;
using System;

public class ChronoHelperEditor : EditorWindow
{
    private class ChronoButton
    {
        public GUIContent content;
        public float value;
        public bool state
        {
            get
            {
                return currentState;
            }

            set
            {
                currentState = value;
                if (!currentState && owner.currentChronoButton == this)
                    owner.currentChronoButton = null;
                if (!oldState && currentState) // button was pressed in
                {
                    if (null != owner.currentChronoButton)
                        owner.currentChronoButton.state = false;
                    owner.currentChronoButton = this;
                    owner.chronoScale = Mathf.Clamp(this.value, chronoMinScale, chronoMaxScale);
                }
                oldState = currentState;
            }
        }

        private ChronoHelperEditor owner;
        private bool currentState = false;
        private bool oldState = false;

        public ChronoButton(ChronoHelperEditor owner, string title, float value)
        {
            this.owner = owner;
            this.value = value;
            content = new GUIContent(title);
        }

        public ChronoButton(ChronoHelperEditor owner, GUIContent content, float value)
        {
            this.owner = owner;
            this.value = value;
            this.content = content;
        }
    }

    #region Constants
    private const float chronoMinScale = 0.0f;
    private const float chronoMaxScale = 2.0f;
    private const float horizontalLayoutWidth = 600.0f;
    private const float horizontalLayoutHeight = 24.0f;
    private const float verticalLayoutHeight = 42.0f;
    private const float windowMinWidth = 330.0f;
    private const float windowMaxWidth = 8192.0f;

    private const string githubUrl = "https://github.com/dotsquid/ChronoHelper";
    private const string assetStoreUrl = "https://www.assetstore.unity3d.com";
    private const string buttonStyle = "Button";
    private const string resetButtonTitle = "↻";
    private const string pauseButtonTitle = "❚❚";
    private const string oneEighthButtonTitle = "×⅛";
    private const string oneFourthButtonTitle = "×¼";
    private const string halfButtonTitle = "×½";
    private const string oneButtonTitle = "×1";
    private const string oneAndHalfButtonTitle = "×1½";
    private const string twiceButtonTitle = "×2";
    private static readonly GUIContent githubMenuItemContent = new GUIContent("Github page");
    private static readonly GUIContent assetStoreMenuItemContent = new GUIContent("AssetStore page");
    private static readonly GUIContent resetButtonContent = new GUIContent(resetButtonTitle, "Reset");
    private static readonly GUIContent pauseButtonContent = new GUIContent(pauseButtonTitle, "Pause");
    private static readonly GUIContent windowTooltipContent = new GUIContent("", "Disabled while in EditorMode");
    private static readonly GUILayoutOption controlButtonWidth = GUILayout.Width(42.0f);
    private static readonly GUILayoutOption chronoSliderMinWidth = GUILayout.MinWidth(256.0f);
    private static readonly GUILayoutOption chronoSliderMaxWidth = GUILayout.MaxWidth(8192.0f);
    private static readonly GUILayoutOption chronoSliderExpandWidth = GUILayout.ExpandWidth(true);
    #endregion

    private bool isPlayMode = false;
    private float chronoScale = 1.0f;
    private float originalTimeScale = 1.0f;
    private Rect windowRect;
    private ChronoButton[] chronoButtons;
    private ChronoButton currentChronoButton = null;

    [MenuItem("Tools/Chrono Helper", false, 105)]
    static void ShowWindow()
    {
        GetWindow<ChronoHelperEditor>();
    }
    
    public ChronoHelperEditor()
    {
        EditorApplication.playmodeStateChanged += OnApplicationStateChanged;
        titleContent = new GUIContent("Chrono Helper");

        chronoButtons = new ChronoButton[]{
            new ChronoButton(this, pauseButtonContent, 0.0f),
            new ChronoButton(this, oneEighthButtonTitle, 0.1f),
            new ChronoButton(this, oneFourthButtonTitle, 0.25f),
            new ChronoButton(this, halfButtonTitle, 0.5f),
            new ChronoButton(this, oneButtonTitle, 1.0f),
            new ChronoButton(this, oneAndHalfButtonTitle, 1.5f),
            new ChronoButton(this, twiceButtonTitle, 2.0f)
        };
    }

    private void OnEnable()
    {
    }

    private void OnDestroy()
    {
        EditorApplication.playmodeStateChanged -= OnApplicationStateChanged;
        ResetTimeScale();
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        float viewWidth = EditorGUIUtility.currentViewWidth;

        chronoScale = Time.timeScale;
        UpdateWindowRect();
        DrawContextMenu();
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (viewWidth > horizontalLayoutWidth)
            {
                DrawHorizontalLayout();
            }
            else
            {
                DrawVerticalLayout();
            }
        }
        DrawWindowTooltip();
        UpdateTimeScale();
    }

    private void DrawHorizontalLayout()
    {
        minSize = new Vector2(windowMinWidth, horizontalLayoutHeight);
        maxSize = new Vector2(windowMaxWidth, horizontalLayoutHeight);

        EditorGUILayout.BeginHorizontal();
        DrawControlButtons();
        DrawChronoSlider();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawVerticalLayout()
    {
        minSize = new Vector2(windowMinWidth, verticalLayoutHeight);
        maxSize = new Vector2(windowMaxWidth, verticalLayoutHeight);

        EditorGUILayout.BeginVertical();
        DrawChronoSlider();
        DrawControlButtons();
        EditorGUILayout.EndVertical();
    }

    private void DrawChronoSlider()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(6.0f);
        GUILayout.FlexibleSpace();

        var oldChronoScale = chronoScale;
        chronoScale = EditorGUILayout.Slider(chronoScale, 0.0f, 2.0f, chronoSliderExpandWidth, chronoSliderMaxWidth);
        if (oldChronoScale != chronoScale)
        {
            UpdateChronoButtons();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawControlButtons()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(4.0f);
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(resetButtonContent, controlButtonWidth))
            ResetTimeScale();

        // chrono buttons
        for (int i = 0; i < chronoButtons.Length; ++i)
        {
            var button = chronoButtons[i];
            button.state = GUILayout.Toggle(button.state, button.content, buttonStyle, controlButtonWidth);
        }
        UpdateChronoButtons();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawWindowTooltip()
    {
        if (!EditorApplication.isPlaying)
        {
            GUI.Label(windowRect, windowTooltipContent);
        }
    }

    private void DrawContextMenu()
    {
        Event evt = Event.current;
        if (evt.type == EventType.ContextClick)
        {
            Vector2 mousePos = evt.mousePosition;
            if (windowRect.Contains(mousePos))
            {
                var menu = new GenericMenu();
                menu.AddItem(githubMenuItemContent, false, ()=>OpenURL(githubUrl));
                //menu.AddItem(assetStoreMenuItemContent, false, () => OpenURL(assetStoreUrl));
                menu.ShowAsContext();

                evt.Use();
            }
        }
    }

    private void UpdateWindowRect()
    {
        windowRect = new Rect(Vector2.zero, position.size);
    }

    private void UpdateTimeScale()
    {
        if (Application.isPlaying)
            Time.timeScale = chronoScale;
    }

    private void UpdateChronoButtons()
    {
        for (int i = 0; i < chronoButtons.Length; ++i)
        {
            var button = chronoButtons[i];
            button.state = (Mathf.Abs(chronoScale - button.value) <= Mathf.Min(button.value * 0.05f, 0.05f));
            if (button.state)
                chronoScale = button.value;
        }
    }

    private void StoreTimeScale()
    {
        originalTimeScale = Time.timeScale;
    }

    private void ResetTimeScale()
    {
        Time.timeScale = chronoScale = originalTimeScale;
    }

    private void OnApplicationStateChanged()
    {
        var oldPlayMode = isPlayMode;
        isPlayMode = EditorApplication.isPlaying;
        if (isPlayMode && !oldPlayMode) // EditorMode -> PlayMode
        {
            StoreTimeScale();
        }
        else if (!isPlayMode && oldPlayMode) // PlayMode -> EditorMode
        {
            ResetTimeScale();
        }
    }

    private static void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

#if NOPE
    private static Texture2D CreateTextureFromBase64(int width, int height, string base64, string name = "")
    {
        byte[] data = Convert.FromBase64String(base64);
        var tex = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
        tex.hideFlags = HideFlags.HideAndDontSave;
        tex.name = name;
        tex.filterMode = FilterMode.Bilinear;
        tex.LoadImage(data);
        return tex;
    }
#endif
}
