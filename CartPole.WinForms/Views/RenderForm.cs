using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using CartPolePhysics.SinglePole.DoublePrecision;
using CartPoleShared.Constants;
using CartPoleShared.Extensions;
using CartPoleShared.Functions;
using CartPoleShared.Helpers;
using CartPoleShared.Models.Environment;
using CartPoleWinForms.Controls;
using Graphs.Functions;
using Graphs.Models;
using SimpleAI.Models;

namespace CartPoleWinForms.Views;

public class RenderForm : Form
{
    private readonly CartPole _cartPole;
    private Stopwatch PhysicsStopwatch { get; set; }
    private CartSinglePolePhysicsRK4 Physics { get; set; }

    private double ForceMagnitude { get; set; } = 0;

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
        _cartPole = EnvironmentHelpers.CreateNewCartPole(
            // AngleConstants.Radians.Up - 0.01
            EnvironmentConstants.CartPoleDimensions.CartStartingPosition,
            AngleConstants.Radians.Down
        );

        // Load graph
        var graph = new DirectedAcyclicGraph();
        graph.LoadGraph(
            "C:\\Users\\Cedric\\Personal\\Development\\.NET\\CartPoleAI\\CartPole.Console\\bin\\Debug\\net8.0\\V1\\graph-0.bin"
        );
        var (sortedLayers, _) = graph.GetTopologicallySortedNodes();

        Controls.Add(new RenderControl(_cartPole, sortedLayers));

        // Register keypress handlers
        KeyPreview = true;
        KeyDown += Handle_KeyDown;
        KeyUp += Handle_KeyUp;

        StartPhysics(graph);

