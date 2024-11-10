using ShortestRouteFinder.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace ShortestRouteFinder.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string? _sortingStatus = "Ready";
        private Route _selectedRoute = new();
        private SortType _selectedSortType = SortType.QuickSort;
        private SortDirection _selectedSortDirection = SortDirection.Ascending;
        private bool _isEditing = false;
        private Route? _routeBeforeEdit = null;
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public ObservableCollection<Route> Routes { get; } = new();
        public Array SortTypes => Enum.GetValues(typeof(SortType));
        public Array SortDirections => Enum.GetValues(typeof(SortDirection));
        
        public ICommand SortCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddNewCommand { get; }

        public bool IsEditing
        {
            get => _isEditing;
            private set => Update(ref _isEditing, value);
        }

        public SortType SelectedSortType
        {
            get => _selectedSortType;
            set => Update(ref _selectedSortType, value);
        }

        public SortDirection SelectedSortDirection
        {
            get => _selectedSortDirection;
            set => Update(ref _selectedSortDirection, value);
        }
        
        public Route SelectedRoute
        {
            get => _selectedRoute;
            set
            {
                if (_isEditing) return; // Prevent selection change while editing
                if (value != null)
                {
                    _selectedRoute = value.Clone(); // Create a clone for editing
                    _routeBeforeEdit = value;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedRoute)));
            }
        }

        public string? SortingStatus
        {
            get => _sortingStatus;
            private set => Update(ref _sortingStatus, value);
        }

        public MainViewModel()
        {
            SortCommand = new RelayCommand(Sort, () => !IsEditing);
            LoadCommand = new RelayCommand(Load, () => !IsEditing);
            SaveCommand = new RelayCommand(Save, () => IsEditing);
            CancelCommand = new RelayCommand(Cancel, () => IsEditing);
            AddNewCommand = new RelayCommand(AddNew, () => !IsEditing);
            Load();
        }

        private void AddNew()
        {
            var newRoute = new Route { Start = "New Start", Destination = "New Destination", Distance = 0 };
            Routes.Add(newRoute);
            SelectedRoute = newRoute;
            IsEditing = true;
        }

        private void Save()
        {
            Try(() =>
            {
                if (_routeBeforeEdit != null)
                {
                    var index = Routes.IndexOf(_routeBeforeEdit);
                    if (index != -1)
                    {
                        Routes[index] = _selectedRoute;
                    }
                }
                
                // Save to file
                File.WriteAllText("routes.json", JsonConvert.SerializeObject(Routes, Formatting.Indented));
                
                IsEditing = false;
                _routeBeforeEdit = null;
                SortingStatus = "Changes saved successfully";
            });
        }

        private void Cancel()
        {
            if (_routeBeforeEdit != null)
            {
                SelectedRoute = _routeBeforeEdit;
                _routeBeforeEdit = null;
            }
            IsEditing = false;
            SortingStatus = "Edit cancelled";
        }

        private void Sort() => Try(() =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var list = Routes.ToList();
            
            if (SelectedSortType == SortType.QuickSort)
                Sort(list, 0, list.Count - 1);
            else
                BubbleSort(list);
            
            Routes.Clear();
            list.ForEach(Routes.Add);
            SortingStatus = $"Done in {sw.ElapsedMilliseconds}ms";
        });

        private void BubbleSort(List<Route> list)
        {
            for (var i = 0; i < list.Count - 1; i++)
            {
                for (var j = 0; j < list.Count - i - 1; j++)
                {
                    var shouldSwap = SelectedSortDirection == SortDirection.Ascending
                        ? list[j].Distance > list[j + 1].Distance
                        : list[j].Distance < list[j + 1].Distance;

                    if (shouldSwap)
                    {
                        (list[j], list[j + 1]) = (list[j + 1], list[j]);
                    }
                }
            }
        }

        private void Sort(List<Route> r, int l, int h)
        {
            if (l >= h) return;
            
            var pivotIndex = Partition(r, l, h);
            Sort(r, l, pivotIndex - 1);
            Sort(r, pivotIndex + 1, h);
        }

        private int Partition(List<Route> r, int l, int h)
        {
            var pivot = r[h].Distance;
            var i = l - 1;

            for (var j = l; j < h; j++)
            {
                var shouldSwap = SelectedSortDirection == SortDirection.Ascending
                    ? r[j].Distance <= pivot
                    : r[j].Distance >= pivot;

                if (shouldSwap)
                {
                    i++;
                    (r[i], r[j]) = (r[j], r[i]);
                }
            }

            (r[i + 1], r[h]) = (r[h], r[i + 1]);
            return i + 1;
        }

        private void Load() => Try(() =>
        {
            Routes.Clear();
            if (File.Exists("routes.json"))
            {
                JsonConvert.DeserializeObject<List<Route>>(
                    File.ReadAllText("routes.json"))?.ForEach(Routes.Add);
            }
            SelectedRoute = Routes.FirstOrDefault() ?? new();
            SortingStatus = "Loaded";
        });

        private void Try(Action a)
        {
            try { a(); }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SortingStatus = "Failed";
            }
        }

        private void Update<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? p = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        }
    }

    public enum SortType { BubbleSort, QuickSort }
    public enum SortDirection { Ascending, Descending }
}