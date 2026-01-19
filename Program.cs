using Newtonsoft.Json;
using System.Drawing;
using static Galaxy_Generator.SizesClass;
using static Galaxy_Generator.SizesClass.Sizes;
using static Galaxy_Generator.CentralBody;
using static Galaxy_Generator.CentralBody.CentralBodies;

namespace Galaxy_Generator
{
    public static class Program
    {
        public static void Main()
        {
            
            Sizes? galaxySize;
            Console.Write("Enter galaxy size: ");
            string? galaxySizeString = Console.ReadLine();
            if (galaxySizeString != null)
            {
                try
                { galaxySize = Enum.Parse<Sizes>(galaxySizeString); }
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
        public Sizes GalaxySize { get; set; }
        public GalaxyTypes GalaxyType { get; set; }
        public (double, double, double) GalaxySizeCoords { get; set; }
        public int NumberOfStarSystems { get; set; }
        public enum GalaxyTypes
        {
            spiral,
            elliptical,
        }
        public Galaxy(Sizes? galaxySize, GalaxyTypes? galaxyType)
        {
            Random random = new();
            GalaxySize = galaxySize != null ? galaxySize.Value : Enum.GetValues<Sizes>()[random.Next(Enum.GetValues<Sizes>().Length)];
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
        public List<CentralBody>? CentralBodies { get; set; } = [];

        public StarSystem(bool blackHole)
        {
            if (blackHole)
            { CentralBodies.Add(new CentralBody(huge, blackhole)); }
            else
            {
                Random random = new();

                List<int> numberOfPotentialStars = [ 1, 2, 3, 4 ];
                List<double> numberOfPotentialStarsWeights = [ 0.7, 0.2, 0.075, 0.025 ];

                int chosenIndex = 0;
                double randomDouble = random.NextDouble();
                double cumulative = 0;
                for (int i = 0; i < numberOfPotentialStars.Count; i++)
                {
                    cumulative += numberOfPotentialStarsWeights[i];
                    if (randomDouble <= cumulative)
                    {
                        chosenIndex = i;
                        break;
                    }
                }

                for (int i = 1; i <= numberOfPotentialStars[chosenIndex]; i++)
                { CentralBodies.Add(new(Enum.GetValues<Sizes>()[random.Next(Enum.GetValues<Sizes>().Length)], random.NextDouble() <= 0.95 ? star : blackhole)); }

                foreach (CentralBody centralBody in CentralBodies)
                {
                    List<int> numberOfPotentialPlanets = [ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 ];
                    List<double> numberOfPotentialPlanetsWeights = [ 0.2, 0.2, 0.1, 0.1, 0.1, 0.1, 0.05, 0.05, 0.05, 0.05 ];

                    chosenIndex = 0;
                    randomDouble = random.NextDouble();
                    cumulative = 0;
                    for (int i = 0; i < numberOfPotentialPlanets.Count; i++)
                    {
                        cumulative += numberOfPotentialPlanetsWeights[i];
                        if (randomDouble <= cumulative)
                        {
                            chosenIndex = i;
                            break;
                        }
                    }

                    for (int i = 1; i <= numberOfPotentialPlanets[chosenIndex]; i++)
                    { centralBody.Planets.Add(new(Enum.GetValues<Sizes>()[random.Next(Enum.GetValues<Sizes>().Length)])); }
                    
                    foreach (Planet planet in centralBody.Planets)
                    {
                        int numberOfMoons = random.Next(26);
                        for (int i = 1; i <= numberOfMoons; i++)
                        { planet.Moons.Add(new(Enum.GetValues<Sizes>()[random.Next(Enum.GetValues<Sizes>().Length)])); }
                    }
                }
            }
        }
    }

    public abstract class StellarBody(Sizes size = normal)
    {
        public Sizes Size { get; set; } = size;
    }

    public class CentralBody(Sizes size, CentralBodies bodyType)
    {
        public List<Planet> Planets { get; set; } = [];
        public Sizes Size { get; set; } = size;
        public enum CentralBodies
        {
            star,
            blackhole
        }
        public CentralBodies BodyType = bodyType;
    }

    public class Planet(Sizes size) : StellarBody(size)
    {
        public List<Moon> Moons { get; set; } = [];
    }

    public class Moon(Sizes size) : StellarBody(size)
    {
        
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

    public static class SizesClass
    {
        public enum Sizes
        {
            tiny = 10,
            small = 25,
            normal = 50,
            large = 75,
            huge = 100,
        }
    }
}