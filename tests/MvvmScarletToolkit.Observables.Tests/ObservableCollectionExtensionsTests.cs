using DynamicData.Binding;
using System.Collections.Specialized;

namespace MvvmScarletToolkit.Observables.Tests
{
    public sealed class ObservableCollectionExtensionsTests : TraceTestBase
    {
        [Fact]
        public void Sort_DoesThrow_For_Null()
        {
            Assert.Throws<ArgumentNullException>(() => default(IObservableCollection<int>)!.Sort());
            Assert.Throws<ArgumentNullException>(() => default(IObservableCollection<int>)!.Sort(Comparer<int>.Default));
        }

        [Fact]
        public void Sort_Does_Work_On_Empty_Collection()
        {
            IObservableCollection<int> sut = new ObservableCollectionExtended<int>();

            sut.Sort();

            Assert.Empty(sut);
        }

        [Fact]
        public void Sort_Does_Sort()
        {
            IObservableCollection<int> sut = new ObservableCollectionExtended<int>([5, 3, 1, 4, 2]);

            sut.Sort();

            Assert.Equal([1, 2, 3, 4, 5], sut);
        }

        [Fact]
        public void Sort_Does_Sort_With_Duplicates()
        {
            IObservableCollection<int> sut = new ObservableCollectionExtended<int>([2, 1, 1, 3, 2, 1]);

            sut.Sort();

            Assert.Equal([1, 1, 1, 2, 2, 3], sut);
        }

        [Fact]
        public void Sort_Does_Sort_With_Duplicate_References()
        {
            var b = "b";
            var a = "a";
            IObservableCollection<string> sut = new ObservableCollectionExtended<string>([b, a, a]);

            sut.Sort();

            Assert.Equal(["a", "a", "b"], sut);
        }

        [Fact]
        public void Sort_Does_Use_The_Comparer()
        {
            IObservableCollection<int> sut = new ObservableCollectionExtended<int>([1, 3, 2]);

            sut.Sort(Comparer<int>.Create((x, y) => y.CompareTo(x)));

            Assert.Equal([3, 2, 1], sut);
        }

        [Fact]
        public void Sort_Does_Not_Raise_CollectionChanged_When_Already_Sorted()
        {
            IObservableCollection<int> sut = new ObservableCollectionExtended<int>([1, 2, 3]);
            var raised = false;
            sut.CollectionChanged += (_, _) => raised = true;

            sut.Sort();

            Assert.False(raised);
        }

        [Fact]
        public void Sort_Does_Only_Move()
        {
            IObservableCollection<int> sut = new ObservableCollectionExtended<int>([3, 2, 1]);
            var actions = new List<NotifyCollectionChangedAction>();
            sut.CollectionChanged += (_, e) => actions.Add(e.Action);

            sut.Sort();

            Assert.All(actions, p => Assert.Equal(NotifyCollectionChangedAction.Move, p));
        }
    }
}
