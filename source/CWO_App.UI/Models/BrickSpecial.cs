using System;

namespace CWO_App.UI.Models
{
    public class BrickSpecial
    {
        public float Value { get; set; }
        public float CoBelow { get; set; }
        public float CoAbove { get; set; }
        public float DifBelow { get; set; }
        public float DifAbove { get; set; }

        public BrickSpecial(float value) {
            Value = value;
            float below = (float)Math.Floor(value / 112.5f);
            CoBelow = below * 112.5f;
            DifBelow = value - CoBelow;

            float above = (float)Math.Ceiling(value / 112.5f);
            CoAbove = above * 112.5f;
            DifAbove = CoAbove - value;
        }
    }
}
