using System;
using UnityEngine;

namespace dotsquid.ChronoHelper.Internal
{
    internal static class GUIHelper
    {
        public struct ReplaceColor : IDisposable
        {
            public static ReplaceColor With(Color color) => new ReplaceColor(color);

            private Color _oldColor;

            private ReplaceColor(Color color)
            {
                _oldColor = GUI.color;
                GUI.color = color;
            }

            void IDisposable.Dispose() => GUI.color = _oldColor;
        }
    }
}
