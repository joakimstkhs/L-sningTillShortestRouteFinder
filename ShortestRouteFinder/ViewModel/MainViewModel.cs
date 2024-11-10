using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ShortestRouteFinder.Model;

namespace ShortestRouteFinder.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly SortService _sortService;
        private readonly RouteRepository _repository;
        private string? _sortingStatus = "Ready";
        private Route _selectedRoute = new();
        private SortType _selectedSortType = SortType.QuickSort;
        private SortDirection _selectedSortDirection = SortDirection.Ascending;
        private bool _isEditing;
        private Route? _routeBeforeEdit;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Route> Routes { get; } = new();
        public Array SortTypes => Enum.GetValues(typeof(SortType));
        public Array SortDirections => Enum.GetValues(typeof(SortDirection));

        public ICommand SortCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddNewCommand { get; }
        public ICommand RemoveCommand { get; }

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
                if (_isEditing) return;
                if (value != null)
                {
                    _selectedRoute = value.Clone();
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
            _sortService = new SortService();
            _repository = new RouteRepository();

            SortCommand = new RelayCommand(Sort, () => !IsEditing);
            LoadCommand = new RelayCommand(Load, () => !IsEditing);
            SaveCommand = new RelayCommand(Save, () => IsEditing);
            CancelCommand = new RelayCommand(Cancel, () => IsEditing);
            AddNewCommand = new RelayCommand(AddNew, () => !IsEditing);
            RemoveCommand = new RelayCommand(RemoveRoute, () => !IsEditing && SelectedRoute != null);
            
            Load();
        }

        private void RemoveRoute()
        {
            if (SelectedRoute == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to remove the route from {SelectedRoute.Start} to {SelectedRoute.Destination}?",
                "Confirm Removal",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Try(() =>
                {
                    Routes.Remove(_routeBeforeEdit!);
                    _repository.SaveRoutes(Routes);
                    SelectedRoute = Routes.FirstOrDefault() ?? new();
                    SortingStatus = "Route removed successfully";
                });
            }
        }

        private void AddNew()
        {
            var newRoute = new Route { Start = "New Start", Destination = "New Destination", Distance = 0 };
            Routes.Add(newRoute);
            SelectedRoute = newRoute;
            IsEditing = true;
            _routeBeforeEdit = newRoute;
            SortingStatus = "Adding new route";
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

                _repository.SaveRoutes(Routes);
                
                IsEditing = false;
                _routeBeforeEdit = null;
                SortingStatus = "Changes saved successfully";
            });
        }

        private void Cancel()
        {
            if (_routeBeforeEdit != null)
            {
                if (Routes.Contains(_routeBeforeEdit) && _routeBeforeEdit.Start == "New Start" 
                    && _routeBeforeEdit.Destination == "New Destination" && _routeBeforeEdit.Distance == 0)
                {
                    Routes.Remove(_routeBeforeEdit);
                }
                SelectedRoute = _routeBeforeEdit;
                _routeBeforeEdit = null;
            }
            IsEditing = false;
            SortingStatus = "Edit cancelled";
        }

        private void Sort()
        {
            Try(() =>
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var list = Routes.ToList();

                if (SelectedSortType == SortType.QuickSort)
                    _sortService.QuickSort(list, 0, list.Count - 1, SelectedSortDirection);
                else
                    _sortService.BubbleSort(list, SelectedSortDirection);

                Routes.Clear();
                list.ForEach(Routes.Add);
                SortingStatus = $"Done in {sw.ElapsedMilliseconds}ms";
            });
        }

        private void Load()
        {
            Try(() =>
            {
                Routes.Clear();
                var routes = _repository.LoadRoutes();
                routes.ForEach(Routes.Add);
                SelectedRoute = Routes.FirstOrDefault() ?? new();
                SortingStatus = "Loaded";
            });
        }

        private void Try(Action action)
        {
            try 
            { 
                action(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SortingStatus = "Failed";
            }
        }

        private void Update<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}