using System.Drawing;

namespace Galaxy_Generator
{
    public static class Program
    {
        public static void Main()
        {
            
            Galaxy.GalaxySizes? galaxySize;
            Console.Write("Enter galaxy size:");
            string? galaxySizeString = Console.ReadLine();
            if (galaxySizeString != null)
            {
                try
                { galaxySize = Enum.Parse<Galaxy.GalaxySizes>(galaxySizeString); }
                catch
                { galaxySize = null; }
            }
            else { galaxySize = null; }

            Galaxy.GalaxyTypes? galaxyType;
            Console.Write("Enter galaxy type:");
            string? galaxyTypeString = Console.ReadLine();
            if (galaxyTypeString != null)
            {
                try
                { galaxyType = Enum.Parse<Galaxy.GalaxyTypes>(galaxyTypeString); }
                catch
                { galaxyType = null; }
            }
            else { galaxyType = null; }

            Galaxy galaxy = new(galaxySize, galaxyType);
        }
    }

    public class Galaxy
    {
        public Dictionary<(double, double, double), StarSystem> StarSystems { get; set; }
        public GalaxySizes GalaxySize { get; set; }
        public GalaxyTypes GalaxyType { get; set; }
        public (int, int, int) SizeCoords { get; set; }
        public int NumberOfStarSystems { get; set; }
        public enum GalaxySizes
        {
            tiny = 10,
            small = 25,
            normal = 50,
            large = 75,
            massive = 100,
        }
        public enum GalaxyTypes
        {
            spiral,
            elliptical,
        }
        public Galaxy(GalaxySizes? galaxySize, GalaxyTypes? galaxyType)
        {
            Random random = new();
            GalaxySize = galaxySize != null ? galaxySize.Value : Enum.GetValues<GalaxySizes>()[random.Next(Enum.GetValues<GalaxySizes>().Length)];
            GalaxyType = galaxyType != null ? galaxyType.Value : Enum.GetValues<GalaxyTypes>()[random.Next(Enum.GetValues<GalaxyTypes>().Length)];

            SizeCoords = (100000 * (int)GalaxySize, 190 * (int)GalaxySize, 100000 * (int)GalaxySize);
            NumberOfStarSystems = 20000000 * (int)GalaxySize;

            Bitmap bitmap = new($"{AppContext.BaseDirectory}/Probability textures/{GalaxyType}.png");
            
        }
    }

    public class StarSystem
    {
        public Dictionary<(double, double, double), Body> Bodies { get; set; }
    }

    public class Body
    {
        
    }
}