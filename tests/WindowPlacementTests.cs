using System.Drawing;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using Presentation.Wpf;
using Point = System.Drawing.Point;

namespace Minimal.Mvvm.Wpf.Tests
{
    [Apartment(ApartmentState.STA)]
    internal class WindowPlacementTests
    {
        [OneTimeTearDown]
        public void Dispose()
        {
            Dispatcher.CurrentDispatcher.InvokeShutdown();//to prevent InvalidComObjectException
        }

        [Test]
        public void SerializationTest()
        {
            var json = """
                {
                  "Flags": 0,
                  "MinPosition": {
                    "X": -1,
                    "Y": -1
                  },
                  "MaxPosition": {
                    "X": -1,
                    "Y": -1
                  },
                  "NormalPosition": {
                    "X": 503,
                    "Y": 112,
                    "Width": 1040,
                    "Height": 563
                  },
                  "ShowCmd": 1,
                  "ResizeMode": 2,
                  "WindowStyle": 1
                }
                """;

            var windowPlacement = JsonSerializer.Deserialize<WindowPlacement>(json, WindowExtensions.SerializerOptions);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(windowPlacement, Is.Not.Null);
                Assert.That(windowPlacement!.MinPosition, Is.EqualTo(new Point(-1, -1)));
                Assert.That(windowPlacement!.MaxPosition, Is.EqualTo(new Point(-1, -1)));
                Assert.That(windowPlacement!.NormalPosition, Is.EqualTo(new Rectangle(503, 112, 1040, 563)));
                Assert.That(windowPlacement!.ShowCmd, Is.EqualTo(1));
                Assert.That(windowPlacement!.ResizeMode, Is.EqualTo(ResizeMode.CanResize));
                Assert.That(windowPlacement!.WindowStyle, Is.EqualTo(WindowStyle.SingleBorderWindow));
            }

            var output = JsonSerializer.Serialize(windowPlacement, WindowExtensions.SerializerOptions);
            Assert.That(output, Is.EqualTo(json));
        }

        [Test]
        public async Task IncorrectWindowPlacementTest()
        {
            var incorrectJson = """
                {
                  "showCmd": 1,
                  "minPosition": {
                    "X": -1,
                    "Y": -1
                  },
                  "maxPosition": {
                    "X": -1,
                    "Y": -1
                  },
                  "normalPosition": {
                    "Left": 300,
                    "Top": 44,
                    "Right": 1580,
                    "Bottom": 1004
                  },
                  "WindowStyle": 1,
                  "ResizeMode": 2
                }
                """;

            var windowPlacement = JsonSerializer.Deserialize<WindowPlacement>(incorrectJson, WindowExtensions.SerializerOptions);
            Assert.That(windowPlacement, Is.Not.Null);

            var output = JsonSerializer.Serialize(windowPlacement, WindowExtensions.SerializerOptions);
            Assert.That(output, Is.Not.EqualTo(incorrectJson));

            var window = new Window();
            var tcs = new TaskCompletionSource<bool>();
            bool isSourceInitialized = false;
            window.SourceInitialized += (sender, e) =>
            {
                Assert.That(isSourceInitialized, Is.False);

                var placementStr = window.GetPlacementAsJson();
                Assert.That(placementStr, Is.Not.Null.Or.Empty);

                bool result = window.SetPlacement(windowPlacement);
                Assert.That(result, Is.False);

                var newPlacementStr = window.GetPlacementAsJson();
                Assert.That(newPlacementStr, Is.EqualTo(placementStr));

                tcs.SetResult(true);
                isSourceInitialized = true;
            };
            window.Show();
            await tcs.Task;
            window.Close();
            Assert.Pass();
        }

        [Test]
        public async Task NoResizeNoneStyleTest()
        {
            var window = new Window() { ResizeMode = ResizeMode.NoResize, WindowStyle = WindowStyle.None };
            var tcs = new TaskCompletionSource<bool>();
            bool isSourceInitialized = false;
            window.SourceInitialized += (sender, e) =>
            {
                Assert.That(isSourceInitialized, Is.False);
                var windowPlacement = window.GetPlacement()!;
                Assert.That(windowPlacement, Is.Not.Null);
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(windowPlacement.ResizeMode, Is.EqualTo(ResizeMode.NoResize));
                    Assert.That(windowPlacement.WindowStyle, Is.EqualTo(WindowStyle.None));
                }

                window.ResizeMode = ResizeMode.CanResize;
                window.WindowStyle = WindowStyle.SingleBorderWindow;

                var placementStr = JsonSerializer.Serialize(windowPlacement, WindowExtensions.SerializerOptions);
                Assert.That(placementStr, Is.Not.Null.Or.Empty);

                var placement = JsonSerializer.Deserialize<WindowPlacement>(placementStr, WindowExtensions.SerializerOptions)!;
                Assert.That(placement, Is.Not.Null);
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(placement.ResizeMode, Is.EqualTo(ResizeMode.NoResize));
                    Assert.That(placement.WindowStyle, Is.EqualTo(WindowStyle.None));
                }

                var result = window.SetPlacement(placement);
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(result, Is.True);

                    Assert.That(window.ResizeMode, Is.EqualTo(ResizeMode.NoResize));
                    Assert.That(window.WindowStyle, Is.EqualTo(WindowStyle.None));
                }

                tcs.SetResult(true);
                isSourceInitialized = true;
            };
            window.Show();
            await tcs.Task;
            window.Close();
            Assert.Pass();
        }
    }
}
