namespace NexumNovus.AppSettings.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NexumNovus.AppSettings.Common;

/// <summary>
/// Extension methods for adding <see cref="JsonConfigurationProvider"/>.
/// </summary>
public static class JsonConfigurationExtensions
{
  /// <summary>
  /// Adds the Json configuration provider at <paramref name="path"/> to <paramref name="builder"/>.
  /// </summary>
  /// <param name="builder">The <see cref="IHostBuilder"/> to add to.</param>
  /// <param name="path">Path relative to the base path stored in
  /// <see cref="IHostBuilder.Properties"/> of <paramref name="builder"/>).</param>
  /// <param name="optional">Whether the file is optional.</param>
  /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes. Default is true.</param>
  /// <returns>The <see cref="IHostBuilder"/>.</returns>
  public static IHostBuilder AddJsonConfig(this IHostBuilder builder, string path, bool optional = false, bool reloadOnChange = true)
  {
    if (string.IsNullOrEmpty(path))
    {
      throw new ArgumentException("Path to json config file is required.", nameof(path));
    }

    return builder.AddJsonConfig(s =>
    {
      s.Path = path;
      s.Optional = optional;
      s.ReloadOnChange = reloadOnChange;
      s.ResolveFileProvider();
    });
  }

  /// <summary>
  /// Adds a JSON configuration source to <paramref name="builder"/>.
  /// </summary>
  /// <param name="builder">The <see cref="IHostBuilder"/> to add to.</param>
  /// <param name="configureSource">Configures the source.</param>
  /// <returns>The <see cref="IHostBuilder"/>.</returns>
  public static IHostBuilder AddJsonConfig(this IHostBuilder builder, Action<JsonConfigurationSource> configureSource)
  {
    if (builder == null)
    {
      throw new ArgumentNullException(nameof(builder));
    }

    var source = new JsonConfigurationSource();
    configureSource?.Invoke(source);

    builder.ConfigureAppConfiguration((HostBuilderContext _, IConfigurationBuilder cfg) => cfg.Add(source));

    builder.ConfigureServices((HostBuilderContext _, IServiceCollection services) =>
    {
      services.AddSingleton(source);
      services.AddScoped<ISettingsRepository, JsonSettingsRepository>();
    });

    return builder;
  }
}
