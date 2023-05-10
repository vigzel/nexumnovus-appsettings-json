![Banner](Images/Banner.png)

# NexumNovus.AppSettings.Json

[![NexumNovus.AppSettings.Json NuGet Package](https://img.shields.io/nuget/v/NexumNovus.AppSettings.Json.svg)](https://www.nuget.org/packages/NexumNovus.AppSettings.Json/) [![NexumNovus.AppSettings.Json NuGet Package Downloads](https://img.shields.io/nuget/dt/NexumNovus.AppSettings.Json)](https://www.nuget.org/packages/NexumNovus.AppSettings.Json) [![GitHub Actions Status](https://github.com/vigzel/nexumnovus-appsettings-json/workflows/Build/badge.svg?branch=main)](https://github.com/vigzel/nexumnovus-appsettings-json/actions)

[![GitHub Actions Build History](https://buildstats.info/github/chart/vigzel/nexumnovus-appsettings-json?branch=main&includeBuildsFromPullRequest=false)](https://github.com/vigzel/nexumnovus-appsettings-json/actions)


### About

JSON configuration provider implementation for [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration/). This package enables you to 
 - read application's settings from a JSON file. 
 - update application's settings and save changes to a JSON file. 
 - cryptographically protect settings

Use `JsonConfigurationExtensions.AddJsonConfig` extension method on `IHostBuilder` to add the JSON configuration provider to the configuration builder and register `JsonSettingsRepository` with service collection.

Use `ISettingsRepository.UpdateSettingsAsync` to update JSON settings file.

Mark properties with `SecretSetting` attribute to cryptographically protect them.

### Example

```cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NexumNovus.AppSettings.Common;
using NexumNovus.AppSettings.Common.Secure;
using NexumNovus.AppSettings.Json;

var builder = WebApplication.CreateBuilder(args);

// Load settings from json file and register settings repository with service collection
builder.Host.AddJsonConfig("appsettings.json");

// Use of options pattern to register configuration elements is optional.
builder.Services.AddOptions<EmailSettings>().BindConfiguration(EmailSettings.ConfigElement);

var app = builder.Build();

// Api's to get and update EmailSettings
app.MapGet("/emailSettings", (IOptionsMonitor<EmailSettings> emailSettings) => emailSettings.CurrentValue);
app.MapPost("/emailSettings", async (EmailSettings emailSettings, ISettingsRepository settingsRepo)
  => await settingsRepo.UpdateSettingsAsync(EmailSettings.ConfigElement, emailSettings)
);

// Api's to get and update setting
app.MapGet("/settings", (string section, IConfiguration settings) => settings.GetSection(section));
app.MapPost("/settings", async (string section, string value, ISettingsRepository settingsRepo)
  => await settingsRepo.UpdateSettingsAsync(section, value)
);

app.Run();

public record EmailSettings
{
  public static string ConfigElement = "Email";
  public string Host { get; set; }
  public string Username { get; set; }
  [SecretSetting]
  public string Password { get; set; } //this setting will be cryptographically protected
} 
```

To run this example, include an `appsettings.json` file with the following content in your project:

```json
{
  "Region": "demo",
  "Email": {
    "Host": "example.com",
    "Username": "",
    "Password": ""
  }
}
```

If a setting is not found in the configuration file, then it's created.

Note that password is marked with `[SecretSetting]` and it will be protected. After update `appsettings.json` will look like: 

```json
{
  "Region": "demo",
  "Email": {
    "Host": "example.dom",
    "Username": "my_username",
    "Password*": "CfDJ8IBGRtcA2S1Ji7VPVwaKmLYnTN6skE_2RQqvNZ8_CN5y3Xvk3LkFC6GXCe8EY7AicxH5...."
  }
}
```

Default implementation for `ISecretProtector` uses [Microsoft.AspNetCore.DataProtection](https://www.nuget.org/packages/Microsoft.AspNetCore.DataProtection/). 
You can also provide your own implementation:

```c#
builder.Host.AddJsonConfig(x =>
{
  x.Path = "appsettings.json",
  x.Protector = <your implementation of ISecretProtector>
});
```
