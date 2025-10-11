using SAS.Utilities.TagSystem;

public class JsonFileSaveSystem : FileSaveSystemBase
{
    public JsonFileSaveSystem(IContextBinder _)
    {
        _serializer = new JsonDataSerializer();
    }
}
