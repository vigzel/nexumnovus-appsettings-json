namespace NexumNovus.AppSettings.Json.Test;
using NexumNovus.AppSettings.Common.Secure;

public class JsonSettingsRepository_Tests
{
  private const string TestFilePath = "test.json";
  private readonly JsonSettingsRepository _sut;

  public JsonSettingsRepository_Tests()
  {
    var mockProtector = new Mock<ISecretProtector>();
    mockProtector.Setup(x => x.Protect(It.IsAny<string>())).Returns("***");

    var source = new JsonConfigurationSource
    {
      Protector = mockProtector.Object,
      ReloadDelay = 0,
      ReloadOnChange = false,
      Path = TestFilePath,
    };
    _sut = new JsonSettingsRepository(source);
  }

  [Fact]
  public async Task Should_Update_Json_Property_Async()
  {
    try
    {
      // Arrange
      var jsonString =
@"{
  ""Name"": ""Old Name"",
  ""Age"": 25,
}";
      File.WriteAllText(TestFilePath, jsonString);

      // Act
      await _sut.UpdateSettingsAsync("Name", "New Name").ConfigureAwait(false);

      // Assert
      var result = File.ReadAllText(TestFilePath);
      result.Should().Be(
@"{
  ""Name"": ""New Name"",
  ""Age"": 25
}");
    }
    finally
    {
      File.Delete(TestFilePath);
    }
  }

  [Fact]
  public async Task Should_Update_Json_Array_Property_Async()
  {
    try
    {
      // Arrange
      var jsonString =
@"{
  ""Letters"": [
    ""A"",
    ""B""
  ]
}";
      File.WriteAllText(TestFilePath, jsonString);

      // Act
      await _sut.UpdateSettingsAsync("Letters", new string[] { "A", "B", "C" }).ConfigureAwait(false);

      // Assert
      var result = File.ReadAllText(TestFilePath);
      result.Should().Be(
@"{
  ""Letters"": [
    ""A"",
    ""B"",
    ""C""
  ]
}");
    }
    finally
    {
      File.Delete(TestFilePath);
    }
  }

  [Fact]
  public async Task Should_Add_Json_Property_Async()
  {
    try
    {
      // Arrange
      var jsonString =
@"{
  ""Name"": ""test name""
}";
      File.WriteAllText(TestFilePath, jsonString);

      // Act
      await _sut.UpdateSettingsAsync("Age", 25).ConfigureAwait(false);

      // Assert
      var result = File.ReadAllText(TestFilePath);
      result.Should().Be(
@"{
  ""Name"": ""test name"",
  ""Age"": 25
}");
    }
    finally
    {
      File.Delete(TestFilePath);
    }
  }

  [Fact]
  public async Task Should_Update_Json_Object_Async()
  {
    try
    {
      // Arrange
      var jsonString =
@"{
  ""Name"": ""testName"",
  ""Child"": {
    ""Name"": ""childName"",
  },
}";
      File.WriteAllText(TestFilePath, jsonString);
      var newChild = new TestChildSetting
      {
        Name = "childName",
        Password = "test",
        Types = new List<string> { "A", "B" },
        Data = new Dictionary<string, int>
        {
          { "A", 1 },
          { "B", 2 },
        },
      };

      // Act
      await _sut.UpdateSettingsAsync("Child", newChild).ConfigureAwait(false);

      // Assert
      var result = File.ReadAllText(TestFilePath);
      result.Should().Be(
@"{
  ""Name"": ""testName"",
  ""Child"": {
    ""Name"": ""childName"",
    ""Password*"": ""***"",
    ""Types"": [
      ""A"",
      ""B""
    ],
    ""Data"": {
      ""A"": 1,
      ""B"": 2
    }
  }
}");
    }
    finally
    {
      File.Delete(TestFilePath);
    }
  }

  private sealed class TestChildSetting
  {
    public string Name { get; set; } = string.Empty;

    [SecretSetting]
    public string Password { get; set; } = string.Empty;

    public IList<string> Types { get; set; } = Array.Empty<string>();

    public IDictionary<string, int> Data { get; set; } = new Dictionary<string, int>();
  }
}
