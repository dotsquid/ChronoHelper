using UnityEngine;
using UnityEditor;

namespace dotsquid.ChronoHelper.Internal
{
    internal class Settings : ScriptableObject
    {
        private static readonly Color kDefaultNormalBackColor = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        private static readonly Color kDefaultWarningBackColor = new Color32(212, 77, 246, 225);
        private const string kEditorPrefName = Consts.kNamePrefix + "settings";
        private const float kDefaultWarningBlinkPeriod = 2.0f;
        private const float kDefaultWarningBlinkDuration = 4.0f;

        public static Settings I
        {
            get
            {
                if (_instance == null)
                {
                    Load();
                }
                return _instance;
            }
        }

        #region Stored data
        public ChronoPointList chronoPointList = new ChronoPointList()
        {
            new ChronoPoint(0.0f, true),
            new ChronoPoint(0.125f),
            new ChronoPoint(0.25f),
            new ChronoPoint(0.5f),
            new ChronoPoint(1.0f, true),
            new ChronoPoint(1.5f),
            new ChronoPoint(2.0f)
        };
        public Layout layout = Layout.Auto;
        public BlockOrder blockOrder = BlockOrder.Normal;
        public ButtonWidth buttonWidth = ButtonWidth.Equal;
        public Format buttonFormat = Format.Compact;
        public WarningMode warningMode = WarningMode.WhenNotSuppressing;
        public bool showResetButton = true;
        public Color normalBackColor = kDefaultNormalBackColor;
        public Color warningBackColor = kDefaultWarningBackColor;
        public float warningBlinkPeriod = kDefaultWarningBlinkPeriod;
        public float warningBlinkDuration = kDefaultWarningBlinkDuration;
        #endregion

        private static Settings _instance;
        private SerializedObject _serializedObject;
        private SerializedProperty _chronoPointsListProp;

        public SerializedObject serializedObject => _serializedObject;
        public SerializedProperty chronoPointsListProp => _chronoPointsListProp;

        public void ResetToDefault()
        {
            _instance = CreateInstance<Settings>();
            _instance.Init();
        }

        public void Save()
        {
            var json = JsonUtility.ToJson(_instance, false);
            EditorPrefs.SetString(kEditorPrefName, json);
        }

        private static void Load()
        {
            var json = EditorPrefs.GetString(kEditorPrefName);
            _instance = CreateInstance<Settings>();
            try
            {
                JsonUtility.FromJsonOverwrite(json, _instance);
            }
            catch
            {
                _instance = CreateInstance<Settings>();
            }
            _instance.Init();
        }

        private void Init()
        {
            _serializedObject = new SerializedObject(this);
            _chronoPointsListProp = _serializedObject.FindProperty(nameof(chronoPointList));
        }
    }
}
