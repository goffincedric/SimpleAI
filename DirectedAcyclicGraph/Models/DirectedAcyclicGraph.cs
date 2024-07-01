using Graphs.Extensions;
using Graphs.Functions;

namespace Graphs.Models;

/// <summary>
/// A Directed Acyclic Graph (DAG) with biased nodes and directional weighted edges.
/// This graph doesn't have any cycles and is used for neural networks.
/// </summary>
public class DirectedAcyclicGraph
{
    /// <summary>
    /// Contains all nodes in the graph.
    /// </summary>
    public HashSet<WeightedNode> UnsortedNodes { get; }

    /// <summary>
    /// Contains a list of source nodes for each node, if there is one.
    /// </summary>
    public List<WeightedNode> NodesWithIncomingEdges =>
        UnsortedNodes.Where(node => node.Parents.Count > 0).ToList();

    /// <summary>
    /// Contains a list of destination nodes for each node, if there is one
    /// </summary>
    public List<WeightedNode> NodesWithOutgoingEdges =>
        UnsortedNodes.Where(node => node.Children.Count > 0).ToList();

    public int NodeCount => UnsortedNodes.Count;

    public DirectedAcyclicGraph()
        : this([]) { }

    private DirectedAcyclicGraph(HashSet<WeightedNode> unsortedNodes)
    {
        UnsortedNodes = unsortedNodes;
    }

    /// <summary>
    /// Splits a random edge in two and adds the provided node in between.
    /// </summary>
    public bool SplitRandomEdge(Func<WeightedNode> hiddenNodeFactory)
    {
        if (NodesWithOutgoingEdges.Count < 1)
            return false;

        var nodeToAdd = hiddenNodeFactory();
        if (nodeToAdd.Type is not NodeType.Hidden)
            throw new ArgumentException("Can only add hidden nodes in between.");

        // Choose random outgoing edge
        var random = new Random();
        var from = NodesWithOutgoingEdges.ElementAt(random.Next(NodesWithOutgoingEdges.Count));
        var to = from.Children.ElementAt(random.Next(from.Children.Count));

        // Save old weight
        var weight = to.Parents[from];

        // Remove the edge
        RemoveEdge(from, to, false);

        // Try to add the new node in between
        AddNode(nodeToAdd, []);
        AddEdge(from, nodeToAdd, weight);
        try
        {
            AddEdge(nodeToAdd, to, 1);
            UnsortedNodes.Add(nodeToAdd);
        }
        catch (Exception)
        {
            // Removes the previous edge and restores initial connection
            RemoveEdge(from, nodeToAdd, false);
            RemoveNode(nodeToAdd, false);
            AddEdge(from, to, weight);
            throw;
        }

        return true;
    }

    /// <summary>
    /// Creates a random edge between two nodes, starting from one that
    /// already has outgoing edges, to one that already has incoming edges.
    /// </summary>
    public bool AddRandomEdge()
    {
        // Try to find two possible nodes to link
        var possibleEdge = FindPossibleNonCyclicEdge();
        if (possibleEdge is null)
            return false;
        var (from, to) = possibleEdge.Value;

        // Add the edge
        AddEdge(from, to, 0);
        return true;
    }

    /// <summary>
    /// Removes a random edge from the graph.
    /// !!! THIS IS A DESTRUCTIVE OPERATIONS, AS IT DELETES ALL CONNECTIONS BEFORE THE REMOVED EDGE IF THE NODE IS DANGLING !!!
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public bool RemoveRandomEdge()
    {
        if (NodesWithOutgoingEdges.Count < 1)
            return false;

        // Choose random outgoing edge
        var random = new Random();
        var from = NodesWithOutgoingEdges.ElementAt(random.Next(NodesWithOutgoingEdges.Count));
        var to = from.Children.ElementAt(random.Next(from.Children.Count));

        // Remove the edge
        RemoveEdge(from, to, true);
        return true;
    }

