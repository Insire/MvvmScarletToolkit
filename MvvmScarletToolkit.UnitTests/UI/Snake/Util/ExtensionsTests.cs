using Microsoft.VisualStudio.TestTools.UnitTesting;

using MvvmScarletToolkit.UnitTests.Mocks;

using System.Collections.Generic;

namespace MvvmScarletToolkit.UnitTests.UI.Snake.Util
{
    [TestClass]
    public class ExtensionsTests
    {
        private static readonly List<KeyValuePair<IPositionable, IPositionable>> _data = new List<KeyValuePair<IPositionable, IPositionable>>()
        {
            new KeyValuePair<IPositionable, IPositionable>(new Positionable()
                {
                    CurrentPosition = new Position(0,0),
                    Size = new Size(4,4),
                },new Positionable()
                {
                    CurrentPosition = new Position(9,9),
                    Size = new Size(4,4),
                }),

                new KeyValuePair<IPositionable, IPositionable>(new Positionable()
                {
                    CurrentPosition = new Position(-1,-1),
                    Size = new Size(4,4),
                },new Positionable()
                {
                    CurrentPosition = new Position(8,8),
                    Size = new Size(4,4),
                }),

                new KeyValuePair<IPositionable, IPositionable>(new Positionable()
                {
                    CurrentPosition = new Position(0,-1),
                    Size = new Size(4,4),
                },new Positionable()
                {
                    CurrentPosition = new Position(9,9),
                    Size = new Size(4,4),
                }),
        };

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void Should_NotIntersect(int index)
        {
            var pair = _data[index];
            var left = pair.Key;
            var right = pair.Value;

            Assert.AreEqual(false, Extensions.Intersect(left, right));
        }
    }
}
