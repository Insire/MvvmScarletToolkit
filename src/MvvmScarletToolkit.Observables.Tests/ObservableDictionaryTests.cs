using System;
using System.Collections.Generic;
using System.Linq;

namespace MvvmScarletToolkit.Observables.Tests
{
    [TraceTest]
    public sealed class ObservableDictionaryTests
    {
        [Fact]
        public void Ctor_DoesNotThrow_1()
        {
            new ObservableDictionary<object, object>();
        }

        [Fact]
        public void Ctor_DoesNotThrow_2()
        {
            new ObservableDictionary<object, object>(new Dictionary<object, object>());
        }

        [Fact]
        public void Ctor_DoesNotThrow_3()
        {
            new ObservableDictionary<object, object>(EqualityComparer<object>.Default);
        }

        [Fact]
        public void Ctor_DoesNotThrow_4()
        {
            new ObservableDictionary<object, object>(0, EqualityComparer<object>.Default);
        }

        [Fact]
        public void Ctor_DoesNotThrow_5()
        {
            new ObservableDictionary<object, object>(new Dictionary<object, object>(), EqualityComparer<object>.Default);
        }

        [Fact]
        public void Ctor_DoesThrow_1()
        {
            Assert.Throws<ArgumentNullException>(() => new ObservableDictionary<object, object>(default(Dictionary<object, object>)!));
        }

        [Fact]
        public void Ctor_DoesNotThrow_6()
        {
            new ObservableDictionary<object, object>(default(IEqualityComparer<object>));
        }

