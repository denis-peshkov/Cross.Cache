namespace Cross.Cache.UnitTests;

[TestFixture]
public class CacheValueConverterHelperTests
{
    [Test(Description = "GetConvertedValue should successfully return Dictionary from string")]
    [TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DictionaryCases))]
    public void GetConvertedValue_ShouldReturnValueSuccessful_ForDictionary(TestDataWrapper<string, Dictionary<string, SampleTestDto>> data)
    {
        // Act
        var result = CacheValueConverterHelper.GetConvertedValue<Dictionary<string, SampleTestDto>>(data.Value);
        // Assert
        result.Should().BeOfType(typeof(Dictionary<string, SampleTestDto>));
        result.Should().HaveSameCount(data.Expected);
        result.Should().BeEquivalentTo(data.Expected);
    }

    [Test(Description = "GetConvertedValue should successfully return Dto object from string")]
    [TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DtoCases))]
    public void GetConvertedValue_ShouldReturnValueSuccessful_ForDto(TestDataWrapper<string, SampleTestDto> data)
    {
        // Act
        var result = CacheValueConverterHelper.GetConvertedValue<SampleTestDto>(data.Value);
        // Assert
        result.Should().BeOfType(typeof(SampleTestDto));
        result.Should().BeEquivalentTo(data.Expected);
    }
    
    [Test(Description = "GetConvertedValue should successfully return value type object from string")]
    [TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.ValueTypeCases))]
    public void GetConvertedValue_ShouldReturnValueSuccessful_ForNullableValueType(TestDataWrapper<string, int> data)
    {
        // Act
        var result = CacheValueConverterHelper.GetConvertedValue<int>(data.Value);
        // Assert
        result.Should().BeOfType(typeof(int));
        result.Should().Be(data.Expected);
    }

    [Test(Description = "GetConvertedValue should successfully return IEnumerable from string")]
    [TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.EnumerableTypeCases))]
    public void GetConvertedValue_ShouldReturnValueSuccessful_ForEnumerable(TestDataWrapper<string, IEnumerable<string>> data)
    {
        // Act
        var result = CacheValueConverterHelper.GetConvertedValue<List<string>>(data.Value);
        // Assert
        result.Should().BeOfType(typeof(List<string>));
        result.Should().HaveSameCount(data.Expected);
        result.Should().BeEquivalentTo(data.Expected);
    }

    [Test(Description = "GetConvertedValue should return Null from empty IEnumerable")]
    [TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.EmptyEnumerableCase))]
    public void GetConvertedValue_ShouldReturnNull_ForEmptyCollection(TestDataWrapper<string, IEnumerable<SampleTestDto>> data)
    {
        // Act
        var result = CacheValueConverterHelper.GetConvertedValue<IEnumerable<SampleTestDto>>(data.Value);
        // Assert
        result.Should().BeNull();
    }

    [Test(Description = "GetConvertedValue should return Null when cannot convert to type")]
    public void GetConvertedValue_ShouldReturnNull_WhenCannotConvertToType()
    {
        // Act
        var result = CacheValueConverterHelper.GetConvertedValue<SampleTestDto>("Not a dto");
        // Assert
        result.Should().BeNull();
    }
}