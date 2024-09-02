using Microsoft.Maui.Graphics;
using Trimble.Models;
using Trimble.Views.Renderers;
using Trimble.Graphics;
using System.Collections.ObjectModel;

namespace Trimble;

public partial class ScenePage : ContentPage
{
    private readonly Scene _currentScene;
    private readonly SceneRenderer _sceneRenderer;

    public ScenePage(Scene scene)
    {
        InitializeComponent();
        _currentScene = scene;
        _sceneRenderer = new SceneRenderer(scene);
        sceneGraphicsView.Drawable = new GraphicsDrawable(DrawScene);
        BindingContext = scene;

        // Initialize the ListView
        measurementListView.ItemsSource = _currentScene.Measurements;

        // Set the page title to the scene name
        Title = scene.Name;
    }

    private void DrawScene(ICanvas canvas, RectF dirtyRect)
    {
        _sceneRenderer.SetCanvas(canvas);
        _sceneRenderer.SetBounds(dirtyRect.Width, dirtyRect.Height);
        _sceneRenderer.Draw();
    }

    private async void OnAddMeasurementClicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Add Measurement", "Enter X,Y coordinates (e.g., 10,20):");
        if (string.IsNullOrWhiteSpace(result))
            return;

        if (TryParseMeasurement(result, out Measurement newMeasurement))
        {
            _currentScene.Measurements.Add(newMeasurement);
            sceneGraphicsView.Invalidate();
            UpdateMeasurementList();
        }
        else
        {
            await DisplayAlert("Invalid Input", "Please enter valid X and Y coordinates.", "OK");
        }
    }

    private bool TryParseMeasurement(string input, out Measurement measurement)
    {
        measurement = null;
        string[] coordinates = input.Split(',');
        if (coordinates.Length == 2 &&
            double.TryParse(coordinates[0], out double x) &&
            double.TryParse(coordinates[1], out double y))
        {
            measurement = new Measurement { X = x, Y = y };
            return true;
        }
        return false;
    }

    private void UpdateMeasurementList()
    {
        // Force the ListView to refresh
        measurementListView.ItemsSource = null;
        measurementListView.ItemsSource = _currentScene.Measurements;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // TODO: Implement actual saving logic
        await DisplayAlert("Save", "Scene saved successfully!", "OK");
    }
}