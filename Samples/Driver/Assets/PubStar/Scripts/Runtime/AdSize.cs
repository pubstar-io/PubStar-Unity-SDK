namespace PubStar.Io
{
    public class AdSize
    {
        public float Width { get; }
        public float Height { get; }
        public string SizeValue { get; }

        public AdSize(float width, float height)
        {
            Width = width;
            Height = height;
        }

        private class Size
        {
            public static readonly string Small = "small";
            public static readonly string Medium = "medium";
            public static readonly string Large = "large";
        }

        private AdSize(string size)
        {
            Width = FULL_WIDTH;
            switch (size)
            {
                case "small":
                    Height = 58;
                    SizeValue = Size.Small;
                    break;
                case "medium":
                    Height = 100;
                    SizeValue = Size.Medium;
                    break;
                case "large":
                    Height = 230;
                    SizeValue = Size.Large;
                    break;
                default:
                    Height = 58;
                    SizeValue = Size.Small;
                    break;
            }
        }

        public static readonly float FULL_WIDTH = -1;
        public static readonly float FULL_HEIGHT = -1;
        public static readonly AdSize Small = new AdSize(Size.Small);
        public static readonly AdSize Medium = new AdSize(Size.Medium);
        public static readonly AdSize Large = new AdSize(Size.Large);
    }
}