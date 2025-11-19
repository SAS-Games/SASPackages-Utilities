using SAS.Utilities.TagSystem;

public class MessagePackFileSaveSystem : FileSaveSystemBase
{
    protected override IDataSerializer Serializer { get; }

    public MessagePackFileSaveSystem(IContextBinder _) : base()
    {
        Serializer = new MessagePackDataSerializer();
    }
}