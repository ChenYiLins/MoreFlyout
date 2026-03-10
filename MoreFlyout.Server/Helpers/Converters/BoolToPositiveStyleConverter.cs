using Microsoft.UI.Xaml.Data;
 
namespace MoreFlyout.Server.Helpers.Converters;

public partial class BoolToPositiveStyleConverter : IValueConverter
{
    public Style? PositiveStyle { get; set; }
    public Style? NegativeStyle { get; set; }

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            return b ? PositiveStyle : NegativeStyle;
        }
        return NegativeStyle;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
