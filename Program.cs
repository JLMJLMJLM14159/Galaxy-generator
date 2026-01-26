using Newtonsoft.Json;
using System.Drawing;
using static Galaxy_Generator.SizesClass;
using static Galaxy_Generator.SizesClass.Sizes;
using static Galaxy_Generator.CentralBody;
using static Galaxy_Generator.CentralBody.CentralBodies;
using static Galaxy_Generator.Planet;
using static Galaxy_Generator.Planet.Habitabilities;

namespace Galaxy_Generator
{
    public static class Program
    {
        public static readonly Dictionary<string, Action> AdditionalModes = new()
        {
            { "normal", () => { Galaxy galaxy = Create(); MapAndSaveMap(galaxy); Save(galaxy); } },
            { "debug", () => { Galaxy galaxy = Create(); galaxy.PrintDebugInfo(); MapAndSaveMap(galaxy); Save(galaxy); } },
            { "normalandviewsystems", () => { Galaxy galaxy = Create(); MapAndSaveMap(galaxy); Save(galaxy); ViewSystem(galaxy, AskForSystemToView()); } },
            { "loadandviewsystems", () => { Galaxy galaxy = Load()!; ViewSystem(galaxy, AskForSystemToView()); } }
        };

        public static void Main()
        {
            Directory.CreateDirectory("results");

            Console.Write("Change mode? Leave blank for normal excecution. Enter here: ");
            string? modeString = Console.ReadLine();
            if (modeString != null)
            {
                if (AdditionalModes.TryGetValue(modeString, out Action? value)) { value(); }
                else { AdditionalModes["normal"](); }
            }
            else { AdditionalModes["normal"](); }
        }

        private static Galaxy Create()
        {
            Random random = new();

            Sizes? galaxySize;
            Console.Write("Enter galaxy size: ");
            string? galaxySizeString = Console.ReadLine();
            if (galaxySizeString != null)
            {
                try
                { galaxySize = Enum.Parse<Sizes>(galaxySizeString); }
                catch
                { galaxySize = Enum.GetValues<Sizes>()[random.Next(Enum.GetValues<Sizes>().Length)]; }
            }
            else { galaxySize = Enum.GetValues<Sizes>()[random.Next(Enum.GetValues<Sizes>().Length)]; }

            Galaxy.GalaxyTypes? galaxyType;
            Console.Write("Enter galaxy type: ");
            string? galaxyTypeString = Console.ReadLine();
            if (galaxyTypeString != null)
            {
                try
                { galaxyType = Enum.Parse<Galaxy.GalaxyTypes>(galaxyTypeString); }
                catch
                { galaxyType = Enum.GetValues<Galaxy.GalaxyTypes>()[random.Next(Enum.GetValues<Galaxy.GalaxyTypes>().Length)]; }
            }
            else { galaxyType = Enum.GetValues<Galaxy.GalaxyTypes>()[random.Next(Enum.GetValues<Galaxy.GalaxyTypes>().Length)]; }

            return new(galaxySize, galaxyType);
        }

        private static Galaxy? Load() => GalaxyFileManagement.Load(File.ReadAllText("results/galaxy.json"));

        private static void Save(Galaxy galaxy) => GalaxyFileManagement.Save(galaxy);

        private static void MapAndSaveMap(Galaxy galaxy) => galaxy.MapAndSaveGalaxyMap();

        private static (double, double, double) AskForSystemToView()
        {
            Console.Write("Enter x coord for system: ");
            double x = double.Parse(Console.ReadLine()!);
            Console.Write("Enter y coord for system: ");
            double y = double.Parse(Console.ReadLine()!);
            Console.Write("Enter z coord for system: ");
            double z = double.Parse(Console.ReadLine()!);

            return (x, y, z);
        }

