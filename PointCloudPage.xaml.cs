using Microsoft.Maui.Graphics;
using System.Buffers;
using System.Globalization;
using System.Reflection;

namespace Trimble
{
    public partial class PointCloudPage : ContentPage
    {
        private List<Point3D> points = new List<Point3D>();
        private string plyFolderPath;

        public PointCloudPage()
        {
            InitializeComponent();
            InitializePLYFolder();
            InitializePointCloudView();
        }

        private void InitializePLYFolder()
        {
            try
            {
                plyFolderPath = "Trimble.Resources.PLY";
                Console.WriteLine($"PLY folder path: {plyFolderPath}");

                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();
                var plyFiles = resourceNames.Where(r => r.StartsWith(plyFolderPath) && r.EndsWith(".ply"));

                Console.WriteLine($"Number of .ply files found: {plyFiles.Count()}");
                foreach (var file in plyFiles)
                {
                    Console.WriteLine($"Found file: {Path.GetFileName(file)}");
                }

                if (!plyFiles.Any())
                {
                    Console.WriteLine("No .ply files found in the resources.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while initializing the PLY folder: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void InitializePointCloudView()
        {
            pointCloudView.Drawable = new PointCloudDrawable(points);
            pointCloudView.HeightRequest = 300;
            pointCloudView.WidthRequest = 300;
            pointCloudView.BackgroundColor = Colors.LightGray;

            var openFileButton = new Button
            {
                Text = "Load Selected PLY File",
                Command = new Command(() => OnOpenFileClicked(null, EventArgs.Empty))
            };

            if (Content is Grid grid)
            {
                grid.Children.Add(openFileButton);
                Grid.SetRow(openFileButton, 0);
            }
        }

        private async void OnOpenFileClicked(object sender, EventArgs e)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();
                var plyFiles = resourceNames.Where(r => r.StartsWith(plyFolderPath) && r.EndsWith(".ply")).ToArray();

                if (plyFiles.Length == 0)
                {
                    await DisplayAlert("No Files", "No .ply files found in the embedded resources.", "OK");
                    return;
                }

                var fileNames = plyFiles.Select(Path.GetFileName).ToArray();
                var selectedFile = await DisplayActionSheet("Select a PLY file", "Cancel", null, fileNames);

                if (selectedFile != "Cancel" && !string.IsNullOrEmpty(selectedFile))
                {
                    var fullResourcePath = plyFiles.First(r => r.EndsWith(selectedFile));
                    points = await ReadPLYFile(fullResourcePath);
                    pointCloudView.Drawable = new PointCloudDrawable(points);
                    pointCloudView.Invalidate();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task<List<Point3D>> ReadPLYFile(string resourcePath)
        {
            var points = new List<Point3D>();
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(resourcePath))
            using (var reader = new BinaryReader(stream))
            {
                string header = "";
                bool headerEnd = false;
                int vertexCount = 0;
                bool isBinary = false;
                bool isLittleEndian = true;

                // Read the header
                while (!headerEnd)
                {
                    var line = await ReadLineAsync(reader);
                    header += line + "\n";

                    if (line.StartsWith("format"))
                    {
                        if (line.Contains("binary_little_endian"))
                        {
                            isBinary = true;
                            isLittleEndian = true;
                        }
                        else if (line.Contains("binary_big_endian"))
                        {
                            isBinary = true;
                            isLittleEndian = false;
                        }
                        else if (line.Contains("ascii"))
                        {
                            isBinary = false;
                        }
                        else
                        {
                            throw new FormatException("Unsupported PLY format.");
                        }
                    }
                    else if (line.StartsWith("element vertex"))
                    {
                        vertexCount = int.Parse(line.Split()[2]);
                    }
                    else if (line == "end_header")
                    {
                        headerEnd = true;
                    }
                }

                // Verify that this is a binary file with the expected properties
                if (!isBinary)
                {
                    throw new FormatException("PLY file is not in binary format as expected.");
                }

                // Read vertex data
                for (int i = 0; i < vertexCount; i++)
                {
                    float x = isLittleEndian ? reader.ReadSingle() : BitConverter.ToSingle(reader.ReadBytes(4).Reverse().ToArray(), 0);
                    float y = isLittleEndian ? reader.ReadSingle() : BitConverter.ToSingle(reader.ReadBytes(4).Reverse().ToArray(), 0);
                    float z = isLittleEndian ? reader.ReadSingle() : BitConverter.ToSingle(reader.ReadBytes(4).Reverse().ToArray(), 0);

                    points.Add(new Point3D { X = x, Y = y, Z = z });
                }
            }
            return points;
        }

        private async Task<string> ReadLineAsync(BinaryReader reader)
        {
            const int bufferSize = 1024;
            using var memoryStream = new MemoryStream();
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

            try
            {
                while (true)
                {
                    var bytesRead = await Task.Run(() => reader.Read(buffer, 0, bufferSize));
                    if (bytesRead == 0) break;

                    var newLineIndex = Array.IndexOf(buffer, (byte)'\n', 0, bytesRead);
                    if (newLineIndex >= 0)
                    {
                        await memoryStream.WriteAsync(buffer, 0, newLineIndex);
                        reader.BaseStream.Position -= (bytesRead - newLineIndex - 1);
                        break;
                    }

                    await memoryStream.WriteAsync(buffer, 0, bytesRead);
                }

                return System.Text.Encoding.ASCII.GetString(memoryStream.ToArray()).TrimEnd('\r');
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public class Point3D
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
        }

        public class PointCloudDrawable : IDrawable
        {
            private readonly List<Point3D> _points;

            public PointCloudDrawable(List<Point3D> points)
            {
                _points = points;
            }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                if (_points.Count == 0) return;

                var minX = _points.Min(p => p.X);
                var maxX = _points.Max(p => p.X);
                var minY = _points.Min(p => p.Y);
                var maxY = _points.Max(p => p.Y);

                var scaleX = dirtyRect.Width / (maxX - minX);
                var scaleY = dirtyRect.Height / (maxY - minY);
                var scale = Math.Min(scaleX, scaleY);

                canvas.StrokeColor = Colors.Blue;
                canvas.StrokeSize = 1;

                foreach (var point in _points)
                {
                    var x = (point.X - minX) * scale;
                    var y = dirtyRect.Height - (point.Y - minY) * scale;
                    canvas.DrawCircle((float)x, (float)y, 1);
                }
            }
        }
    }
}