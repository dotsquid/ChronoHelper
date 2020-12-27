using System;
using UnityEngine;

namespace dotsquid.ChronoHelper.Internal
{
    internal static class Helper
    {
        public static Texture2D CreateTextureFromBase64(string base64, string name = "")
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

        public struct Version
        {
            public enum State
            {
                Alpha,
                Beta,
                ReleaseCandidate,
                Final,
            }

            private int _major;
            private int _minor;
            private int _patch;
            private string _formatted;

            public Version(int major, int minor, int patch, State state)
            {
                _major = major;
                _minor = minor;
                _patch = patch;
                _formatted = $"{_major}.{_minor}.{_patch}{GetStateName(state)}";
            }

            public static implicit operator string(Version self)
            {
                return self.ToString();
            }

            public override string ToString()
            {
                if (string.IsNullOrEmpty(_formatted))
                    throw new NotImplementedException("Can't be used with default value");
                return _formatted;
            }

            private static string GetStateName(State state)
            {
                switch (state)
                {
                    case State.Alpha:
                        return "a";
                    case State.Beta:
                        return "b";
                    case State.ReleaseCandidate:
                        return "rc";
                    case State.Final:
                        return "f";
                    default:
                        return "";
                }
            }
        }

        public static readonly Version version = new Version(2, 0, 0, Version.State.Final);
    }
}