        private static void ViewSystem(Galaxy galaxy, (double, double, double) coords)
        {
            if (galaxy.StarSystems.TryGetValue(coords, out StarSystem? value))
            {
                Console.WriteLine($"System at coordinates ({coords.Item1}, {coords.Item2}, {coords.Item3}):");
                Console.WriteLine($"    Central bodies:");
                foreach (CentralBody centralBody in value.CentralBodies)
                {
                    Console.WriteLine($"        A {centralBody.Size} {centralBody.BodyType} with:");
                    foreach (Planet planet in centralBody.Planets)
                    {
                        Console.WriteLine($"            A {planet.Size}, {planet.Temperature}, {planet.Habitability} planet with:");
                        foreach (Moon moon in planet.Moons)
                        {
                            Console.WriteLine($"                A {moon.Size} moon.");
                        }
                    }
                }
            }
        }
    }

    public class Galaxy
    {
        public Dictionary<(double, double, double), StarSystem> StarSystems { get; set; }
        public Sizes GalaxySize { get; set; }
        public GalaxyTypes GalaxyType { get; set; }
        public (int, double, int) GalaxySizeCoords { get; set; }
        public int NumberOfStarSystems { get; set; }
        public enum GalaxyTypes
        {
            elliptical,
        }

        public Galaxy() { StarSystems = []; }

