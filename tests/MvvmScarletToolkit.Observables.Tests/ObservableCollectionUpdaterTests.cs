using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MvvmScarletToolkit.Observables.Tests
{
    public sealed class ObservableCollectionUpdaterTests : TraceTestBase
    {
        private sealed class Entry(int id, string name)
        {
            public int Id { get; } = id;
            public string Name { get; set; } = name;
        }

        private sealed class Updater(IViewModelMerger<Entry> merger, IEqualityComparer<int>? comparer = null)
            : ObservableCollectionUpdater<Entry, int>(p => p.Id, merger, comparer);

        /// <summary>
        /// updates the source instance in place
        /// </summary>
        private sealed class InPlaceMerger : IViewModelMerger<Entry>
        {
            public int Calls { get; private set; }

            public (Entry Instance, bool Replace) Perform(Entry sourceInstance, Entry changedInstance)
            {
                Calls++;
                sourceInstance.Name = changedInstance.Name;
                return (sourceInstance, false);
            }
        }

        /// <summary>
        /// replaces the source instance
        /// </summary>
        private sealed class ReplacingMerger : IViewModelMerger<Entry>
        {
            public (Entry Instance, bool Replace) Perform(Entry sourceInstance, Entry changedInstance)
            {
                return (changedInstance, true);
            }
        }

        /// <summary>
        /// only considers the last digit of a key
        /// </summary>
        private sealed class LastDigitComparer : IEqualityComparer<int>
        {
            public bool Equals(int x, int y) => x % 10 == y % 10;

            public int GetHashCode(int obj) => obj % 10;
        }

        [Fact]
        public void Ctor_DoesThrow_For_Null_Arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new Updater(null!));
        }

        [Fact]
        public void Update_DoesThrow_For_Null_Arguments()
        {
            var sut = new Updater(new InPlaceMerger());

            Assert.Throws<ArgumentNullException>(() => sut.Update(null!, []));
            Assert.Throws<ArgumentNullException>(() => sut.Update([], null!));
        }

        [Fact]
        public void Update_Does_Nothing_For_Empty_Changes()
        {
            var sut = new Updater(new InPlaceMerger());
            var source = new ObservableCollection<Entry>([new Entry(1, "a")]);

            sut.Update(source, []);

            Assert.Single(source);
        }

        [Fact]
        public void Update_Does_Add_Everything_To_Empty_Source()
        {
            var sut = new Updater(new InPlaceMerger());
            var source = new ObservableCollection<Entry>();

            sut.Update(source, [new Entry(1, "a"), new Entry(2, "b")]);

            Assert.Equal([1, 2], source.Select(p => p.Id));
        }

        [Fact]
        public void Update_Does_Remove_Missing_Instances()
        {
            var sut = new Updater(new InPlaceMerger());
            var source = new ObservableCollection<Entry>([new Entry(1, "a"), new Entry(2, "b"), new Entry(3, "c")]);

            sut.Update(source, [new Entry(2, "b")]);

            Assert.Equal([2], source.Select(p => p.Id));
        }

        [Fact]
        public void Update_Does_Add_New_Instances()
        {
            var sut = new Updater(new InPlaceMerger());
            var source = new ObservableCollection<Entry>([new Entry(1, "a")]);

            sut.Update(source, [new Entry(1, "a"), new Entry(2, "b")]);

            Assert.Equal([1, 2], source.Select(p => p.Id));
        }

        [Fact]
        public void Update_Does_Merge_Existing_Instances_In_Place()
        {
            var merger = new InPlaceMerger();
            var sut = new Updater(merger);
            var original = new Entry(1, "a");
            var source = new ObservableCollection<Entry>([original]);
            var actions = new List<NotifyCollectionChangedAction>();
            source.CollectionChanged += (_, e) => actions.Add(e.Action);

            sut.Update(source, [new Entry(1, "updated")]);

            Assert.Multiple(() =>
            {
                Assert.Same(original, Assert.Single(source));
                Assert.Equal("updated", original.Name);
                Assert.Equal(1, merger.Calls);
                Assert.Empty(actions);
            });
        }

        [Fact]
        public void Update_Does_Replace_Existing_Instances_At_The_Same_Index()
        {
            var sut = new Updater(new ReplacingMerger());
            var replacement = new Entry(2, "replaced");
            var source = new ObservableCollection<Entry>([new Entry(1, "a"), new Entry(2, "b"), new Entry(3, "c")]);

            sut.Update(source, [new Entry(1, "a"), replacement, new Entry(3, "c")]);

            Assert.Multiple(() =>
            {
                Assert.Equal([1, 2, 3], source.Select(p => p.Id));
                Assert.Same(replacement, source[1]);
            });
        }

        [Fact]
        public void Update_Does_Combine_Remove_Add_And_Merge()
        {
            var sut = new Updater(new InPlaceMerger());
            var kept = new Entry(2, "b");
            var source = new ObservableCollection<Entry>([new Entry(1, "a"), kept, new Entry(3, "c")]);

            sut.Update(source, [new Entry(4, "d"), new Entry(2, "b2")]);

            Assert.Multiple(() =>
            {
                Assert.Equal([2, 4], source.Select(p => p.Id));
                Assert.Same(kept, source[0]);
                Assert.Equal("b2", kept.Name);
            });
        }

        [Fact]
        public void Update_Does_Use_The_EqualityComparer()
        {
            var sut = new Updater(new InPlaceMerger(), new LastDigitComparer());
            var original = new Entry(1, "a");
            var source = new ObservableCollection<Entry>([original]);

            // 11 equals 1 according to the comparer
            sut.Update(source, [new Entry(11, "b")]);

            Assert.Multiple(() =>
            {
                Assert.Same(original, Assert.Single(source));
                Assert.Equal("b", original.Name);
            });
        }

        [Fact]
        public void Update_DoesThrow_For_Duplicate_Keys_In_Changes()
        {
            var sut = new Updater(new InPlaceMerger());

            Assert.Throws<ArgumentException>(() => sut.Update([], [new Entry(1, "a"), new Entry(1, "b")]));
        }
    }
}
