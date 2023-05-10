namespace NexumNovus.AppSettings.Json;

using System.IO;
using NexumNovus.AppSettings.Common;
using NexumNovus.AppSettings.Common.Secure;
using NexumNovus.AppSettings.Common.Utils;

/// <summary>
/// Used to update settings in appsettings.json file.
/// </summary>
public class JsonSettingsRepository : ISettingsRepository
{
  private readonly JsonConfigurationSource _source;

  /// <summary>
  /// Initializes a new instance of the <see cref="JsonSettingsRepository"/> class.
  /// </summary>
  /// <param name="source">The source settings.</param>
  public JsonSettingsRepository(JsonConfigurationSource source) => _source = source;

  /// <summary>
  /// Adds or updates setting in json settings file.
  /// </summary>
  /// <param name="name">Name or section of the setting.</param>
  /// <param name="settings">New setting object.</param>
  /// <returns>Task.</returns>
  public async Task UpdateSettingsAsync(string name, object settings)
  {
    var newSettings = AppSettingsParser.Flatten(settings, name, SecretAttributeAction.MarkWithStarAndProtect, _source.Protector);
    var settingsData = AppSettingsParser.Parse(_source.Path!);

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

    var newJson = AppSettingsParser.ConvertSettingsDictionaryToJson(settingsData);
    File.WriteAllText(_source.Path!, newJson);

    if (_source.ReloadOnChange)
    {
      // FileConfigurationSource has a built in delay (default 250 ms) to wait before reloading config file
      // Update should not complete before settings are actually reloaded
      await Task.Delay(_source.ReloadDelay + 50).ConfigureAwait(true);
    }
  }
}
