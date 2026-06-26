namespace Cross.Cache.Tests;

[TestFixture]
[Category(TestCategory.UNIT)]
public class CachedServiceBaseTests
{
    [Test]
    public void GetAll_ShouldCallLoadOnce_WhenForceFalseAndCacheEmpty()
    {
        var loadCalls = 0;
        var service = new TestCachedService(() =>
        {
            loadCalls++;
            return Task.FromResult<SampleTestDto>(new SampleTestDto { Id = 1, Code = "a", Description = "b" });
        });

        var result = service.GetAll(force: false);

        loadCalls.Should().Be(1);
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Code.Should().Be("a");
    }

    [Test]
    public void GetAll_ShouldReturnCachedValue_WhenForceFalseAndCachePopulated()
    {
        var service = new TestCachedService(() =>
            Task.FromResult<SampleTestDto>(new SampleTestDto { Id = 2, Code = "x", Description = "y" }));

        var first = service.GetAll(force: false);
        var second = service.GetAll(force: false);

        first.Should().BeSameAs(second);
        first!.Id.Should().Be(2);
    }

    [Test]
    public void GetAll_ShouldCallLoadAgain_WhenForceTrue()
    {
        var loadCalls = 0;
        var service = new TestCachedService(() =>
        {
            loadCalls++;
            return Task.FromResult<SampleTestDto>(new SampleTestDto { Id = loadCalls, Code = "c", Description = "d" });
        });

        service.GetAll(force: false);
        service.GetAll(force: true);

        loadCalls.Should().Be(2);
        service.GetAll(force: false).Id.Should().Be(2);
    }

    [Test]
    public void Invalidate_ShouldClearCache_SoNextGetAllCallsLoad()
    {
        var loadCalls = 0;
        var service = new TestCachedService(() =>
        {
            loadCalls++;
            return Task.FromResult<SampleTestDto>(new SampleTestDto { Id = loadCalls, Code = "c", Description = "d" });
        });

        service.GetAll(force: false);
        loadCalls.Should().Be(1);
        service.Invalidate();
        service.GetAll(force: false);
        loadCalls.Should().Be(2);
    }

    [Test]
    public void GetAll_ShouldReturnDefaultValue_WhenLoadReturnsNull()
    {
        var service = new TestCachedService(() => Task.FromResult<SampleTestDto>(null!));

        var result = service.GetAll(force: false);

        result.Should().NotBeNull();
        result!.Id.Should().Be(0);
        result.Code.Should().Be("default");
    }

    private sealed class TestCachedService : CachedServiceBase<SampleTestDto>
    {
        private readonly Func<Task<SampleTestDto>> _load;

        public TestCachedService(Func<Task<SampleTestDto>> load) => _load = load;

        protected override Task<SampleTestDto> Load() => _load();

        protected override SampleTestDto GetDefaultValue() =>
            new SampleTestDto { Id = 0, Code = "default", Description = "default" };
    }
}
