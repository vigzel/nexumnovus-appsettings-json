namespace NexumNovus.AppSettings.Json;
using Microsoft.Extensions.Configuration;
using NexumNovus.AppSettings.Common.Secure;

/// <summary>
/// Represents a JSON file as an <see cref="IConfigurationSource"/>.
/// </summary>
public class JsonConfigurationSource : FileConfigurationSource
{
  /// <summary>
  /// Gets or sets a protector used to cryptographically protect/unprotect a piece of plaintext data.
  /// </summary>
  public ISecretProtector? Protector { get; set; }

  /// <summary>
  /// Builds the <see cref="JsonConfigurationProvider"/> for this source.
  /// </summary>
  /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
  /// <returns>An <see cref="JsonConfigurationProvider"/>.</returns>
  public override IConfigurationProvider Build(IConfigurationBuilder builder)
  {
    EnsureDefaults(builder);
    Protector ??= DefaultSecretProtector.Instance;
    return new JsonConfigurationProvider(this);
  }
}
