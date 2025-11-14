using SAS.Utilities.TagSystem;

public class MessagePackFileSaveSystem : FileSaveSystemBase
{
    public MessagePackFileSaveSystem(IContextBinder _) : base(new MessagePackDataSerializer())
    {
    }
}