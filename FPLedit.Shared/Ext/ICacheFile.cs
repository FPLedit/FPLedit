namespace FPLedit.Shared
{
    public interface ICacheFile
    {
        void Set(string key, string? value);

        string? Get(string key);

        bool Any();
    }
}