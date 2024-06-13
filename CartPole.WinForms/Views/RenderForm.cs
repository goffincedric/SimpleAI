using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using CartPolePhysics.SinglePole.DoublePrecision;
using CartPoleWinForms.Constants;
using CartPoleWinForms.Controls;
using CartPoleWinForms.Models;
using DirectedAcyclicGraph.Models;
using SkiaSharp;

namespace CartPoleWinForms.Views;

public class RenderForm : Form
{
    private readonly CartPole _cartPole;
    private Stopwatch PhysicsStopwatch { get; set; }
    private CartSinglePolePhysicsRK4 Physics { get; set; }
    private const int PhysicsUpdateTargetMs = 10;

    public RenderForm()
    {
        // Set window to fullscreen
        WindowState = FormWindowState.Maximized;

        // Disable window resizing
        FormBorderStyle = FormBorderStyle.FixedSingle;

        // Add RenderControl to window and make it fullscreen
        _cartPole = new CartPole(
            0,
            0.5f,
            0.08f,
            new SKColor(81, 97, 114),
            0.02f,
            1,
            // AngleConstants.Radians.Up - 0.01,
            AngleConstants.Radians.Down,
            new SKColor(178, 149, 91),
            new SKColor(171, 144, 199),
            14,
            SKColors.Black
        );

        var randomGraph = GetRandomGraph();
        var (sortedLayers, _) = randomGraph.GetTopologicallySortedNodes();

        Controls.Add(new RenderControl(_cartPole, sortedLayers));

        StartPhysics();
    }

    private void StartPhysics()
    {
        // Initialize Physics with initial CartPole state
        Physics = new CartSinglePolePhysicsRK4(
            (double)PhysicsUpdateTargetMs / 1000,
            _cartPole.GetState()
        );

        // Start the physics loop in a separate thread
        PhysicsStopwatch = new Stopwatch();
        new Thread(() =>
        {
            while (true)
            {
                // Only trigger a repaint if enough time has passed for the next physics update
                if (PhysicsStopwatch.ElapsedMilliseconds < PhysicsUpdateTargetMs)
                    continue; // 100Hz
                PhysicsStopwatch.Restart();
                UpdatePhysics();
            }
        })
        {
            IsBackground = true
        }.Start(); // Run the thread in the background so it doesn't prevent the application from exiting
        PhysicsStopwatch.Start();
    }

    private void UpdatePhysics()
    {
        // Update physics
        Physics.Update(0);
        _cartPole.SetState(Physics.State);
    }

    private DirectedAcyclicGraph.Models.DirectedAcyclicGraph GetRandomGraph()
    {
        var graph = new DirectedAcyclicGraph.Models.DirectedAcyclicGraph();
        graph.AddNode(new Node("Input 1", NodeType.Start), []);
        graph.AddNode(new Node("Input 2", NodeType.Start), []);
        graph.AddNode(new Node("Input 3", NodeType.Start), []);
        graph.AddNode(new Node("Input 4", NodeType.Start), []);
        graph.AddNode(new Node("Output 1", NodeType.End), []);
        graph.AddNode(new Node("Output 2", NodeType.End), []);

        // Add edges and split random edges 3 times
        for (var i = 0; i < 3; i++)
        {
            // Add five random edges
            for (var j = 0; j < 2; j++)
                graph.AddRandomEdge();
            // split five random edges
            for (var j = 0; j < 2; j++)
                graph.SplitRandomEdge(new Node($"{i}_{j}", NodeType.Hidden));
        }

        return graph;
    }
}
