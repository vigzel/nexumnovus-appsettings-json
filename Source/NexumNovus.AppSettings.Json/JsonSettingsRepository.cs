namespace NexumNovus.AppSettings.Json;

using System.IO;
using NexumNovus.AppSettings.Common;
using NexumNovus.AppSettings.Common.Secure;
using NexumNovus.AppSettings.Common.Utils;

/// <summary>
/// Used to update settings in appsettings.json file.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="JsonSettingsRepository"/> class.
/// </remarks>
/// <param name="source">The source settings.</param>
public class JsonSettingsRepository(JsonConfigurationSource source) : ISettingsRepository
{
  private readonly JsonConfigurationSource _source = source;

  /// <summary>
  /// Adds or updates setting in json settings file.
  /// </summary>
  /// <param name="name">Name or section of the setting.</param>
  /// <param name="settings">New setting object.</param>
  /// <returns>Task.</returns>
  public async Task UpdateSettingsAsync(string name, object settings)
  {
    var file = _source.FileProvider?.GetFileInfo(_source.Path ?? string.Empty);
    var filePath = file?.PhysicalPath ?? _source.Path;
    if (string.IsNullOrWhiteSpace(filePath))
    {
      throw new FileNotFoundException("Undefined settings file path!");
    }

    var newSettings = AppSettingsParser.Flatten(settings, name, SecretAttributeAction.MarkWithStarAndProtect, _source.Protector);
    IDictionary<string, string?> settingsData;
    if (File.Exists(filePath))
    {
      settingsData = AppSettingsParser.Parse(filePath);

      var keysToRemove = settingsData
        .Where(x => x.Key.StartsWith($"{name}:") || x.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
        .Select(x => x.Key);

      foreach (var key in keysToRemove)
      {
        settingsData.Remove(key);
      }

      foreach (var (key, value) in newSettings)
      {
        settingsData.Add(key, value);
      }
    }
    else
    {
      settingsData = newSettings;
    }

    var newJson = AppSettingsParser.ConvertSettingsDictionaryToJson(settingsData);
    File.WriteAllText(filePath, newJson);

    if (_source.ReloadOnChange)
    {
      // FileConfigurationSource has a built in delay (default 250 ms) to wait before reloading config file
      // Update should not complete before settings are actually reloaded
      await Task.Delay(_source.ReloadDelay + 50).ConfigureAwait(true);
    }
  }
}
