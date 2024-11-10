using ShortestRouteFinder.Model;
using static ShortestRouteFinder.Sorting.SortingMethods;
using SortDirection = ShortestRouteFinder.Sort_Enums.SortDirection;

namespace ShortestRouteFinder.Sorting
{
    public class QuickSort : ISorter
    {
        public void Sort(List<Route> routes, SortDirection direction)
        {
            QuickSortAlgo(routes, 0, routes.Count - 1, direction);
        }

        private void QuickSortAlgo(List<Route> routes, int low, int high, SortDirection direction)
        {
            while (true)
            {
                if (low >= high) return;
                var p = Partition(routes, low, high, direction);
                QuickSortAlgo(routes, low, p - 1, direction);
                low = p + 1;
            }
        }

        private int Partition(List<Route> routes, int low, int high, SortDirection direction)
        {
            var pivot = routes[high].Distance;
            var i = low - 1;

            for (var j = low; j < high; j++)
            {
                if (ShouldSwap(routes[j].Distance, pivot, direction)) continue;
                i++;
                (routes[i], routes[j]) = (routes[j], routes[i]);
            }

            (routes[i + 1], routes[high]) = (routes[high], routes[i + 1]);
            return i + 1;
        }
    }
}