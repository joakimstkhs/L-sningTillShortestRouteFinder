using ShortestRouteFinder.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using System.Windows.Input;

namespace ShortestRouteFinder.ViewModel
{
    public enum SortType
    {
        BubbleSort,
        QuickSort
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Route> Routes { get; private set; }
        private string? _sortingStatus;
        private bool _isSorting;
        private Route _selectedRoute;
        private SortType _selectedSortType = SortType.QuickSort;
        private SortDirection _selectedSortDirection = SortDirection.Ascending;

        public MainViewModel()
        {
            Routes = new ObservableCollection<Route>(LoadRoutes());
            _selectedRoute = Routes.FirstOrDefault() ?? new Route();
            SortingStatus = "Ready to sort";
        }

        public Route SelectedRoute
        {
            get => _selectedRoute;
            set => SetProperty(ref _selectedRoute, value);
        }

        private bool IsSorting
        {
            get => _isSorting;
            set => SetProperty(ref _isSorting, value);
        }

        public string? SortingStatus
        {
            get => _sortingStatus;
            private set => SetProperty(ref _sortingStatus, value);
        }

        public SortType SelectedSortType
        {
            get => _selectedSortType;
            set => SetProperty(ref _selectedSortType, value);
        }

        public SortDirection SelectedSortDirection
        {
            get => _selectedSortDirection;
            set => SetProperty(ref _selectedSortDirection, value);
        }

        public Array SortTypes => Enum.GetValues(typeof(SortType));
        public Array SortDirections => Enum.GetValues(typeof(SortDirection));

        public ICommand SortCommand => new RelayCommand(ExecuteSort, () => !IsSorting);
        public ICommand LoadCommand => new RelayCommand(ExecuteLoad, () => !IsSorting);
        public ICommand VisualizeSortCommand => new RelayCommand(Execute, () => !IsSorting);

        private async void Execute()
        {
            await ExecuteVisualizeSort();
        }

        private void ExecuteLoad()
        {
            try
            {
                var newRoutes = LoadRoutes();
                Routes.Clear();
                foreach (var route in newRoutes)
                {
                    Routes.Add(route);
                }
                SelectedRoute = Routes.FirstOrDefault() ?? new Route();
                SortingStatus = "Routes reloaded successfully";
            }
            catch (Exception ex)
            {
                DisplayError("Error reloading routes", ex);
            }
        }

        private void ExecuteSort()
        {
            try
            {
                IsSorting = true;
                var routesList = Routes.ToList();

                if (SelectedSortType == SortType.BubbleSort)
                {
                    BubbleSort(routesList);
                }
                else
                {
                    QuickSort(routesList, 0, routesList.Count - 1);
                }

                UpdateRoutes(routesList);
                SortingStatus = $"Sorted using {SelectedSortType} in {System.Diagnostics.Stopwatch.StartNew().ElapsedMilliseconds} ms";
            }
            catch (Exception ex)
            {
                DisplayError("Error during sorting", ex);
            }
            finally
            {
                IsSorting = false;
            }
        }

        private async Task ExecuteVisualizeSort()
        {
            try
            {
                IsSorting = true;
                var routesList = Routes.ToList();

                if (SelectedSortType == SortType.BubbleSort)
                {
                    await BubbleSortWithVisualization(routesList);
                }
                else
                {
                    await QuickSortWithVisualization(routesList, 0, routesList.Count - 1, routesList.Count);
                }

                SortingStatus = $"Visualization of {SelectedSortType} completed";
            }
            catch (Exception ex)
            {
                DisplayError("Error during sort visualization", ex);
            }
            finally
            {
                IsSorting = false;
            }
        }

        private void BubbleSort(List<Route> routes)
        {
            for (var i = 0; i < routes.Count - 1; i++)
            {
                for (var j = 0; j < routes.Count - i - 1; j++)
                {
                    if (ShouldSwap(routes[j].Distance, routes[j + 1].Distance))
                    {
                        (routes[j], routes[j + 1]) = (routes[j + 1], routes[j]);
                    }
                }
            }
        }

