namespace NexumNovus.AppSettings.Json;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using NexumNovus.AppSettings.Common.Secure;
using NexumNovus.AppSettings.Common.Utils;

/// <summary>
/// A JSON file based <see cref="FileConfigurationProvider"/>.
/// </summary>
public class JsonConfigurationProvider : FileConfigurationProvider
{
  private readonly JsonConfigurationSource _config;

  /// <summary>
  /// Initializes a new instance of the <see cref="JsonConfigurationProvider"/> class.
  /// </summary>
  /// <param name="source">The source settings.</param>
  public JsonConfigurationProvider(JsonConfigurationSource source)
    : base(source) => _config = source;

  /// <summary>
  /// Loads the JSON data from a stream.
  ///
  /// Properties with suffix '*' are assumed to be encrypted. These properties will be decrypted and suffix '*' will be removed from their name.
  /// </summary>
  /// <param name="stream">The stream to read.</param>
  public override void Load(Stream stream)
  {
    try
    {
      var settings = AppSettingsParser.Parse(stream);

      var encryptedSettings = settings.Keys.Where(x => x.EndsWith("*")).ToList();

      foreach (var key in encryptedSettings)
      {
        var newKey = key[..^1];
        var newValue = UnprotectSafe(key, settings[key]);

        settings.Remove(key);
        if (settings.ContainsKey(newKey))
        {
          settings[newKey] = newValue;
        }
        else
        {
          settings.Add(newKey, newValue);
        }
      }

      Data = settings;
    }
    catch (CryptographicException)
    {
      throw; // CryptographicException was hadled inside UnprotectSafe, if we see it here just rethrow it
    }
    catch (Exception e)
    {
      HandleException(ExceptionDispatchInfo.Capture(new FormatException("Failed to parse secure json.", e)));
    }
  }

  private string? UnprotectSafe(string settingName, string? settingValue)
  {
    if (string.IsNullOrWhiteSpace(settingValue))
    {
      return settingValue;
    }

    try
    {
      var secretProtector = _config.Protector ?? DefaultSecretProtector.Instance;
      return secretProtector.Unprotect(settingValue);
    }
    catch (CryptographicException ex)
    {
      HandleException(ExceptionDispatchInfo.Capture(new CryptographicException($"Failed to decrypt value \"{settingValue}\" for \"{settingName}\".", ex)));
      return null; // if we get here, HandleException ignored the error
    }
  }

  private void HandleException(ExceptionDispatchInfo info)
  {
    var ignoreException = false;
    if (Source.OnLoadException != null)
    {
      var exceptionContext = new FileLoadExceptionContext
      {
        Provider = this,
        Exception = info.SourceException,
      };
      Source.OnLoadException.Invoke(exceptionContext);
      ignoreException = exceptionContext.Ignore;
    }

    if (!ignoreException)
    {
      info.Throw();
    }
  }
}
