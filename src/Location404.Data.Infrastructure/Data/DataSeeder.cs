using Location404.Data.Domain.Entities;
using Location404.Data.Domain.ValueObjects;
using Location404.Data.Infrastructure.Context;
using Microsoft.Extensions.Logging;

namespace Location404.Data.Infrastructure.Data;

public class DataSeeder(GeoDataDbContext context, ILogger<DataSeeder> logger)
{
    private readonly GeoDataDbContext _context = context;
    private readonly ILogger<DataSeeder> _logger = logger;

    public async Task SeedAsync()
    {
        if (_context.Locations.Any())
        {
            _logger.LogInformation("Database already seeded");
            return;
        }

        _logger.LogInformation("Seeding database with 60 locations...");

        var locations = new List<Location>
        {
            // South America - Brazil
            CreateLocation(-23.5505, -46.6333, "São Paulo, Brazil", "Brazil", "South America", "urban", "metropolitan"),
            CreateLocation(-22.9068, -43.1729, "Rio de Janeiro, Brazil", "Brazil", "South America", "urban", "beach"),
            CreateLocation(-15.7942, -47.8822, "Brasília, Brazil", "Brazil", "South America", "urban", "capital"),
            CreateLocation(-12.9714, -38.5014, "Salvador, Brazil", "Brazil", "South America", "urban", "historic"),
            CreateLocation(-3.1190, -60.0217, "Manaus, Brazil", "Brazil", "South America", "urban", "jungle"),
            CreateLocation(-25.4284, -49.2733, "Curitiba, Brazil", "Brazil", "South America", "urban"),

            // South America - Other
            CreateLocation(-34.6037, -58.3816, "Buenos Aires, Argentina", "Argentina", "South America", "urban", "capital"),
            CreateLocation(-33.4489, -70.6693, "Santiago, Chile", "Chile", "South America", "urban", "capital"),
            CreateLocation(-12.0464, -77.0428, "Lima, Peru", "Peru", "South America", "urban", "capital"),
            CreateLocation(4.7110, -74.0721, "Bogotá, Colombia", "Colombia", "South America", "urban", "capital"),

            // North America - USA
            CreateLocation(40.7580, -73.9855, "New York, USA", "United States", "North America", "urban", "metropolitan"),
            CreateLocation(37.7749, -122.4194, "San Francisco, USA", "United States", "North America", "urban", "coastal"),
            CreateLocation(34.0522, -118.2437, "Los Angeles, USA", "United States", "North America", "urban", "metropolitan"),
            CreateLocation(41.8781, -87.6298, "Chicago, USA", "United States", "North America", "urban", "metropolitan"),
            CreateLocation(25.7617, -80.1918, "Miami, USA", "United States", "North America", "urban", "beach"),
            CreateLocation(47.6062, -122.3321, "Seattle, USA", "United States", "North America", "urban", "coastal"),

            // North America - Other
            CreateLocation(19.4326, -99.1332, "Mexico City, Mexico", "Mexico", "North America", "urban", "capital"),
            CreateLocation(43.6532, -79.3832, "Toronto, Canada", "Canada", "North America", "urban", "metropolitan"),
            CreateLocation(49.2827, -123.1207, "Vancouver, Canada", "Canada", "North America", "urban", "coastal"),

            // Europe - Western
            CreateLocation(48.8566, 2.3522, "Paris, France", "France", "Europe", "urban", "capital"),
            CreateLocation(51.5074, -0.1278, "London, UK", "United Kingdom", "Europe", "urban", "capital"),
            CreateLocation(52.3676, 4.9041, "Amsterdam, Netherlands", "Netherlands", "Europe", "urban", "capital"),
            CreateLocation(50.8503, 4.3517, "Brussels, Belgium", "Belgium", "Europe", "urban", "capital"),
            CreateLocation(41.3851, 2.1734, "Barcelona, Spain", "Spain", "Europe", "urban", "coastal"),
            CreateLocation(40.4168, -3.7038, "Madrid, Spain", "Spain", "Europe", "urban", "capital"),
            CreateLocation(38.7223, -9.1393, "Lisbon, Portugal", "Portugal", "Europe", "urban", "capital"),

            // Europe - Central & Eastern
            CreateLocation(41.9028, 12.4964, "Rome, Italy", "Italy", "Europe", "urban", "capital", "historic"),
            CreateLocation(52.5200, 13.4050, "Berlin, Germany", "Germany", "Europe", "urban", "capital"),
            CreateLocation(48.2082, 16.3738, "Vienna, Austria", "Austria", "Europe", "urban", "capital"),
            CreateLocation(50.0755, 14.4378, "Prague, Czech Republic", "Czech Republic", "Europe", "urban", "capital"),
            CreateLocation(59.3293, 18.0686, "Stockholm, Sweden", "Sweden", "Europe", "urban", "capital"),
            CreateLocation(55.6761, 12.5683, "Copenhagen, Denmark", "Denmark", "Europe", "urban", "capital"),
            CreateLocation(52.2297, 21.0122, "Warsaw, Poland", "Poland", "Europe", "urban", "capital"),

            // Europe - Southern & Eastern
            CreateLocation(37.9838, 23.7275, "Athens, Greece", "Greece", "Europe", "urban", "capital", "historic"),
            CreateLocation(41.0082, 28.9784, "Istanbul, Turkey", "Turkey", "Europe", "urban", "metropolitan"),
            CreateLocation(55.7558, 37.6173, "Moscow, Russia", "Russia", "Europe", "urban", "capital"),

            // Asia - East
            CreateLocation(35.6762, 139.6503, "Tokyo, Japan", "Japan", "Asia", "urban", "capital", "metropolitan"),
            CreateLocation(37.5665, 126.9780, "Seoul, South Korea", "South Korea", "Asia", "urban", "capital"),
            CreateLocation(39.9042, 116.4074, "Beijing, China", "China", "Asia", "urban", "capital"),
            CreateLocation(31.2304, 121.4737, "Shanghai, China", "China", "Asia", "urban", "metropolitan"),
            CreateLocation(22.3193, 114.1694, "Hong Kong", "Hong Kong", "Asia", "urban", "metropolitan"),
            CreateLocation(25.0330, 121.5654, "Taipei, Taiwan", "Taiwan", "Asia", "urban", "capital"),

            // Asia - Southeast
            CreateLocation(1.3521, 103.8198, "Singapore", "Singapore", "Asia", "urban", "metropolitan"),
            CreateLocation(13.7563, 100.5018, "Bangkok, Thailand", "Thailand", "Asia", "urban", "capital"),
            CreateLocation(-6.2088, 106.8456, "Jakarta, Indonesia", "Indonesia", "Asia", "urban", "capital"),
            CreateLocation(14.5995, 120.9842, "Manila, Philippines", "Philippines", "Asia", "urban", "capital"),
            CreateLocation(21.0285, 105.8542, "Hanoi, Vietnam", "Vietnam", "Asia", "urban", "capital"),

            // Asia - South & Middle East
            CreateLocation(28.6139, 77.2090, "New Delhi, India", "India", "Asia", "urban", "capital"),
            CreateLocation(19.0760, 72.8777, "Mumbai, India", "India", "Asia", "urban", "metropolitan"),
            CreateLocation(25.2048, 55.2708, "Dubai, UAE", "United Arab Emirates", "Asia", "urban", "metropolitan"),
            CreateLocation(33.8938, 35.5018, "Beirut, Lebanon", "Lebanon", "Asia", "urban", "capital"),

            // Africa
            CreateLocation(30.0444, 31.2357, "Cairo, Egypt", "Egypt", "Africa", "urban", "capital", "historic"),
            CreateLocation(-26.2041, 28.0473, "Johannesburg, South Africa", "South Africa", "Africa", "urban", "metropolitan"),
            CreateLocation(-33.9249, 18.4241, "Cape Town, South Africa", "South Africa", "Africa", "urban", "coastal"),
            CreateLocation(-1.2921, 36.8219, "Nairobi, Kenya", "Kenya", "Africa", "urban", "capital"),

            // Oceania
            CreateLocation(-33.8688, 151.2093, "Sydney, Australia", "Australia", "Oceania", "urban", "coastal", "metropolitan"),
            CreateLocation(-37.8136, 144.9631, "Melbourne, Australia", "Australia", "Oceania", "urban", "metropolitan"),
            CreateLocation(-41.2865, 174.7762, "Wellington, New Zealand", "New Zealand", "Oceania", "urban", "capital", "coastal"),

            // Additional Brazil locations
            CreateLocation(-8.0476, -34.8770, "Recife, Brazil", "Brazil", "South America", "urban", "coastal"),
            CreateLocation(-30.0346, -51.2177, "Porto Alegre, Brazil", "Brazil", "South America", "urban"),
            CreateLocation(-19.9167, -43.9345, "Belo Horizonte, Brazil", "Brazil", "South America", "urban"),
            CreateLocation(-16.6869, -49.2648, "Goiânia, Brazil", "Brazil", "South America", "urban"),

            // Additional Europe
            CreateLocation(53.3498, -6.2603, "Dublin, Ireland", "Ireland", "Europe", "urban", "capital"),
            CreateLocation(60.1699, 24.9384, "Helsinki, Finland", "Finland", "Europe", "urban", "capital"),
            CreateLocation(59.9139, 10.7522, "Oslo, Norway", "Norway", "Europe", "urban", "capital"),
            CreateLocation(64.1466, -21.9426, "Reykjavik, Iceland", "Iceland", "Europe", "urban", "capital"),
            CreateLocation(45.4642, 9.1900, "Milan, Italy", "Italy", "Europe", "urban", "metropolitan"),
            CreateLocation(43.7696, 11.2558, "Florence, Italy", "Italy", "Europe", "urban", "historic"),
            CreateLocation(45.4408, 12.3155, "Venice, Italy", "Italy", "Europe", "urban", "historic", "coastal"),

            // Additional Asia
            CreateLocation(22.5726, 88.3639, "Kolkata, India", "India", "Asia", "urban", "metropolitan"),
            CreateLocation(13.0827, 80.2707, "Chennai, India", "India", "Asia", "urban", "metropolitan"),
            CreateLocation(3.1390, 101.6869, "Kuala Lumpur, Malaysia", "Malaysia", "Asia", "urban", "capital"),
            CreateLocation(23.8103, 90.4125, "Dhaka, Bangladesh", "Bangladesh", "Asia", "urban", "capital"),
            CreateLocation(33.3152, 44.3661, "Baghdad, Iraq", "Iraq", "Asia", "urban", "capital"),
            CreateLocation(31.9454, 35.9284, "Amman, Jordan", "Jordan", "Asia", "urban", "capital"),

            // Additional North America
            CreateLocation(45.5017, -73.5673, "Montreal, Canada", "Canada", "North America", "urban", "metropolitan"),
            CreateLocation(51.0447, -114.0719, "Calgary, Canada", "Canada", "North America", "urban"),
            CreateLocation(32.7157, -117.1611, "San Diego, USA", "United States", "North America", "urban", "coastal"),
            CreateLocation(29.7604, -95.3698, "Houston, USA", "United States", "North America", "urban", "metropolitan"),
            CreateLocation(33.4484, -112.0740, "Phoenix, USA", "United States", "North America", "urban"),
            CreateLocation(39.7392, -104.9903, "Denver, USA", "United States", "North America", "urban"),

            // Additional South America
            CreateLocation(-0.1807, -78.4678, "Quito, Ecuador", "Ecuador", "South America", "urban", "capital"),
            CreateLocation(10.4806, -66.9036, "Caracas, Venezuela", "Venezuela", "South America", "urban", "capital"),
            CreateLocation(-25.2637, -57.5759, "Asunción, Paraguay", "Paraguay", "South America", "urban", "capital"),

            // Additional Africa
            CreateLocation(-4.0383, 39.6682, "Mombasa, Kenya", "Kenya", "Africa", "urban", "coastal"),
            CreateLocation(6.5244, 3.3792, "Lagos, Nigeria", "Nigeria", "Africa", "urban", "metropolitan"),
            CreateLocation(-1.9706, 30.1044, "Kigali, Rwanda", "Rwanda", "Africa", "urban", "capital"),
            CreateLocation(33.8869, 9.5375, "Tunis, Tunisia", "Tunisia", "Africa", "urban", "capital"),

            // Scenic/Tourist locations
            CreateLocation(27.9881, 86.9250, "Mount Everest Base Camp, Nepal", "Nepal", "Asia", "mountain", "scenic"),
            CreateLocation(37.8651, -119.5383, "Yosemite National Park, USA", "United States", "North America", "nature", "scenic"),
            CreateLocation(-13.1631, -72.5450, "Machu Picchu, Peru", "Peru", "South America", "historic", "scenic", "mountain"),
            CreateLocation(25.1972, 55.2744, "Burj Khalifa, Dubai", "United Arab Emirates", "Asia", "urban", "landmark"),
            CreateLocation(48.8584, 2.2945, "Eiffel Tower, Paris", "France", "Europe", "urban", "landmark"),
            CreateLocation(27.1751, 78.0421, "Taj Mahal, India", "India", "Asia", "historic", "landmark"),
            CreateLocation(43.7230, 10.3966, "Pisa, Italy", "Italy", "Europe", "urban", "historic", "landmark"),
            CreateLocation(-22.9519, -43.2105, "Christ the Redeemer, Rio", "Brazil", "South America", "landmark", "scenic"),
        };

        await _context.Locations.AddRangeAsync(locations);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully seeded {Count} locations", locations.Count);
    }

    private static Location CreateLocation(double lat, double lng, string name, string country, string region, params string[] tags)
    {
        var coordinate = new Coordinate(lat, lng);
        var location = new Location(Guid.NewGuid(), coordinate, name, country, region);

        // Set random heading and pitch for variety
        var random = new Random(name.GetHashCode()); // Deterministic based on name
        location.SetStreetViewParams(random.Next(0, 360), random.Next(-10, 10));

        foreach (var tag in tags)
        {
            location.AddTag(tag);
        }

        return location;
    }
}
