namespace PubStar.Io
{
    public class AdPosition
    {
        public float X { get; }
        public float Y { get; }
        public string Preset { get; } = PresetValue.None;

        private class PresetValue
        {
            public static readonly string None = "None";
            public static readonly string Center = "Center";
            public static readonly string Top = "Top";
            public static readonly string TopLeft = "TopLeft";
            public static readonly string TopRight = "TopRight";
            public static readonly string Bottom = "Bottom";
            public static readonly string BottomLeft = "BottomLeft";
            public static readonly string BottomRight = "BottomRight";
        }

        public AdPosition(int x, int y)
        {
            X = x;
            Y = y;
            Preset = PresetValue.None;
        }

        private AdPosition(string preset)
        {
            X = 0;
            Y = 0;
            Preset = preset;
        }

        private AdPosition(string preset, float x, float y)
        {
            X = x;
            Y = y;
            Preset = preset;
        }

        public static readonly AdPosition Center = new AdPosition(PresetValue.Center);
        public static readonly AdPosition Top = new AdPosition(PresetValue.Top);
        public static readonly AdPosition TopLeft = new AdPosition(PresetValue.TopLeft);
        public static readonly AdPosition TopRight = new AdPosition(PresetValue.TopRight);
        public static readonly AdPosition Bottom = new AdPosition(PresetValue.Bottom);
        public static readonly AdPosition BottomLeft = new AdPosition(PresetValue.BottomLeft);
        public static readonly AdPosition BottomRight = new AdPosition(PresetValue.BottomRight);

        public AdPosition WithOffset(float x, float y)
        {
            return new AdPosition(Preset, x, y);
        }
    }
}