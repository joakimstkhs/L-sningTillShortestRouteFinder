using ShortestRouteFinder.Enumerators;
using ShortestRouteFinder.Model;
using ShortestRouteFinder.ViewModel;

namespace ShortestRouteFinder.Services
{
    public class SortService
    {
        public void BubbleSort(List<Route?> list, SortDirection direction)
        {
            for (var i = 0; i < list.Count - 1; i++)
            {
                for (var j = 0; j < list.Count - i - 1; j++)
                {
                    var shouldSwap = direction == SortDirection.Ascending
                        ? list[j].Distance > list[j + 1].Distance
                        : list[j].Distance < list[j + 1].Distance;

                    if (shouldSwap)
                    {
                        (list[j], list[j + 1]) = (list[j + 1], list[j]);
                    }
                }
            }
        }

        public void QuickSort(List<Route?> routes, int low, int high, SortDirection direction)
        {
            while (true)
            {
                if (low >= high) return;

                var pivotIndex = Partition(routes, low, high, direction);
                QuickSort(routes, low, pivotIndex - 1, direction);
                low = pivotIndex + 1;
            }
        }

        private int Partition(List<Route?> routes, int low, int high, SortDirection direction)
        {
            var pivot = routes[high].Distance;
            var i = low - 1;

            for (var j = low; j < high; j++)
            {
                var shouldSwap = direction == SortDirection.Ascending
                    ? routes[j].Distance <= pivot
                    : routes[j].Distance >= pivot;

                if (!shouldSwap) continue;
                i++;
                (routes[i], routes[j]) = (routes[j], routes[i]);
            }

            (routes[i + 1], routes[high]) = (routes[high], routes[i + 1]);
            return i + 1;
        }
    }
}