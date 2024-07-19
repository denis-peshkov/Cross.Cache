namespace Cross.Cache.UnitTests;

public class TestDataWrapper<T, TExp>
{
    public T? Value { get; set; }
    public TExp? Expected { get; set; }
}