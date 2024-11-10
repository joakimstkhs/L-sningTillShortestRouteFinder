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
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public ObservableCollection<Route> Routes { get; } = new();
        public Array SortTypes => Enum.GetValues(typeof(SortType));
        public Array SortDirections => Enum.GetValues(typeof(SortDirection));
        public ICommand SortCommand { get; }
        public ICommand LoadCommand { get; }

        private SortType SortType { get; set; } = SortType.QuickSort;
        public SortDirection SortDirection { get; set; }
        
        public Route SelectedRoute
        {
            get => _selectedRoute;
            set => Update(ref _selectedRoute, value);
        }

        public string? SortingStatus
        {
            get => _sortingStatus;
            private set => Update(ref _sortingStatus, value);
        }

        public MainViewModel()
        {
            SortCommand = new RelayCommand(Sort);
            LoadCommand = new RelayCommand(Load);
            Load();
        }

        private void Sort() => Try(() =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var list = Routes.ToList();
            
            if (SortType == SortType.QuickSort)
                Sort(list, 0, list.Count - 1);
            else
                for (var i = 0; i < list.Count - 1; i++)
                    for (var j = 0; j < list.Count - i - 1; j++)
                        if ((SortDirection == SortDirection.Ascending) == (list[j].Distance > list[j + 1].Distance))
                            (list[j], list[j + 1]) = (list[j + 1], list[j]);
            
            Routes.Clear();
            list.ForEach(Routes.Add);
            SortingStatus = $"Done in {sw.ElapsedMilliseconds}ms";
        });

        private void Sort(List<Route> r, int l, int h)
        {
            if (l >= h) return;
            var p = r[h].Distance;
            var i = l - 1;

            for (var j = l; j < h; j++)
                if ((SortDirection == SortDirection.Ascending) == (r[j].Distance <= p))
                    (r[++i], r[j]) = (r[j], r[i]);

            (r[++i], r[h]) = (r[h], r[i]);
            Sort(r, l, i - 1);
            Sort(r, i + 1, h);
        }

        private void Load() => Try(() =>
        {
            Routes.Clear();
            JsonConvert.DeserializeObject<List<Route>>(
                File.ReadAllText("routes.json"))?.ForEach(Routes.Add);
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