namespace JsonUnrollTest;

using Xunit;
using JsonUnroll;
using Shouldly;

public class JsonUnrollTests
{
    [Fact]
    public void Flatten_SimpleObject_ReturnsSingleRow()
    {
        // Arrange
        var json = @"{ ""name"": ""John"", ""age"": 30 }";
        var token = JsonUnroll.Prepare(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        result.ShouldHaveSingleItem();
        result[0]["name"].ToString().ShouldBe("John");
        result[0]["age"].ToString().ShouldBe("30");
    }

    [Fact]
    public void Flatten_PrimitiveArray_JoinsIntoString()
    {
        // Arrange
        var json = @"{ ""hobbies"": [""reading"", ""swimming""] }";
        var token = JsonUnroll.Prepare(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        result.Count.ShouldBe(2);
        result[0]["hobbies"].ToString().ShouldBe("reading");
        result[1]["hobbies"].ToString().ShouldBe("swimming");
    }

    [Fact(Skip = "TODO")]
    public void MatchHeaders()
    {
        // Test implementation will be added later
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
        var token = JsonUnroll.Prepare(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        result.Count.ShouldBe(2);
        result[0]["contacts.type"].ToString().ShouldBe("email");
        result[0]["contacts.value"].ToString().ShouldBe("test@test.com");
        result[1]["contacts.type"].ToString().ShouldBe("phone");
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
        var token = JsonUnroll.Prepare(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        result.ShouldNotBeEmpty();
        result[0]["name"].ToString().ShouldBe("John");
        result[0]["address.city"].ToString().ShouldBe("New York");
        result[0]["address.zip"].ToString().ShouldBe("10001");
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
        var token = JsonUnroll.Prepare(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        result.Count.ShouldBe(2);
        result[0]["hobbies"].ToString().ShouldBe("reading");
        result[0]["contacts.type"].ToString().ShouldBe("email");
        result[0]["contacts.value"].ToString().ShouldBe("john@test.com");
        result[1]["hobbies"].ToString().ShouldBe("traveling");
        result[1]["contacts.type"].ToString().ShouldBe("email");
        result[1]["contacts.value"].ToString().ShouldBe("john@test.com");
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
        var token = JsonUnroll.Prepare(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        result.Count.ShouldBe(5);

        // Verify John's first contact
        result[0]["name"].ToString().ShouldBe("John Doe");
        result[0]["age"].ToString().ShouldBe("30");
        result[0]["address.street"].ToString().ShouldBe("123 Main St");
        result[0]["contacts.type"].ToString().ShouldBe("email");
        result[1]["contacts.type"].ToString().ShouldBe("phone");
        result[0]["hobbies"].ToString().ShouldBe("reading");
        result[1]["hobbies"].ToString().ShouldBe("reading");
        result[2]["contacts.type"].ToString().ShouldBe("email");
        result[3]["contacts.type"].ToString().ShouldBe("phone");
        result[2]["hobbies"].ToString().ShouldBe("traveling");
        result[3]["hobbies"].ToString().ShouldBe("traveling");

        // Verify Jane's contact
        result[4]["name"].ToString().ShouldBe("Jane Doe");
        result[4]["hobbies"].ToString().ShouldBe("skiing");
        result[4]["contacts.value"].ToString().ShouldBe("jane@example.com");
    }

    [Fact]
    public void Flatten_EmptyArray_ReturnsNoRows()
    {
        // Arrange
        var json = @"{ ""contacts"": [] }";
        var token = JsonUnroll.Prepare(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        result.ShouldBe([[]]);
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
        var token = JsonUnroll.Prepare(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        result.Count.ShouldBe(2);
        result[0]["level1.level2.level3.value"].ToString().ShouldBe("A");
        result[1]["level1.level2.level3.value"].ToString().ShouldBe("B");
    }

    [Fact]
    public void ParseLarge()
    {
        // Arrange
        var json = TestData.LargeJsonArray;

        var token = JsonUnroll.Prepare(json);

        // Act
        var result = JsonUnroll.Flatten(token);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(1_000_000);
    }
}