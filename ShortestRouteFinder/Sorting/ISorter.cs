using ShortestRouteFinder.Model;
using SortDirection = ShortestRouteFinder.Sort_Enums.SortDirection;

namespace ShortestRouteFinder.Sorting;

public interface ISorter
{
    void Sort(List<Route> routes, SortDirection direction);
}