using CartPoleShared.DTOs;
using CartPoleShared.Models.Graph;
using DirectedAcyclicGraph.Models;
using MemoryPack;

namespace CartPoleShared.Extensions;

public static class GraphExtensions
{
    public static void SaveGraph(this DirectedAcyclicGraph<WeightedNode> graph, string filePath)
    {
        // Check if path is a binary file
        if (Path.GetExtension(filePath) != ".bin")
            throw new ArgumentException("File path must be a binary file");

        // Get sorted nodes
        var (layers, _) = graph.GetTopologicallySortedNodes();
        var sortedNodes = layers.SelectMany(layer => layer).ToList();

        // Get list of nodes
        var nodes = sortedNodes
            .Select(node =>
            {
                int? stateIndex = null;
                if (node is IndexedNode indexedNode)
                    stateIndex = indexedNode.Index;
                return new NodeDto
                {
                    Id = node.Id,
                    Label = node.Label,
                    Type = node.Type,
                    Bias = node.Bias,
                    StateIndex = stateIndex
                };
            })
            .ToList();

        // Get list of weights
        var edges = sortedNodes
            .SelectMany(node =>
                node.IncomingNodeWeights.Select(edge =>
                    (From: node, To: (WeightedNode)edge.Key, Weight: edge.Value)
                )
            )
            .Select(edge => new EdgeDto
            {
                FromId = edge.From.Id,
                ToId = edge.To.Id,
                Weight = edge.Weight
            })
            .ToList();

        // Create graph dto
        var graphDto = new GraphDto { Nodes = nodes, Edges = edges };

        // Save graphDto to file
        var serializedBytes = MemoryPackSerializer.Serialize(graphDto);
        File.WriteAllBytes(filePath, serializedBytes);
    }

    public static DirectedAcyclicGraph<WeightedNode> LoadGraph(
        this DirectedAcyclicGraph<WeightedNode> graph,
        string filePath
    )
    {
        // Check if path is a binary file
        if (Path.GetExtension(filePath) != ".bin")
            throw new ArgumentException("File path must be a binary file");
        // Check if file exists
        if (!File.Exists(filePath))
            throw new ArgumentException("File does not exist");

        // Load graphDto from file
        var serializedBytes = File.ReadAllBytes(filePath);
        var graphDto = MemoryPackSerializer.Deserialize<GraphDto>(serializedBytes);
        if (graphDto is null)
            throw new Exception("Failed to deserialize graph DTO");

        // Create nodes
        var nodes = graphDto
            .Nodes.Select(nodeDto =>
            {
                var node = nodeDto.StateIndex.HasValue
                    ? new IndexedNode(
                        nodeDto.Label,
                        nodeDto.Type,
                        [],
                        [],
                        nodeDto.Bias,
                        nodeDto.StateIndex.Value,
                        nodeDto.Id
                    )
                    : new WeightedNode(
                        nodeDto.Label,
                        nodeDto.Type,
                        [],
                        [],
                        nodeDto.Bias,
                        nodeDto.Id
                    );
                return (nodeDto.Id, node);
            })
            .ToList();

        // Clear graph
        graph.Clear();

        // Add nodes to graph
        foreach (var (_, node) in nodes)
            graph.AddNode(node, []);

        // Add edges
        foreach (var edge in graphDto.Edges)
        {
            var fromNode = nodes.First(node => node.Id == edge.FromId).node;
            var toNode = nodes.First(node => node.Id == edge.ToId).node;
            graph.AddEdge(fromNode, toNode);
            toNode.SetEdgeWeight(fromNode, edge.Weight);
        }

        return graph;
    }
}
