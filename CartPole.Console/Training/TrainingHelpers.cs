using System.Diagnostics;
using CartPoleShared.Functions;
using Graphs.Extensions;
using Graphs.Functions;
using Graphs.Models;

namespace CartPoleConsole.Training;

public static class TrainingHelpers
{
    public static class Selection
    {
        public static List<DirectedAcyclicGraph> SelectTopPerformers(
            IDictionary<DirectedAcyclicGraph, double> sortedGraphsByFitness,
            double topPerformersPercentage
        ) =>
            sortedGraphsByFitness
                .OrderByDescending(pair => pair.Value)
                .ThenBy(pair => pair.Key.NodeCount)
                .Select(pair => (pair.Key, pair.Value))
                .Take((int)(sortedGraphsByFitness.Count * topPerformersPercentage))
                .Select(graph => graph.Item1)
                .ToList();

        public static List<DirectedAcyclicGraph> SelectRandomCollection(
            List<DirectedAcyclicGraph> graphs,
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
        public static List<DirectedAcyclicGraph> MutateGraphs(List<DirectedAcyclicGraph> graphs) =>
            graphs.Select(MutateGraph).ToList();

        private static DirectedAcyclicGraph MutateGraph(DirectedAcyclicGraph graph)
        {
            // Select random mutation
            var mutationChance = new Random().NextDouble();

            // // Works with default setup
            // if (mutationChance is >= 0 and < 0.04)
            //     return AddEdgeMutation(graph);
            // // if (mutationChance is >= 0.05 and < 0.15)
            // //     return RemoveEdgeMutation(graph);
            // // if (mutationChance is >= 0.15 and < 0.2)
            // //     return SplitEdgeMutation(graph);
            // // if (mutationChance is >= 0.2 and < 0.25)
            // //     return AddNodeMutation(graph);
            // // if (mutationChance is >= 0.25 and < 0.40)
            // //     return RemoveNodeMutation(graph);
            // // if (mutationChance is >= 0.4 and < 0.7)
            // //     return ChangeBiasMutation(graph);
            // // return ChangeWeightMutation(graph);
            //
            // // TODO: Changes to consider: Add mutation that doesn't mutate???
            // if (mutationChance is >= 0.04 and < 0.52)
            //     return ChangeBiasMutation(graph);
            // return ChangeWeightMutation(graph);


            // Works with default setup
            if (mutationChance is >= 0 and < 0.02)
                return AddEdgeMutation(graph);
            if (mutationChance is >= 0.02 and < 0.1)
                return RemoveEdgeMutation(graph);
            if (mutationChance is >= 0.1 and < 0.125)
                return SplitEdgeMutation(graph);
            if (mutationChance is >= 0.125 and < 0.15)
                return AddNodeMutation(graph);
            if (mutationChance is >= 0.15 and < 0.2)
                return RemoveNodeMutation(graph);

            // TODO: Changes to consider: Add mutation that doesn't mutate???
            if (mutationChance is >= 0.2 and < 0.7)
                return ChangeBiasMutation(graph);
            return ChangeWeightMutation(graph);
        }

        /// <summary>
        /// Adds a random edge between two nodes in the graph.
        /// </summary>
        public static DirectedAcyclicGraph AddEdgeMutation(DirectedAcyclicGraph graph)
        {
            var isSuccess = graph.AddRandomEdge();
            if (!isSuccess)
                return MutateGraph(graph);
            return graph;
        }

        /// <summary>
        /// Removes a random edge from the graph.
        /// </summary>
        private static DirectedAcyclicGraph RemoveEdgeMutation(DirectedAcyclicGraph graph)
        {
            var isSuccess = graph.RemoveRandomEdge();
            if (!isSuccess)
                return MutateGraph(graph);
            return graph;
        }

        private static DirectedAcyclicGraph SplitEdgeMutation(DirectedAcyclicGraph graph)
        {
            var newHiddenNode = GraphFunctions.CreateHiddenNode(
                "Split edge",
                ActivationFunctions.ResolveActivationFunction(NodeType.Hidden),
                0
            );
            var isSuccess = graph.SplitRandomEdge(() => newHiddenNode);
            if (!isSuccess)
                return MutateGraph(graph);
            return graph;
        }

        private static DirectedAcyclicGraph AddNodeMutation(DirectedAcyclicGraph graph)
        {
            var newHiddenNode = GraphFunctions.CreateHiddenNode(
                "Random node",
                ActivationFunctions.ResolveActivationFunction(NodeType.Hidden),
                0
            );
            var isSuccess = graph.AddRandomNode(() => newHiddenNode);
            if (!isSuccess)
                return MutateGraph(graph);
            return graph;
        }

        private static DirectedAcyclicGraph RemoveNodeMutation(DirectedAcyclicGraph graph)
        {
            var isSuccess = graph.RemoveRandomNode();
            if (!isSuccess)
                return MutateGraph(graph);
            return graph;
        }

        private static DirectedAcyclicGraph ChangeBiasMutation(DirectedAcyclicGraph graph)
        {
            // Select random node from unsorted nodes
            var randomNode = graph.UnsortedNodes.ToList().GetRandomEntry(null);
            GraphFunctions.UpdateNodeBias(randomNode);
            return graph;
        }

        private static DirectedAcyclicGraph ChangeWeightMutation(DirectedAcyclicGraph graph)
        {
            if (graph.NodesWithOutgoingEdges.Count == 0)
                return MutateGraph(graph);

            // Select random node and random incoming node
            var random = new Random();
            var from = graph.NodesWithOutgoingEdges.GetRandomEntry(random);
            var to = from.Children.GetRandomEntry(random);

            // Mutate the edge weight
            GraphFunctions.UpdateIncomingNodeWeight(from, to);

            return graph;
        }
    }
}
