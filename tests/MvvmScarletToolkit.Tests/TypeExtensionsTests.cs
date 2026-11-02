namespace MvvmScarletToolkit.Tests
{
    public sealed class TypeExtensionsTests : TraceTestBase
    {
        [Fact]
        public void GetGenericTypeName_Should_Return_The_Name_Of_A_Plain_Type()
        {
            Assert.Equal("String", typeof(string).GetGenericTypeName());
        }

        [Fact]
        public void GetGenericTypeName_Should_Replace_The_Arity_With_The_Type_Argument()
        {
            Assert.Equal("List<Int32>", typeof(List<int>).GetGenericTypeName());
        }

        [Fact]
        public void GetGenericTypeName_Should_Resolve_Nested_Type_Arguments()
        {
            Assert.Equal("List<List<Int32>>", typeof(List<List<int>>).GetGenericTypeName());
        }

        [Fact]
        public void GetGenericTypeName_Should_Resolve_Types_With_Several_Arguments()
        {
            Assert.Equal("Dictionary<String, Int32>", typeof(Dictionary<string, int>).GetGenericTypeName());
        }

        [Fact]
        public void GetGenericTypeName_Should_Resolve_Arguments_With_Several_Arguments()
        {
            Assert.Equal("List<Dictionary<String, List<Int32>>>", typeof(List<Dictionary<string, List<int>>>).GetGenericTypeName());
        }

        private sealed class Outer<T>
        {
            public sealed class Inner;
        }

        [Fact]
        public void GetGenericTypeName_Should_Return_The_Name_Of_A_Type_Nested_In_A_Generic_Type()
        {
            Assert.Equal("Inner", typeof(Outer<int>.Inner).GetGenericTypeName());
        }
    }
}
