using DirectedAcyclicGraph.Models;

namespace SimpleAI;

internal class GraphTests
{
    public static void Benchmark(string[] args)
    {
        var graph = new DirectedAcyclicGraph.Models.DirectedAcyclicGraph();

        // Node counts
        const int startNodesCount = 3;
        const int hiddenNodesCount = 10;
        const int endNodesCount = 1;

        // Add start, hidden and end nodes
        for (var i = 0; i < startNodesCount; i++)
            graph.AddNode(new Node(i.ToString(), NodeType.Start), []);
        for (var i = startNodesCount; i < startNodesCount + hiddenNodesCount; i++)
            graph.AddNode(new Node(i.ToString(), NodeType.Hidden), []);
        for (
            var i = startNodesCount + hiddenNodesCount;
            i < startNodesCount + hiddenNodesCount + endNodesCount;
            i++
        )
            graph.AddNode(new Node(i.ToString(), NodeType.End), []);

        // Add graphs until max possible edges (Benchmark)
        var maxEdges = graph.GetMaxPossibleEdges();
        var count = 0;
        var start = DateTimeOffset.Now;
        while (count <= maxEdges)
        {
            var added = graph.AddRandomEdge();

            // Count up if not reached max edges
            if (count < maxEdges && added)
                count++;
            else
            {
                // Should not be able to add more edges after max
                if (count == maxEdges && !added)
                    break;
                if (count == maxEdges && added)
                    throw new Exception("Added more edges while it should be impossible.");
            }
        }

        var end = DateTimeOffset.Now;
        var timespan = end - start;

        var (sortedLayers, detachedNodes) = graph.GetTopologicallySortedNodes();
        var sortedNodes = sortedLayers.SelectMany(x => x).ToList();
    }

    public static void Test1(string[] args)
    {
        var graph = new DirectedAcyclicGraph.Models.DirectedAcyclicGraph();

        var node1 = new Node(1.ToString(), NodeType.Start);
        var node2 = new Node(2.ToString(), NodeType.Start);
        var node3 = new Node(3.ToString(), NodeType.Start);
        var node4 = new Node(4.ToString(), NodeType.Hidden);
        var node5 = new Node(5.ToString(), NodeType.Hidden);
        var node6 = new Node(6.ToString(), NodeType.Hidden);
        var node7 = new Node(7.ToString(), NodeType.Hidden);
        var node8 = new Node(8.ToString(), NodeType.End);
        var node9 = new Node(9.ToString(), NodeType.End);

        graph.AddNode(node1, [node5, node4]);
        graph.AddNode(node2, [node4, node6]);
        graph.AddNode(node3, [node7]);
        graph.AddNode(node4, [node6, node9]);
        graph.AddNode(node5, [node8]);
        graph.AddNode(node6, [node7]);
        graph.AddEdge(node6, node8);
        graph.AddNode(node7, [node9]);

        // graph.AddEdge(node7, node4); // Cycle error
        // graph.AddRandomEdge(); // Adds random edge

        // graph.SplitRandomEdge(new Node(10)); // Splits random edge and adds node 10

        // graph.RemoveEdge(node5, node8, true);

        // graph.TrimDeadEnds();

        // graph.RemoveNode(node4);
        // graph.RemoveNode(node9); // Cannot remove node 9 because it is a final node

        // graph.RemoveNode(node5);
        // graph.RemoveNode(node6); // Orphans node 8, should fail? (if end node is not useful, no connection will be encouraged?). Check if there still exists a path from any end node to any start node

        var (sortedLayers, detachedNodes) = graph.GetTopologicallySortedNodes();
        var sortedNodes = sortedLayers.SelectMany(x => x).ToList();
    }

    public static void Test2(string[] args)
    {
        var graph = new DirectedAcyclicGraph.Models.DirectedAcyclicGraph();

        var node1 = new Node(1.ToString(), NodeType.Start);
        var node2 = new Node(2.ToString(), NodeType.Start);
        var node3 = new Node(3.ToString(), NodeType.Start);

        var node4 = new Node(4.ToString(), NodeType.Hidden);

        var node5 = new Node(5.ToString(), NodeType.Hidden);
        var node6 = new Node(6.ToString(), NodeType.Hidden);

        var node7 = new Node(7.ToString(), NodeType.Hidden);

        var node8 = new Node(8.ToString(), NodeType.Hidden);
        var node9 = new Node(9.ToString(), NodeType.Hidden);

        var node10 = new Node(10.ToString(), NodeType.Hidden);

        var node11 = new Node(11.ToString(), NodeType.End);

        graph.AddNode(node1, [node4, node8]);
        graph.AddNode(node2, [node4]);
        graph.AddNode(node3, [node6]);
        graph.AddNode(node4, [node5, node6]);
        graph.AddNode(node5, [node7]);
        graph.AddNode(node6, [node7, node9]);
        graph.AddNode(node7, [node8]);
        graph.AddNode(node8, [node10]);
        graph.AddNode(node9, [node10]);
        graph.AddNode(node10, [node11]);

        // graph.RemoveEdge(node8, node10, true); // Prunes 8, 7 and 5

        // graph.RemoveEdge(node8, node10, false); // Keeps 8, 7 and 5
        // graph.TrimDeadEnds(); // Prunes 8, 7 and 5

        // graph.RemoveEdge(node10, node11, true); // Prunes all hidden nodes


        // for (var i = 12; i < 22; i++)
        //     graph.SplitRandomEdge(new Node(i, NodeType.Hidden));

        var (sortedLayers, detachedNodes) = graph.GetTopologicallySortedNodes();
        var sortedNodes = sortedLayers.SelectMany(x => x).ToList();
    }
}
