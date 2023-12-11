using NUnit.Framework;

namespace MvvmScarletToolkit.Wpf.Tests
{
    [TestFixture]
    public sealed class AllowableCharactersTextBoxBehaviorServiceTests
    {
        [Test]
        // full length original text, insert off, with text selection
        // paste
        [TestCase(true, "Mvvm", 4, 4, 0, true, "test", ".*", false)]
        [TestCase(true, "Mvvm", 4, 3, 1, true, "tes", ".*", false)]
        [TestCase(true, "Mvvm", 4, 2, 2, true, "te", ".*", false)]
        [TestCase(true, "Mvvm", 4, 1, 3, true, "t", ".*", false)]
        [TestCase(true, "Mvvm", 4, 0, 4, true, "", ".*", false)]
        [TestCase(true, "Mvvm", 4, 0, 4, true, null, ".*", false)]
        // edit
        [TestCase(true, "Mvvm", 4, 4, 0, false, "test", ".*", false)]
        [TestCase(true, "Mvvm", 4, 3, 1, false, "tes", ".*", false)]
        [TestCase(true, "Mvvm", 4, 2, 2, false, "te", ".*", false)]
        [TestCase(true, "Mvvm", 4, 1, 3, false, "t", ".*", false)]
        [TestCase(true, "Mvvm", 4, 0, 4, false, "", ".*", false)]
        [TestCase(true, "Mvvm", 4, 0, 4, false, null, ".*", false)]
        // not full length original text, insert off, with text selection
        // paste
        [TestCase(true, "Mvv", 4, 3, 0, true, "test", ".*", false)]
        [TestCase(true, "Mvv", 4, 3, 0, true, "tes", ".*", false)]
        [TestCase(true, "Mvv", 4, 2, 1, true, "te", ".*", false)]
        [TestCase(true, "Mvv", 4, 1, 2, true, "t", ".*", false)]
        [TestCase(true, "Mvv", 4, 0, 3, true, "", ".*", false)]
        [TestCase(true, "Mvv", 4, 0, 3, true, null, ".*", false)]
        // edit
        [TestCase(true, "Mvv", 4, 3, 0, false, "test", ".*", false)]
        [TestCase(true, "Mvv", 4, 3, 0, false, "tes", ".*", false)]
        [TestCase(true, "Mvv", 4, 2, 1, false, "te", ".*", false)]
        [TestCase(true, "Mvv", 4, 1, 2, false, "t", ".*", false)]
        [TestCase(true, "Mvv", 4, 0, 3, false, "", ".*", false)]
        [TestCase(true, "Mvv", 4, 0, 3, false, null, ".*", false)]
        // full length original text, insert on, with text selection
        // paste
        [TestCase(true, "Mvvm", 4, 4, 0, true, "test", ".*", true)]
        [TestCase(true, "Mvvm", 4, 3, 1, true, "tes", ".*", true)]
        [TestCase(true, "Mvvm", 4, 2, 2, true, "te", ".*", true)]
        [TestCase(true, "Mvvm", 4, 1, 3, true, "t", ".*", true)]
        [TestCase(true, "Mvvm", 4, 0, 4, true, "", ".*", true)]
        [TestCase(true, "Mvvm", 4, 0, 4, true, null, ".*", true)]
        // edit
        [TestCase(true, "Mvvm", 4, 4, 0, false, "test", ".*", true)]
        [TestCase(true, "Mvvm", 4, 3, 1, false, "tes", ".*", true)]
        [TestCase(true, "Mvvm", 4, 2, 2, false, "te", ".*", true)]
        [TestCase(true, "Mvvm", 4, 1, 3, false, "t", ".*", true)]
        [TestCase(true, "Mvvm", 4, 0, 4, false, "", ".*", true)]
        [TestCase(true, "Mvvm", 4, 0, 4, false, null, ".*", true)]
        // not full length original text, insert on, with text selection
        // paste
        [TestCase(true, "Mvv", 4, 3, 0, true, "test", ".*", true)]
        [TestCase(true, "Mvv", 4, 3, 0, true, "tes", ".*", true)]
        [TestCase(true, "Mvv", 4, 2, 1, true, "te", ".*", true)]
        [TestCase(true, "Mvv", 4, 1, 2, true, "t", ".*", true)]
        [TestCase(true, "Mvv", 4, 0, 3, true, "", ".*", true)]
        [TestCase(true, "Mvv", 4, 0, 3, true, null, ".*", true)]
        // edit
        [TestCase(true, "Mvv", 4, 3, 0, false, "test", ".*", true)]
        [TestCase(true, "Mvv", 4, 3, 0, false, "tes", ".*", true)]
        [TestCase(true, "Mvv", 4, 2, 1, false, "te", ".*", true)]
        [TestCase(true, "Mvv", 4, 1, 2, false, "t", ".*", true)]
        [TestCase(true, "Mvv", 4, 0, 3, false, "", ".*", true)]
        [TestCase(true, "Mvv", 4, 0, 3, false, null, ".*", true)]
        // full length original text, insert on, no text selection
        // paste
        [TestCase(true, "Mvvm", 4, 0, 0, true, "test", ".*", true)]
        [TestCase(true, "Mvvm", 4, 0, 1, true, "tes", ".*", true)]
        [TestCase(true, "Mvvm", 4, 0, 2, true, "te", ".*", true)]
        [TestCase(true, "Mvvm", 4, 0, 3, true, "t", ".*", true)]
        [TestCase(true, "Mvvm", 4, 0, 4, true, "", ".*", true)]
        [TestCase(true, "Mvvm", 4, 0, 4, true, null, ".*", true)]
        // edit
        [TestCase(true, "Mvvm", 4, 0, 0, false, "test", ".*", true)]
        [TestCase(true, "Mvvm", 4, 0, 1, false, "tes", ".*", true)]
        [TestCase(true, "Mvvm", 4, 0, 2, false, "te", ".*", true)]
        [TestCase(true, "Mvvm", 4, 0, 3, false, "t", ".*", true)]
        [TestCase(true, "Mvvm", 4, 0, 4, false, "", ".*", true)]
        [TestCase(true, "Mvvm", 4, 0, 4, false, null, ".*", true)]
        // not full length original text, insert on, no text selection
        // paste
        [TestCase(true, "Mvv", 4, 0, 0, true, "test", ".*", true)]
        [TestCase(true, "Mvv", 4, 0, 0, true, "tes", ".*", true)]
        [TestCase(true, "Mvv", 4, 0, 1, true, "te", ".*", true)]
        [TestCase(true, "Mvv", 4, 0, 2, true, "t", ".*", true)]
        [TestCase(true, "Mvv", 4, 0, 3, true, "", ".*", true)]
        [TestCase(true, "Mvv", 4, 0, 3, true, null, ".*", true)]
        // edit
        [TestCase(true, "Mvv", 4, 0, 0, false, "test", ".*", true)]
        [TestCase(true, "Mvv", 4, 0, 0, false, "tes", ".*", true)]
        [TestCase(true, "Mvv", 4, 0, 1, false, "te", ".*", true)]
        [TestCase(true, "Mvv", 4, 0, 2, false, "t", ".*", true)]
        [TestCase(true, "Mvv", 4, 0, 3, false, "", ".*", true)]
        [TestCase(true, "Mvv", 4, 0, 3, false, null, ".*", true)]

