using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ShortestRouteFinder.Model;
using ShortestRouteFinder.Services;
using ShortestRouteFinder.Sort_Enums;
using ShortestRouteFinder.Sorting;
using System.Linq;
using System.Windows.Input;

namespace ShortestRouteFinder.ViewModel;

public sealed class MainViewModel : INotifyPropertyChanged
{
   private readonly IRouteService _routeService;
   private readonly Dictionary<SortType, ISorter> _sorters;
   
   public ObservableCollection<Route> Routes { get; private set; }

   private string? _sortingStatus;
   private SortType _selectedSortType;
   private SortDirection _selectedSortDirection;

   public MainViewModel(IRouteService routeService)
   {
      _routeService = routeService;

      _sorters = new Dictionary<SortType, ISorter>
      {
         { SortType.BubbleSort, new BubbleSort() },
         { SortType.QuickSort, new QuickSort() }
      };
      
      Routes = new ObservableCollection<Route>(_routeService.LoadRoutes());
      
      SelectedSortType = SortType.QuickSort;
      SelectedSortDirection = SortDirection.Ascending;
      SortingStatus = "Ready to sort.";
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

   public ICommand SortCommand => Command.Create(ExecuteSort, () => Routes.Count > 1);
   public ICommand LoadCommand => Command.Create(ExecuteLoad, () => Routes.Count > 1);

   private void ExecuteLoad()
   {
      try
      {
         var newRoutes = _routeService.LoadRoutes();
         Routes.Clear();
         foreach (var route in newRoutes)
         {
            Routes.Add(route);
         }

         SortingStatus = "Routes reloaded successfully.";
      }
      catch (Exception ex)
      {
         SortingStatus = $"Error reloading routes: {ex.Message}";
      }
   }

   private void ExecuteSort()
   {
      try
      {
         if (_sorters.TryGetValue(SelectedSortType, out var sorter))
         {
            var routesList = Routes.ToList();

            sorter.Sort(routesList, SelectedSortDirection);

            Routes.Clear();
            foreach (var route in routesList)
            {
               Routes.Add(route);
            }

            SortingStatus = $"Sorted using {SelectedSortType} in {SelectedSortDirection} order.";
         }
         else
         {
            SortingStatus = $"Selected sort type not available";
         }
      }
      catch (Exception ex)
      {
         SortingStatus = $"Error sorting routes: {ex.Message}";
      }
   }
   
   public event PropertyChangedEventHandler? PropertyChanged;
   private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

