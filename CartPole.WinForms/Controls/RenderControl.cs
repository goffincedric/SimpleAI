using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using CartPoleWinForms.Helpers;
using CartPoleWinForms.Models;
using DirectedAcyclicGraph.Models;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace CartPoleWinForms.Controls;

public sealed class RenderControl : SKControl
{
    public readonly CartPole CartPole;
    public List<List<DirectedNode>> SortedLayers { get; private set; }

    private readonly Stopwatch _stopwatch = new();
    private readonly Stopwatch _rotationStopwatch = new();
    private readonly Stopwatch _fpsStopwatch = new();
    private const int TargetFrameRate = 60;
    private double _fps = TargetFrameRate;
    private int _frameCount;

    public RenderControl(CartPole cartPole, List<List<DirectedNode>> sortedLayers)
    {
        CartPole = cartPole;
        SortedLayers = sortedLayers;

        _stopwatch.Start();
        _rotationStopwatch.Start();
        _fpsStopwatch.Start();

        // Set control to entire window size
        Dock = DockStyle.Fill;

        // Start the render loop in a separate thread
        new Thread(() =>
        {
            while (true)
            {
                // Only trigger a repaint if enough time has passed for the next frame
                if (_stopwatch.ElapsedMilliseconds >= 1000.0 / TargetFrameRate) // 60 FPS
                {
                    _stopwatch.Restart();
                    Invalidate(); // trigger repaint
                }
            }
        })
        {
            IsBackground = true
        }.Start(); // Run the thread in the background so it doesn't prevent the application from exiting
    }

    public void SetSortedLayers(List<List<DirectedNode>> sortedLayers) =>
        SortedLayers = sortedLayers;

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        // Increase the frame count
        _frameCount++;

        // Get canvas and clear
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

        // Draw the cart track
        CanvasHelper.CartPole.DrawCartTrack(this, canvas);

        // Draw the CartPole
        CanvasHelper.CartPole.DrawCartPole(this, canvas);

        // Draw the graph
        CanvasHelper.AI.DrawGraph(this, canvas);

        // Update and draw the FPS counter
        UpdateFpsCounter();
        CanvasHelper.DrawFpsCounter(_fps, canvas);
    }

    private void UpdateFpsCounter()
    {
        if (_fpsStopwatch.ElapsedMilliseconds < 1000)
            return;
        _fps = _frameCount;
        _frameCount = 0;
        _fpsStopwatch.Restart();
    }
}