    /// <summary>
    /// Adds a node in between two random nodes.
    /// </summary>
    /// <param name="hiddenNodeFactory"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public bool AddRandomNode(Func<WeightedNode> hiddenNodeFactory)
    {
        var nodeToAdd = hiddenNodeFactory();
        if (nodeToAdd.Type is not NodeType.Hidden)
            throw new ArgumentException("Can only add hidden nodes in between.");

        // Try to find two possible nodes to link
        var possibleEdge = FindPossibleNonCyclicEdge();
        if (possibleEdge is null)
            return false;
        var (from, to) = possibleEdge.Value;

        // Add the node and edges
        AddNode(nodeToAdd, []);
        AddEdge(from, nodeToAdd, 0);
        AddEdge(nodeToAdd, to, 0);
        return true;
    }

    public bool RemoveRandomNode()
    {
        // If no hidden nodes are present, no node can be removed
        var hiddenNodes = UnsortedNodes.Where(node => node.Type is NodeType.Hidden).ToList();
        if (hiddenNodes.Count == 0)
            return false;

        // Choose random hidden node
        var random = new Random();
        var nodeToRemove = hiddenNodes.ElementAt(random.Next(hiddenNodes.Count));

        // Remove the node
        RemoveNode(nodeToRemove, true);
        return true;
    }

    /// <summary>
    /// Adds a node with outgoing edges to the graph.
    /// </summary>
    public void AddNode(WeightedNode from, List<WeightedNode> outgoingEdges)
    {
        if (UnsortedNodes.Contains(from))
            throw new ArgumentException("Provided from node already exists.");
        if (CausesCycle(from, outgoingEdges))
            throw new ArgumentException("Adding this node causes a cycle.");

        // Add node and outgoing edges
        UnsortedNodes.Add(from);
        outgoingEdges.ForEach(to =>
        {
            UnsortedNodes.Add(to); // Adds to graph if not present already
            AddEdge(from, to); // Add edge to graph
        });
    }

    /// <summary>
    /// Removes a node and its edges.
    /// !!! THIS IS A DESTRUCTIVE OPERATIONS, AS IT DELETES ALL CONNECTIONS !!!
    /// </summary>
    public void RemoveNode(WeightedNode node, bool removeDanglingIncomingHiddenNodes)
    {
        // Check if node exists in graph
        if (!UnsortedNodes.Contains(node))
            throw new ArgumentException("Provided node does not exist in graph.");

        // Remove any outgoing edges
        foreach (var outgoingEdge in node.Children)
            RemoveEdge(node, outgoingEdge, removeDanglingIncomingHiddenNodes);

        // Remove any incoming edges
        foreach (var incomingEdge in node.Parents.Keys)
            RemoveEdge(incomingEdge, node, removeDanglingIncomingHiddenNodes);

        UnsortedNodes.Remove(node);
    }

    /// <summary>
    /// Adds an edge to the graph, if it does not cause a cycle.
    /// </summary>
    public void AddEdge(WeightedNode from, WeightedNode to, double? weight = null)
    {
        // Check if nodes exist in graph
        if (!UnsortedNodes.Contains(from))
            throw new ArgumentException("Provided from node does not exist in graph.");
        if (!UnsortedNodes.Contains(to))
            throw new ArgumentException("Provided to node does not exist in graph.");

        // Check if edge already exists
        if (from.Children.Contains(to))
            throw new ArgumentException("Provided edge already exists.");
        // Check if adding edge causes a cycle
        if (CausesCycle(from, [to]))
            throw new ArgumentException("Adding this edge causes a cycle.");

        // Add from node to outgoing edges
        from.Children.Add(to);

        // Add to node to incoming edges
        to.Parents.Add(from, weight ?? GraphFunctions.GenerateRandomNormalWeight());
    }

