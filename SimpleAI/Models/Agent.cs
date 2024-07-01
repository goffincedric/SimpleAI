using Graphs.Models;

namespace SimpleAI.Models;

public class Agent(DirectedAcyclicGraph graph)
{
    public DirectedAcyclicGraph Graph { get; } = graph;
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
            if (node.Type == NodeType.Input && node is IndexedNode inputNode)
                inputValue = state[inputNode.Index];

            // Add output values multiplied with the edge weight to the input value
            if (node.Parents.Count > 0)
                foreach (var (parentNode, weight) in node.Parents)
                {
                    // Get parent output value
                    var parentOutputValue = outputValues.GetValueOrDefault(parentNode, 0);

                    // Add weighted value
                    inputValue += parentOutputValue * weight;
                }

            // Calculate output value and add to outputValues
            var outputValue = node.CalculateOutputValue(inputValue);
            outputValues[node] = outputValue;
            if (node.Type == NodeType.Output)
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
