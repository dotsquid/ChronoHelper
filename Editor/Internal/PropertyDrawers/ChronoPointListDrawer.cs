using UnityEditor;
using UnityEngine;

namespace dotsquid.ChronoHelper.Internal
{
    [CustomPropertyDrawer(typeof(ChronoPointList), true)]
    internal class ChronoPointListDrawer : BaseListDrawer
    {
        private const string kHeader = "Shortcut buttons";
        private const float kHeightEnlargement = 2.0f;

        protected override string header => kHeader;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            _list.displayRemove = false;
            _list.draggable = false;
        }

        protected override void OnChanged(SerializedProperty element, int index)
        {
            Validate(element);
            Sort();
        }

        protected override bool HasRemoveButton(SerializedProperty element, int index)
        {
            return !IsReadOnly(element);
        }

        protected override float GetElementHeight(int index)
        {
            return base.GetElementHeight(index) + kHeightEnlargement;
        }

        private bool IsReadOnly(SerializedProperty element)
        {
            if (element != null)
            {
                var prop = element.FindPropertyRelative(ChronoPoint.kIsReadOnlyPropName);
                if (prop != null)
                {
                    return prop.boolValue;
                }
            }
            return true;
        }

        private void Validate(SerializedProperty element)
        {
            if (element != null)
            {
                var prop = element.FindPropertyRelative(ChronoPoint.kValuePropName);
                if (prop != null)
                {
                    prop.floatValue = ChronoPoint.ValidateValue(prop.floatValue);
                }
            }
        }

        private void Sort()
        {
            var settings = (_property.serializedObject.targetObject as Settings);
            var list = settings.chronoPointList;
            if (list != null)
            {
                list.SortByValue();
                _list.serializedProperty.serializedObject.Update();
            }
        }
    }
}
