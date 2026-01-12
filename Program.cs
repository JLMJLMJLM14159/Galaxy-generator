using System.Drawing;
using System.Text.Json;

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
            string json = JsonSerializer.Serialize(galaxy);
            Console.WriteLine(json);
            File.WriteAllText("galaxy.json", json);
        }
    }

    public class Galaxy
    {
        public Dictionary<(double, double, double), StarSystem> StarSystems { get; set; }
        public GalaxySizes GalaxySize { get; set; }
        public GalaxyTypes GalaxyType { get; set; }
        public (int, int, int) GalaxySizeCoords { get; set; }
        public int NumberOfStarSystems { get; set; }
        public enum GalaxySizes
        {
            rlysmall = 1,
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

            GalaxySizeCoords = (100000 * (int)GalaxySize, 190 * (int)GalaxySize, 100000 * (int)GalaxySize);
            NumberOfStarSystems = 20000000 * (int)GalaxySize;

            Bitmap bitmap = new($"{AppContext.BaseDirectory}/Probability textures/{GalaxyType}.png");

            int numberOfStarSystemsToMake = NumberOfStarSystems;
            int times = 0;
            while (numberOfStarSystemsToMake > 0)
            {
                float x = random.Next((GalaxySizeCoords.Item1 / 2) + 1) * new List<int>([-1, 1])[random.Next(2)];
                float y = random.Next((GalaxySizeCoords.Item2 / 2) + 1) * new List<int>([-1, 1])[random.Next(2)];
                float z = random.Next((GalaxySizeCoords.Item3 / 2) + 1) * new List<int>([-1, 1])[random.Next(2)];

                float threshold = SampleProbability(bitmap, x, z, GalaxySizeCoords.Item1, GalaxySizeCoords.Item3);

                if ((float)random.NextDouble() < threshold && !StarSystems.ContainsKey((x, y, z))) //GOING DOWN ON PURPORSE, FIGURE IT OUT DUMBASS
                {
                    StarSystems.Add((x, y, z), new(false));
                    numberOfStarSystemsToMake--;

                    times++;
                    Console.WriteLine($"({x}, {y}, {z}), {times}/{NumberOfStarSystems}, {(decimal)times / NumberOfStarSystems * 100}%");
                }
            }
        }

        public static float SampleProbability(Bitmap bitmap, float x, float z, float galaxySizeX, float galaxySizeZ)
        {
            float u = x / galaxySizeX;
            float w = z / galaxySizeZ;

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
        public Dictionary<(double, double, double), Body>? Bodies { get; set; }

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

    public class Body
    {
        
    }
}