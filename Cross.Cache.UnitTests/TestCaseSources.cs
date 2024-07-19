namespace Cross.Cache.UnitTests;

public static class TestCaseSources
{
    public static IEnumerable<TestDataWrapper<string, Dictionary<string, SampleTestDto>>> DictionaryCases()
    {
        yield return new TestDataWrapper<string, Dictionary<string, SampleTestDto>>
        {
            Value = JsonSerializer.Serialize(new Dictionary<string, SampleTestDto>
            {
                { "key 1", new SampleTestDto() { Id = 1, Code = "Code 1", Description = "Description 1" } },
                { "key 2", new SampleTestDto() { Id = 2, Code = "Code 2", Description = "Description 2" } },
                { "key 3", new SampleTestDto() { Id = 3, Code = "Code 3", Description = "Description 3" } },
                { "key 4", new SampleTestDto() { Id = 4, Code = "Code 4", Description = "Description 4" } },
            }),
            Expected = new Dictionary<string, SampleTestDto>()
            {
                { "key 1", new SampleTestDto() { Id = 1, Code = "Code 1", Description = "Description 1" } },
                { "key 2", new SampleTestDto() { Id = 2, Code = "Code 2", Description = "Description 2" } },
                { "key 3", new SampleTestDto() { Id = 3, Code = "Code 3", Description = "Description 3" } },
                { "key 4", new SampleTestDto() { Id = 4, Code = "Code 4", Description = "Description 4" } },
            }
        };
        
        yield return new TestDataWrapper<string, Dictionary<string, SampleTestDto>>
        {
            Value = JsonSerializer.Serialize(new Dictionary<string, SampleTestDto>
            {
                { "223", new SampleTestDto() { Id = 1, Code = "Code 1", Description = "Description 1" } },
                { "@#%%5$$@", new SampleTestDto() { Id = 2, Code = "Code 2", Description = "Description 2" } },
                { "someString", new SampleTestDto() { Id = 3, Code = "Code 3", Description = "Description 3" } },
                { "!sdfsdf@", new SampleTestDto() { Id = 4, Code = "Code 4", Description = "Description 4" } },
            }),
            Expected = new Dictionary<string, SampleTestDto>()
            {
                { "223", new SampleTestDto() { Id = 1, Code = "Code 1", Description = "Description 1" } },
                { "@#%%5$$@", new SampleTestDto() { Id = 2, Code = "Code 2", Description = "Description 2" } },
                { "someString", new SampleTestDto() { Id = 3, Code = "Code 3", Description = "Description 3" } },
                { "!sdfsdf@", new SampleTestDto() { Id = 4, Code = "Code 4", Description = "Description 4" } },
            }
        };
    }

    public static IEnumerable<TestDataWrapper<string, SampleTestDto>> DtoCases()
    {
        yield return new TestDataWrapper<string, SampleTestDto>
        {
            Value = JsonSerializer.Serialize(new SampleTestDto
            {
                Id = 1, Code = "Code 1", Description = "Description 1"
            }),
            Expected = new SampleTestDto { Id = 1, Code = "Code 1", Description = "Description 1" }
        };
        
        yield return new TestDataWrapper<string, SampleTestDto>
        {
            Value = JsonSerializer.Serialize(new SampleTestDto
            {
                Id = 2, Code = "Code 2", Description = "Description 2"
            }),
            Expected = new SampleTestDto { Id = 2, Code = "Code 2", Description = "Description 2" }
        };
    }

    public static IEnumerable<TestDataWrapper<string, int>> ValueTypeCases()
    {
        yield return new TestDataWrapper<string, int>
        {
            Value = Int32.MinValue.ToString(),
            Expected = Int32.MinValue,
        };
        
        yield return new TestDataWrapper<string, int>
        {
            Value = Int32.MaxValue.ToString(),
            Expected = Int32.MaxValue,
        };
        
        yield return new TestDataWrapper<string, int>
        {
            Value = 33.ToString(),
            Expected = 33,
        };
    }

    public static IEnumerable<TestDataWrapper<string, IEnumerable<string>>> EnumerableTypeCases()
    {
        yield return new TestDataWrapper<string, IEnumerable<string>>
        {
            Value = JsonSerializer.Serialize(new List<string>
            {
                "Object 1",
                "Object 2",
                "Object 3"
            }),
            Expected = new List<string>
            {
                "Object 1",
                "Object 2",
                "Object 3"
            },
        };
        
        yield return new TestDataWrapper<string, IEnumerable<string>>
        {
            Value = JsonSerializer.Serialize(new HashSet<string>
            {
                "Test Data 1",
                "Test Data 2",
                "Test Data 3"
            }),
            Expected = new HashSet<string>
            {
                "Test Data 1",
                "Test Data 2",
                "Test Data 3"
            },
        };
    }

    public static IEnumerable<TestDataWrapper<string, IEnumerable<SampleTestDto>>> EmptyEnumerableCase()
    {
        yield return new TestDataWrapper<string, IEnumerable<SampleTestDto>>
        {
            Value = JsonSerializer.Serialize(new List<SampleTestDto>()),
            Expected = new List<SampleTestDto>(),
        };
        
        yield return new TestDataWrapper<string, IEnumerable<SampleTestDto>>
        {
            Value = JsonSerializer.Serialize(new HashSet<SampleTestDto>()),
            Expected = new HashSet<SampleTestDto>(),
        };
    }
}