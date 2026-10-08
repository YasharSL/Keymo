namespace Keymo.Tests;

public class UpdateCheckTests
{
    [Theory]
    [InlineData("""{ "tag_name": "v1.4.2" }""", "1.4.2")]
    [InlineData("""{ "tag_name": "2.0" }""", "2.0")]
    public void Release_tag_becomes_a_version(string json, string expected)
    {
        Assert.Equal(Version.Parse(expected), UpdateCheck.ParseLatest(json));
    }

    [Theory]
    [InlineData("""{ "tag_name": "nightly" }""")]
    [InlineData("""{ "name": "no tag here" }""")]
    public void Release_without_a_version_tag_is_rejected(string json)
    {
        Assert.Throws<FormatException>(() => UpdateCheck.ParseLatest(json));
    }

    [Fact]
    public void A_later_release_is_newer_than_this_build()
    {
        Assert.True(UpdateCheck.ParseLatest("""{ "tag_name": "v999.0.0" }""") > UpdateCheck.Current);
    }
}
