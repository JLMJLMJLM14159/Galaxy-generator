namespace Galaxy_Generator
{
    public static class Program
    {
        public static void Main()
        {
            
        }
    }

    public class Galaxy
    {
        public Dictionary<(double, double, double), System> Systems { get; set; }
        public GalaxyTypes GalaxyType { get; set; }
        public GalaxySizes GalaxySize { get; set; }
        public enum GalaxyTypes
        {
            spiral,
            elliptical,
            lenticular,
            irregular
        }
        public enum GalaxySizes
        {
            tiny,
            small,
            normal,
            large,
            massive,
        }
        public Galaxy(GalaxyTypes? galaxyType, GalaxySizes? galaxySize)
        {
            Random random = new();
            GalaxyType = galaxyType != null ? galaxyType.Value : Enum.GetValues<GalaxyTypes>()[random.Next(Enum.GetValues<GalaxyTypes>().Length)];
            GalaxySize = galaxySize != null ? galaxySize.Value : Enum.GetValues<GalaxySizes>()[random.Next(Enum.GetValues<GalaxySizes>().Length)];
        }
    }

    public class System
    {
        public Dictionary<(double, double, double), Planet> Planets { get; set; }
    }

    public class Planet
    {
        
    }
}