    /// <summary>
    /// Removes an edge from the graph. This might cause dangling nodes.
    /// </summary>
    public void RemoveEdge(WeightedNode from, WeightedNode to, bool removeFromNodeIfIsDangling)
    {
        var hashes = UnsortedNodes.Select(n => (n.GetHashCode(), n == from)).ToList();
        var fromHash = from.GetHashCode();
        // Check if node exists in graph
        if (!UnsortedNodes.Contains(from))
            throw new ArgumentException("Provided node does not exist in graph.");

        if (from.Children.Count == 0)
            throw new ArgumentException(
                "Cannot remove outgoing node if it has no outgoing connections."
            );

        if (!from.Children.Remove(to))
            throw new ArgumentException("Provided edge does not exist.");

        // Remove node from list if it has no incoming or outgoing edges
        to.Parents.Remove(from);

        // If node is hidden node and has no outgoing edges, remove it if requested
        if (removeFromNodeIfIsDangling && from.Type is NodeType.Hidden && from.Children.Count == 0)
            RemoveNode(from, removeFromNodeIfIsDangling);
    }

    /// <summary>
    /// Removes hidden nodes that have no outgoing edges.
    /// </summary>
    public void TrimDeadEnds(bool doSinglePass = false)
    {
        bool hasRemovedNodes;
        do
        {
            // Check if any hidden nodes have no outgoing edges
            var hiddenDeadEnds = UnsortedNodes
                .Where(node => node.Type == NodeType.Hidden && node.Children.Count == 0)
                .ToList();

            // Check if any nodes need to be removed
            var hasNodesToRemove = hiddenDeadEnds.Count > 0;
            if (hasNodesToRemove)
            {
                // If found, remove them and repeat
                foreach (var deadEnd in hiddenDeadEnds)
                    RemoveNode(deadEnd, true);
            }

            hasRemovedNodes = hasNodesToRemove;
        } while (hasRemovedNodes && !doSinglePass);
    }

    /// <summary>
    /// Removes all nodes that are not reachable from any of the supplied starting nodes. (Untested)
    /// </summary>
    public void Trim(IEnumerable<WeightedNode> nodesToStartFrom)
    {
        ArgumentNullException.ThrowIfNull(nodesToStartFrom);

        var unsearched = new Queue<WeightedNode>(nodesToStartFrom);
        var unconnected = new HashSet<WeightedNode>(NodesWithOutgoingEdges);
        while (unsearched.Count != 0)
        {
            var key = unsearched.Dequeue();
            if (!unconnected.Contains(key))
                continue; // We have already found this key.

            // We've found the key, so we can remove it from the unconnected list.
            unconnected.Remove(key);
            foreach (var outgoing in key.Children)
                unsearched.Enqueue(outgoing);
        }

        foreach (var key in unconnected)
            RemoveNode(key, false);
    }

