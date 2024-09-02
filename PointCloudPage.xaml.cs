using Microsoft.Maui.Graphics;
using System.Globalization;

namespace Trimble
{
    public partial class PointCloudPage : ContentPage
    {
        private List<Point3D> points = new List<Point3D>();
        private string plyFolderPath;
        private Picker filePicker;

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
                plyFolderPath = Path.Combine(FileSystem.AppDataDirectory, "Documents", "PLY");
                Console.WriteLine($"Attempting to access or create PLY folder at: {plyFolderPath}");

                if (!Directory.Exists(plyFolderPath))
                {
                    Console.WriteLine("PLY folder does not exist. Creating it now.");
                    Directory.CreateDirectory(plyFolderPath);
                    Console.WriteLine("PLY folder created successfully.");
                }
                else
                {
                    Console.WriteLine("PLY folder already exists.");
                }

                var files = Directory.GetFiles(plyFolderPath, "*.ply");
                Console.WriteLine($"Number of .ply files found: {files.Length}");
                foreach (var file in files)
                {
                    Console.WriteLine($"Found file: {Path.GetFileName(file)}");
                }

                if (files.Length == 0)
                {
                    Console.WriteLine("No .ply files found in the PLY folder.");
                    Console.WriteLine("Please ensure you have copied some .ply files to this location:");
                    Console.WriteLine(plyFolderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while initializing the PLY folder: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

            // Use the correct path where the PLY files are located
            plyFolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "SOURCE", "REPOS", "TRIMBLE", "DOCUMENTS", "PLY");

            if (!Directory.Exists(plyFolderPath))
            {
                throw new DirectoryNotFoundException($"PLY folder not found: {plyFolderPath}");
            }
            
            // Debug: Print all files in the directory
            var allFiles = Directory.GetFiles(plyFolderPath);
            Console.WriteLine($"All files in {plyFolderPath}:");
            foreach (var file in allFiles)
            {
                Console.WriteLine(Path.GetFileName(file));
>>>>>>> 165b8e085e114890e06932c9ee9603532e39df6a
            }
        }

        private void InitializePointCloudView()
        {
            pointCloudView.Drawable = new PointCloudDrawable(points);
            pointCloudView.HeightRequest = 300;
            pointCloudView.WidthRequest = 300;
            pointCloudView.BackgroundColor = Colors.LightGray;

            filePicker = new Picker
            {
                Title = "Select a PLY file"
            };
            filePicker.SelectedIndexChanged += OnFileSelected;

            var openFileButton = new Button
            {
                Text = "Load Selected PLY File",
                Command = new Command(OnOpenFileClicked)
            };

            if (Content is Grid grid)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.Children.Add(filePicker);
                Grid.SetRow(filePicker, 0);
                grid.Children.Add(openFileButton);
                Grid.SetRow(openFileButton, 1);
            }

            LoadFileList();
        }

        private void LoadFileList()
        {
            var files = Directory.GetFiles(plyFolderPath, "*.ply");
            var fileNames = files.Select(Path.GetFileName).ToArray();
            filePicker.ItemsSource = fileNames;
        }

        private void OnFileSelected(object? sender, EventArgs e)
        {
            // This method is called when a file is selected in the Picker
            // You can add any additional logic here if needed
        }

        private void OnOpenFileClicked()
        {
            if (filePicker.SelectedItem is string selectedFile)
            {
                var fullPath = Path.Combine(plyFolderPath, selectedFile);
                try
                {
                    points = ReadPLYFile(fullPath);
                    pointCloudView.Drawable = new PointCloudDrawable(points);
                    pointCloudView.Invalidate();
                }
                catch (Exception ex)
                {
                    DisplayAlert("Error", $"An error occurred while reading the file: {ex.Message}", "OK");
                }
            }
            else
            {
                DisplayAlert("No File Selected", "Please select a PLY file first.", "OK");
            }
        }

        private async Task<List<Point3D>> ReadPLYFile(string filePath)
        {
            var points = new List<Point3D>();
            using (var stream = File.OpenRead(filePath))
            using (var reader = new StreamReader(stream))
            {
                string? line;
                bool headerEnd = false;
                int vertexCount = 0;
                var propertyIndices = new Dictionary<string, int>();
                bool isBinary = false;

                while ((line = await reader.ReadLineAsync()) is not null)
                {
                    if (line.StartsWith("format"))
                    {
                        isBinary = line.Contains("binary");
                        if (isBinary)
                        {
                            throw new NotSupportedException("Binary PLY format is not supported yet.");
                        }
                    }
                    else if (line.StartsWith("element vertex"))
                    {
                        vertexCount = int.Parse(line.Split()[2]);
                    }
                    else if (line.StartsWith("property"))
                    {
                        var parts = line.Split();
                        propertyIndices[parts[2]] = propertyIndices.Count;
                    }
                    else if (line == "end_header")
                    {
                        headerEnd = true;
                        break;
                    }
                }

                if (!headerEnd)
                {
                    throw new FormatException("Invalid PLY file: Header not found or incomplete.");
                }

                if (!propertyIndices.ContainsKey("x") || !propertyIndices.ContainsKey("y") || !propertyIndices.ContainsKey("z"))
                {
                    throw new FormatException("Invalid PLY file: X, Y, or Z property not found.");
                }

                for (int i = 0; i < vertexCount; i++)
                {
                    line = await reader.ReadLineAsync();
                    if (line is null)
                    {
                        throw new FormatException($"Unexpected end of file. Expected {vertexCount} vertices, found {i}.");
                    }

                    var values = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length < propertyIndices.Count)
                    {
                        throw new FormatException($"Invalid vertex data on line {i + 1}. Expected {propertyIndices.Count} values, found {values.Length}.");
                    }

                    try
                    {
                        points.Add(new Point3D
                        {
                            X = float.Parse(values[propertyIndices["x"]], CultureInfo.InvariantCulture),
                            Y = float.Parse(values[propertyIndices["y"]], CultureInfo.InvariantCulture),
                            Z = float.Parse(values[propertyIndices["z"]], CultureInfo.InvariantCulture)
                        });
                    }
                    catch (FormatException)
                    {
                        throw new FormatException($"Invalid number format on line {i + 1}. Please ensure all coordinate values are valid numbers.");
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new FormatException($"Missing coordinate value on line {i + 1}. Please ensure all vertices have X, Y, and Z values.");
                    }
                }
            }
            return points;
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