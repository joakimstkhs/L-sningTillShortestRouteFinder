using System.IO;
using System.Windows;
using Newtonsoft.Json;
using ShortestRouteFinder.Model;

namespace ShortestRouteFinder.Services;

public class FileRouteService(string filePath) : IRouteService
{
   public List<Route> LoadRoutes()
   {
      try {
         var json = File.ReadAllText(filePath);
         return JsonConvert.DeserializeObject<List<Route>>(json) ?? new List<Route>();
      }
      catch (Exception ex) {
         MessageBox.Show($"Error loading routes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         return [];
      }
   }
}