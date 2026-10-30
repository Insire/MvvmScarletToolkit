using Microsoft.Xaml.Behaviors;
using MvvmScarletToolkit.Wpf;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class DataGridStylesTests : TraceTestBase
    {
        public sealed class Row
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public decimal Price { get; set; }
        }

        private sealed class BindingErrors : TraceListener
        {
            public List<string> Errors { get; } = [];

            public override void Write(string? message)
            {
            }

            public override void WriteLine(string? message)
            {
                if (message is not null)
                {
                    Errors.Add(message);
                }
            }
        }

        private static ResourceDictionary LoadStyles()
        {
            return (ResourceDictionary)Application.LoadComponent(new Uri("/MvvmScarletToolkit.Wpf;component/Features/DataGridExtensions/Generic.xaml", UriKind.Relative));
        }

        private static async Task Rendered()
        {
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            await Task.Delay(100);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        }

        [Fact]
        public Task Default_Styles_Should_Work_Together()
        {
            return StaRunner.Run(async () =>
            {
                var errors = new BindingErrors();
                PresentationTraceSources.Refresh();
                PresentationTraceSources.DataBindingSource.Listeners.Add(errors);
                PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

                var styles = LoadStyles();
                var builder = Utils.GetTestCommandBuilder(new ScarletDispatcher(Dispatcher.CurrentDispatcher), messenger: new CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger());
                var rows = new ObservableCollection<Row>(Enumerable.Range(1, 5).Select(i => new Row { Id = i, Name = "Name " + i, Price = i * 1.5m }));
                var filter = DataGridFilterViewModel.Create<Row>(builder);

                var grid = new DataGrid
                {
                    AutoGenerateColumns = false,
                    CanUserAddRows = false,
                    ItemsSource = rows,
                    ColumnHeaderStyle = (Style)styles["ScarletDataGridFilterColumnHeaderStyle"],
                };

                var id = new DataGridTextColumn { Header = "Id", Binding = new Binding(nameof(Row.Id)), Width = 120 };
                var name = new DataGridTextColumn { Header = "Name", Binding = new Binding(nameof(Row.Name)), Width = 200 };
                var price = new DataGridTextColumn { Header = "Price", Binding = new Binding(nameof(Row.Price)), Width = 200 };
                DataGridSummaryBar.SetFunction(price, DataGridSummaryFunction.Sum);
                grid.Columns.Add(id);
                grid.Columns.Add(name);
                grid.Columns.Add(price);
                Interaction.GetBehaviors(grid).Add(new DataGridFilterBehavior { Filter = filter });

                var panel = new ItemsControl { Style = (Style)styles["ScarletDataGridGroupPanelStyle"] };
                var bar = new DataGridSummaryBar { DataGrid = grid };

                var root = new DockPanel { LastChildFill = true };
                DockPanel.SetDock(panel, Dock.Top);
                DockPanel.SetDock(bar, Dock.Bottom);
                root.Children.Add(panel);
                root.Children.Add(bar);
                root.Children.Add(grid);
                root.Resources.MergedDictionaries.Add(styles);

                var window = new Window
                {
                    Content = root,
                    Width = 560,
                    Height = 320,
                    Left = -10000,
                    Top = -10000,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                };

                try
                {
                    window.Show();
                    await Rendered();

                    var headers = grid.GetVisualDescendants().OfType<DataGridColumnHeader>().Where(p => p.Column is not null).ToArray();
                    Assert.Equal(3, headers.Length);
                    Assert.All(headers, header =>
                    {
                        Assert.Empty(header.GetVisualDescendants().OfType<TextBox>());
                        Assert.Single(header.GetVisualDescendants().OfType<DataGridColumnFilter>(), p => p.IsVisible);
                    });

                    Assert.Equal([120d, 200d, 200d], bar.Cells.Select(p => p.Width));
                    Assert.Equal(22.5m.ToString(System.Globalization.CultureInfo.CurrentCulture), bar.Cells[2].Text);
                    Assert.True(bar.ActualHeight > 10, $"the bar is {bar.ActualHeight} high");

                    var cells = bar.GetVisualDescendants().OfType<TextBlock>().ToArray();
                    Assert.Contains(cells, p => p.Text == bar.Cells[2].Text && p.ActualWidth == 200);

                    // the cells have to follow the grid while it scrolls
                    grid.Columns[1].Width = 600;
                    await Rendered();
                    grid.GetVisualDescendants().OfType<ScrollViewer>().First(p => p.TemplatedParent == grid).ScrollToHorizontalOffset(100);
                    await Rendered();
                    Assert.Equal(grid.RowHeaderActualWidth - 100, bar.CellsMargin.Left);
                    grid.Columns[1].Width = 200;
                    await Rendered();

                    var hint = panel.GetVisualDescendants().OfType<TextBlock>().Single(p => p.Name == "Hint");
                    Assert.True(hint.IsVisible);

                    // a filter typed into the popup of a header reaches the grid and the summary when the popup closes
                    var button = headers[1].GetVisualDescendants().OfType<DataGridColumnFilter>().Single();
                    button.IsOpen = true;
                    await Rendered();

                    var popup = button.GetVisualDescendants().OfType<Popup>().Single();
                    var content = popup.Child;
                    var input = content.GetVisualDescendants().OfType<TextBox>().First(p => p.Name == "PART_Text");
                    Assert.True(input.IsVisible);

                    input.Text = "Name 2";
                    await Rendered();
                    Assert.Equal(5, grid.Items.Count);

                    // keys of the popup travel through the header
                    input.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(input), 0, Key.Enter) { RoutedEvent = Keyboard.KeyDownEvent });
                    await Rendered();

                    Assert.False(button.IsOpen);
                    Assert.Single(grid.Items);
                    Assert.Equal(3m.ToString("0.0", System.Globalization.CultureInfo.CurrentCulture), bar.Cells[2].Text);

                    SaveScreenshot(root);

                    Assert.True(errors.Errors.Count == 0, string.Join(Environment.NewLine, errors.Errors));
                }
                finally
                {
                    window.Close();
                    PresentationTraceSources.DataBindingSource.Listeners.Remove(errors);
                }
            });
        }

        private static void SaveScreenshot(FrameworkElement element)
        {
            var path = Environment.GetEnvironmentVariable("SCARLET_SCREENSHOT");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var bitmap = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            var background = new DrawingVisual();
            using (var context = background.RenderOpen())
            {
                context.DrawRectangle(Brushes.White, null, new Rect(0, 0, element.ActualWidth, element.ActualHeight));
            }

            bitmap.Render(background);
            bitmap.Render(element);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using var stream = File.Create(path);
            encoder.Save(stream);
        }
    }
}
