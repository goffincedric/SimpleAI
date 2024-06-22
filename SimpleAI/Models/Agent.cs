using CartPoleShared.Graph;
using CartPoleShared.Models.Graph;
using DirectedAcyclicGraph.Models;

namespace SimpleAI.Models;

public class Agent(DirectedAcyclicGraph<WeightedNode> graph)
{
    public DirectedAcyclicGraph<WeightedNode> Graph { get; } = graph;
    public double Fitness { get; private set; }

    public double[] CalculateOutputValue(double[] state)
    {
        // Get sorted nodes
        var sortedNodes = GetSortedNodes();

        // Calculate output values for all nodes
        Dictionary<WeightedNode, double> outputValues = new();
        List<double> endNodeOutputValues = [];
        // Calculate output values
        foreach (var node in sortedNodes)
        {
            // Set inputValue
            var inputValue = 0d;
            if (node.Type == NodeType.Start && node is IndexedNode inputNode)
                inputValue = state[inputNode.Index];

            // Add output values multiplied with the edge weight to the output value
            if (node.Parents.Count > 0)
                foreach (var parentNode in node.Parents)
                {
                    // Get edge weight and parent output value
                    var weight = node.GetEdgeWeight(parentNode);
                    var parentOutputValue = outputValues.GetValueOrDefault(
                        (WeightedNode)parentNode,
                        0
                    );

                    // Add weighted value
                    inputValue += parentOutputValue * weight;
                }

            // Calculate output value and add to outputValues
            var outputValue = node.CalculateOutputValue(inputValue);
            outputValues[node] = outputValue;
            if (node.Type == NodeType.End)
            {
                var indexedNode = (IndexedNode)node;
                endNodeOutputValues.Insert(indexedNode.Index, outputValue);
            }
        }

        return endNodeOutputValues.ToArray();
    }

    public void AddFitness(double score) => Fitness += score;

    private List<WeightedNode> GetSortedNodes()
    {
        // Sort nodes topologically
        var (sortedLayers, _) = Graph.GetTopologicallySortedNodes();
        return sortedLayers.SelectMany(x => x).ToList();
    }
}
