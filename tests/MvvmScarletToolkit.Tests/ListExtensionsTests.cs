namespace MvvmScarletToolkit.Tests
{
    public sealed class ListExtensionsTests : TraceTestBase
    {
        private sealed class Target(int id, string value)
        {
            public int Id { get; } = id;
            public string Value { get; set; } = value;
        }

        private sealed record Update(int Id, string Value);

        private static readonly Update[] _noUpdates = [];

        private static bool Matches(Target target, Update update)
        {
            return target.Id == update.Id;
        }

        private static Target Map(Update update)
        {
            return new Target(update.Id, update.Value);
        }

        [Fact]
        public void UpdateItems_Should_Add_New_Items()
        {
            var target = new List<Target> { new(1, "a") };

            target.UpdateItems([new Update(1, "a"), new Update(2, "b")], Matches, Map);

            Assert.Equal([1, 2], target.Select(p => p.Id));
        }

        [Fact]
        public void UpdateItems_Should_Remove_Items_That_Are_Gone()
        {
            var target = new List<Target> { new(1, "a"), new(2, "b") };

            target.UpdateItems([new Update(2, "b")], Matches, Map);

            Assert.Equal([2], target.Select(p => p.Id));
        }

        [Fact]
        public void UpdateItems_Should_Keep_The_Instance_Of_Matching_Items_And_Update_Them()
        {
            var existing = new Target(1, "old");
            var target = new List<Target> { existing };

            target.UpdateItems([new Update(1, "new")], Matches, Map, (t, u) => t.Value = u.Value);

            Assert.Multiple(() =>
            {
                Assert.Same(existing, Assert.Single(target));
                Assert.Equal("new", existing.Value);
            });
        }

        [Fact]
        public void UpdateItems_Should_Not_Remove_Items_Excluded_By_The_Filter()
        {
            var target = new List<Target> { new(1, "keep"), new(2, "drop") };

            target.UpdateItems(_noUpdates, Matches, Map, null, p => p.Value == "drop");

            Assert.Equal([1], target.Select(p => p.Id));
        }

        [Fact]
        public void UpdateItems_Should_Not_Remove_Ignored_Items()
        {
            var target = new List<Target> { new(1, "ignored"), new(2, "drop") };

            target.UpdateItems(_noUpdates, Matches, Map, null, _ => true, p => p.Value == "ignored");

            Assert.Equal([1], target.Select(p => p.Id));
        }

        [Fact]
        public void UpdateItems_Should_Not_Update_Ignored_Items_Without_A_Match()
        {
            var target = new List<Target> { new(1, "ignored") };
            var updates = new List<Update?>();

            target.UpdateItems(_noUpdates, Matches, Map, (_, u) => updates.Add(u), _ => true, _ => true);

            Assert.Empty(updates);
        }

        [Fact]
        public void UpdateItems_Should_Ignore_A_Missing_Update_Collection()
        {
            var target = new List<Target> { new(1, "a") };

            target.UpdateItems<Target, Update>(null, Matches, Map, null, _ => true, _ => false);

            Assert.Single(target);
        }

        [Fact]
        public void UpdateItems_Should_Remove_Items_Without_A_Match_For_Value_Type_Updates()
        {
            var target = new List<string> { "1", "2" };

            target.UpdateItems([2], (t, u) => t == u.ToString(), u => u.ToString());

            Assert.Equal(["2"], target);
        }
    }
}
