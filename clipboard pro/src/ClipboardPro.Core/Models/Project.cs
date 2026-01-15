namespace ClipboardPro.Models;

/// <summary>
/// Represents a project for organizing clippings
/// </summary>
public class Project
{
    public int Id { get; set; }
    
    /// <summary>
    /// Project name (e.g., "Project Alpha")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Accent color for the project (hex code)
    /// </summary>
    public string Color { get; set; } = "#FF7A00";
    
    /// <summary>
    /// When true, all new clippings are auto-tagged to this project
    /// </summary>
    public bool IsLocked { get; set; }
    
    /// <summary>
    /// When the project was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// All clippings associated with this project
    /// </summary>
    public ICollection<Clipping> Clippings { get; set; } = new List<Clipping>();
}
