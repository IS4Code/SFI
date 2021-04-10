namespace IS4.MultiArchiver.Services
{
    public interface IPersistentKey
    {
        object ReferenceKey { get; }
        object DataKey { get; }
    }
}
