namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class AllowableCharactersTextBoxBehaviorServiceTests
    {
        [Theory]
        // full length original text, insert off, with text selection
        // paste
        [InlineData(true, "Mvvm", 4, 4, 0, true, "test", ".*", false)]
        [InlineData(true, "Mvvm", 4, 3, 1, true, "tes", ".*", false)]
        [InlineData(true, "Mvvm", 4, 2, 2, true, "te", ".*", false)]
        [InlineData(true, "Mvvm", 4, 1, 3, true, "t", ".*", false)]
        [InlineData(true, "Mvvm", 4, 0, 4, true, "", ".*", false)]
        [InlineData(true, "Mvvm", 4, 0, 4, true, null, ".*", false)]
        // edit
        [InlineData(true, "Mvvm", 4, 4, 0, false, "test", ".*", false)]
        [InlineData(true, "Mvvm", 4, 3, 1, false, "tes", ".*", false)]
        [InlineData(true, "Mvvm", 4, 2, 2, false, "te", ".*", false)]
        [InlineData(true, "Mvvm", 4, 1, 3, false, "t", ".*", false)]
        [InlineData(true, "Mvvm", 4, 0, 4, false, "", ".*", false)]
        [InlineData(true, "Mvvm", 4, 0, 4, false, null, ".*", false)]
        // not full length original text, insert off, with text selection
        // paste
        [InlineData(true, "Mvv", 4, 3, 0, true, "test", ".*", false)]
        [InlineData(true, "Mvv", 4, 3, 0, true, "tes", ".*", false)]
        [InlineData(true, "Mvv", 4, 2, 1, true, "te", ".*", false)]
        [InlineData(true, "Mvv", 4, 1, 2, true, "t", ".*", false)]
        [InlineData(true, "Mvv", 4, 0, 3, true, "", ".*", false)]
        [InlineData(true, "Mvv", 4, 0, 3, true, null, ".*", false)]
        // edit
        [InlineData(true, "Mvv", 4, 3, 0, false, "test", ".*", false)]
        [InlineData(true, "Mvv", 4, 3, 0, false, "tes", ".*", false)]
        [InlineData(true, "Mvv", 4, 2, 1, false, "te", ".*", false)]
        [InlineData(true, "Mvv", 4, 1, 2, false, "t", ".*", false)]
        [InlineData(true, "Mvv", 4, 0, 3, false, "", ".*", false)]
        [InlineData(true, "Mvv", 4, 0, 3, false, null, ".*", false)]
        // full length original text, insert on, with text selection
        // paste
        [InlineData(true, "Mvvm", 4, 4, 0, true, "test", ".*", true)]
        [InlineData(true, "Mvvm", 4, 3, 1, true, "tes", ".*", true)]
        [InlineData(true, "Mvvm", 4, 2, 2, true, "te", ".*", true)]
        [InlineData(true, "Mvvm", 4, 1, 3, true, "t", ".*", true)]
        [InlineData(true, "Mvvm", 4, 0, 4, true, "", ".*", true)]
        [InlineData(true, "Mvvm", 4, 0, 4, true, null, ".*", true)]
        // edit
        [InlineData(true, "Mvvm", 4, 4, 0, false, "test", ".*", true)]
        [InlineData(true, "Mvvm", 4, 3, 1, false, "tes", ".*", true)]
        [InlineData(true, "Mvvm", 4, 2, 2, false, "te", ".*", true)]
        [InlineData(true, "Mvvm", 4, 1, 3, false, "t", ".*", true)]
        [InlineData(true, "Mvvm", 4, 0, 4, false, "", ".*", true)]
        [InlineData(true, "Mvvm", 4, 0, 4, false, null, ".*", true)]
        // not full length original text, insert on, with text selection
        // paste
        [InlineData(true, "Mvv", 4, 3, 0, true, "test", ".*", true)]
        [InlineData(true, "Mvv", 4, 3, 0, true, "tes", ".*", true)]
        [InlineData(true, "Mvv", 4, 2, 1, true, "te", ".*", true)]
        [InlineData(true, "Mvv", 4, 1, 2, true, "t", ".*", true)]
        [InlineData(true, "Mvv", 4, 0, 3, true, "", ".*", true)]
        [InlineData(true, "Mvv", 4, 0, 3, true, null, ".*", true)]
        // edit
        [InlineData(true, "Mvv", 4, 3, 0, false, "test", ".*", true)]
        [InlineData(true, "Mvv", 4, 3, 0, false, "tes", ".*", true)]
        [InlineData(true, "Mvv", 4, 2, 1, false, "te", ".*", true)]
        [InlineData(true, "Mvv", 4, 1, 2, false, "t", ".*", true)]
        [InlineData(true, "Mvv", 4, 0, 3, false, "", ".*", true)]
        [InlineData(true, "Mvv", 4, 0, 3, false, null, ".*", true)]
        // full length original text, insert on, no text selection
        // paste
        [InlineData(true, "Mvvm", 4, 0, 0, true, "test", ".*", true)]
        [InlineData(true, "Mvvm", 4, 0, 1, true, "tes", ".*", true)]
        [InlineData(true, "Mvvm", 4, 0, 2, true, "te", ".*", true)]
        [InlineData(true, "Mvvm", 4, 0, 3, true, "t", ".*", true)]

        // edit
        [InlineData(true, "Mvvm", 4, 0, 0, false, "test", ".*", true)]
        [InlineData(true, "Mvvm", 4, 0, 1, false, "tes", ".*", true)]
        [InlineData(true, "Mvvm", 4, 0, 2, false, "te", ".*", true)]
        [InlineData(true, "Mvvm", 4, 0, 3, false, "t", ".*", true)]

        // not full length original text, insert on, no text selection
        // paste
        [InlineData(true, "Mvv", 4, 0, 0, true, "test", ".*", true)]
        [InlineData(true, "Mvv", 4, 0, 0, true, "tes", ".*", true)]
        [InlineData(true, "Mvv", 4, 0, 1, true, "te", ".*", true)]
        [InlineData(true, "Mvv", 4, 0, 2, true, "t", ".*", true)]

        // edit
        [InlineData(true, "Mvv", 4, 0, 0, false, "test", ".*", true)]
        [InlineData(true, "Mvv", 4, 0, 0, false, "tes", ".*", true)]
        [InlineData(true, "Mvv", 4, 0, 1, false, "te", ".*", true)]
        [InlineData(true, "Mvv", 4, 0, 2, false, "t", ".*", true)]

        // edge cases
        // maxlength allows no input
        [InlineData(true, "", 0, 0, 0, true, null, ".*", false)]
        [InlineData(true, "", 0, 0, 0, true, "", ".*", false)]
        [InlineData(true, "", 0, 0, 0, false, null, ".*", false)]
        [InlineData(true, "", 0, 0, 0, false, "", ".*", false)]

        // maxlength allows no input, but invalid input already inside, but we dont actually modify it
        [InlineData(true, "Mvvm", 0, 0, 0, true, "", ".*", false)]
        [InlineData(true, "Mvvm", 0, 0, 0, true, null, ".*", false)]
        [InlineData(true, "Mvvm", 0, 0, 0, false, "", ".*", false)]
        [InlineData(true, "Mvvm", 0, 0, 0, false, null, ".*", false)]

        // maxlength allows no input
        [InlineData(true, "", 0, 0, 0, true, null, ".*", true)]
        [InlineData(true, "", 0, 0, 0, true, "", ".*", true)]
        [InlineData(true, "", 0, 0, 0, false, null, ".*", true)]
        [InlineData(true, "", 0, 0, 0, false, "", ".*", true)]

        // maxlength allows no input, but invalid input already inside, but we dont actually modify it
        [InlineData(true, "Mvvm", 0, 0, 0, true, "", ".*", true)]
        [InlineData(true, "Mvvm", 0, 0, 0, true, null, ".*", true)]
        [InlineData(true, "Mvvm", 0, 0, 0, false, "", ".*", true)]
        [InlineData(true, "Mvvm", 0, 0, 0, false, null, ".*", true)]

        // maxlength exceeded
        // full length original text, insert on, with text selection
        [InlineData(false, "MvvM", 4, 4, 0, false, "TooLong", ".*", true)]
        [InlineData(false, "MvvM", 4, 3, 0, false, "TooLong", ".*", true)]
        [InlineData(false, "MvvM", 4, 2, 0, false, "TooLong", ".*", true)]
        [InlineData(false, "MvvM", 4, 1, 0, false, "TooLong", ".*", true)]
        [InlineData(false, "MvvM", 4, 0, 0, false, "TooLong", ".*", true)]

        // full length original text, insert off, with text selection
        [InlineData(false, "MvvM", 4, 4, 0, false, "TooLong", ".*", false)]
        [InlineData(false, "MvvM", 4, 3, 0, false, "TooLong", ".*", false)]
        [InlineData(false, "MvvM", 4, 2, 0, false, "TooLong", ".*", false)]
        [InlineData(false, "MvvM", 4, 1, 0, false, "TooLong", ".*", false)]
        [InlineData(false, "MvvM", 4, 0, 0, false, "TooLong", ".*", false)]

        // full length original text, insert on, with text selection, with paste
        [InlineData(false, "MvvM", 4, 4, 0, true, "TooLong", ".*", true)]
        [InlineData(false, "MvvM", 4, 3, 0, true, "TooLong", ".*", true)]
        [InlineData(false, "MvvM", 4, 2, 0, true, "TooLong", ".*", true)]
        [InlineData(false, "MvvM", 4, 1, 0, true, "TooLong", ".*", true)]
        [InlineData(false, "MvvM", 4, 0, 0, true, "TooLong", ".*", true)]

        // full length original text, insert off, with text selection, with paste
        [InlineData(false, "MvvM", 4, 4, 0, true, "TooLong", ".*", false)]
        [InlineData(false, "MvvM", 4, 3, 0, true, "TooLong", ".*", false)]
        [InlineData(false, "MvvM", 4, 2, 0, true, "TooLong", ".*", false)]
        [InlineData(false, "MvvM", 4, 1, 0, true, "TooLong", ".*", false)]
        [InlineData(false, "MvvM", 4, 0, 0, true, "TooLong", ".*", false)]
        public void IsValid_Should_Return_Expected_Result(bool expectedResult, string originalText, int maxLength, int selectedTextLength, int caretIndex, bool isPaste, string? newText, string regex, bool isInsertKeyToggled)
        {
            Assert.Equal(expectedResult, AllowableCharactersTextBoxBehaviorService.IsValid(newText, isPaste, selectedTextLength, maxLength, originalText, caretIndex, regex, isInsertKeyToggled));
        }
    }
}
