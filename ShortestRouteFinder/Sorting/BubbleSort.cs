using ShortestRouteFinder.Model;
using static ShortestRouteFinder.Sorting.SortingMethods;
using SortDirection = ShortestRouteFinder.Sort_Enums.SortDirection;

namespace ShortestRouteFinder.Sorting
{
    public class BubbleSort : ISorter
    {
        public void Sort(List<Route> routes, SortDirection direction)
        {
            BubbleSortAlgo(routes, direction);
        }

        private static void BubbleSortAlgo(List<Route> routes, SortDirection direction)
        {
            for (var i = 0; i < routes.Count - 1; i++)
            {
                for (var j = 0; j < routes.Count - i - 1; j++)
                {
                    if (ShouldSwap(routes[j].Distance, routes[j + 1].Distance, direction))
                    {
                        (routes[j], routes[j + 1]) = (routes[j + 1], routes[j]);
                    }
                }
            }
        }
    }
}