    /// <summary>
    /// Returns the nodes topologically sorted into layers. Layers are sorted from left to right (start nodes to end nodes).
    /// Any node that has no path to a start node is considered detached.
    /// </summary>
    /// <returns>A tuple containing a list of layers and a list of detached keys.</returns>
    public (
        List<List<WeightedNode>> layers,
        List<WeightedNode> detached
    ) GetTopologicallySortedNodes()
    {
        var clonedGraph = Clone();
        var layers = new List<List<WeightedNode>>();
        var startBias = UnsortedNodes
            .Where(node =>
                node.Type == NodeType.Hidden && node.Children.Count > 0 && node.Parents.Count == 0
            )
            .ToList();

        // First, add all start nodes
        var remainingNodes = clonedGraph
            .UnsortedNodes.Select(clonedNode => (clonedNode, realNode: GetNodeById(clonedNode.Id)))
            .ToList();
        layers.Add([]); // Start node layer
        layers.Add([]); // Biased node layer

        remainingNodes = remainingNodes
            .Where(nodePair =>
            {
                // Any start node
                var isStartNode = nodePair.clonedNode.Type == NodeType.Input;
                if (isStartNode)
                {
                    clonedGraph.RemoveNode(nodePair.clonedNode, false);
                    layers[0].Add(nodePair.realNode);
                    return false;
                }
                // Any hidden node that does not have incoming edges but has outgoing edges
                if (
                    nodePair.realNode.Type == NodeType.Hidden
                    && nodePair.realNode.Parents.Count == 0
                    && nodePair.realNode.Children.Count > 0
                )
                {
                    clonedGraph.RemoveNode(nodePair.clonedNode, false);
                    layers[1].Add(nodePair.realNode);
                    return false;
                }

                return true;
            })
            .ToList();
        if (layers[1].Count == 0)
            layers.RemoveAt(1); // Remove biased layer if no biased nodes are present

        // Get all end nodes
        var endLayer = new List<WeightedNode>();
        remainingNodes = remainingNodes
            .Where(nodePair =>
            {
                var isEndNode = nodePair.clonedNode.Type == NodeType.Output;
                if (isEndNode)
                {
                    clonedGraph.RemoveNode(nodePair.clonedNode, false);
                    endLayer.Add(nodePair.realNode);
                }

                return !isEndNode;
            })
            .ToList();

        // Filter out any detached nodes
        var detached = new List<WeightedNode>();
        remainingNodes = remainingNodes
            .Where(nodePair =>
            {
                // Check if the current node has a path to any end node
                var hasPathToEnd = endLayer.Any(end => PathExists(nodePair.realNode, end));
                // If not, add it to the detached list and remove from cloned graph
                if (!hasPathToEnd)
                {
                    detached.Add(nodePair.realNode);
                    clonedGraph.RemoveNode(nodePair.clonedNode, false);
                }
                return hasPathToEnd;
            })
            .ToList();

        // Sort remaining nodes topologically until none are left
        while (remainingNodes.Count > 0)
        {
            // Create new layer
            var currentLayer = new List<WeightedNode>();

            // Find all nodes that have no incoming edges
            var newRemainingNodes = new List<(WeightedNode, WeightedNode)>();
            var nodesToRemove = new List<WeightedNode>();
            foreach (var nodePair in remainingNodes)
            {
                // If the cloned node has no incoming edges, add the real node to the current layer
                if (nodePair.clonedNode.Parents.Count == 0)
                {
                    currentLayer.Add(nodePair.realNode);
                    nodesToRemove.Add(nodePair.clonedNode);
                }
                // Otherwise, add the cloned node to the new remaining nodes
                else
                    newRemainingNodes.Add((nodePair.clonedNode, nodePair.realNode));
            }

            // Add current layer to layers
            layers.Add(currentLayer);
            // Remove cloned nodes from graph
            foreach (var node in nodesToRemove)
                clonedGraph.RemoveNode(node, true);
            // Update remaining nodes
            remainingNodes = newRemainingNodes;
        }

        // Add end layer as final layer
        layers.Add(endLayer);

        return (layers, detached);
    }

    /// <summary>
    /// Creates a shallow copy of the graph, referencing the same keys and data as the original.
    /// </summary>
    /// <returns>A shallow copy of the original.</returns>
    public DirectedAcyclicGraph Clone()
    {
        var nodePairs = UnsortedNodes
            .Select(node =>
            {
                if (node is IndexedNode indexedNode)
                    return (node, indexedNode.CloneNode(indexedNode, [], []));
                return (node, node.CloneNode(node, [], []));
            })
            .ToDictionary();

        // Link cloned nodes like the original
        foreach (var (originalNode, clonedNode) in nodePairs)
        {
            foreach (var (originalParent, weight) in originalNode.Parents)
                clonedNode.Parents.Add(nodePairs[originalParent], weight);
            clonedNode.Children.UnionWith(
                originalNode.Children.Select(originalChild => nodePairs[originalChild])
            );
        }

        return new DirectedAcyclicGraph(nodePairs.Values.ToHashSet());
    }

