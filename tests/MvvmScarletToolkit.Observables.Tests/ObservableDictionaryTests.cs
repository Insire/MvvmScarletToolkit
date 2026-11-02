using System.Collections.Specialized;

namespace MvvmScarletToolkit.Observables.Tests
{
    public sealed class ObservableDictionaryTests : TraceTestBase
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

        [Fact]
        public void Indexer_Set_Does_Replace_Existing_Entry()
        {
            var sut = new ObservableDictionary<int, string>()
            {
                [1] = "a",
                [2] = "b",
            };
            var actions = new List<NotifyCollectionChangedAction>();
            sut.CollectionChanged += (_, e) => actions.Add(e.Action);

            sut[1] = "c";

            Assert.Multiple(() =>
            {
                Assert.Equal(2, sut.Count);
                Assert.Equal("c", sut[1]);
                Assert.Equal(new KeyValuePair<int, string>(1, "c"), ((IList<KeyValuePair<int, string>>)sut)[0]);
                Assert.Equal([NotifyCollectionChangedAction.Replace], actions);
            });
        }

        [Fact]
        public void Indexer_Set_Does_Respect_The_EqualityComparer()
        {
            var sut = new ObservableDictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["key"] = 1,
            };

            sut["KEY"] = 2;

            Assert.Multiple(() =>
            {
                Assert.Single(sut);
                Assert.Single(sut.Keys);
                Assert.Equal(2, sut["key"]);
            });
        }

        [Fact]
        public void Remove_Does_Respect_The_EqualityComparer()
        {
            var sut = new ObservableDictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["key"] = 1,
            };

            Assert.Multiple(() =>
            {
                Assert.True(sut.Remove("KEY"));
                Assert.Empty(sut);
                Assert.Empty(sut.Keys);
            });
        }

        [Fact]
        public void Remove_Does_Raise_CollectionChanged()
        {
            var sut = new ObservableDictionary<int, string>()
            {
                [1] = "a",
            };
            var actions = new List<NotifyCollectionChangedAction>();
            sut.CollectionChanged += (_, e) => actions.Add(e.Action);

            sut.Remove(1);

            Assert.Equal([NotifyCollectionChangedAction.Remove], actions);
        }

        [Fact]
        public void Collection_Add_DoesThrow_For_Duplicate_Key()
        {
            var sut = new ObservableDictionary<int, string>()
            {
                [1] = "a",
            };

            Assert.Throws<ArgumentException>(() => ((ICollection<KeyValuePair<int, string>>)sut).Add(new KeyValuePair<int, string>(1, "b")));

            Assert.Multiple(() =>
            {
                Assert.Single(sut);
                Assert.Equal("a", sut[1]);
            });
        }

        [Fact]
        public void Insert_DoesThrow_For_Duplicate_Key()
        {
            var sut = new ObservableDictionary<int, string>()
            {
                [1] = "a",
            };

            Assert.Throws<ArgumentException>(() => sut.Insert(0, new KeyValuePair<int, string>(1, "b")));
            Assert.Single(sut);
        }

        [Fact]
        public void Insert_Does_Add_To_The_Dictionary()
        {
            var sut = new ObservableDictionary<int, string>()
            {
                [1] = "a",
            };

            sut.Insert(0, new KeyValuePair<int, string>(2, "b"));

            Assert.Multiple(() =>
            {
                Assert.Equal("b", sut[2]);
                Assert.Equal(2, ((IList<KeyValuePair<int, string>>)sut)[0].Key);
            });
        }

        [Fact]
        public void SetItem_With_New_Key_Does_Remove_The_Old_Key()
        {
            var sut = new ObservableDictionary<int, string>()
            {
                [1] = "a",
            };

            ((IList<KeyValuePair<int, string>>)sut)[0] = new KeyValuePair<int, string>(2, "b");

            Assert.Multiple(() =>
            {
                Assert.False(sut.ContainsKey(1));
                Assert.True(sut.ContainsKey(2));
                Assert.Single(sut.Keys);
                Assert.Single(sut);
            });
        }

        [Fact]
        public void SetItem_With_Existing_Key_Of_Another_Entry_DoesThrow()
        {
            var sut = new ObservableDictionary<int, string>()
            {
                [1] = "a",
                [2] = "b",
            };

            Assert.Throws<ArgumentException>(() => ((IList<KeyValuePair<int, string>>)sut)[0] = new KeyValuePair<int, string>(2, "c"));

            Assert.Multiple(() =>
            {
                Assert.Equal("a", sut[1]);
                Assert.Equal("b", sut[2]);
            });
        }

        [Fact]
        public void RemoveAt_Does_Remove_From_The_Dictionary()
        {
            var sut = new ObservableDictionary<int, string>()
            {
                [1] = "a",
                [2] = "b",
            };

            sut.RemoveAt(0);

            Assert.Multiple(() =>
            {
                Assert.False(sut.ContainsKey(1));
                Assert.Equal("b", sut[2]);
            });
        }

        [Fact]
        public void Keys_And_Values_Stay_In_Sync_With_The_Collection()
        {
            var sut = new ObservableDictionary<int, string>();

            sut.Add(1, "a");
            sut[2] = "b";
            sut[1] = "c";
            sut.Remove(2);
            sut.Add(3, "d");

            Assert.Multiple(() =>
            {
                Assert.Equal(sut.Select(p => p.Key).OrderBy(p => p), sut.Keys.OrderBy(p => p));
                Assert.Equal(sut.Select(p => p.Value).OrderBy(p => p), sut.Values.OrderBy(p => p));
            });
        }
    }
}
