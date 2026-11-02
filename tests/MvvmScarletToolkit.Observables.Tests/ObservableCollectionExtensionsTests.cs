using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MvvmScarletToolkit.Observables.Tests
{
    /// <summary>
    /// tests for <see cref="MvvmScarletToolkit.ObservableCollectionExtensions"/> of the core library
    /// </summary>
    public sealed class ObservableCollectionExtensionsTests : TraceTestBase
    {
        [Fact]
        public void Sort_DoesThrow_For_Null()
        {
            Assert.Throws<ArgumentNullException>(() => ObservableCollectionExtensions.Sort(default(ObservableCollection<int>)!));
            Assert.Throws<ArgumentNullException>(() => ObservableCollectionExtensions.Sort(default(ObservableCollection<int>)!, Comparer<int>.Default));
        }

        [Fact]
        public void Sort_Does_Sort_With_Duplicates()
        {
            var sut = new ObservableCollection<int>([2, 1, 1, 3, 2, 1]);

            sut.Sort();

            Assert.Equal([1, 1, 1, 2, 2, 3], sut);
        }

        [Fact]
        public void Sort_Does_Use_The_Comparer()
        {
            var sut = new ObservableCollection<int>([1, 3, 2, 3]);

            sut.Sort(Comparer<int>.Create((x, y) => y.CompareTo(x)));

            Assert.Equal([3, 3, 2, 1], sut);
        }

        [Fact]
        public void Sort_Does_Not_Raise_CollectionChanged_When_Already_Sorted()
        {
            var sut = new ObservableCollection<int>([1, 2, 3]);
            var raised = false;
            sut.CollectionChanged += (_, _) => raised = true;

            sut.Sort();

            Assert.False(raised);
        }

        [Fact]
        public void Sort_Does_Only_Move()
        {
            var sut = new ObservableCollection<int>([3, 2, 1]);
            var actions = new List<NotifyCollectionChangedAction>();
            sut.CollectionChanged += (_, e) => actions.Add(e.Action);

            sut.Sort();

            Assert.All(actions, p => Assert.Equal(NotifyCollectionChangedAction.Move, p));
        }
    }
}
