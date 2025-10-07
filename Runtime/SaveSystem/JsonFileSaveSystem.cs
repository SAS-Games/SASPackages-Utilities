using SAS.Utilities.TagSystem;

public class JsonFileSaveSystem : FileSaveSystemBase
{
    protected JsonFileSaveSystem(IContextBinder _)
    {
        _serializer = new JsonDataSerializer();
    }
}
