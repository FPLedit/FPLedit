namespace FPLedit.Shared
{
    public interface IFileState
    {
        bool Opened { get; }
        bool Saved { get; }

        bool LineCreated { get; }
        bool TrainsCreated { get; }

        bool CanGoBack { get; }

        string FileName { get; }
        int SelectedRoute { get; set; }
    }
}