using Microsoft.UI.Xaml.Data;

namespace MoreFlyout.App.Helpers.Converters;

public partial class VisibilityNegationConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if ((value is Visibility visibilityValue) && visibilityValue == Visibility.Visible)
        {
            return Visibility.Collapsed;
        }
        else
        {
            return Visibility.Visible;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if ((value is Visibility visibilityValue) && visibilityValue == Visibility.Visible)
        {
            return Visibility.Collapsed;
        }
        else
        {
            return Visibility.Visible;
        }
    }
}
