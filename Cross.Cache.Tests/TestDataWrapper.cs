namespace Cross.Cache.Tests;

public class TestDataWrapper<T, TExp>
{
    public T? Value { get; set; }
    public TExp? Expected { get; set; }
}