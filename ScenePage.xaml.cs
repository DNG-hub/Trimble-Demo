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

        string[] coordinates = result.Split(',');
        if (coordinates.Length != 2 || !double.TryParse(coordinates[0], out double x) || !double.TryParse(coordinates[1], out double y))
        {
            await DisplayAlert("Invalid Input", "Please enter valid X and Y coordinates.", "OK");
            return;
        }

        _currentScene.Measurements.Add(new Measurement { X = x, Y = y });
        sceneGraphicsView.Invalidate();
    }
}