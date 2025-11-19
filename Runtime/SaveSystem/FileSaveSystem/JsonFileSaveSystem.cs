using SAS.Utilities.TagSystem;

public class JsonFileSaveSystem : FileSaveSystemBase
{
    protected override IDataSerializer Serializer { get; }
    public JsonFileSaveSystem(IContextBinder _) : base()
    {
        Serializer = new JsonDataSerializer();
    }
}