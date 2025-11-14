using SAS.Utilities.TagSystem;

public class BinaryFileSaveSystem : FileSaveSystemBase
{
    public BinaryFileSaveSystem(IContextBinder _) : base(new BinaryDataSerializer())
    {
    }
}