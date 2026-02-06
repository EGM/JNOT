using JNOT.Shared.Config.IO;
using JNOT.Shared.Config.Models;
using JNOT.Shared.Config.Validation;
using System;

namespace JNOT.Shared.Config.Services;

public class ConfigService : IConfigService
{
    private readonly ConfigLoader _loader;
    private readonly ConfigWriter _writer;
    private readonly string _path;

    public ConfigService(ConfigLoader loader, ConfigWriter writer)
    {
        _loader = loader;
        _writer = writer;
        _path = ConfigFileLocator.GetConfigPath();
    }

    public RootConfig Load()
    {
        return _loader.LoadOrCreate(_path);
    }

    public RootConfig LoadOrCreate()
    {
        return _loader.LoadOrCreate(_path);
    }

    public void Save(RootConfig config)
    {
        var validation = ConfigValidator.Validate(config);
        if (!validation.IsValid)
        {
            var message = string.Join(Environment.NewLine, validation.Errors);
            throw new InvalidOperationException($"Config is invalid:{Environment.NewLine}{message}");
        }

        _writer.Save(_path, config);
    }
}