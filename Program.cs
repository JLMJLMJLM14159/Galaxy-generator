using Newtonsoft.Json;
using System.Drawing;

namespace Galaxy_Generator
{
    public static class Program
    {
        public static void Main()
        {
            
            Galaxy.GalaxySizes? galaxySize;
            Console.Write("Enter galaxy size: ");
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
            Console.Write("Enter galaxy type: ");
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

            GalaxyFileManagement.Save(galaxy);
        }
    }

    public class Galaxy
    {
        public Dictionary<(double, double, double), StarSystem> StarSystems { get; set; } //COORDS MEASURED IN PETAMETRES, NOT LY
        public GalaxySizes GalaxySize { get; set; }
        public GalaxyTypes GalaxyType { get; set; }
        public (double, double, double) GalaxySizeCoords { get; set; }
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
            StarSystems = [];
            StarSystems.Add((0, 0, 0), new(true));

            GalaxySizeCoords = (10 * (int)GalaxySize, 0.019 * (int)GalaxySize, 10 * (int)GalaxySize);
            NumberOfStarSystems = 2000 * (int)GalaxySize;

            Bitmap bitmap = new($"{AppContext.BaseDirectory}/Probability textures/{GalaxyType}.png");

            int numberOfStarSystemsToMake = NumberOfStarSystems;
            while (numberOfStarSystemsToMake > 0)
            {
                double x = random.NextDouble() * (GalaxySizeCoords.Item1 / 2) + 1 * new List<int>([-1, 1])[random.Next(2)];
                double y = random.NextDouble() * (GalaxySizeCoords.Item2 / 2) + 1 * new List<int>([-1, 1])[random.Next(2)];
                double z = random.NextDouble() * (GalaxySizeCoords.Item3 / 2) + 1 * new List<int>([-1, 1])[random.Next(2)];

                float threshold = SampleProbability(bitmap, x, z, GalaxySizeCoords.Item1, GalaxySizeCoords.Item3);

                if ((float)random.NextDouble() < threshold && !StarSystems.ContainsKey((x, y, z))) //GOING DOWN ON PURPORSE, FIGURE IT OUT DUMBASS
                {
                    StarSystems.Add((x, y, z), new(false));
                    numberOfStarSystemsToMake--;

                    Console.WriteLine($"({x}, {y}, {z}), {NumberOfStarSystems - numberOfStarSystemsToMake}/{NumberOfStarSystems}, {(decimal)(NumberOfStarSystems - numberOfStarSystemsToMake) / NumberOfStarSystems * 100}%");
                }
            }
        }

        public static float SampleProbability(Bitmap bitmap, double x, double z, double galaxySizeX, double galaxySizeZ)
        {
            double u = x / galaxySizeX;
            double w = z / galaxySizeZ;

            int imgX = (int)(u * (bitmap.Width - 1));
            int imgZ = (int)(w * (bitmap.Height - 1));

            imgX = Math.Clamp(imgX, 0, bitmap.Width - 1);
            imgZ = Math.Clamp(imgZ, 0, bitmap.Height - 1);

            Color c = bitmap.GetPixel(imgX, imgZ);
            return (c.R + c.G + c.B) / 3 / 255f;
        }
    }

    public class StarSystem
    {
        public Dictionary<int, StellarBody<Planet>>? StarsAndBelow { get; set; } //THE NO TUPLE IS FOR MEASURING RADIUS WOAH

        public StarSystem(bool blackHole)
        {
            if (blackHole)
            {
                
            }
            else
            {
                
            }
        }
    }

    public abstract class StellarBody<TChild>
    {
        public List<TChild> Children = [];
    }

    public class BlackHole : StellarBody<Planet>
    {
        
    }

    public class Star : StellarBody<Planet>
    {
        
    }

    public class Planet : StellarBody<Moon>
    {
        
    }

    public class Moon : StellarBody<NoChild>
    {
        
    }

    public sealed class NoChild : StellarBody<NoChild>
    {
        private NoChild() { }
    }


    public static class GalaxyFileManagement
    {
        public static void Save<T>(T obj)
        {
            string file = JsonConvert.SerializeObject(
                obj,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }
            );
            File.WriteAllText("galaxy.json", file);
        }

        public static Galaxy? Load(string json)
        {
            return JsonConvert.DeserializeObject<Galaxy>(
                json,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }
            );
        }
    }
}