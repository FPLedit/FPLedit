namespace FPLedit;

internal sealed class RestartHandler : IRestartable
{
    private readonly Bootstrapper bootstrapper;

    public RestartHandler(Bootstrapper bootstrapper)
    {
        this.bootstrapper = bootstrapper;
    }

    public void RestartWithCurrentFile()
    {
        if (!bootstrapper.FileHandler.NotifyIfUnsaved())
            return;
        if (bootstrapper.FileState.Opened)
            bootstrapper.FullSettings.Set("restart.file", bootstrapper.FileState.FileName!);

        Program.Restart();
    }
}