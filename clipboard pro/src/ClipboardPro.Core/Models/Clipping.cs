namespace ClipboardPro.Models;

/// <summary>
/// Represents the type of clipboard content
/// </summary>
public enum ClippingType
{
    Text = 0,
    HexCode = 1,
    Link = 2
}

/// <summary>
/// Represents a single clipboard entry
/// </summary>
public class Clipping
{
    public int Id { get; set; }
    
    /// <summary>
    /// The actual clipboard content
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// A truncated preview for display (max 200 chars)
    /// </summary>
    public string Preview { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of content (Text, HexCode, Link)
    /// </summary>
    public ClippingType Type { get; set; } = ClippingType.Text;
    
    /// <summary>
    /// Name of the source application (e.g., "Chrome", "Figma")
    /// </summary>
    public string SourceApp { get; set; } = string.Empty;
    
    /// <summary>
    /// When the content was copied
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Content hash for duplicate detection
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this clipping is marked as favorite
    /// </summary>
    public bool IsFavorite { get; set; }
    
    /// <summary>
    /// Associated project ID (null if not in a project)
    /// </summary>
    public int? ProjectId { get; set; }
    
    /// <summary>
    /// Navigation property to project
    /// </summary>
    public Project? Project { get; set; }
    
    /// <summary>
    /// Tags associated with this clipping
    /// </summary>
    public ICollection<ClippingTag> ClippingTags { get; set; } = new List<ClippingTag>();
}
