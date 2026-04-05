namespace ClipboardPro.Models;

/// <summary>
/// Represents a tag for categorizing clippings
/// </summary>
public class Tag
{
    public int Id { get; set; }

    /// <summary>
    /// Tag name (e.g., "important", "code", "link")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tag color (hex code)
    /// </summary>
    public string Color { get; set; } = "#888888";

    /// <summary>
    /// Clippings with this tag
    /// </summary>
    public ICollection<ClippingTag> ClippingTags { get; set; } = new List<ClippingTag>();
}

/// <summary>
/// Join table for many-to-many relationship between Clipping and Tag
/// </summary>
public class ClippingTag
{
    public int ClippingId { get; set; }
    public Clipping Clipping { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
