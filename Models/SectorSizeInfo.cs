public class SectorSizeInfo
{
    public string DriveLetter { get; set; }
    public int PhysicalBytesPerSectorForAtomicity { get; set; }
    public int PhysicalBytesPerSectorForPerformance { get; set; }
    public int FileSystemEffectivePhysicalBytes { get; set; }
    public bool HasIssue { get; set; }
    public string RawOutput { get; set; }
    public string ErrorMessage { get; set; }
    
    public SectorSizeInfo()
    {
        PhysicalBytesPerSectorForAtomicity = 0;
        PhysicalBytesPerSectorForPerformance = 0;
        FileSystemEffectivePhysicalBytes = 0;
        HasIssue = false;
    }
}