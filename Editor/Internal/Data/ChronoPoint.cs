using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace dotsquid.ChronoHelper.Internal
{
    [Serializable]
    internal struct ChronoPoint : ISerializationCallbackReceiver
    {
        public const string kValuePropName = nameof(_value);
        public const string kCustomDisplayPropName =  nameof(_customDisplay);
        public const string kIsReadOnlyPropName = nameof(_isReadOnly);

        public static ChronoPoint Default = new ChronoPoint()
        {
            _value = 1.0f
        };

        [SerializeField]
        private float _value;
        [SerializeField, HideInInspector]
        private string _customDisplay;
        [SerializeField, HideInInspector]
        private bool _isReadOnly;

        public float value
        {
            get { return _value; }
            set { _value = ValidateValue(value); }
        }

        public bool isReadOnly => _isReadOnly;

        public ChronoPoint(float value, string customDisplay = null)
        {
            _value = value;
            _customDisplay = customDisplay;
            _isReadOnly = (null != _customDisplay);
        }

        public ChronoPoint(float value, bool isReadOnly)
        {
            _value = value;
            _customDisplay = null;
            _isReadOnly = isReadOnly;
        }

        public static float ValidateValue(float value)
        {
            return Math.Max(value, 0.0f);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _value = ValidateValue(_value);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        { }
    }

    [Serializable]
    internal class ChronoPointList : CustomPropertyList<ChronoPoint>, IEnumerable
    {
        public ChronoPointList() : base()
        { }

        public ChronoPointList(IEnumerable<ChronoPoint> collection) : base()
        {
            _list = new List<ChronoPoint>(collection);
        }

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public void SortByValue()
        {
            _list = _list.OrderBy(point => point.value).ThenBy(point => !point.isReadOnly).ToList();
        }
    }
}