        // edge cases
        // maxlength allows no input
        [TestCase(true, "", 0, 0, 0, true, null, ".*", false)]
        [TestCase(true, "", 0, 0, 0, true, "", ".*", false)]
        [TestCase(true, "", 0, 0, 0, false, null, ".*", false)]
        [TestCase(true, "", 0, 0, 0, false, "", ".*", false)]

        // maxlength allows no input, but invalid input already inside, but we dont actually modify it
        [TestCase(true, "Mvvm", 0, 0, 0, true, "", ".*", false)]
        [TestCase(true, "Mvvm", 0, 0, 0, true, null, ".*", false)]
        [TestCase(true, "Mvvm", 0, 0, 0, false, "", ".*", false)]
        [TestCase(true, "Mvvm", 0, 0, 0, false, null, ".*", false)]

        // maxlength allows no input
        [TestCase(true, "", 0, 0, 0, true, null, ".*", true)]
        [TestCase(true, "", 0, 0, 0, true, "", ".*", true)]
        [TestCase(true, "", 0, 0, 0, false, null, ".*", true)]
        [TestCase(true, "", 0, 0, 0, false, "", ".*", true)]

        // maxlength allows no input, but invalid input already inside, but we dont actually modify it
        [TestCase(true, "Mvvm", 0, 0, 0, true, "", ".*", true)]
        [TestCase(true, "Mvvm", 0, 0, 0, true, null, ".*", true)]
        [TestCase(true, "Mvvm", 0, 0, 0, false, "", ".*", true)]
        [TestCase(true, "Mvvm", 0, 0, 0, false, null, ".*", true)]

