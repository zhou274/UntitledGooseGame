public static class AspectRatios
{
    public static readonly Ratio[] aspectRatios = new Ratio[]
    {
        new Ratio("AppStore Vert 6.5 inch (iPhone XS Max, iPhone XR)", 1242, 2688),
        new Ratio("AppStore Land 6.5 inch (iPhone XS Max, iPhone XR)", 2688, 1242),
        new Ratio("AppStore Vert 5.5 inch (iPhone 6s Plus, iPhone 7 Plus, iPhone 8 Plus)", 1242, 2208),
        new Ratio("AppStore Land 5.5 inch (iPhone 6s Plus, iPhone 7 Plus, iPhone 8 Plus)", 2208, 1242),
        new Ratio("AppStore Vert 4.7 inch (iPhone SE)", 750, 1334),
        new Ratio("AppStore Land 4.7 inch (iPhone SE)", 1334, 750),
        new Ratio("AppStore Vert 12.9 inch (iPad Pro (3rd gen))", 2048, 2732),
        new Ratio("AppStore Land 12.9 inch (iPad Pro (3rd gen))", 2732, 2048),


        new Ratio("GooglePlay Vert FullHD", 1080, 1920),
        new Ratio("GooglePlay Land FullHD", 1920, 1080),
        new Ratio("GooglePlay Vert 7-inch tablet", 1200, 1920),
        new Ratio("GooglePlay Land 7-inch tablet", 1920, 1200),
    };

    public struct Ratio
    {
        public string name;
        public int xAspect;
        public int yAspect;

        public Ratio(string name, int xAspect, int yAspect)
        {
            this.name = name;
            this.xAspect = xAspect;
            this.yAspect = yAspect;
        }
    }
}
