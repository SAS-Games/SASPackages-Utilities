using SAS.Utilities.TagSystem;

public class UserModel : IUserModel
{
    public UserModel(IContextBinder _) { }
    int IUserModel.GetActiveUserId()
    {
        return 0;
    }

    void IBindable.OnInstanceCreated()
    {
    }
}
