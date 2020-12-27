using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace dotsquid.ChronoHelper.Internal
{
    [CustomPropertyDrawer(typeof(CustomPropertyList), true)]
    internal class BaseListDrawer : PropertyDrawer
    {
        private static readonly Color kElementSeparatorColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);

        protected readonly Dictionary<string, ReorderableList> _container = new Dictionary<string, ReorderableList>();
        protected readonly HashSet<int> _toRemove = new HashSet<int>();
        protected ReorderableList _list;
        protected SerializedProperty _property;
        protected bool _isDirty = false;
        protected int _prevIndex = -1;
        protected Texture2D _lineTexture = EditorGUIUtility.whiteTexture;

        protected virtual string header
        {
            get
            {
                string result = string.Empty;
                if (null != _property)
                {
                    result = _property.displayName;
                }
                return result;
            }
        }

        protected virtual bool isFoldable => true;
        protected virtual bool hasPerItemRemoveButton => true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            CacheList(property);
            return _list.GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _property = property;
            CacheList(property);
            CheckDirty();
            _list.DoList(position);
            RemoveEnqueued();
        }

        protected virtual GUIContent GetElementTitle(SerializedProperty element)
        {
            return new GUIContent(element.displayName);
        }

        protected virtual void DrawListElement(Rect rect, int index, bool selected, bool focused)
        {
            const float kRemoveButtonWidth = 16.0f;
            bool hasPerItemRemoveButton = this.hasPerItemRemoveButton;

            DrawListElementSeparator(rect, index);

            if (isFoldable)
            {
                const float kRightShift = 10.0f;
                rect.x += kRightShift;
                rect.width -= kRightShift;
            }

            if (hasPerItemRemoveButton)
            {
                rect.width -= kRemoveButtonWidth;
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var serializedProperty = _list.serializedProperty;
                var element = serializedProperty.GetArrayElementAtIndex(index);
                bool hasCustomTitle = DrawListElementTitle(element, index, ref rect);
                if (hasCustomTitle)
                    EditorGUI.PropertyField(rect, element, GUIContent.none, true);
                else
                    EditorGUI.PropertyField(rect, element, true);
                if (hasPerItemRemoveButton && HasRemoveButton(element, index))
                {
                    var singleHeight = GUI.skin.button.lineHeight;
                    var buttonRect = new Rect()
                    {
                        width = kRemoveButtonWidth,
                        height = singleHeight,
                        x = rect.x + rect.width,
                        y = rect.y + Mathf.Floor((rect.height - singleHeight) * 0.5f)
                    };
                    if (GUI.Button(buttonRect, ReorderableList.defaultBehaviours.iconToolbarMinus, ReorderableList.defaultBehaviours.preButton))
                    {
                        EnqueueToRemove(index);
                    }
                }
                if (check.changed)
                {
                    serializedProperty.serializedObject.ApplyModifiedProperties();
                    _isDirty = true;
                    OnChanged(element, index);
                }
            }
        }

        private void DrawListElementSeparator(Rect rect, int index)
        {
            if (index < _list.count - 1)
            {
                using (GUIHelper.ReplaceColor.With(kElementSeparatorColor))
                {
                    var lineRect = rect;
                    lineRect.y += rect.height - 1.0f;
                    lineRect.height = 1.0f;
                    GUI.DrawTexture(lineRect, _lineTexture);
                }
            }
        }

        private void CacheList(SerializedProperty property)
        {
            if (!_container.TryGetValue(property.propertyPath, out _list))
            {
                var listProperty = property.FindPropertyRelative(CustomPropertyList.kListFieldName);

                if (null == listProperty)
                    Debug.LogErrorFormat("Property '{0}' not found!", CustomPropertyList.kListFieldName);

                _list = new ReorderableList(property.serializedObject, listProperty, true, true, true, true)
                {
                    drawHeaderCallback = DrawListHeader,
                    drawFooterCallback = DrawListFooter,
                    onAddCallback = OnAdd,
                    onRemoveCallback = OnRemove,
                    onChangedCallback = OnChanged,
                    onReorderCallback = OnReordered,
                    drawElementCallback = DrawListElement,
                    elementHeightCallback = GetElementHeight,
                };
                _container.Add(property.propertyPath, _list);
                OnInit();
            }
            _prevIndex = _list.index;
        }

        protected virtual void DrawListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, header, EditorStyles.miniBoldLabel);
        }

        protected virtual void DrawListFooter(Rect rect)
        {
            ReorderableList.defaultBehaviours.DrawFooter(rect, _list);
        }

        protected virtual bool DrawListElementTitle(SerializedProperty element, int index, ref Rect rect)
        {
            return true;
        }

        protected virtual bool HasRemoveButton(SerializedProperty element, int index)
        {
            return true;
        }

        protected virtual void OnInit()
        { }

        protected virtual void OnDirty()
        { }

        protected virtual void OnChanged(ReorderableList list)
        {
            _isDirty = true;
        }

        protected virtual void OnChanged(SerializedProperty element, int index)
        { }

        protected virtual void OnReordered(ReorderableList list)
        { }

        protected virtual float GetElementHeight(int index)
        {
            var element = _list.serializedProperty.GetArrayElementAtIndex(index);
            float elementHeight = EditorGUI.GetPropertyHeight(element);
            return elementHeight;
        }

        protected virtual void OnAdd(ReorderableList list)
        {
            int listIndex = list.index;
            int listCount = list.count;
            int index = (listIndex >= 0 && listCount > 0)
                      ? Mathf.Clamp(listIndex + 1, 0, listCount)
                      : listCount;
            var serializedProperty = _list.serializedProperty;
            var serializedObject = serializedProperty.serializedObject;
            serializedProperty.InsertArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            list.index = index;
        }

        protected virtual void OnRemove(ReorderableList list)
        {
            var serializedProperty = list.serializedProperty;
            var serializedObject = serializedProperty.serializedObject;
            if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference &&
                serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue != null)
            {
                serializedProperty.DeleteArrayElementAtIndex(list.index);
            }
            serializedProperty.DeleteArrayElementAtIndex(list.index);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            list.index = Mathf.Max(0, list.index - 1);
        }

        protected virtual void OnRemove(int index)
        {
            var serializedProperty = _list.serializedProperty;
            var serializedObject = serializedProperty.serializedObject;
            if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference &&
                serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue != null)
            {
                serializedProperty.DeleteArrayElementAtIndex(index);
            }
            serializedProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            if (index >= _list.count)
                _list.index = _list.count - 1;
        }

        private void EnqueueToRemove(int index)
        {
            _toRemove.Add(index);
        }

        private void RemoveEnqueued()
        {
            foreach (var index in _toRemove)
            {
                OnRemove(index);
            }
            _toRemove.Clear();
        }

        private void CheckDirty()
        {
            if (_isDirty)
            {
                OnDirty();
                _isDirty = false;
            }
        }
    }
}