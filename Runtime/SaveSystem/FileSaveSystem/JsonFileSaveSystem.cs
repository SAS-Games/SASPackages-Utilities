using SAS.Utilities.TagSystem;

public class JsonFileSaveSystem : FileSaveSystemBase
{
    public JsonFileSaveSystem(IContextBinder _) : base(new JsonDataSerializer())
    {
    }
}