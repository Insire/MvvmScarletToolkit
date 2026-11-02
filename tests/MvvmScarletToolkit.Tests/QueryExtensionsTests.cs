namespace MvvmScarletToolkit.Tests
{
    public sealed class QueryExtensionsTests : TraceTestBase
    {
        private static readonly IQueryable<int> _source = Enumerable.Range(0, 5).AsQueryable();

        [Theory]
        [InlineData(null, 5)]
        [InlineData(0, 5)]
        [InlineData(2, 3)]
        [InlineData(5, 0)]
        [InlineData(-1, 0)]
        public void TrySkip_Should_Skip_Valid_Indices_And_Return_Nothing_For_Negative_Ones(int? index, int expectedCount)
        {
            Assert.Equal(expectedCount, _source.TrySkip(index).Count());
        }

        [Theory]
        [InlineData(null, 5)]
        [InlineData(2, 2)]
        [InlineData(10, 5)]
        [InlineData(0, 0)]
        [InlineData(-1, 0)]
        public void TryTake_Should_Take_Valid_Counts_And_Return_Nothing_Otherwise(int? count, int expectedCount)
        {
            Assert.Equal(expectedCount, _source.TryTake(count).Count());
        }

        [Fact]
        public void TryPage_Should_Return_The_Requested_Page()
        {
            Assert.Equal([2, 3], _source.TryPage(2, 2));
        }

        [Fact]
        public void TryPage_Without_Arguments_Should_Return_Everything()
        {
            Assert.Equal(_source, _source.TryPage(null, null));
        }
    }
}
