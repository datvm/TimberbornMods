namespace ModdableTimberborn.Tests.Areas;

public class AreaSegmentServiceTests
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(25, 25, 0)]
    [InlineData(26, 0, 10)]
    [InlineData(0, 26, 1)]
    [InlineData(26, 26, 11)]
    [InlineData(254, 254, 99)]
    public void GetSegment(int x, int y, int expected)
    {
        var service = AreaTestFactory.CreateSegmentService();

        var segment = service.GetSegment(x, y);

        Assert.Equal(expected, segment);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(255, 0)]
    [InlineData(0, 255)]
    public void GetSegmentOutOfMap(int x, int y)
    {
        var service = AreaTestFactory.CreateSegmentService();

        var segment = service.GetSegment(x, y);

        Assert.Equal(-1, segment);
    }

    [Fact]
    public void SegmentCounts()
    {
        var service = AreaTestFactory.CreateSegmentService();

        Assert.Equal(10, service.HorizontalSegmentsCount);
        Assert.Equal(10, service.VerticalSegmentsCount);
        Assert.Equal(100, service.SegmentsCount);
    }

    [Fact]
    public void GetSegmentsInsideOneSegment()
    {
        var service = AreaTestFactory.CreateSegmentService();

        var segments = service.GetSegments(new BoundsInt(10, 10, 0, 10, 10, 1));

        Assert.Equal([0], segments);
    }

    [Fact]
    public void GetSegmentsAcrossBoundaries()
    {
        var service = AreaTestFactory.CreateSegmentService();

        var segments = service.GetSegments(new BoundsInt(20, 20, 0, 20, 20, 1));

        Assert.Equal([0, 1, 10, 11], segments.Order());
    }

    [Fact]
    public void GetSegmentsClipNegativeBounds()
    {
        var service = AreaTestFactory.CreateSegmentService();

        var segments = service.GetSegments(new BoundsInt(-10, -10, 0, 20, 20, 1));

        Assert.Equal([0], segments);
    }

    [Fact]
    public void GetSegmentsOutsideNegativeMap()
    {
        var service = AreaTestFactory.CreateSegmentService();

        var segments = service.GetSegments(new BoundsInt(-20, -20, 0, 10, 10, 1));

        Assert.Empty(segments);
    }

    [Fact]
    public void GetSegmentsOutsidePositiveMap()
    {
        var service = AreaTestFactory.CreateSegmentService();

        var segments = service.GetSegments(new BoundsInt(255, 255, 0, 10, 10, 1));

        Assert.Empty(segments);
    }

    [Fact]
    public void GetSegmentsClipPositiveBounds()
    {
        var service = AreaTestFactory.CreateSegmentService();

        var segments = service.GetSegments(new BoundsInt(250, 250, 0, 20, 20, 1));

        Assert.Equal([99], segments);
    }

    [Fact]
    public void GetSegmentsMergeAreas()
    {
        var service = AreaTestFactory.CreateSegmentService();

        var segments = service.GetSegments([
            new BoundsInt(10, 10, 0, 10, 10, 1),
            new BoundsInt(20, 20, 0, 20, 20, 1),
        ]);

        Assert.Equal([0, 1, 10, 11], segments.Order());
    }
}
