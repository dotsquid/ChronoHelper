using UnityEditor;
using UnityEngine;

namespace dotsquid.ChronoHelper.Internal
{
    [CustomPropertyDrawer(typeof(ChronoPoint), true)]
    internal class ChronoPointDrawer : PropertyDrawer
    {
        private class Styles
        {
            public GUIStyle chronoValueField;
        }

        private const float kMarginWidth = 8.0f;
        private const float kGapWidth = 16.0f;
        private const float kValueRectWidth = 64.0f;
        private const float kHeightReduction = 4.0f;

        private Styles _styles;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnsureStyles();
            DrawPoint(position, property);
        }

        private void DrawPoint(Rect position, SerializedProperty property)
        {
            var isReadOnlyProp = property.FindPropertyRelative(ChronoPoint.kIsReadOnlyPropName);
            var valueProp = property.FindPropertyRelative(ChronoPoint.kValuePropName);

            bool isReadOnly = isReadOnlyProp != null
                            ? isReadOnlyProp.boolValue
                            : false;

            position.y += kHeightReduction * 0.5f;
            position.height -= kHeightReduction;
            float valueRectWidth = position.width - kValueRectWidth - kGapWidth - kMarginWidth;
            var valueRect = new Rect(position)
            {
                width = kValueRectWidth,
                x = position.x + kMarginWidth
            };
            var displayRect = new Rect(position)
            {
                width = valueRectWidth,
                x = valueRect.x + kValueRectWidth + kGapWidth
            };

            using (var group = new EditorGUI.DisabledGroupScope(isReadOnly))
            {
                float value = valueProp.floatValue;
                string displayValue = GetDisplayValue(value);
                valueProp.floatValue = EditorGUI.DelayedFloatField(valueRect, GUIContent.none, value, _styles.chronoValueField);
                EditorGUI.LabelField(displayRect, displayValue);
            }
        }

        private void EnsureStyles()
        {
            if (_styles == null)
            {
                _styles = new Styles();
                _styles.chronoValueField = new GUIStyle(GUI.skin.textField);
                _styles.chronoValueField.alignment = TextAnchor.MiddleRight;
            }
        }

        private static string GetDisplayValue(float value)
        {
            if (Mathf.Approximately(value, 0.0f))
                return "▍▍ (paused)";
            else if(Mathf.Approximately(value, 1.0f))
                return "×1 (normal)";
            return ChronoValueFormatter.Nicify(value, Settings.I.buttonFormat);
        }
    }
}