        [Fact]
        public void Ctor_DoesThrow_2()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ObservableDictionary<object, object>(-1, default(IEqualityComparer<object>)));
        }

        [Fact]
        public void Ctor_DoesThrow_3()
        {
            Assert.Throws<ArgumentNullException>(() => new ObservableDictionary<object, object>(default(Dictionary<object, object>)!, default(IEqualityComparer<object>)));
        }

        [Fact]
        public void Ctor_Does_Initialize_With_Expected_Data_1()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            };

            Assert.Multiple(() =>
            {
                Assert.Equal(2, sut.Count);
                Assert.Equal(2, sut.Keys.Count);
                Assert.Equal(2, sut.Values.Count);
            });

            IReadOnlyDictionary<int, object> castedSut = sut;

            Assert.Multiple(() =>
            {
                Assert.Equal(2, castedSut.Keys.Count());
                Assert.Equal(2, castedSut.Values.Count());
            });
        }

        [Fact]
        public void Ctor_Does_Initialize_With_Expected_Data_2()
        {
            var sut = new ObservableDictionary<int, object>(new Dictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            });

            Assert.Multiple(() =>
            {
                Assert.Equal(2, sut.Count);
                Assert.Equal(2, sut.Keys.Count);
                Assert.Equal(2, sut.Values.Count);
            });

            IReadOnlyDictionary<int, object> castedSut = sut;

            Assert.Multiple(() =>
            {
                Assert.Equal(2, castedSut.Keys.Count());
                Assert.Equal(2, castedSut.Values.Count());
            });
        }

        [Fact]
        public void Indexer_DoesThrow_Get()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
            };

            Assert.Throws<KeyNotFoundException>(() => _ = sut[2]);
        }

        [Fact]
        public void Indexer_DoesNotThrow_Get()
        {
            var data = new object();
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = data,
            };

            Assert.Equal(sut[1], data);
        }

        [Fact]
        public void Indexer_DoesNotThrow_Set_1()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
            };

            sut[2] = new object();
        }

        [Fact]
        public void Indexer_DoesNotThrow_Set_2()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            };

            sut[1] = new object();
        }

        [Fact]
        public void Add_DoesNotThrow_1()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            };

            sut.Add(3, new object());
        }

        [Fact]
        public void Add_DoesNotThrow_2()
        {
            var sut = new ObservableDictionary<int, object>();

            sut.Add(1, new object());
        }

        [Fact]
        public void Add_DoesThrow_1()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
            };

            Assert.Throws<ArgumentException>(() => sut.Add(1, new object()));
        }

        [Fact]
        public void Add_DoesThrow_2()
        {
            var sut = new ObservableDictionary<object, object>()
            {
                [new object()] = new object(),
            };

            Assert.Throws<ArgumentNullException>(() => sut.Add(null!, new object()));
        }

        [Fact]
        public void ContainsKey_DoesNotThrow_1()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            };

            Assert.False(sut.ContainsKey(3));
        }

        [Fact]
        public void ContainsKey_DoesNotThrow_2()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            };

            Assert.True(sut.ContainsKey(2));
        }

        [Fact]
        public void Clear_Does_Empty_Collection()
        {
            var sut = new ObservableDictionary<int, object>(new Dictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            });

            sut.Clear();

            Assert.Multiple(() =>
            {
                Assert.Empty(sut);
                Assert.Empty(sut.Keys);
                Assert.Empty(sut.Values);
            });

            IReadOnlyDictionary<int, object> castedSut = sut;

            Assert.Multiple(() =>
            {
                Assert.Empty(castedSut.Keys);
                Assert.Empty(castedSut.Values);
            });
        }

        [Fact]
        public void Clear_Does_Work_On_Empty_Collection()
        {
            var sut = new ObservableDictionary<int, object>();

            sut.Clear();

            Assert.Multiple(() =>
            {
                Assert.Empty(sut);
                Assert.Empty(sut.Keys);
                Assert.Empty(sut.Values);
            });

            IReadOnlyDictionary<int, object> castedSut = sut;

            Assert.Multiple(() =>
            {
                Assert.Empty(castedSut.Keys);
                Assert.Empty(castedSut.Values);
            });
        }

        [Fact]
        public void Remove_Does_Work()
        {
            var sut = new ObservableDictionary<int, object>(new Dictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            });

            Assert.Multiple(() =>
            {
                Assert.True(sut.Remove(2));
                Assert.Single(sut);
                Assert.Single(sut.Keys);
                Assert.Single(sut.Values);
            });

            IReadOnlyDictionary<int, object> castedSut = sut;

            Assert.Multiple(() =>
            {
                Assert.Single(castedSut.Keys);
                Assert.Single(castedSut.Values);

                Assert.False(sut.Remove(2));
            });
        }

        [Fact]
        public void Remove_Does_Not_Throw_1()
        {
            var sut = new ObservableDictionary<int, object>(new Dictionary<int, object>()
            {
                [1] = new object(),
            });

            Assert.Multiple(() =>
            {
                Assert.False(sut.Remove(2));
                Assert.Single(sut);
                Assert.Single(sut.Keys);
                Assert.Single(sut.Values);
            });

            IReadOnlyDictionary<int, object> castedSut = sut;

            Assert.Multiple(() =>
            {
                Assert.Single(castedSut.Keys);
                Assert.Single(castedSut.Values);

                Assert.False(sut.Remove(2));
            });
        }

        [Fact]
        public void Remove_Does_Not_Throw_2()
        {
            var sut = new ObservableDictionary<int, object>(new Dictionary<int, object>()
            {
                [1] = new object(),
            });

            Assert.Multiple(() =>
            {
                Assert.True(sut.Remove(1));
                Assert.Empty(sut);
                Assert.Empty(sut.Keys);
                Assert.Empty(sut.Values);
            });

            IReadOnlyDictionary<int, object> castedSut = sut;

            Assert.Multiple(() =>
            {
                Assert.Empty(castedSut.Keys);
                Assert.Empty(castedSut.Values);

                Assert.False(sut.Remove(1));
            });
        }

        [Fact]
        public void Remove_Does_Not_Throw_3()
        {
            var sut = new ObservableDictionary<int, object>();

            Assert.False(sut.Remove(2));
        }

        [Fact]
        public void TryGetValue_Does_Work_1()
        {
            var expectedResult = new object();
            var sut = new ObservableDictionary<int, object>(new Dictionary<int, object>()
            {
                [1] = new object(),
                [2] = expectedResult,
            });

            Assert.Multiple(() =>
            {
                Assert.True(sut.TryGetValue(2, out var result));
                Assert.Equal(expectedResult, result);
            });
        }

        [Fact]
        public void TryGetValue_Does_Work_2()
        {
            var expectedResult = new object();
            var sut = new ObservableDictionary<int, object>(new Dictionary<int, object>()
            {
                [1] = new object(),
                [2] = expectedResult,
            });

            Assert.Multiple(() =>
            {
                Assert.False(sut.TryGetValue(3, out var result));
                Assert.Null(result);
            });
        }
    }
}
