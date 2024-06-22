using CartPoleShared.Graph;
using CartPoleShared.Models.Graph;
using DirectedAcyclicGraph.Extensions;
using DirectedAcyclicGraph.Models;
using SimpleAI.Models;

namespace CartPoleConsole.Training;

public static class TrainingHelpers
{
    public static class Selection
    {
        public static List<DirectedAcyclicGraph<WeightedNode>> SelectTopPerformers(
            IDictionary<DirectedAcyclicGraph<WeightedNode>, double> sortedGraphsByFitness,
            double topPerformersPercentage
        ) =>
            sortedGraphsByFitness
                .OrderByDescending(pair => pair.Value)
                .Select(pair => (pair.Key, pair.Value))
                .Take((int)(sortedGraphsByFitness.Count * topPerformersPercentage))
                .Select(graph => graph.Item1)
                .ToList();

        public static List<DirectedAcyclicGraph<WeightedNode>> SelectRandomCollection(
            List<DirectedAcyclicGraph<WeightedNode>> graphs,
            int amountToSelect
        )
        {
            var random = new Random();
            return Enumerable
                .Range(0, amountToSelect)
                .Select(_ => graphs[random.Next(graphs.Count)])
                .ToList();
        }
    }

    public static class MutationHelpers
    {
        public static List<DirectedAcyclicGraph<WeightedNode>> MutateGraphs(
            List<DirectedAcyclicGraph<WeightedNode>> graphs
        ) => graphs.Select(MutateGraph).ToList();

        private static DirectedAcyclicGraph<WeightedNode> MutateGraph(
            DirectedAcyclicGraph<WeightedNode> graph
        )
        {
            // Select random mutation
            var mutationChance = new Random().NextDouble();

            // TODO: Changes to consider: Add mutation that doesn't mutate???
            // 5% chance to add edge, 5% chance to remove edge, 2.5% chance to split edge, 2.5% chance to add node, 5% chance to remove node
            if (mutationChance is >= 0 and < 0.05)
                return AddEdgeMutation(graph);
            if (mutationChance is >= 0.05 and < 0.1)
                return RemoveEdgeMutation(graph);
            if (mutationChance is >= 0.1 and < 0.125)
                return SplitEdgeMutation(graph);
            if (mutationChance is >= 0.125 and < 0.15)
                return AddNodeMutation(graph);
            if (mutationChance is >= 0.15 and < 0.2)
                return RemoveNodeMutation(graph);

            // 40% chance to change bias, 40% chance to change weight
            if (mutationChance is >= 0.2 and < 0.6)
                return ChangeBiasMutation(graph);
            return ChangeWeightMutation(graph);
        }

        /// <summary>
        /// Adds a random edge between two nodes in the graph.
        /// </summary>
        private static DirectedAcyclicGraph<WeightedNode> AddEdgeMutation(
            DirectedAcyclicGraph<WeightedNode> graph
        )
        {
            var isSuccess = graph.AddRandomEdge();
            if (!isSuccess)
                return MutateGraph(graph);
            // Add random weight to edge

            return graph;
        }

        /// <summary>
        /// Removes a random edge from the graph.
        /// </summary>
        private static DirectedAcyclicGraph<WeightedNode> RemoveEdgeMutation(
            DirectedAcyclicGraph<WeightedNode> graph
        )
        {
            var isSuccess = graph.RemoveRandomEdge();
            if (!isSuccess)
                return MutateGraph(graph);
            return graph;
        }

        private static DirectedAcyclicGraph<WeightedNode> SplitEdgeMutation(
            DirectedAcyclicGraph<WeightedNode> graph
        )
        {
            var isSuccess = graph.SplitRandomEdge(
                () => GraphFunctions.CreateHiddenNode("Split edge")
            );
            if (!isSuccess)
                return MutateGraph(graph);
            return graph;
        }

        private static DirectedAcyclicGraph<WeightedNode> AddNodeMutation(
            DirectedAcyclicGraph<WeightedNode> graph
        )
        {
            // Changes to perform: Implement function to add a node by connecting two random nodes
            return graph;
        }

        private static DirectedAcyclicGraph<WeightedNode> RemoveNodeMutation(
            DirectedAcyclicGraph<WeightedNode> graph
        )
        {
            // Changes to perform: Implement in graph
            return graph;
        }

        private static DirectedAcyclicGraph<WeightedNode> ChangeBiasMutation(
            DirectedAcyclicGraph<WeightedNode> graph
        )
        {
            // Select random node from unsorted nodes
            var randomNode = graph.UnsortedNodes.ToList().GetRandomEntry(null);
            GraphFunctions.UpdateNodeBias(randomNode);
            return graph;
        }

        private static DirectedAcyclicGraph<WeightedNode> ChangeWeightMutation(
            DirectedAcyclicGraph<WeightedNode> graph
        )
        {
            if (graph.NodesWithIncomingEdges.Count == 0)
                return MutateGraph(graph);

            // Select random node and random incoming node
            var random = new Random();
            var nodePair = graph.NodesWithIncomingEdges.ToList().GetRandomEntry(random);
            var randomIncomingNode = nodePair.Value.ToList().GetRandomEntry(random);

            // Mutate the edge weight
            GraphFunctions.UpdateIncomingNodeWeight(nodePair.Key, randomIncomingNode);

            return graph;
        }
    }
}