        // maxlength exceeded
        // full length original text, insert on, with text selection
        [TestCase(false, "MvvM", 4, 4, 0, false, "TooLong", ".*", true)]
        [TestCase(false, "MvvM", 4, 3, 0, false, "TooLong", ".*", true)]
        [TestCase(false, "MvvM", 4, 2, 0, false, "TooLong", ".*", true)]
        [TestCase(false, "MvvM", 4, 1, 0, false, "TooLong", ".*", true)]
        [TestCase(false, "MvvM", 4, 0, 0, false, "TooLong", ".*", true)]

        // full length original text, insert off, with text selection
        [TestCase(false, "MvvM", 4, 4, 0, false, "TooLong", ".*", false)]
        [TestCase(false, "MvvM", 4, 3, 0, false, "TooLong", ".*", false)]
        [TestCase(false, "MvvM", 4, 2, 0, false, "TooLong", ".*", false)]
        [TestCase(false, "MvvM", 4, 1, 0, false, "TooLong", ".*", false)]
        [TestCase(false, "MvvM", 4, 0, 0, false, "TooLong", ".*", false)]

        // full length original text, insert on, with text selection, with paste
        [TestCase(false, "MvvM", 4, 4, 0, true, "TooLong", ".*", true)]
        [TestCase(false, "MvvM", 4, 3, 0, true, "TooLong", ".*", true)]
        [TestCase(false, "MvvM", 4, 2, 0, true, "TooLong", ".*", true)]
        [TestCase(false, "MvvM", 4, 1, 0, true, "TooLong", ".*", true)]
        [TestCase(false, "MvvM", 4, 0, 0, true, "TooLong", ".*", true)]

        // full length original text, insert off, with text selection, with paste
        [TestCase(false, "MvvM", 4, 4, 0, true, "TooLong", ".*", false)]
        [TestCase(false, "MvvM", 4, 3, 0, true, "TooLong", ".*", false)]
        [TestCase(false, "MvvM", 4, 2, 0, true, "TooLong", ".*", false)]
        [TestCase(false, "MvvM", 4, 1, 0, true, "TooLong", ".*", false)]
        [TestCase(false, "MvvM", 4, 0, 0, true, "TooLong", ".*", false)]
        public void IsValid_Should_Return_Expected_Result(bool expectedResult, string originalText, int maxLength, int selectedTextLength, int caretIndex, bool isPaste, string newText, string regex, bool isInsertKeyToggled)
        {
            Assert.AreEqual(expectedResult, AllowableCharactersTextBoxBehaviorService.IsValid(newText, isPaste, selectedTextLength, maxLength, originalText, caretIndex, regex, isInsertKeyToggled));
        }
    }
}
