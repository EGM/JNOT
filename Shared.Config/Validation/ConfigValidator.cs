using Jnot.Shared.Config.Models;

namespace Jnot.Shared.Config.Validation
{
    public static class ConfigValidator
    {
        public static ConfigValidationResult Validate(RootConfig config)
        {
            var result = new ConfigValidationResult();

            if (string.IsNullOrWhiteSpace(config.Title))
                result.Errors.Add("Title must not be empty.");

            if (config.FileRenamer is null)
            {
                result.Errors.Add("FileRenamer section is missing.");
            }
            else
            {
                ValidateFileRenamer(config.FileRenamer, result);
            }

            return result;
        }

        private static void ValidateFileRenamer(FileRenamerConfig cfg, ConfigValidationResult result)
        {
            // These can be relaxed later if you want optional folders
            if (string.IsNullOrWhiteSpace(cfg.InputFolder))
                result.Errors.Add("[FileRenamer] InputFolder must not be empty.");

            if (string.IsNullOrWhiteSpace(cfg.OutputFolder))
                result.Errors.Add("[FileRenamer] OutputFolder must not be empty.");
        }
    }
}
