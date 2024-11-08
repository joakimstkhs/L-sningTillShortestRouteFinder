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
        public ObservableCollection<Route> Routes { get; set; }
        private string? _sortingStatus;
        private ICommand? _sortCommand;
        private ICommand? _loadCommand;
        private ICommand? _visualizeSortCommand;
        private SortType _selectedSortType;
        private SortDirection _selectedSortDirection;
        private bool _isSorting;

        private Route _selectedRoute;
        public Route SelectedRoute
        {
            get => _selectedRoute;
            set
            {
                _selectedRoute = value;
                OnPropertyChanged(nameof(SelectedRoute));
            }
        }

        public MainViewModel()
        {
            Routes = new ObservableCollection<Route>(LoadRoutes());
            _selectedRoute = Routes.FirstOrDefault() ?? new Route();
            _selectedSortType = SortType.QuickSort;
            _selectedSortDirection = SortDirection.Ascending;
            _isSorting = false;
            SortingStatus = "Ready to sort";
        }

        public bool IsSorting
        {
            get => _isSorting;
            private set
            {
                _isSorting = value;
                OnPropertyChanged(nameof(IsSorting));
            }
        }

        public string? SortingStatus
        {
            get => _sortingStatus;
            set
            {
                _sortingStatus = value;
                OnPropertyChanged(nameof(SortingStatus));
            }
        }

        public SortType SelectedSortType
        {
            get => _selectedSortType;
            set
            {
                _selectedSortType = value;
                OnPropertyChanged(nameof(SelectedSortType));
            }
        }

        public SortDirection SelectedSortDirection
        {
            get => _selectedSortDirection;
            set
            {
                _selectedSortDirection = value;
                OnPropertyChanged(nameof(SelectedSortDirection));
            }
        }
        
        public Array SortTypes => Enum.GetValues(typeof(SortType));
        public Array SortDirections => Enum.GetValues(typeof(SortDirection));
        
        public ICommand SortCommand => _sortCommand ??= new RelayCommand(ExecuteSort, () => !IsSorting);
        public ICommand LoadCommand => _loadCommand ??= new RelayCommand(ExecuteLoad, () => !IsSorting);
        public ICommand VisualizeSortCommand => _visualizeSortCommand ??= new RelayCommand(Execute, () => !IsSorting);

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
                MessageBox.Show($"Error reloading routes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SortingStatus = "Error reloading routes";
            }
        }

        private void ExecuteSort()
        {
            try
            {
                IsSorting = true;
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var routesList = Routes.ToList();

                if (SelectedSortType == SortType.BubbleSort)
                {
                    BubbleSort(routesList);
                }
                else
                {
                    QuickSort(routesList, 0, routesList.Count - 1);
                }
                
                watch.Stop();
                
                Routes.Clear();
                foreach (var route in routesList)
                {
                    Routes.Add(route);
                }
                
                SortingStatus = $"Sorted using {SelectedSortType} in {watch.ElapsedMilliseconds} ms";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during sorting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SortingStatus = "Error during sorting";
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
                    await QuickSortWithVisualization(routesList, 0, routesList.Count - 1);
                }
                
                SortingStatus = $"Visualization of {SelectedSortType} completed";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during sort visualization: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SortingStatus = "Error during sort visualization";
            }
            finally
            {
                IsSorting = false;
            }
        }

        private void BubbleSort(List<Route> routes)
        {
            int totalComparisons = routes.Count * (routes.Count - 1) / 2;
            int comparisons = 0;

            for (int i = 0; i < routes.Count - 1; i++)
            {
                for (int j = 0; j < routes.Count - i - 1; j++)
                {
                    comparisons++;
                    if (comparisons % 10 == 0) // Update status every 10 comparisons
                    {
                        SortingStatus = $"Bubble Sort in progress... {(comparisons * 100 / totalComparisons)}%";
                    }

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
                        
                        // Update UI every swap for visualization
                        Routes.Clear();
                        foreach (var route in routes)
                        {
                            Routes.Add(route);
                        }
                        await Task.Delay(100); // Delay to show the swap
                    }
                }
                SortingStatus = $"Bubble Sort: {((i + 1) * 100 / (routes.Count - 1))}% complete";
            }
        }

        private void QuickSort(List<Route> routes, int low, int high)
        {
            while (true)
            {
                if (low >= high) return;
                var p = Partition(routes, low, high);
                QuickSort(routes, low, p - 1);
                low = p + 1;
            }
        }

        private async Task QuickSortWithVisualization(List<Route> routes, int low, int high)
        {
            if (low >= high) return;

            var p = await PartitionWithVisualization(routes, low, high);
            await QuickSortWithVisualization(routes, low, p - 1);
            await QuickSortWithVisualization(routes, p + 1, high);
        }

        private int Partition(List<Route> routes, int low, int high)
        {
            var pivot = routes[high].Distance;
            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (ShouldSwap(routes[j].Distance, pivot)) continue;
                i++;
                (routes[i], routes[j]) = (routes[j], routes[i]);
            }
            
            (routes[i + 1], routes[high]) = (routes[high], routes[i + 1]);
            
            return i + 1;
        }

        private async Task<int> PartitionWithVisualization(List<Route> routes, int low, int high)
        {
            var pivot = routes[high].Distance;
            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (!ShouldSwap(routes[j].Distance, pivot))
                {
                    i++;
                    (routes[i], routes[j]) = (routes[j], routes[i]);
                    
                    // Update UI for visualization
                    Routes.Clear();
                    foreach (var route in routes)
                    {
                        Routes.Add(route);
                    }
                    await Task.Delay(100);
                }
            }
            
            (routes[i + 1], routes[high]) = (routes[high], routes[i + 1]);
            
            // Update UI for visualization
            Routes.Clear();
            foreach (var route in routes)
            {
                Routes.Add(route);
            }
            await Task.Delay(100);
            
            return i + 1;
        }

        private bool ShouldSwap(int a, int b)
        {
            return SelectedSortDirection == SortDirection.Ascending ? a > b : a < b;
        }

        private List<Route> LoadRoutes()
        {
            try
            {
                var json = File.ReadAllText("routes.json");
                return JsonConvert.DeserializeObject<List<Route>>(json) ?? [];
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error loading routes: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}