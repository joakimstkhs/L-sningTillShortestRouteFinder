using System.IO;
using Newtonsoft.Json;
using ShortestRouteFinder.Model;

namespace ShortestRouteFinder.Repositories
{
    public class RouteRepository
    {
        private const string FileName = "routes.json";

        public List<Route> LoadRoutes()
        {
            if (!File.Exists(FileName))
                return new List<Route>();

            var json = File.ReadAllText(FileName);
            return JsonConvert.DeserializeObject<List<Route>>(json) ?? new List<Route>();
        }

        public void SaveRoutes(IEnumerable<Route?> routes)
        {
            var json = JsonConvert.SerializeObject(routes, Formatting.Indented);
            File.WriteAllText(FileName, json);
        }
    }
}