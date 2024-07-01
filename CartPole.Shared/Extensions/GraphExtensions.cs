using CartPoleShared.DTOs;
using CartPoleShared.Functions;
using Graphs.Models;
using MemoryPack;

namespace CartPoleShared.Extensions;

public static class GraphExtensions
{
    public static void SaveGraph(this DirectedAcyclicGraph graph, string filePath)
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
        var edges1 = sortedNodes
            .SelectMany(node =>
                node.Parents.Select(edge => (From: edge.Key, To: node, Weight: edge.Value))
            )
            .ToList();

        var edges = edges1
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
        // Create file path directories if it does not exist already
        Directory.CreateDirectory(
            Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException()
        );
        File.WriteAllBytes(filePath, serializedBytes);
    }

    public static DirectedAcyclicGraph LoadGraph(this DirectedAcyclicGraph graph, string filePath)
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
                        nodeDto.Id,
                        nodeDto.Label,
                        nodeDto.Type,
                        [],
                        [],
                        nodeDto.Bias,
                        ActivationFunctions.ResolveActivationFunction(nodeDto.Type),
                        nodeDto.StateIndex.Value
                    )
                    : new WeightedNode(
                        nodeDto.Id,
                        nodeDto.Label,
                        nodeDto.Type,
                        [],
                        [],
                        nodeDto.Bias,
                        ActivationFunctions.ResolveActivationFunction(nodeDto.Type)
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
            graph.AddEdge(fromNode, toNode, edge.Weight);
        }

        return graph;
    }
}
