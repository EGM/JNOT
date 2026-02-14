using System;
using System.IO;

namespace JNOT.Shared.Config.Tests.TestHelpers;

public sealed class TempFile : IDisposable
{
    public string Path { get; }

    public TempFile()
    {
        Path = System.IO.Path.GetTempFileName();
    }

    public void Dispose()
    {
        if (File.Exists(Path))
            File.Delete(Path);
    }
}
