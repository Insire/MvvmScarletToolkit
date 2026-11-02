using System.Collections.Specialized;
using System.ComponentModel;

namespace MvvmScarletToolkit.Observables.Tests
{
    public sealed class ObservableCircularBufferTests : TraceTestBase
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Ctor_DoesThrow_For_Invalid_Capacity(int capacity)
        {
            Assert.Throws<ArgumentException>(() => new ObservableCircularBuffer<int>(capacity));
        }

        [Fact]
        public void Ctor_DoesThrow_For_Null_Items()
        {
            Assert.Throws<ArgumentNullException>(() => new ObservableCircularBuffer<int>(1, null!));
        }

        [Fact]
        public void Ctor_DoesThrow_For_Too_Many_Items()
        {
            Assert.Throws<ArgumentException>(() => new ObservableCircularBuffer<int>(2, [1, 2, 3]));
        }

        [Fact]
        public void Ctor_Does_Initialize_With_Items()
        {
            var sut = new ObservableCircularBuffer<int>(3, [1, 2]);

            Assert.Multiple(() =>
            {
                Assert.Equal([1, 2], sut);
                Assert.Equal(2, sut.Size);
                Assert.Equal(3, sut.Capacity);
                Assert.False(sut.IsFull);
            });
        }

        [Fact]
        public void Push_Does_Add_To_The_Beginning()
        {
            var sut = new ObservableCircularBuffer<int>(3);

            sut.Push(1);
            sut.Push(2);

            Assert.Multiple(() =>
            {
                Assert.Equal([2, 1], sut);
                Assert.Equal(2, sut.Size);
                Assert.Equal(2, sut.First());
                Assert.False(sut.IsFull);
            });
        }

        [Fact]
        public void Push_Does_Remove_The_Oldest_Entry_When_Full()
        {
            var sut = new ObservableCircularBuffer<int>(3);

            for (var i = 1; i <= 5; i++)
            {
                sut.Push(i);
            }

            Assert.Multiple(() =>
            {
                Assert.Equal([5, 4, 3], sut);
                Assert.Equal(3, sut.Size);
                Assert.True(sut.IsFull);
            });
        }

        [Fact]
        public void Push_Does_Work_With_Capacity_Of_One()
        {
            var sut = new ObservableCircularBuffer<int>(1);

            sut.Push(1);
            sut.Push(2);

            Assert.Equal([2], sut);
        }

        [Fact]
        public void Push_Does_Raise_IsFull_Once()
        {
            var sut = new ObservableCircularBuffer<int>(2);
            var isFullNotifications = 0;
            sut.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ObservableCircularBuffer<int>.IsFull))
                {
                    isFullNotifications++;
                }
            };

            sut.Push(1);
            sut.Push(2);
            sut.Push(3);

            // once for each size change, none when the buffer stays full
            Assert.Equal(2, isFullNotifications);
        }

        [Fact]
        public void Push_Does_Raise_CollectionChanged()
        {
            var sut = new ObservableCircularBuffer<int>(1);
            var actions = new List<NotifyCollectionChangedAction>();
            sut.CollectionChanged += (_, e) => actions.Add(e.Action);

            sut.Push(1);
            sut.Push(2);

            Assert.Equal([NotifyCollectionChangedAction.Add, NotifyCollectionChangedAction.Remove, NotifyCollectionChangedAction.Add], actions);
        }

        [Fact]
        public void Capacity_Reduction_Does_Trim_The_Oldest_Entries()
        {
            var sut = new ObservableCircularBuffer<int>(4, [1, 2, 3, 4]);

            sut.Capacity = 2;

            Assert.Multiple(() =>
            {
                Assert.Equal([1, 2], sut);
                Assert.Equal(2, sut.Size);
                Assert.True(sut.IsFull);
            });
        }

        [Fact]
        public void Capacity_Increase_Does_Allow_More_Entries()
        {
            var sut = new ObservableCircularBuffer<int>(1, [1]);

            sut.Capacity = 2;
            sut.Push(2);

            Assert.Multiple(() =>
            {
                Assert.Equal([2, 1], sut);
                Assert.True(sut.IsFull);
            });
        }

        [Fact]
        public void Capacity_DoesThrow_For_Invalid_Value()
        {
            var sut = new ObservableCircularBuffer<int>(1);

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.Capacity = 0);
            Assert.Equal(1, sut.Capacity);
        }

        [Fact]
        public void Capacity_Change_Does_Raise_IsFull()
        {
            var sut = new ObservableCircularBuffer<int>(2, [1, 2]);
            var properties = new List<string?>();
            sut.PropertyChanged += (_, e) => properties.Add(e.PropertyName);

            sut.Capacity = 3;

            Assert.Contains(nameof(ObservableCircularBuffer<int>.IsFull), properties);
            Assert.False(sut.IsFull);
        }

        [Fact]
        public void Is_A_INotifyPropertyChanged()
        {
            Assert.IsAssignableFrom<INotifyPropertyChanged>(new ObservableCircularBuffer<int>(1));
        }
    }
}