    /// <summary>
    /// Clears the graph of all nodes and edges.
    /// </summary>
    public void Clear()
    {
        NodesWithIncomingEdges.Clear();
        NodesWithOutgoingEdges.Clear();
        UnsortedNodes.Clear();
    }

    /// <summary>
    /// Calculates maximum amount of possible edges for the current graph
    /// </summary>
    /// <returns></returns>
    public int GetMaxPossibleEdges()
    {
        var startNodesCount = UnsortedNodes.Count(n => n.Type == NodeType.Input);
        var hiddenNodesCount = UnsortedNodes.Count(n => n.Type == NodeType.Hidden);
        var endNodesCount = UnsortedNodes.Count(n => n.Type == NodeType.Output);
        return startNodesCount * (hiddenNodesCount + endNodesCount) // Edges from start nodes to all hidden and end nodes
            + hiddenNodesCount * (hiddenNodesCount - 1) / 2 // Edges between hidden nodes: n * ( n − 1 ) / 2
            + hiddenNodesCount * endNodesCount; // Edges from hidden nodes to all end nodes
    }

    /// <summary>
    /// Calculates the current amount of edges in the graph
    /// </summary>
    /// <returns></returns>
    public int GetEdgeCount() => NodesWithOutgoingEdges.Sum(node => node.Children.Count);

    private WeightedNode GetNodeById(Guid id) => UnsortedNodes.Single(node => node.Id == id);

    /// <summary>
    /// Checks if adding the node causes a cycle.
    /// </summary>
    private bool CausesCycle(WeightedNode key, IList<WeightedNode> outgoing)
    {
        if (outgoing.Contains(key))
            return true; // Self cycle
        if (key.Parents.Count == 0)
            return false; // No incoming edges, so we can't have a cycle.

        // If a path exists from any outgoing edge to any incoming edge, adding the node will cause a cycle.
        return outgoing.Any(start => key.Parents.Keys.Any(end => PathExists(start, end)));
    }

    /// <summary>
    /// Checks if a path exists between the given start and end nodes.
    /// </summary>
    private bool PathExists(WeightedNode start, WeightedNode end)
    {
        var tested = new HashSet<WeightedNode> { start };
        var queued = new Queue<WeightedNode>(tested);

        while (queued.Count > 0)
        {
            var current = queued.Dequeue();
            if (current.Equals(end)) // A path exists
                return true;

            if (current.Children.Count == 0)
                continue; // No out edges for current

            foreach (
                var destination in current.Children.Where(destination => tested.Add(destination))
            )
            {
                queued.Enqueue(destination);
            }
        }

        return false;
    }

    private (WeightedNode, WeightedNode)? FindPossibleNonCyclicEdge()
    {
        // Get all nodes that are not end nodes
        var fromNodes = UnsortedNodes.Where(node => node.Type is not NodeType.Output).ToList();

        // Try to create an edge until successful
        var random = new Random();
        var edgeCreatedSuccessfully = false;
        do
        {
            // Choose a random node
            var from = fromNodes.RemoveRandomEntry(random);

            // Get all nodes that are not start nodes, the node itself, or are not connected to the node yet
            var toNodes = UnsortedNodes
                .Where(node =>
                    node.Type is not NodeType.Input
                    && node != from
                    && !(
                        from.Children.Contains(node) // No existing connections
                        || from.Children.Contains(node) // No cyclic connections
                    )
                )
                .ToList();

            // If no possible nodes are left, pick another from node
            if (toNodes.Count == 0)
                continue;

            // Try to create an edge with a random toNode until successful
            do
            {
                var to = toNodes.RemoveRandomEntry(random);
                if (!CausesCycle(from, [to]))
                    return (from, to);
            } while (!edgeCreatedSuccessfully && toNodes.Count > 0);
        } while (!edgeCreatedSuccessfully && fromNodes.Count > 0);

        return null;
    }
}
