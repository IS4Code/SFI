namespace IS4.MultiArchiver.Services
{
    public interface IFormatObject<out T>
    {
        string Extension { get; }
        string MediaType { get; }
        T Value { get; }
    }
}
