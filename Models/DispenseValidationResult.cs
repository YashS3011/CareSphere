namespace CareSphere.Models
{
    /// <summary>
    /// DTO for prescription dispensing validation results.
    /// Not a database entity.
    /// </summary>
    public class DispenseValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }
}
