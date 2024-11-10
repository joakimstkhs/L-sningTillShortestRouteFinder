using ShortestRouteFinder.Model;

namespace ShortestRouteFinder.Services;

public interface IRouteService
{
    List<Route> LoadRoutes();
}