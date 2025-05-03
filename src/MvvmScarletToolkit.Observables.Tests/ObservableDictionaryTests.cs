using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MvvmScarletToolkit.Observables.Tests
{
    internal sealed class ObservableDictionaryTests
    {
        [Test]
        public void Ctor_DoesNotThrow_1()
        {
            new ObservableDictionary<object, object>();
        }

        [Test]
        public void Ctor_DoesNotThrow_2()
        {
            new ObservableDictionary<object, object>(new Dictionary<object, object>());
        }

        [Test]
        public void Ctor_DoesNotThrow_3()
        {
            new ObservableDictionary<object, object>(EqualityComparer<object>.Default);
        }

        [Test]
        public void Ctor_DoesNotThrow_4()
        {
            new ObservableDictionary<object, object>(0, EqualityComparer<object>.Default);
        }

        [Test]
        public void Ctor_DoesNotThrow_5()
        {
            new ObservableDictionary<object, object>(new Dictionary<object, object>(), EqualityComparer<object>.Default);
        }

        [Test]
        public void Ctor_DoesThrow_1()
        {
            Assert.Throws<ArgumentNullException>(() => new ObservableDictionary<object, object>(default(Dictionary<object, object>)!));
        }

        [Test]
        public void Ctor_DoesNotThrow_6()
        {
            new ObservableDictionary<object, object>(default(IEqualityComparer<object>));
        }

        [Test]
        public void Ctor_DoesThrow_2()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ObservableDictionary<object, object>(-1, default(IEqualityComparer<object>)));
        }

        [Test]
        public void Ctor_DoesThrow_3()
        {
            Assert.Throws<ArgumentNullException>(() => new ObservableDictionary<object, object>(default(Dictionary<object, object>)!, default(IEqualityComparer<object>)));
        }

        [Test]
        public void Ctor_Does_Initialize_With_Expected_Data_1()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            };

            Assert.Multiple(() =>
            {
                Assert.That(sut, Has.Count.EqualTo(2));
                Assert.That(sut.Keys, Has.Count.EqualTo(2));
                Assert.That(sut.Values, Has.Count.EqualTo(2));
            });

            IReadOnlyDictionary<int, object> castedSut = sut;

            Assert.Multiple(() =>
            {
                Assert.That(castedSut.Keys.Count(), Is.EqualTo(2));
                Assert.That(castedSut.Values.Count(), Is.EqualTo(2));
            });
        }

        [Test]
        public void Ctor_Does_Initialize_With_Expected_Data_2()
        {
            var sut = new ObservableDictionary<int, object>(new Dictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            });

            Assert.Multiple(() =>
            {
                Assert.That(sut, Has.Count.EqualTo(2));
                Assert.That(sut.Keys, Has.Count.EqualTo(2));
                Assert.That(sut.Values, Has.Count.EqualTo(2));
            });

            IReadOnlyDictionary<int, object> castedSut = sut;

            Assert.Multiple(() =>
            {
                Assert.That(castedSut.Keys.Count(), Is.EqualTo(2));
                Assert.That(castedSut.Values.Count(), Is.EqualTo(2));
            });
        }

        [Test]
        public void Indexer_DoesThrow_Get()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
            };

            Assert.Throws<KeyNotFoundException>(() => _ = sut[2]);
        }

        [Test]
        public void Indexer_DoesNotThrow_Get()
        {
            var data = new object();
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = data,
            };

            Assert.That(data, Is.EqualTo(sut[1]));
        }

        [Test]
        public void Indexer_DoesNotThrow_Set_1()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
            };

            sut[2] = new object();
        }

        [Test]
        public void Indexer_DoesNotThrow_Set_2()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            };

            sut[1] = new object();
        }

        [Test]
        public void Add_DoesNotThrow_1()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            };

            sut.Add(3, new object());
        }

        [Test]
        public void Add_DoesNotThrow_2()
        {
            var sut = new ObservableDictionary<int, object>();

            sut.Add(1, new object());
        }

        [Test]
        public void Add_DoesThrow_1()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
            };

            Assert.Throws<ArgumentException>(() => sut.Add(1, new object()));
        }

        [Test]
        public void Add_DoesThrow_2()
        {
            var sut = new ObservableDictionary<object, object>()
            {
                [new object()] = new object(),
            };

            Assert.Throws<ArgumentNullException>(() => sut.Add(null!, new object()));
        }

        [Test]
        public void ContainsKey_DoesNotThrow_1()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            };

            Assert.That(sut.ContainsKey(3), Is.EqualTo(false));
        }

        [Test]
        public void ContainsKey_DoesNotThrow_2()
        {
            var sut = new ObservableDictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            };

            Assert.That(sut.ContainsKey(2), Is.EqualTo(true));
        }

        [Test]
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
                Assert.That(sut, Is.Empty);
                Assert.That(sut.Keys, Is.Empty);
                Assert.That(sut.Values, Is.Empty);
            });

            IReadOnlyDictionary<int, object> castedSut = sut;

            Assert.Multiple(() =>
            {
                Assert.That(castedSut.Keys.Count(), Is.EqualTo(0));
                Assert.That(castedSut.Values.Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public void Clear_Does_Work_On_Empty_Collection()
        {
            var sut = new ObservableDictionary<int, object>();

            sut.Clear();

            Assert.Multiple(() =>
            {
                Assert.That(sut, Is.Empty);
                Assert.That(sut.Keys, Is.Empty);
                Assert.That(sut.Values, Is.Empty);
            });

            IReadOnlyDictionary<int, object> castedSut = sut;

            Assert.Multiple(() =>
            {
                Assert.That(castedSut.Keys.Count(), Is.EqualTo(0));
                Assert.That(castedSut.Values.Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public void Remove_Does_Work()
        {
            var sut = new ObservableDictionary<int, object>(new Dictionary<int, object>()
            {
                [1] = new object(),
                [2] = new object(),
            });

            Assert.Multiple(() =>
            {
                Assert.That(sut.Remove(2), Is.EqualTo(true));
                Assert.That(sut, Has.Count.EqualTo(1));
                Assert.That(sut.Keys, Has.Count.EqualTo(1));
                Assert.That(sut.Values, Has.Count.EqualTo(1));
            });

            IReadOnlyDictionary<int, object> castedSut = sut;

            Assert.Multiple(() =>
            {
                Assert.That(castedSut.Keys.Count(), Is.EqualTo(1));
                Assert.That(castedSut.Values.Count(), Is.EqualTo(1));

                Assert.That(sut.Remove(2), Is.EqualTo(false));
            });
        }

        [Test]
        public void Remove_Does_Not_Throw_1()
        {
            var sut = new ObservableDictionary<int, object>(new Dictionary<int, object>()
            {
                [1] = new object(),
            });

            Assert.Multiple(() =>
            {
                Assert.That(sut.Remove(2), Is.EqualTo(false));
                Assert.That(sut, Has.Count.EqualTo(1));
                Assert.That(sut.Keys, Has.Count.EqualTo(1));
                Assert.That(sut.Values, Has.Count.EqualTo(1));
            });

            IReadOnlyDictionary<int, object> castedSut = sut;

            Assert.Multiple(() =>
            {
                Assert.That(castedSut.Keys.Count(), Is.EqualTo(1));
                Assert.That(castedSut.Values.Count(), Is.EqualTo(1));

                Assert.That(sut.Remove(2), Is.EqualTo(false));
            });
        }

        [Test]
        public void Remove_Does_Not_Throw_2()
        {
            var sut = new ObservableDictionary<int, object>(new Dictionary<int, object>()
            {
                [1] = new object(),
            });

            Assert.Multiple(() =>
            {
                Assert.That(sut.Remove(1), Is.EqualTo(true));
                Assert.That(sut, Is.Empty);
                Assert.That(sut.Keys, Is.Empty);
                Assert.That(sut.Values, Is.Empty);
            });

            IReadOnlyDictionary<int, object> castedSut = sut;

            Assert.Multiple(() =>
            {
                Assert.That(castedSut.Keys.Count(), Is.EqualTo(0));
                Assert.That(castedSut.Values.Count(), Is.EqualTo(0));

                Assert.That(sut.Remove(1), Is.EqualTo(false));
            });
        }

        [Test]
        public void Remove_Does_Not_Throw_3()
        {
            var sut = new ObservableDictionary<int, object>();

            Assert.That(sut.Remove(2), Is.EqualTo(false));
        }

        [Test]
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
                Assert.That(sut.TryGetValue(2, out var result), Is.EqualTo(true));
                Assert.That(result, Is.EqualTo(expectedResult));
            });
        }

        [Test]
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
                Assert.That(sut.TryGetValue(3, out var result), Is.EqualTo(false));
                Assert.That(result, Is.EqualTo(null));
            });
        }
    }
}
