using SAS.Utilities.TagSystem;

public class BinaryFileSaveSystem : FileSaveSystemBase
{
    protected override IDataSerializer Serializer { get; }

    public BinaryFileSaveSystem(IContextBinder _) : base()
    {
        Serializer = new BinaryDataSerializer();
    }
}