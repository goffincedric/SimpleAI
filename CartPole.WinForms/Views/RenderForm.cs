using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using CartPolePhysics.SinglePole.DoublePrecision;
using CartPoleShared.Constants;
using CartPoleShared.Models;
using CartPoleShared.Models.Environment;
using CartPoleWinForms.Controls;
using DirectedAcyclicGraph.Models;

namespace CartPoleWinForms.Views;

public class RenderForm : Form
{
    private readonly CartPole _cartPole;
    private Stopwatch PhysicsStopwatch { get; set; }
    private CartSinglePolePhysicsRK4 Physics { get; set; }
    private const int PhysicsUpdateTargetMs = 1000 / 60;

    public RenderForm()
    {
        // Start on secondary screen
        StartPosition = FormStartPosition.Manual;
        var screen = Screen.AllScreens.Length > 1 ? Screen.AllScreens[1] : Screen.PrimaryScreen!;
        Location = screen.WorkingArea.Location;

        // Set window to fullscreen
        WindowState = FormWindowState.Maximized;

        // Disable window resizing
        FormBorderStyle = FormBorderStyle.FixedSingle;

        // Add RenderControl to window and make it fullscreen
        _cartPole = new CartPole(
            0,
            0.5f,
            0.08f,
            0.02f,
            1,
            AngleConstants.Radians.Up - 0.01,
            // AngleConstants.Radians.Down,
            14
        );

        var randomGraph = GetRandomGraph();
        var (sortedLayers, _) = randomGraph.GetTopologicallySortedNodes();

        Controls.Add(new RenderControl(_cartPole, sortedLayers as List<List<DirectedNode>>));

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

    private DirectedAcyclicGraph<DirectedNode> GetRandomGraph()
    {
        var graph = new DirectedAcyclicGraph<DirectedNode>();
        graph.AddNode(new DirectedNode("Input 1", NodeType.Start), []);
        graph.AddNode(new DirectedNode("Input 2", NodeType.Start), []);
        graph.AddNode(new DirectedNode("Input 3", NodeType.Start), []);
        graph.AddNode(new DirectedNode("Input 4", NodeType.Start), []);
        graph.AddNode(new DirectedNode("Output 1", NodeType.End), []);
        graph.AddNode(new DirectedNode("Output 2", NodeType.End), []);

        // Add edges and split random edges 3 times
        for (var i = 0; i < 3; i++)
        {
            // Add five random edges
            for (var j = 0; j < 2; j++)
                graph.AddRandomEdge();
            // split five random edges
            for (var j = 0; j < 2; j++)
                graph.SplitRandomEdge(() => new DirectedNode($"{i}_{j}", NodeType.Hidden));
        }

        return graph;
    }

    private DirectedAcyclicGraph<DirectedNode> GetTestGraph()
    {
        var graph = new DirectedAcyclicGraph<DirectedNode>();

        var node1 = new DirectedNode(1.ToString(), NodeType.Start);
        var node2 = new DirectedNode(2.ToString(), NodeType.Start);
        var node3 = new DirectedNode(3.ToString(), NodeType.Start);
        var node4 = new DirectedNode(4.ToString(), NodeType.Hidden);
        var node5 = new DirectedNode(5.ToString(), NodeType.Hidden);
        var node6 = new DirectedNode(6.ToString(), NodeType.Hidden);
        var node7 = new DirectedNode(7.ToString(), NodeType.Hidden);
        var node8 = new DirectedNode(8.ToString(), NodeType.End);
        var node9 = new DirectedNode(9.ToString(), NodeType.End);

        graph.AddNode(node1, [node5, node4]);
        graph.AddNode(node2, [node4, node6]);
        graph.AddNode(node3, [node7]);
        graph.AddNode(node4, [node6, node9]);
        graph.AddNode(node5, [node8]);
        graph.AddNode(node6, [node7, node8]);
        graph.AddNode(node7, [node9]);

        return graph;
    }
}
