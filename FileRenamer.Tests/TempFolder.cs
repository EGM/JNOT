using System;
using System.IO;

public sealed class TempFolder : IDisposable
{
    public string Path { get; }

    public TempFolder()
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "FRPP_" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
        catch
        {
            // swallow cleanup errors
        }
    }
}
