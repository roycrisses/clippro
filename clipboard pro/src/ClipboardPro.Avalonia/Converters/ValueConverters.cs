using System.Globalization;
using Avalonia.Data.Converters;
using ClipboardPro.Models;

namespace ClipboardPro.Avalonia.Converters;

public class ClippingTypeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ClippingType type)
        {
            return type switch
            {
                ClippingType.HexCode => "🎨",
                ClippingType.Link => "🔗",
                _ => "📝"
            };
        }
        return "📝";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class TimeAgoConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime timestamp)
        {
            var diff = DateTime.Now - timestamp;
            return diff.TotalMinutes switch
            {
                < 1 => "just now",
                < 60 => $"{(int)diff.TotalMinutes}m ago",
                < 1440 => $"{(int)diff.TotalHours}h ago",
                _ => $"{(int)diff.TotalDays}d ago"
            };
        }
        return "unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter as string == "star")
        {
            return (value is true) ? "⭐" : "☆";
        }
        
        // For Avalonia IsVisible, return bool
        return value is true; 
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
