using SortDirection = ShortestRouteFinder.Sort_Enums.SortDirection;
namespace ShortestRouteFinder.Sorting;

public abstract class SortingMethods
{
    public static bool ShouldSwap(int a, int b, SortDirection direction)
    {
        return direction == SortDirection.Ascending ? a > b : a < b;
    }
}