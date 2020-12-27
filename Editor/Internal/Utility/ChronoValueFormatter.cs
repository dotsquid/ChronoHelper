using System;

namespace dotsquid.ChronoHelper.Internal
{
    internal static class ChronoValueFormatter
    {
        private struct Fraction
        {
            public float value;
            public string glyph;

            public Fraction(float value, string glyph)
            {
                this.value = value;
                this.glyph = glyph;
            }
        }

        private static readonly Fraction[] kFractions = new Fraction[]
        {
            new Fraction(0.000f, ""),
            new Fraction(0.100f, "⅒"),
            new Fraction(0.111f, "⅑"),
            new Fraction(0.125f, "⅛"),
            new Fraction(0.143f, "⅐"),
            new Fraction(0.167f, "⅙"),
            new Fraction(0.200f, "⅕"),
            new Fraction(0.250f, "¼"),
            new Fraction(0.333f, "⅓"),
            new Fraction(0.375f, "⅜"),
            new Fraction(0.400f, "⅖"),
            new Fraction(0.500f, "½"),
            new Fraction(0.600f, "⅗"),
            new Fraction(0.625f, "⅝"),
            new Fraction(0.667f, "⅔"),
            new Fraction(0.750f, "¾"),
            new Fraction(0.800f, "⅘"),
            new Fraction(0.833f, "⅚"),
            new Fraction(0.875f, "⅞"),
            new Fraction(1.000f, ""),
        };

        private const string kChronoValuePrefix = "×";

        public static string Nicify(float value, Format mode)
        {
            switch (mode)
            {
                case Format.Compact:
                    return GetChronoValueCompact(value);

                case Format.Short:
                    return GetChronoValueShort(value);

                case Format.AsIs:
                default:
                    return GetChronoValueAsIs(value);
            }
        }

        private static string GetChronoValueAsIs(float value)
        {
            return $"{kChronoValuePrefix}{value}";
        }

        private static string GetChronoValueShort(float value)
        {
            return $"{kChronoValuePrefix}{value:0.#}";
        }

        private static string GetChronoValueCompact(float value)
        {
            int addition = GetFractionGlyph(value, out var fractionGlyph);
            int integral = (int)Math.Truncate(value) + addition;
            string integralGlyph = ((integral != 0) || string.IsNullOrEmpty(fractionGlyph))
                                 ? integral.ToString()
                                 : null;
            return $"{kChronoValuePrefix}{integralGlyph}{fractionGlyph}";
        }

        private static int GetFractionGlyph(float value, out string glyph)
        {
            float fraction = value - (float)Math.Truncate(value);
            var result = default(Fraction);
            for (int i = 0, count = kFractions.Length - 1; i < count; ++i)
            {
                var leftFraction = kFractions[i];
                var rightFraction = kFractions[i + 1];
                var leftValue = leftFraction.value;
                var rightValue = rightFraction.value;
                if (fraction >= leftValue && fraction <= rightValue)
                {
                    if (fraction - leftValue < rightValue - fraction)
                        result = leftFraction;
                    else
                        result = rightFraction;
                }
            }
            glyph = result.glyph;
            return (int)Math.Floor(result.value);
        }
    }
}
