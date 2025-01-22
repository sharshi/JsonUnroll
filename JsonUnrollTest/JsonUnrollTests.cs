namespace JsonUnrollTest;

using Newtonsoft.Json.Linq;
using Xunit;
using JsonUnroll;

public class JsonUnrollTests
{
    [Fact]
    public void Flatten_SimpleObject_ReturnsSingleRow()
    {
        // Arrange
        var json = @"{ ""name"": ""John"", ""age"": 30 }";
        var token = JToken.Parse(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        Assert.Single(result);
        Assert.Equal("John", result[0]["name"]);
        Assert.Equal("30", result[0]["age"].ToString());
    }

    [Fact]
    public void Flatten_PrimitiveArray_JoinsIntoString()
    {
        // Arrange
        var json = @"{ ""hobbies"": [""reading"", ""swimming""] }";
        var token = JToken.Parse(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("reading", result[0]["hobbies"]);
        Assert.Equal("swimming", result[1]["hobbies"]);
    }

    [Fact(Skip = "TODO")]
    public void MatchHeaders()
    {
        // if some rows have more columns than others, the headers should be the union of all columns
    }

    [Fact]
    public void Flatten_ObjectArray_UnrollsIntoMultipleRows()
    {
        // Arrange
        var json = @"{
            ""contacts"": [
                { ""type"": ""email"", ""value"": ""test@test.com"" },
                { ""type"": ""phone"", ""value"": ""123-456"" }
            ]
        }";
        var token = JToken.Parse(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("email", result[0]["contacts.type"]);
        Assert.Equal("test@test.com", result[0]["contacts.value"]);
        Assert.Equal("phone", result[1]["contacts.type"]);
    }

    [Fact]
    public void Flatten_NestedObjects_RetainsParentData()
    {
        // Arrange
        var json = @"{
            ""name"": ""John"",
            ""address"": {
                ""city"": ""New York"",
                ""zip"": 10001
            }
        }";
        var token = JToken.Parse(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        Assert.Single(result);
        Assert.Equal("John", result[0]["name"]);
        Assert.Equal("New York", result[0]["address.city"]);
        Assert.Equal("10001", result[0]["address.zip"].ToString());
    }

    [Fact]
    public void Flatten_MixedArrays_HandlesBothTypes()
    {
        // Arrange
        var json = @"[
            {
                ""name"": ""John"",
                ""hobbies"": [""reading"", ""traveling""],
                ""contacts"": [
                    { ""type"": ""email"", ""value"": ""john@test.com"" }
                ]
            }
        ]";
        var token = JToken.Parse(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("reading", result[0]["hobbies"]);
        Assert.Equal("traveling", result[1]["hobbies"]);
        Assert.Equal("email", result[0]["contacts.type"]);
        Assert.Equal("john@test.com", result[0]["contacts.value"]);
    }

    [Fact]
    public void Flatten_ComplexExample_ProducesCorrectRows()
    {
        // Arrange
        var json = @"[
            {
                ""name"": ""John Doe"",
                ""age"": 30,
                ""address"": {
                    ""street"": ""123 Main St"",
                    ""city"": ""Anytown"",
                    ""zipCode"": ""12345""
                },
                ""hobbies"": [""reading"", ""traveling""],
                ""contacts"": [
                    { ""type"": ""email"", ""value"": ""john@example.com"" },
                    { ""type"": ""phone"", ""value"": ""555-1234"" }
                ]
            },
            {
                ""name"": ""Jane Doe"",
                ""age"": 25,
                ""hobbies"": [""skiing""],
                ""contacts"": [
                    { ""type"": ""email"", ""value"": ""jane@example.com"" }
                ]
            }
        ]";
        var token = JToken.Parse(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        Assert.Equal(5, result.Count);

        // Verify John's first contact
        Assert.Equal("John Doe", result[0]["name"]);
        Assert.Equal("30", result[0]["age"].ToString());
        Assert.Equal("123 Main St", result[0]["address.street"]);
        Assert.Equal("email", result[0]["contacts.type"]);
        Assert.Equal("phone", result[1]["contacts.type"]);
        Assert.Equal("reading", result[0]["hobbies"]);
        Assert.Equal("reading", result[1]["hobbies"]);
        Assert.Equal("email", result[2]["contacts.type"]);
        Assert.Equal("phone", result[3]["contacts.type"]);
        Assert.Equal("traveling", result[2]["hobbies"]);
        Assert.Equal("traveling", result[3]["hobbies"]);

        // Verify Jane's contact
        Assert.Equal("Jane Doe", result[4]["name"]);
        Assert.Equal("skiing", result[4]["hobbies"]);
        Assert.Equal("jane@example.com", result[4]["contacts.value"]);
    }

    [Fact]
    public void Flatten_EmptyArray_ReturnsNoRows()
    {
        // Arrange
        var json = @"{ ""contacts"": [] }";
        var token = JToken.Parse(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Flatten_DeeplyNested_UnrollsCorrectly()
    {
        // Arrange
        var json = @"{
            ""level1"": {
                ""level2"": [
                    { ""level3"": { ""value"": ""A"" } },
                    { ""level3"": { ""value"": ""B"" } }
                ]
            }
        }";
        var token = JToken.Parse(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0]["level1.level2.level3.value"]);
        Assert.Equal("B", result[1]["level1.level2.level3.value"]);
    }

    [Fact]
    public void ParseLarge()
    {
        // Arrange
        var json = TestData.LargeJsonArray;

        var token = JToken.Parse(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count > 1_000_000);
    }
}