        private async Task BubbleSortWithVisualization(List<Route> routes)
        {
            for (int i = 0; i < routes.Count - 1; i++)
            {
                for (int j = 0; j < routes.Count - i - 1; j++)
                {
                    if (ShouldSwap(routes[j].Distance, routes[j + 1].Distance))
                    {
                        (routes[j], routes[j + 1]) = (routes[j + 1], routes[j]);
                        UpdateRoutes(routes);
                        await Task.Delay(100);
                    }
                }
                SortingStatus = $"Bubble Sort: {((i + 1) * 100 / (routes.Count - 1))}% complete";
            }
        }

        private void QuickSort(List<Route> routes, int low, int high)
        {
            if (low < high)
            {
                int p = Partition(routes, low, high);
                QuickSort(routes, low, p - 1);
                QuickSort(routes, p + 1, high);
            }
        }

        private async Task QuickSortWithVisualization(List<Route> routes, int low, int high, int totalElements)
        {
            if (low < high)
            {
                int pivotIndex = await PartitionWithVisualization(routes, low, high, totalElements);

                // Calculate percentage progress based on current partition
                int sortedElements = high - low + 1; // Elements sorted in this partition
                int progressPercentage = (sortedElements * 100) / totalElements;
                SortingStatus = $"Quick Sort: {progressPercentage}% complete";

                // Recursively sort the partitions with visualization
                await QuickSortWithVisualization(routes, low, pivotIndex - 1, totalElements);
                await QuickSortWithVisualization(routes, pivotIndex + 1, high, totalElements);
            }
        }

        // Make sure to pass `totalElements` when first calling QuickSortWithVisualization
        public async Task StartQuickSortVisualization()
        {
            var totalElements = Routes.Count;
            await QuickSortWithVisualization(Routes.ToList(), 0, Routes.Count - 1, totalElements);
        }


        private async Task<int> PartitionWithVisualization(List<Route> routes, int low, int high, int totalElements)
        {
            var i = low;
            var pivot = routes[high].Distance;

            for (int j = low; j < high; j++)
            {
                if (ShouldSwap(routes[j].Distance, pivot)) continue;
                (routes[i], routes[j]) = (routes[j], routes[i]);
                UpdateRoutes(routes);
                await Task.Delay(100);
                i++;
            }
            (routes[i], routes[high]) = (routes[high], routes[i]);
            UpdateRoutes(routes);
            await Task.Delay(100);

            return i;
        }


        private int Partition(List<Route> routes, int low, int high)
        {
            var pivot = routes[high].Distance;
            int i = low;

            for (int j = low; j < high; j++)
            {
                if (!ShouldSwap(routes[j].Distance, pivot))
                {
                    (routes[i], routes[j]) = (routes[j], routes[i]);
                    i++;
                }
            }
            (routes[i], routes[high]) = (routes[high], routes[i]);
            return i;
        }

        private async Task<int> PartitionWithVisualization(List<Route> routes, int low, int high)
        {
            int i = low;
            var pivot = routes[high].Distance;

            for (int j = low; j < high; j++)
            {
                if (!ShouldSwap(routes[j].Distance, pivot))
                {
                    (routes[i], routes[j]) = (routes[j], routes[i]);
                    UpdateRoutes(routes);
                    await Task.Delay(100);
                    i++;
                }
            }
            (routes[i], routes[high]) = (routes[high], routes[i]);
            UpdateRoutes(routes);
            await Task.Delay(100);
            return i;
        }

        private bool ShouldSwap(int a, int b) =>
            SelectedSortDirection == SortDirection.Ascending ? a > b : a < b;

        private List<Route> LoadRoutes()
        {
            try
            {
                var json = File.ReadAllText("routes.json");
                return JsonConvert.DeserializeObject<List<Route>>(json) ?? new List<Route>();
            }
            catch (Exception e)
            {
                DisplayError("Error loading routes", e);
                return new List<Route>();
            }
        }

        private void DisplayError(string message, Exception e)
        {
            MessageBox.Show($"{message}: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            SortingStatus = message;
        }

        private void UpdateRoutes(List<Route> routes)
        {
            Routes.Clear();
            foreach (var route in routes)
            {
                Routes.Add(route);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