        /**
         * TODO: Start training in the background. When no animation is running, take the best performing graph and
         * run it for 10 seconds. Keep doing this until training is stopped.
         */
    }

    private void StartPhysics(DirectedAcyclicGraph? graph = null)
    {
        // Initialize Physics with initial CartPole state
        Physics = EnvironmentHelpers.CreatePhysics(_cartPole);
        // Initialize agent if necessary
        var agent = graph != null ? new Agent(graph) : null;

        // Start the physics loop in a separate thread
        PhysicsStopwatch = new Stopwatch();
        new Thread(() =>
        {
            while (true)
            {
                // Only trigger a repaint if enough time has passed for the next physics update
                if (PhysicsStopwatch.ElapsedMilliseconds < EnvironmentConstants.Physics.TimeStepMs)
                    continue; // 100Hz
                PhysicsStopwatch.Restart();
                UpdatePhysics(agent);
            }
        })
        {
            IsBackground = true
        }.Start(); // Run the thread in the background so it doesn't prevent the application from exiting
        PhysicsStopwatch.Start();
    }

    private void UpdatePhysics(Agent? agent)
    {
        // Calculate force to apply to cart
        var force = agent?.CalculateOutputValue(Physics.State)[0] ?? 0;
        // Update physics
        Physics.Update(ForceMagnitude != 0 ? ForceMagnitude : force);
        _cartPole.SetState(Physics.State);
    }

    private DirectedAcyclicGraph GetRandomGraph()
    {
        var graph = new DirectedAcyclicGraph();
        graph.AddNode(
            GraphFunctions.CreateInputNode(
                "Input 1",
                0,
                ActivationFunctions.ResolveActivationFunction(NodeType.Input)
            ),
            []
        );
        graph.AddNode(
            GraphFunctions.CreateInputNode(
                "Input 2",
                1,
                ActivationFunctions.ResolveActivationFunction(NodeType.Input)
            ),
            []
        );
        graph.AddNode(
            GraphFunctions.CreateInputNode(
                "Input 3",
                2,
                ActivationFunctions.ResolveActivationFunction(NodeType.Input)
            ),
            []
        );
        graph.AddNode(
            GraphFunctions.CreateInputNode(
                "Input 4",
                3,
                ActivationFunctions.ResolveActivationFunction(NodeType.Input)
            ),
            []
        );
        graph.AddNode(
            GraphFunctions.CreateOutputNode(
                "Output 1",
                0,
                ActivationFunctions.ResolveActivationFunction(NodeType.Output)
            ),
            []
        );
        graph.AddNode(
            GraphFunctions.CreateOutputNode(
                "Output 2",
                1,
                ActivationFunctions.ResolveActivationFunction(NodeType.Output)
            ),
            []
        );

        // Add 5 random hidden nodes
        for (var i = 0; i < 5; i++)
            graph.AddNode(
                GraphFunctions.CreateHiddenNode(
                    $"Hidden {i}",
                    ActivationFunctions.ResolveActivationFunction(NodeType.Hidden)
                ),
                []
            );

        // Add edges and split random edges 3 times
        for (var i = 0; i < 3; i++)
        {
            // Add three random edges
            for (var j = 0; j < 3; j++)
                graph.AddRandomEdge();
            // split two random edges
            for (var j = 0; j < 2; j++)
                graph.SplitRandomEdge(
                    () =>
                        GraphFunctions.CreateHiddenNode(
                            $"{i}_{j}",
                            ActivationFunctions.ResolveActivationFunction(NodeType.Hidden)
                        )
                );
        }

        return graph;
    }

    private DirectedAcyclicGraph GetTestGraph()
    {
        var graph = new DirectedAcyclicGraph();

        var node1 = GraphFunctions.CreateInputNode(
            1.ToString(),
            0,
            ActivationFunctions.ResolveActivationFunction(NodeType.Input)
        );
        var node2 = GraphFunctions.CreateInputNode(
            2.ToString(),
            1,
            ActivationFunctions.ResolveActivationFunction(NodeType.Input)
        );
        var node3 = GraphFunctions.CreateInputNode(
            3.ToString(),
            2,
            ActivationFunctions.ResolveActivationFunction(NodeType.Input)
        );
        var node4 = GraphFunctions.CreateHiddenNode(
            4.ToString(),
            ActivationFunctions.ResolveActivationFunction(NodeType.Hidden)
        );
        var node5 = GraphFunctions.CreateHiddenNode(
            5.ToString(),
            ActivationFunctions.ResolveActivationFunction(NodeType.Hidden)
        );
        var node6 = GraphFunctions.CreateHiddenNode(
            6.ToString(),
            ActivationFunctions.ResolveActivationFunction(NodeType.Hidden)
        );
        var node7 = GraphFunctions.CreateHiddenNode(
            7.ToString(),
            ActivationFunctions.ResolveActivationFunction(NodeType.Hidden)
        );
        var node8 = GraphFunctions.CreateOutputNode(
            8.ToString(),
            0,
            ActivationFunctions.ResolveActivationFunction(NodeType.Output)
        );
        var node9 = GraphFunctions.CreateOutputNode(
            9.ToString(),
            1,
            ActivationFunctions.ResolveActivationFunction(NodeType.Output)
        );

        graph.AddNode(node1, [node5, node4]);
        graph.AddNode(node2, [node4, node6]);
        graph.AddNode(node3, [node7]);
        graph.AddNode(node4, [node6, node9]);
        graph.AddNode(node5, [node8]);
        graph.AddNode(node6, [node7, node8]);
        graph.AddNode(node7, [node9]);

        return graph;
    }

    #region Key press handlers

    private void Handle_KeyDown(object sender, KeyEventArgs e)
    {
        const int forceMagnitude = 10; // Adjust this value to your needs

        switch (e.KeyCode)
        {
            case Keys.Left:
                ForceMagnitude = -forceMagnitude; // Apply force to the left
                break;
            case Keys.Right:
                ForceMagnitude = forceMagnitude; // Apply force to the right
                break;
        }
    }

    private void Handle_KeyUp(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Left:
            case Keys.Right:
                ForceMagnitude = 0; // Stop applying force
                break;
        }
    }

    protected override bool IsInputKey(Keys keyData)
    {
        return keyData switch
        {
            Keys.Left or Keys.Right => true,
            _ => base.IsInputKey(keyData)
        };
    }

    #endregion
}
