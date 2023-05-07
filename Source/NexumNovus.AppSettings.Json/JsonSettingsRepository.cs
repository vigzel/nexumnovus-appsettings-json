namespace NexumNovus.AppSettings.Json;

using Newtonsoft.Json;
using NexumNovus.AppSettings.Common;
using NexumNovus.AppSettings.Common.Utils;

/// <summary>
/// Used to update top-level settings in appsettings.json file.
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
  /// Adds or updates top-level setting in json settings file.
  ///
  /// Top-level settings are top-most settings:
  /// {
  ///   "topSetting1": "demo",
  ///   "topSetting2": {
  ///     "name": "demo",
  ///     "types": [ "A", "B" ]
  ///   },
  /// }
  ///
  /// You can update "topSetting1" and "topSetting2", but wouldn't be able to update just the "topSetting2:name".
  /// </summary>
  /// <param name="name">Name of the top-level setting.</param>
  /// <param name="settings">New setting object.</param>
  /// <returns>Task.</returns>
  public async Task UpdateSettingsAsync(string name, object settings)
  {
    var path = _source.Path!;

    var appSettingsJson = File.ReadAllText(path);
    var appSettingsDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(appSettingsJson)!;
    if (appSettingsDict.ContainsKey(name))
    {
      appSettingsDict[name] = settings;
    }
    else
    {
      appSettingsDict.Add(name, settings);
    }

    appSettingsJson = AppSettingsParser.SerializeObject(appSettingsDict, _source.Protector);
    File.WriteAllText(path, appSettingsJson);

    if (_source.ReloadOnChange)
    {
      // FileConfigurationSource has a built in delay (default 250 ms) to wait before reloading config file
      // Update should not complete before settings are actually reloaded
      await Task.Delay(_source.ReloadDelay + 50).ConfigureAwait(true);
    }
  }
}
