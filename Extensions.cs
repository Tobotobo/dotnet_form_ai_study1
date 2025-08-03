static class Extensions
{
    public static Grid ColumnDefinitions(this Grid target, ColumnDefinitions columnDefinitions)
    {
        target.ColumnDefinitions = columnDefinitions;
        return target;
    }

    public static Grid RowDefinitions(this Grid target, RowDefinitions rowDefinitions)
    {
        target.RowDefinitions = rowDefinitions;
        return target;
    }
}