        public Galaxy(Sizes? galaxySize, GalaxyTypes? galaxyType)
        {
            Random random = new();
            GalaxySize = galaxySize != null ? galaxySize.Value : Enum.GetValues<Sizes>()[random.Next(Enum.GetValues<Sizes>().Length)];
            GalaxyType = galaxyType != null ? galaxyType.Value : Enum.GetValues<GalaxyTypes>()[random.Next(Enum.GetValues<GalaxyTypes>().Length)];
            StarSystems = [];

            GalaxySizeCoords = (10 * (int)GalaxySize, 0.019 * (int)GalaxySize, 10 * (int)GalaxySize);
            NumberOfStarSystems = 2000 * (int)GalaxySize;

            StarSystems.Add((GalaxySizeCoords.Item1 / 2, GalaxySizeCoords.Item2 / 2, GalaxySizeCoords.Item3 / 2), new(true));

            Bitmap bitmap = new($"{AppContext.BaseDirectory}/Probability textures/{GalaxyType}.png");

            int numberOfStarSystemsToMake = NumberOfStarSystems;
            while (numberOfStarSystemsToMake > 0)
            {
                double x = random.NextDouble() * GalaxySizeCoords.Item1;
                double y = random.NextDouble() * GalaxySizeCoords.Item2;
                double z = random.NextDouble() * GalaxySizeCoords.Item3;

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

        public void MapAndSaveGalaxyMap()
        {
            Bitmap bmp = new(GalaxySizeCoords.Item1 + 1, GalaxySizeCoords.Item3 + 1);
            Graphics g = Graphics.FromImage(bmp);

            g.Clear(Color.Black);

            foreach (KeyValuePair<(double, double, double), StarSystem> Kvp in StarSystems)
            {
                int x = (int)Kvp.Key.Item1;
                int z = (int)Kvp.Key.Item3;

                bmp.SetPixel(x, z, Color.White);
            }

            bmp.Save("results/galaxy.png");
        }

        public void PrintDebugInfo()
        {
            Console.WriteLine($"X range: {StarSystems.Keys.Min(k => k.Item1)} to {StarSystems.Keys.Max(k => k.Item1)}");
            Console.WriteLine($"Z range: {StarSystems.Keys.Min(k => k.Item3)} to {StarSystems.Keys.Max(k => k.Item3)}");

            Console.WriteLine($"Average X: {StarSystems.Keys.Average(k => k.Item1)}");
            Console.WriteLine($"Average Z: {StarSystems.Keys.Average(k => k.Item3)}");

            Console.WriteLine($"GalaxySizeCoords: X={GalaxySizeCoords.Item1}, Y={GalaxySizeCoords.Item2}, Z={GalaxySizeCoords.Item3}");
        }

        public static implicit operator Galaxy(KnockoffGalaxy knockoffGalaxy)
        {
            Dictionary<(double, double, double), StarSystem> goodStarSystems = [];
            foreach (KeyValuePair<string, StarSystem> kvp in knockoffGalaxy.StarSystems)
            {
                string[] systemsParts = kvp.Key.Trim('(', ')', ' ').Split(", ");

                goodStarSystems.Add((
                    double.Parse(systemsParts[0]),
                    double.Parse(systemsParts[1]),
                    double.Parse(systemsParts[2])
                ), kvp.Value);
            }

            double[] coordParts = [.. knockoffGalaxy.GalaxySizeCoords.Trim('(', ')', ' ').Split(", ").Select(double.Parse)];
            
            return new() {
                GalaxySize = knockoffGalaxy.GalaxySize,
                GalaxySizeCoords = ((int)coordParts[0], coordParts[1], (int)coordParts[2]),
                GalaxyType = knockoffGalaxy.GalaxyType,
                NumberOfStarSystems = knockoffGalaxy.NumberOfStarSystems,
                StarSystems = goodStarSystems
            };
        }
    }

    public class StarSystem
    {
        public List<CentralBody> CentralBodies { get; set; } = [];

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
                    { centralBody.Planets.Add(new(Enum.GetValues<Sizes>()[random.Next(Enum.GetValues<Sizes>().Length)], Enum.GetValues<Temperatures>()[random.Next(Enum.GetValues<Temperatures>().Length)], random.NextDouble() <= 75 ? sterile : fertile)); }
                    
                    foreach (Planet planet in centralBody.Planets)
                    {
                        int numberOfMoons = random.Next(13);
                        for (int i = 1; i <= numberOfMoons; i++)
                        { planet.Moons.Add(new(Enum.GetValues<Sizes>()[random.Next(Enum.GetValues<Sizes>().Length)])); }
                    }
                }
            }
        }
    }

    public abstract class StellarBody(Sizes size = Sizes.normal)
    {
        public Sizes Size { get; set; } = size;
    }

    public class CentralBody(Sizes size, CentralBodies bodyType) : StellarBody(size)
    {
        public List<Planet> Planets { get; set; } = [];
        public CentralBodies BodyType = bodyType;
        public enum CentralBodies
        {
            star,
            blackhole
        }
    }

    public class Planet(Sizes size, Temperatures temperature, Habitabilities habitability) : StellarBody(size)
    {
        public List<Moon> Moons { get; set; } = [];
        public enum Temperatures
        {
            arid,
            warm,
            normal,
            cold,
            freezing
        }
        public enum Habitabilities
        {
            fertile,
            sterile
        }
        public Temperatures Temperature = temperature;
        public Habitabilities Habitability = habitability;
    }

    public class Moon(Sizes size) : StellarBody(size)
    {
        
    }

    public class KnockoffGalaxy
    {
        public Dictionary<string, StarSystem> StarSystems { get; set; }
        public Sizes GalaxySize { get; set; }
        public Galaxy.GalaxyTypes GalaxyType { get; set; }
        public string GalaxySizeCoords { get; set; }
        public int NumberOfStarSystems { get; set; }

        public KnockoffGalaxy() { StarSystems = []; GalaxySizeCoords = ""; }

        public static implicit operator KnockoffGalaxy(Galaxy galaxy)
        {
            return new()
            {
                StarSystems = galaxy.StarSystems.Keys.ToList().Select(k => k.ToString()).Zip(galaxy.StarSystems.Values).ToDictionary(),
                GalaxySize = galaxy.GalaxySize,
                GalaxyType = galaxy.GalaxyType,
                GalaxySizeCoords = galaxy.GalaxySizeCoords.ToString(),
                NumberOfStarSystems = galaxy.NumberOfStarSystems
            };
        }
    }

    public static class GalaxyFileManagement
    {
        public static void Save(KnockoffGalaxy knockoff)
        {
            string file = JsonConvert.SerializeObject(
                knockoff,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }
            );
            File.WriteAllText("results/galaxy.json", file);
        }

        public static Galaxy Load(string json)
        {
            return JsonConvert.DeserializeObject<KnockoffGalaxy>(
                json,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }
            )!;
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