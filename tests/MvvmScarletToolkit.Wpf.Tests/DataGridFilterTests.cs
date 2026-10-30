using Microsoft.Xaml.Behaviors;
using MvvmScarletToolkit.Wpf;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class DataGridFilterTests : TraceTestBase
    {
        public enum Kind
        {
            First,
            Second,
        }

        public sealed class Row
        {
            public string? Name { get; set; }
            public int Age { get; set; }
            public decimal? Price { get; set; }
            public bool IsActive { get; set; }
            public Kind Kind { get; set; }
            public DateTime Created { get; set; }
            public object? Anything { get; set; }
        }

        private static readonly Row _alice = new() { Name = "Alice", Age = 30, Price = 1.5m, IsActive = true, Kind = Kind.First, Created = new DateTime(2020, 1, 1), Anything = 7 };
        private static readonly Row _bob = new() { Name = "Bob", Age = 40, Price = null, IsActive = false, Kind = Kind.Second, Created = new DateTime(2022, 1, 1) };

        private static DataGridFilterViewModel CreateFilter()
        {
            return DataGridFilterViewModel.Create<Row>(Utils.GetTestCommandBuilder(new TestDispatcher()));
        }

        [Fact]
        public void Without_Values_Everything_Should_Match()
        {
            var sut = CreateFilter();
            sut.GetFilter(nameof(Row.Name));

            Assert.True(sut.Predicate(_alice));
        }

        [Theory]
        [InlineData(nameof(Row.Name), ColumnFilterOperator.Contains, "LIC", true, false)]
        [InlineData(nameof(Row.Name), ColumnFilterOperator.Equals, "bob", false, true)]
        [InlineData(nameof(Row.Age), ColumnFilterOperator.GreaterThan, "30", false, true)]
        [InlineData(nameof(Row.Age), ColumnFilterOperator.GreaterThanOrEqual, "30", true, true)]
        [InlineData(nameof(Row.Age), ColumnFilterOperator.LessThan, "40", true, false)]
        [InlineData(nameof(Row.Age), ColumnFilterOperator.NotEquals, "30", false, true)]
        [InlineData(nameof(Row.IsActive), ColumnFilterOperator.Equals, "true", true, false)]
        [InlineData(nameof(Row.Kind), ColumnFilterOperator.Equals, "second", false, true)]
        [InlineData(nameof(Row.Price), ColumnFilterOperator.GreaterThan, "1", true, false)]
        [InlineData(nameof(Row.Anything), ColumnFilterOperator.Contains, "7", true, false)]
        public void Filters_Should_Match_By_Type(string path, ColumnFilterOperator op, string value, bool alice, bool bob)
        {
            var sut = CreateFilter();
            var filter = sut.GetFilter(path)!;

            filter.Operator = op;
            filter.Value = value;
            filter.Apply();

            Assert.Equal(alice, sut.Predicate(_alice));
            Assert.Equal(bob, sut.Predicate(_bob));
        }

        public sealed class TypedRow
        {
            public Guid Id { get; set; }
            public Guid? OptionalId { get; set; }
            public TimeSpan Duration { get; set; }
            public DateTimeOffset Timestamp { get; set; }
            public DateOnly Day { get; set; }
            public TimeOnly Time { get; set; }
            public char Letter { get; set; }
            public Version? Version { get; set; }
            public long Large { get; set; }
            public double Ratio { get; set; }
        }

        private static readonly Guid _guid = Guid.Parse("11111111-2222-3333-4444-555555555555");

        private static readonly TypedRow _typed = new()
        {
            Id = _guid,
            Duration = TimeSpan.FromMinutes(90),
            Timestamp = new DateTimeOffset(2020, 1, 1, 12, 0, 0, TimeSpan.Zero),
            Day = new DateOnly(2020, 1, 1),
            Time = new TimeOnly(12, 0),
            Letter = 'x',
            Version = new Version(1, 2),
            Large = 5_000_000_000,
            Ratio = 0.5,
        };

        private static DataGridFilterViewModel CreateTypedFilter()
        {
            return DataGridFilterViewModel.Create<TypedRow>(Utils.GetTestCommandBuilder(new TestDispatcher()));
        }

        [Theory]
        [InlineData(nameof(TypedRow.Id), new[] { ColumnFilterOperator.Contains, ColumnFilterOperator.Equals, ColumnFilterOperator.NotEquals })]
        [InlineData(nameof(TypedRow.OptionalId), new[] { ColumnFilterOperator.Contains, ColumnFilterOperator.Equals, ColumnFilterOperator.NotEquals })]
        [InlineData(nameof(TypedRow.Letter), new[] { ColumnFilterOperator.Contains, ColumnFilterOperator.Equals, ColumnFilterOperator.NotEquals })]
        [InlineData(nameof(TypedRow.Version), new[] { ColumnFilterOperator.Contains, ColumnFilterOperator.Equals, ColumnFilterOperator.NotEquals })]
        public void Types_Without_A_Meaningful_Order_Should_Not_Offer_Comparisons(string path, ColumnFilterOperator[] expected)
        {
            Assert.Equal(expected, CreateTypedFilter().GetFilter(path)!.Operators);
        }

        [Theory]
        [InlineData(nameof(TypedRow.Duration))]
        [InlineData(nameof(TypedRow.Timestamp))]
        [InlineData(nameof(TypedRow.Day))]
        [InlineData(nameof(TypedRow.Time))]
        [InlineData(nameof(TypedRow.Large))]
        [InlineData(nameof(TypedRow.Ratio))]
        public void Ordered_Types_Should_Offer_Comparisons(string path)
        {
            var filter = CreateTypedFilter().GetFilter(path)!;

            Assert.Contains(ColumnFilterOperator.GreaterThan, filter.Operators);
            Assert.DoesNotContain(ColumnFilterOperator.Contains, filter.Operators);
        }

        [Theory]
        [InlineData(nameof(TypedRow.Duration), "01:00:00", true)]
        [InlineData(nameof(TypedRow.Duration), "02:00:00", false)]
        [InlineData(nameof(TypedRow.Large), "4000000000", true)]
        public void Comparisons_Should_Work_For_Types_Without_IConvertible(string path, string value, bool expected)
        {
            var sut = CreateTypedFilter();
            var filter = sut.GetFilter(path)!;

            filter.Operator = ColumnFilterOperator.GreaterThan;
            filter.Value = value;
            filter.Apply();

            Assert.False(filter.HasError);
            Assert.Equal(expected, sut.Predicate(_typed));
        }

        [Fact]
        public void Date_And_Time_Types_Should_Compare()
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;

            foreach (var (path, value) in new[]
            {
                (nameof(TypedRow.Timestamp), _typed.Timestamp.AddDays(1).ToString(culture)),
                (nameof(TypedRow.Day), _typed.Day.AddDays(1).ToString(culture)),
                (nameof(TypedRow.Time), _typed.Time.AddHours(1).ToString(culture)),
            })
            {
                var sut = CreateTypedFilter();
                var filter = sut.GetFilter(path)!;

                filter.Operator = ColumnFilterOperator.LessThan;
                filter.Value = value;
                filter.Apply();

                Assert.False(filter.HasError, path);
                Assert.True(sut.Predicate(_typed), path);
            }
        }

        [Fact]
        public void A_Guid_Should_Be_Found_By_A_Part_Of_Its_Value()
        {
            var sut = CreateTypedFilter();
            var filter = sut.GetFilter(nameof(TypedRow.Id))!;

            filter.Value = "3333-44";

            filter.Apply();

            Assert.False(filter.HasError);
            Assert.True(sut.Predicate(_typed));
        }

        [Fact]
        public void A_Guid_Should_Be_Compared_By_Value_For_Equality()
        {
            var sut = CreateTypedFilter();
            var filter = sut.GetFilter(nameof(TypedRow.Id))!;

            filter.Operator = ColumnFilterOperator.Equals;
            filter.Value = _guid.ToString("N");
            filter.Apply();

            Assert.False(filter.HasError);
            Assert.True(sut.Predicate(_typed));
        }

        [Fact]
        public void Switching_The_Operator_Should_Validate_The_Value_Again()
        {
            var sut = CreateTypedFilter();
            var filter = sut.GetFilter(nameof(TypedRow.Id))!;

            filter.Value = "3333";

            filter.Apply();
            Assert.False(filter.HasError);

            filter.Operator = ColumnFilterOperator.Equals;

            Assert.True(filter.HasError);
            Assert.True(sut.Predicate(_typed));

            filter.Operator = ColumnFilterOperator.Contains;

            Assert.False(filter.HasError);
        }

        [Fact]
        public void Equals_On_A_Null_Guid_Should_Not_Match()
        {
            var sut = CreateTypedFilter();
            var filter = sut.GetFilter(nameof(TypedRow.OptionalId))!;

            filter.Operator = ColumnFilterOperator.Equals;
            filter.Value = _guid.ToString();
            filter.Apply();

            Assert.False(sut.Predicate(_typed));
        }

        [Fact]
        public void Dates_Should_Compare()
        {
            var sut = CreateFilter();
            var filter = sut.GetFilter(nameof(Row.Created))!;

            filter.Operator = ColumnFilterOperator.LessThan;
            filter.Value = new DateTime(2021, 1, 1).ToString(System.Globalization.CultureInfo.CurrentCulture);
            filter.Apply();

            Assert.True(sut.Predicate(_alice));
            Assert.False(sut.Predicate(_bob));
        }

        [Fact]
        public void Filters_Should_Be_Combined_With_And()
        {
            var sut = CreateFilter();
            var name = sut.GetFilter(nameof(Row.Name))!;
            name.Value = "a";
            name.Apply();

            var age = sut.GetFilter(nameof(Row.Age))!;
            age.Operator = ColumnFilterOperator.Equals;
            age.Value = "99";
            age.Apply();

            Assert.False(sut.Predicate(_alice));
        }

        [Fact]
        public void Invalid_Values_Should_Disable_The_Filter_And_Report_It()
        {
            var sut = CreateFilter();
            var filter = sut.GetFilter(nameof(Row.Age))!;

            filter.Value = "abc";

            filter.Apply();

            Assert.True(filter.HasError);
            Assert.True(sut.Predicate(_alice));

            filter.Value = "1";

            filter.Apply();

            Assert.False(filter.HasError);
        }

        [Fact]
        public void Operators_Should_Depend_On_The_Type()
        {
            var sut = CreateFilter();

            Assert.Equal(ColumnFilterOperator.Contains, sut.GetFilter(nameof(Row.Name))!.Operator);
            Assert.Equal(ColumnFilterOperator.Equals, sut.GetFilter(nameof(Row.Age))!.Operator);
            Assert.Equal([ColumnFilterOperator.Equals, ColumnFilterOperator.NotEquals], sut.GetFilter(nameof(Row.IsActive))!.Operators);
            Assert.Equal([nameof(Kind.First), nameof(Kind.Second)], sut.GetFilter(nameof(Row.Kind))!.Options);
        }

        [Fact]
        public void GetFilter_Should_Return_The_Same_Instance_And_Reject_Unknown_Paths()
        {
            var sut = CreateFilter();

            Assert.Same(sut.GetFilter(nameof(Row.Age)), sut.GetFilter(nameof(Row.Age)));
            Assert.Null(sut.GetFilter("Unknown"));
        }

        [Fact]
        public void Changes_Should_Increase_The_Version()
        {
            var sut = CreateFilter();
            var filter = sut.GetFilter(nameof(Row.Name))!;
            var version = sut.Version;

            filter.Value = "a";

            filter.Apply();
            Assert.True(sut.Version > version);

            version = sut.Version;
            filter.Operator = ColumnFilterOperator.Equals;
            filter.Apply();
            Assert.True(sut.Version > version);
        }

        [Fact]
        public void AdditionalFilter_Should_Have_To_Match_As_Well()
        {
            var sut = CreateFilter();

            sut.AdditionalFilter = p => ReferenceEquals(p, _bob);

            Assert.False(sut.Predicate(_alice));
            Assert.True(sut.Predicate(_bob));
        }

        [Fact]
        public void Clear_Should_Reset_All_Values()
        {
            var sut = CreateFilter();
            var filter = sut.GetFilter(nameof(Row.Name))!;
            filter.Value = "a";
            filter.Apply();

            sut.ClearCommand.Execute(null);

            Assert.Null(filter.Value);
            Assert.True(sut.Predicate(_bob));
        }

        [Fact]
        public void Input_Should_Not_Change_The_Rows_Until_It_Is_Applied()
        {
            var sut = CreateFilter();
            var filter = sut.GetFilter(nameof(Row.Name))!;
            var version = sut.Version;

            filter.Value = "bob";
            filter.Operator = ColumnFilterOperator.Equals;

            Assert.Equal(version, sut.Version);
            Assert.False(filter.IsApplied);
            Assert.True(sut.Predicate(_alice));

            filter.Apply();

            Assert.True(filter.IsApplied);
            Assert.False(sut.Predicate(_alice));
        }

        [Fact]
        public void Applying_The_Same_Input_Again_Should_Not_Filter_Again()
        {
            var sut = CreateFilter();
            var filter = sut.GetFilter(nameof(Row.Name))!;
            filter.Value = "bob";
            filter.Apply();
            var version = sut.Version;

            filter.Apply();

            Assert.Equal(version, sut.Version);
        }

        [Fact]
        public void An_Applied_Filter_Should_Stay_In_Effect_While_The_Input_Changes()
        {
            var sut = CreateFilter();
            var filter = sut.GetFilter(nameof(Row.Name))!;
            filter.Value = "bob";
            filter.Apply();

            filter.Value = "alice";
            filter.Operator = ColumnFilterOperator.Equals;

            Assert.False(sut.Predicate(_alice));
            Assert.True(sut.Predicate(_bob));
        }

        [Fact]
        public void Clearing_A_Column_Should_Take_Effect_When_Applied()
        {
            var sut = CreateFilter();
            var filter = sut.GetFilter(nameof(Row.Name))!;
            filter.Value = "bob";
            filter.Apply();

            Assert.True(filter.ClearCommand.CanExecute(null));
            filter.ClearCommand.Execute(null);

            Assert.Null(filter.Value);
            Assert.False(sut.Predicate(_alice));

            filter.Apply();

            Assert.True(sut.Predicate(_alice));
            Assert.False(filter.ClearCommand.CanExecute(null));
        }

        [Fact]
        public void Clearing_All_Filters_Should_Only_Filter_Once()
        {
            var sut = CreateFilter();
            foreach (var path in new[] { nameof(Row.Name), nameof(Row.Age) })
            {
                var filter = sut.GetFilter(path)!;
                filter.Value = "1";
                filter.Apply();
            }

            var version = sut.Version;

            sut.ClearCommand.Execute(null);

            Assert.Equal(version + 1, sut.Version);
        }

        [Fact]
        public void Validation_Errors_Should_Be_Reported_While_Typing()
        {
            var filter = CreateFilter().GetFilter(nameof(Row.Age))!;

            filter.Value = "abc";

            Assert.True(filter.HasError);
            Assert.NotNull(filter.Error);

            filter.Value = "1";

            Assert.False(filter.HasError);
            Assert.Null(filter.Error);
        }

        [Fact]
        public void An_Invalid_Value_Should_Disable_An_Applied_Filter()
        {
            var sut = CreateFilter();
            var filter = sut.GetFilter(nameof(Row.Age))!;
            filter.Value = "99";
            filter.Apply();
            Assert.False(sut.Predicate(_alice));

            filter.Value = "abc";
            filter.Apply();

            Assert.False(filter.IsApplied);
            Assert.True(sut.Predicate(_alice));
        }

        [Fact]
        public void Without_Validation_Values_Should_Be_Compared_By_Their_Text()
        {
            var sut = CreateFilter();
            var filter = sut.GetFilter(nameof(Row.Age))!;

            filter.IsValidationEnabled = false;
            filter.Value = "abc";
            filter.Apply();

            Assert.False(filter.HasError);
            Assert.True(filter.IsApplied);
            Assert.False(sut.Predicate(_alice));

            filter.Value = "30";
            filter.Apply();

            Assert.True(sut.Predicate(_alice));
            Assert.False(sut.Predicate(_bob));
        }

        [Fact]
        public void Turning_Validation_Off_And_On_Should_Update_The_Current_Input()
        {
            var filter = CreateFilter().GetFilter(nameof(Row.Age))!;
            filter.Value = "abc";

            filter.IsValidationEnabled = false;
            Assert.False(filter.HasError);

            filter.IsValidationEnabled = true;
            Assert.True(filter.HasError);
        }

        [Fact]
        public void IsValidationEnabled_Should_Apply_To_Existing_And_New_Columns()
        {
            var sut = CreateFilter();
            var existing = sut.GetFilter(nameof(Row.Age))!;

            sut.IsValidationEnabled = false;

            Assert.False(existing.IsValidationEnabled);
            Assert.False(sut.GetFilter(nameof(Row.Name))!.IsValidationEnabled);
        }

        private sealed class RejectRule : ValidationRule
        {
            public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
            {
                return (string)value == "bad"
                    ? new ValidationResult(false, "rejected")
                    : ValidationResult.ValidResult;
            }
        }

        [Fact]
        public void Custom_Rules_Should_Reject_Text()
        {
            var sut = CreateFilter();
            var filter = sut.GetFilter(nameof(Row.Name))!;
            filter.ValidationRules.Add(new RejectRule());

            filter.Value = "bad";
            filter.Apply();

            Assert.True(filter.HasError);
            Assert.Equal("rejected", filter.Error);
            Assert.False(filter.IsApplied);

            filter.IsValidationEnabled = false;
            filter.Apply();

            Assert.False(filter.HasError);
            Assert.True(filter.IsApplied);
        }

        [Fact]
        public void Adding_A_Rule_Should_Check_The_Current_Input()
        {
            var filter = CreateFilter().GetFilter(nameof(Row.Name))!;
            filter.Value = "bad";

            filter.ValidationRules.Add(new RejectRule());

            Assert.True(filter.HasError);
        }

        [Fact]
        public void Custom_Rules_Should_Ignore_Options()
        {
            var filter = CreateFilter().GetFilter(nameof(Row.IsActive))!;
            filter.ValidationRules.Add(new AlwaysFailRule());

            filter.Value = bool.TrueString;

            Assert.False(filter.HasError);
        }

        private sealed class AlwaysFailRule : ValidationRule
        {
            public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
            {
                return new ValidationResult(false, null);
            }
        }

        private static DataGrid CreateGrid()
        {
            var grid = new DataGrid { AutoGenerateColumns = false, CanUserAddRows = false, ItemsSource = new List<Row> { _alice, _bob } };
            grid.Columns.Add(new DataGridTextColumn { Binding = new Binding(nameof(Row.Name)) });
            grid.Columns.Add(new DataGridTemplateColumn());

            return grid;
        }

        [Fact]
        public Task Behavior_Should_Filter_The_Grid_And_Publish_Column_Filters()
        {
            return StaRunner.Run(async () =>
            {
                var grid = CreateGrid();
                var filter = CreateFilter();
                Interaction.GetBehaviors(grid).Add(new DataGridFilterBehavior { Filter = filter });

                var columnFilter = DataGridFilterBehavior.GetColumnFilter(grid.Columns[0]);

                Assert.NotNull(columnFilter);
                Assert.Null(DataGridFilterBehavior.GetColumnFilter(grid.Columns[1]));

                columnFilter.Value = "bob";
                columnFilter.Apply();
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

                Assert.Equal([_bob], grid.Items.Cast<Row>());
            });
        }

        [Fact]
        public Task Detaching_Should_Remove_The_Filter()
        {
            return StaRunner.Run(() =>
            {
                var grid = CreateGrid();
                var behavior = new DataGridFilterBehavior { Filter = CreateFilter() };
                Interaction.GetBehaviors(grid).Add(behavior);

                Interaction.GetBehaviors(grid).Remove(behavior);

                Assert.Null(grid.Items.Filter);
                Assert.Null(DataGridFilterBehavior.GetColumnFilter(grid.Columns[0]));

                return Task.CompletedTask;
            });
        }
    }
}
