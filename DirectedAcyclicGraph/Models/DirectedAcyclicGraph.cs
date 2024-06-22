using DirectedAcyclicGraph.Extensions;
using DirectedAcyclicGraph.Interfaces;

namespace DirectedAcyclicGraph.Models;

/// <summary>
/// Directed Acyclic Graph
/// </summary>
public class DirectedAcyclicGraph<TNode>
    where TNode : DirectedNode, INodeCreatable<TNode>
{
    /// <summary>
    /// Contains a list of source nodes for each node, if there is one.
    /// </summary>
    public Dictionary<TNode, HashSet<TNode>> NodesWithIncomingEdges { get; } = [];

    /// <summary>
    /// Contains a list of destination nodes for each node, if there is one
    /// </summary>
    internal Dictionary<TNode, HashSet<TNode>> NodesWithOutgoingEdges { get; } = [];

    /// <summary>
    /// Contains all nodes in the graph.
    /// </summary>
    public HashSet<TNode> UnsortedNodes { get; } = [];

    public int NodeCount => UnsortedNodes.Count;

    public DirectedAcyclicGraph() { }

    private DirectedAcyclicGraph(
        Dictionary<TNode, HashSet<TNode>> nodesWithIncomingEdges,
        Dictionary<TNode, HashSet<TNode>> nodesWithOutgoingEdges,
        HashSet<TNode> unsortedNodes
    )
    {
        NodesWithIncomingEdges = nodesWithIncomingEdges;
        NodesWithOutgoingEdges = nodesWithOutgoingEdges;
        UnsortedNodes = unsortedNodes;
    }

    /// <summary>
    /// Splits a random edge in two and adds the provided node in between.
    /// </summary>
    public bool SplitRandomEdge(Func<TNode> hiddenNodeFactory)
    {
        var nodeToAdd = hiddenNodeFactory();

        if (NodesWithOutgoingEdges.Count < 1)
            return false;
        if (nodeToAdd.Type is not NodeType.Hidden)
            throw new ArgumentException("Can only add hidden nodes in between.");

        // Choose random outgoing edge
        var random = new Random();
        var from = NodesWithOutgoingEdges.Keys.ElementAt(random.Next(NodesWithOutgoingEdges.Count));
        var to = NodesWithOutgoingEdges[from]
            .ElementAt(random.Next(NodesWithOutgoingEdges[from].Count));

        // Remove the edge
        RemoveEdge(from, to, false);

        // Try to add the new node in between
        AddEdge(from, nodeToAdd);
        try
        {
            AddEdge(nodeToAdd, to);
            UnsortedNodes.Add(nodeToAdd);
        }
        catch (Exception)
        {
            // Removes the previous edge and restores initial connection
            RemoveEdge(from, nodeToAdd, false);
            AddEdge(from, to);
            throw;
        }

        return false;
    }

    /// <summary>
    /// Creates a random edge between two nodes, starting from one that
    /// already has outgoing edges, to one that already has incoming edges.
    /// </summary>
    public bool AddRandomEdge()
    {
        // Get all nodes that are not end nodes
        var fromNodes = UnsortedNodes.Where(node => node.Type is not NodeType.End).ToList();

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
                    node.Type is not NodeType.Start
                    && node != from
                    && !(
                        (
                            NodesWithOutgoingEdges.ContainsKey(from)
                            && NodesWithOutgoingEdges[from].Contains(node)
                        ) // No existing connections
                        || (
                            NodesWithIncomingEdges.ContainsKey(from)
                            && NodesWithIncomingEdges[from].Contains(node)
                        ) // No cyclic connections
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
                try
                {
                    AddEdge(from, to);
                    edgeCreatedSuccessfully = true;
                }
                catch (Exception)
                {
                    // ignored
                }
            } while (!edgeCreatedSuccessfully && toNodes.Count > 0);
        } while (!edgeCreatedSuccessfully && fromNodes.Count > 0);

        return edgeCreatedSuccessfully;
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
            throw new ArgumentException("Not enough edges to remove.");

        // Choose random outgoing edge
        var random = new Random();
        var from = NodesWithOutgoingEdges.Keys.ElementAt(random.Next(NodesWithOutgoingEdges.Count));
        var to = NodesWithOutgoingEdges[from]
            .ElementAt(random.Next(NodesWithOutgoingEdges[from].Count));

        // Remove the edge
        RemoveEdge(from, to, true);
        return true;
    }

    /// <summary>
    /// Adds a node with outgoing edges to the graph.
    /// </summary>
    public void AddNode(TNode from, IList<TNode> outgoingEdges)
    {
        if (NodesWithOutgoingEdges.ContainsKey(from))
            throw new ArgumentException("Provided node already exists.");
        if (CausesCycle(from, outgoingEdges))
            throw new ArgumentException("Adding this node causes a cycle.");

        // Add node and outgoing edges
        if (outgoingEdges.Any())
            NodesWithOutgoingEdges.Add(from, [.. outgoingEdges]);
        UnsortedNodes.Add(from);

        // Add incoming node for each node of outgoing edge
        foreach (var dest in outgoingEdges)
        {
            if (!NodesWithIncomingEdges.TryGetValue(dest, out var incoming))
            {
                NodesWithIncomingEdges[dest] = [from];
                UnsortedNodes.Add(dest);
            }
            else
                incoming.Add(from);
        }
    }

    /// <summary>
    /// Removes a node and its edges.
    /// !!! THIS IS A DESTRUCTIVE OPERATIONS, AS IT DELETES ALL CONNECTIONS !!!
    /// </summary>
    public void RemoveNode(TNode node, bool removeDanglingIncomingHiddenNodes)
    {
        // Check if node exists in graph
        if (!UnsortedNodes.Contains(node))
            throw new ArgumentException("Provided node does not exist in graph.");

        // Remove any outgoing edges
        if (NodesWithOutgoingEdges.TryGetValue(node, out var outgoingEdges))
        {
            foreach (var outgoingEdge in outgoingEdges)
                RemoveEdge(node, outgoingEdge, removeDanglingIncomingHiddenNodes);
            NodesWithOutgoingEdges.Remove(node);
        }

        // Remove any incoming edges
        if (NodesWithIncomingEdges.TryGetValue(node, out var incomingEdges))
            foreach (var incomingEdge in incomingEdges)
                RemoveEdge(incomingEdge, node, removeDanglingIncomingHiddenNodes);
        NodesWithIncomingEdges.Remove(node);

        UnsortedNodes.Remove(node);
    }

    /// <summary>
    /// Adds an edge to the graph, if it does not cause a cycle.
    /// </summary>
    public void AddEdge(TNode from, TNode to)
    {
        if (CausesCycle(from, [to]))
            throw new ArgumentException("Adding this edge causes a cycle.");
        // Check if edge already exists
        if (
            NodesWithOutgoingEdges.TryGetValue(from, out var outgoingEdges)
            && outgoingEdges.Contains(to)
        )
            throw new ArgumentException("Provided edge already exists.");

        // Add from node to outgoing edges
        if (!NodesWithOutgoingEdges.TryGetValue(from, out outgoingEdges))
        {
            NodesWithOutgoingEdges[from] = [to];
            UnsortedNodes.Add(from);
        }
        else
            outgoingEdges.Add(to);

        // Add to node to incoming edges
        if (!NodesWithIncomingEdges.TryGetValue(to, out var incomingEdges))
        {
            NodesWithIncomingEdges[to] = [from];
            UnsortedNodes.Add(to);
        }
        else
            incomingEdges.Add(from);
    }

    /// <summary>
    /// Removes an edge from the graph. This might cause dangling nodes.
    /// </summary>
    public void RemoveEdge(TNode from, TNode to, bool removeFromNodeIfIsDangling)
    {
        // Check if node exists in graph
        if (!UnsortedNodes.Contains(from))
            throw new ArgumentException("Provided node does not exist in graph.");

        if (!NodesWithOutgoingEdges.TryGetValue(from, out var outgoing))
            throw new ArgumentException(
                "Cannot remove outgoing node if it has no outgoing connections."
            );
        if (!outgoing.Remove(to))
            throw new ArgumentException("Provided edge does not exist.");
        if (outgoing.Count == 0)
            NodesWithOutgoingEdges.Remove(from);

        // Remove node from list if it has no incoming or outgoing edges
        NodesWithIncomingEdges[to].Remove(from);
        if (NodesWithIncomingEdges[to].Count == 0)
            NodesWithIncomingEdges.Remove(to);

        // If node is hidden node and has no outgoing edges, remove it if requested
        if (removeFromNodeIfIsDangling && from.Type is NodeType.Hidden && outgoing.Count == 0)
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
                .Where(node =>
                    node.Type == NodeType.Hidden && !NodesWithOutgoingEdges.ContainsKey(node)
                )
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
    public void Trim(IEnumerable<TNode> nodesToStartFrom)
    {
        ArgumentNullException.ThrowIfNull(nodesToStartFrom);

        var unsearched = new Queue<TNode>(nodesToStartFrom);
        var unconnected = new HashSet<TNode>(NodesWithOutgoingEdges.Keys);
        while (unsearched.Count != 0)
        {
            var key = unsearched.Dequeue();
            if (!unconnected.Contains(key))
                continue; // We have already found this key.

            // We've found the key, so we can remove it from the unconnected list.
            unconnected.Remove(key);
            foreach (var outgoing in NodesWithOutgoingEdges[key])
                unsearched.Enqueue(outgoing);
        }

        foreach (var key in unconnected)
            RemoveNode(key, false);
    }

    /// <summary>
    /// Checks if adding the node causes a cycle.
    /// </summary>
    public bool CausesCycle(TNode key, IList<TNode> outgoing)
    {
        if (outgoing.Contains(key))
            return true; // Self cycle
        if (!NodesWithIncomingEdges.TryGetValue(key, out var incoming) || incoming.Count == 0)
            return false; // No incoming edges, so we can't have a cycle.

        // If a path exists from any outgoing edge to any incoming edge, adding the node will cause a cycle.
        return outgoing.Any(start => incoming.Any(end => PathExists(start, end)));
    }

    /// <summary>
    /// Checks if a path exists between the given start and end nodes.
    /// </summary>
    public bool PathExists(TNode start, TNode end)
    {
        var tested = new HashSet<TNode> { start };
        var queued = new Queue<TNode>(tested);

        while (queued.Count > 0)
        {
            var current = queued.Dequeue();
            if (current.Equals(end)) // A path exists
                return true;

            if (!NodesWithOutgoingEdges.TryGetValue(current, out var destinations))
                continue; // No out edges for current

            foreach (var destination in destinations.Where(destination => tested.Add(destination)))
            {
                queued.Enqueue(destination);
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the nodes topologically sorted into layers. Layers are sorted from left to right (start nodes to end nodes).
    /// Any node that has no path to a start node is considered detached.
    /// </summary>
    /// <returns>A tuple containing a list of layers and a list of detached keys.</returns>
    public (List<List<TNode>> layers, List<TNode> detached) GetTopologicallySortedNodes()
    {
        var clonedGraph = Clone();
        var layers = new List<List<TNode>>();

        // First, add all start nodes
        var remainingNodes = clonedGraph.UnsortedNodes.ToList();
        layers.Add([]);
        remainingNodes = remainingNodes
            .Where(clonedNode =>
            {
                // Any start node or any node does not have incoming edges but has outgoing edges
                var isStartNode =
                    clonedNode.Type == NodeType.Start
                    || !clonedGraph.NodesWithIncomingEdges.ContainsKey(clonedNode)
                        && clonedGraph.NodesWithOutgoingEdges.ContainsKey(clonedNode);
                if (isStartNode)
                {
                    clonedGraph.RemoveNode(clonedNode, false);
                    layers[0].Add(GetNodeById(clonedNode.Id));
                }

                return !isStartNode;
            })
            .ToList();

        // Get all end nodes
        var endLayer = new List<TNode>();
        remainingNodes = remainingNodes
            .Where(clonedNode =>
            {
                var isEndNode = clonedNode.Type == NodeType.End;
                if (isEndNode)
                {
                    clonedGraph.RemoveNode(clonedNode, false);
                    endLayer.Add(GetNodeById(clonedNode.Id));
                }

                return !isEndNode;
            })
            .ToList();

        // Filter out any detached nodes
        var detached = new List<TNode>();
        remainingNodes = remainingNodes
            .Where(clonedNode =>
            {
                // Check if the current node has a path to any start node
                var hasPathToStart = layers[0].Any(start => PathExists(start, clonedNode));
                // If not, add it to the detached list
                if (!hasPathToStart)
                    detached.Add(GetNodeById(clonedNode.Id));
                return hasPathToStart;
            })
            .ToList();

        // Sort remaining nodes topologically until none are left
        while (remainingNodes.Count > 0)
        {
            // Create new layer
            var currentLayer = new List<TNode>();

            // Find all nodes that have no incoming edges
            var newRemainingNodes = new List<TNode>();
            var nodesToRemove = new List<TNode>();
            foreach (var clonedNode in remainingNodes)
            {
                // If the cloned node has no incoming edges, add the real node to the current layer
                if (!clonedGraph.NodesWithIncomingEdges.ContainsKey(clonedNode))
                {
                    currentLayer.Add(GetNodeById(clonedNode.Id));
                    nodesToRemove.Add(clonedNode);
                }
                // Otherwise, add the cloned node to the new remaining nodes
                else
                    newRemainingNodes.Add(clonedNode);
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

        // // Convert nodes to directed nodes by linking incoming and outgoing connections to children and parents
        // Dictionary<TNode, TNode> nodeToDirectedNodesMap = new();
        // List<List<TNode>> directedNodeLayers = [];
        // foreach (var layer in layers)
        // {
        //     var directedNodeLayer = new List<TNode>();
        //     foreach (var node in layer)
        //     {
        //         // Map parent and child nodes to directed nodes
        //         HashSet<TNode> incomingDirectedNodes;
        //         if (NodesWithIncomingEdges.TryGetValue(node, out var incomingNodes))
        //         {
        //             incomingDirectedNodes = incomingNodes
        //                 .Select(parent =>
        //                 {
        //                     nodeToDirectedNodesMap.TryGetValue(parent, out var parentDirectedNode);
        //                     if (parentDirectedNode is null)
        //                     {
        //                         parentDirectedNode = parent.CloneNode(
        //                             parent,
        //                             new HashSet<TNode>(),
        //                             new HashSet<TNode>()
        //                         );
        //                         nodeToDirectedNodesMap.Add(parent, parentDirectedNode);
        //                     }
        //
        //                     return parentDirectedNode;
        //                 })
        //                 .ToHashSet();
        //         }
        //         else
        //         {
        //             incomingDirectedNodes = [];
        //         }
        //
        //         HashSet<TNode> outgoingDirectedNodes;
        //         if (NodesWithOutgoingEdges.TryGetValue(node, out var outgoingNodes))
        //         {
        //             outgoingDirectedNodes = outgoingNodes
        //                 .Select(child =>
        //                 {
        //                     nodeToDirectedNodesMap.TryGetValue(child, out var childDirectedNode);
        //                     if (childDirectedNode is null)
        //                     {
        //                         childDirectedNode = child.CloneNode(
        //                             child,
        //                             new HashSet<TNode>(),
        //                             new HashSet<TNode>()
        //                         );
        //                         nodeToDirectedNodesMap.Add(child, childDirectedNode);
        //                     }
        //
        //                     return childDirectedNode;
        //                 })
        //                 .ToHashSet();
        //         }
        //         else
        //         {
        //             outgoingDirectedNodes = [];
        //         }
        //
        //         // Get directed node from map. If it does not exist, create a new one.
        //         nodeToDirectedNodesMap.TryGetValue(node, out var directedNode);
        //         if (directedNode is null)
        //         {
        //             directedNode = node.CloneNode(
        //                 node,
        //                 incomingDirectedNodes,
        //                 outgoingDirectedNodes
        //             );
        //             nodeToDirectedNodesMap.Add(node, directedNode);
        //         }
        //         else
        //         {
        //             foreach (var incomingDirectedNode in incomingDirectedNodes)
        //                 directedNode.Parents.Add(incomingDirectedNode);
        //             foreach (var outgoingDirectedNode in outgoingDirectedNodes)
        //                 directedNode.Children.Add(outgoingDirectedNode);
        //         }
        //
        //         // Add directed node to incoming and outgoing nodes
        //         foreach (var incomingDirectedNode in incomingDirectedNodes)
        //             incomingDirectedNode.Children.Add(directedNode);
        //         foreach (var outgoingDirectedNode in outgoingDirectedNodes)
        //             outgoingDirectedNode.Parents.Add(directedNode);
        //
        //         // Add directed node to layer
        //         directedNodeLayer.Add((TNode)directedNode);
        //     }
        //
        //     // Add layer to directed node layers
        //     directedNodeLayers.Add(directedNodeLayer);
        // }

        return (layers, detached);
    }

    /// <summary>
    /// Creates a shallow copy of the graph, referencing the same keys and data as the original.
    /// </summary>
    /// <returns>A shallow copy of the original.</returns>
    public DirectedAcyclicGraph<TNode> Clone() =>
        new(
            NodesWithIncomingEdges.ToDictionary(
                pair => pair.Key,
                pair => new HashSet<TNode>(pair.Value)
            ),
            NodesWithOutgoingEdges.ToDictionary(
                pair => pair.Key,
                pair => new HashSet<TNode>(pair.Value)
            ),
            UnsortedNodes.ToHashSet()
        );

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
        var startNodesCount = UnsortedNodes.Count(n => n.Type == NodeType.Start);
        var hiddenNodesCount = UnsortedNodes.Count(n => n.Type == NodeType.Hidden);
        var endNodesCount = UnsortedNodes.Count(n => n.Type == NodeType.End);
        return startNodesCount * (hiddenNodesCount + endNodesCount) // Edges from start nodes to all hidden and end nodes
            + hiddenNodesCount * (hiddenNodesCount - 1) / 2 // Edges between hidden nodes: n * ( n − 1 ) / 2
            + hiddenNodesCount * endNodesCount; // Edges from hidden nodes to all end nodes
    }

    /// <summary>
    /// Calculates the current amount of edges in the graph
    /// </summary>
    /// <returns></returns>
    public int GetEdgeCount() => NodesWithOutgoingEdges.Sum(pair => pair.Value.Count);

    public TNode GetNodeById(Guid id) => UnsortedNodes.Single(node => node.Id == id);
}
