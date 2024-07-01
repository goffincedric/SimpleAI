using System.Collections.Concurrent;
using CartPoleConsole.Training;
using CartPolePhysics.SinglePole.DoublePrecision;
using CartPoleShared.Constants;
using CartPoleShared.Extensions;
using CartPoleShared.Functions;
using CartPoleShared.Helpers;
using CartPoleShared.Models.Environment;
using Graphs.Models;
using SimpleAI.Constants;
using SimpleAI.Models;

// Define episode function
var runEpisode = (
    DirectedAcyclicGraph graph,
    CartPole cartPole,
    CartSinglePolePhysicsRK4 physics
) =>
{
    // Run episode
    var agent = new Agent(graph);
    for (var i = 0; i < EpisodeConstant.TimeStepsPerEpisode; i++)
    {
        // Calculate output value
        var outputValues = agent.CalculateOutputValue(physics.State);

        // Advance physics with output value as force and set as new state
        physics.Update(outputValues[0]);
        cartPole.SetState(physics.State);

        // Calculate score of new physics state and add to agent fitness
        var score = FitnessFunctions.CalculateFitness(
            [
                // () =>
                //     FitnessFunctions.PoleHeight(
                //         cartPole.Pole.AngleRadians,
                //         RewardConstants.MaxPoleAngledUpReward
                //     ),
                // () =>
                //     FitnessFunctions.PoleTopAboveThreshold(
                //         cartPole.Pole.AngleRadians,
                //         RewardConstants.MaxPoleHeightAboveThresholdReward
                //     ),
                () =>
                    FitnessFunctions.PoleTopNotMovingAboveThreshold(
                        cartPole.Pole.AngleRadians,
                        cartPole.Pole.Velocity,
                        RewardConstants.MaxPoleHeightAboveThresholdReward
                    ),
                () =>
                    FitnessFunctions.PunishBeingOffTrack(
                        cartPole.Cart.X,
                        cartPole.Track.Length / 2,
                        RewardConstants.MaxTrackLimitExceedPunishment
                    ),
                // () =>
                //     FitnessFunctions.RewardBeingInCenter(
                //         cartPole.Cart.X,
                //         RewardConstants.MaxCenterPositionReward
                //     ),
                // () =>
                //     FitnessFunctions.PunishHighPoleVelocity(
                //         cartPole.Pole.Velocity,
                //         RewardConstants.MaxPoleVelocityPunishment
                //     )
            ]
        );
        agent.AddFitness(score);
    }

    // Return fitness score of graph for provided starting state
    return agent.Fitness;
};

var graphsInGeneration = new List<DirectedAcyclicGraph>();

// Load graphs from files if present
if (Directory.Exists("V1"))
{
    for (var i = 0; i < TrainingConstants.AgentsPerGeneration; i++)
    {
        var graph = new DirectedAcyclicGraph().LoadGraph($"V1/graph-{i}.bin");
        graphsInGeneration.Add(graph);
    }
}
else
{
    // Create 100 new starting graphs
    for (var i = 0; i < TrainingConstants.AgentsPerGeneration; i++)
        graphsInGeneration.Add(EnvironmentHelpers.GenerateStartingGraph());
}

// Stop training when pressing ctrl-c
var trainingStopRequested = false;
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    trainingStopRequested = true;
};

// Start the training loop
var generationCount = 0;
while (!trainingStopRequested)
{
    generationCount++;
    Console.WriteLine("=====================================");
    Console.WriteLine("Generation: " + generationCount);

    // Run episodes for agents in parallel
    ConcurrentDictionary<DirectedAcyclicGraph, double> agentGraphFitnessPairs = new();
    Parallel.ForEach(
        graphsInGeneration,
        graph =>
        {
            // Create cartPole and physics
            var cartPole = EnvironmentHelpers.CreateNewCartPole(
                // Changes to perform: Add random track coordinate
                EnvironmentConstants.CartPoleDimensions.CartStartingPosition,
                // randomPoleAngleForEpisode0,
                AngleConstants.Radians.Down
            );
            var physics = EnvironmentHelpers.CreatePhysics(cartPole);

            // Run episode and save fitness
            var fitness = runEpisode(graph, cartPole, physics);
            agentGraphFitnessPairs.TryAdd(graph, fitness);
        }
    );

    // Create next generation of agents
    var newGeneration = new List<DirectedAcyclicGraph>();

    // Select top performers, clone and add to next generation
    var topPerformers = TrainingHelpers.Selection.SelectTopPerformers(
        agentGraphFitnessPairs,
        TrainingConstants.TopPerformersPercentage
    );
    newGeneration.AddRange(topPerformers.Select(graph => graph.Clone()).ToList());
    // Print min and max incoming weight of output node for graphs in current generation
    var minMaxWeights = new Dictionary<int, (double, double)>();
    topPerformers.ForEach(graph =>
    {
        var inputNodes = graph
            .UnsortedNodes.Where(node => node.Type == NodeType.Input)
            .Select(node => (IndexedNode)node)
            .ToList();
        var outputNode = graph.UnsortedNodes.First(node => node.Type == NodeType.Output);
        inputNodes.ForEach(inputNode =>
        {
            if (!outputNode.Parents.ContainsKey(inputNode))
                return;
            if (!minMaxWeights.ContainsKey(inputNode.Index))
                minMaxWeights.Add(inputNode.Index, (double.MaxValue, double.MinValue));
            var weights = minMaxWeights[inputNode.Index];
            minMaxWeights[inputNode.Index] = (
                Math.Min(weights.Item1, outputNode.Parents[inputNode]),
                Math.Max(weights.Item2, outputNode.Parents[inputNode])
            );
        });
    });
    minMaxWeights
        .ToList()
        .ForEach(pair =>
        {
            Console.WriteLine(
                $"Input node index: {pair.Key}: Min weight: {pair.Value.Item1}, Max weight: {pair.Value.Item2}"
            );
        });

    // Print scores of top performer
    var bestGraph = topPerformers.First();
    Console.WriteLine($"Best score: {agentGraphFitnessPairs[bestGraph]}");
    Console.WriteLine(
        $"Hidden node count: {bestGraph.UnsortedNodes.Count(node => node.Type == NodeType.Hidden)}"
    );
    Console.WriteLine($"Edge count: {bestGraph.GetEdgeCount()}");

    // Select random graphs for remaining amount, clone and mutate them
    var mutations = new List<(int, int)>
    {
        ((int)(TrainingConstants.DoubleMutationPercentage * graphsInGeneration.Count), 2),
        ((int)(TrainingConstants.QuadrupleMutationPercentage * graphsInGeneration.Count), 4)
    };
    mutations.Add(
        (
            graphsInGeneration.Count
                - topPerformers.Count
                - mutations.Sum(mutation => mutation.Item1),
            3
        )
    ); // Remaining amount get 3 mutations

    foreach (var (amountToMutate, mutationCount) in mutations)
    {
        var randomGraphs = TrainingHelpers
            .Selection.SelectRandomCollection(agentGraphFitnessPairs.Keys.ToList(), amountToMutate)
            .Select(graph => graph.Clone())
            .ToList();

        // Mutate random agents specified amount and add to next generation
        for (var i = 0; i < mutationCount; i++)
            randomGraphs = TrainingHelpers
                .MutationHelpers.MutateGraphs(randomGraphs)
                .Select(graph => graph.Clone())
                .ToList();
        newGeneration.AddRange(randomGraphs);
    }

    // Set new generation as current generation
    graphsInGeneration = newGeneration;
}

// Save graphs to file next to current working directory
for (var i = 0; i < graphsInGeneration.Count; i++)
    graphsInGeneration[i].SaveGraph($"V1/graph-{i}.bin");





// Create agent and loop
// Changes to perform: Start with all input nodes and output nodes not connected
// Initialize biases and weights randomly from a normal distribution:
/**
 * Random rand = new Random();
 * node.Bias = mean + stdDev * rand.NextDouble(); // Normal distribution bias
 * // Bias: mean = 0.1, stdDev = 0.1
 * // Weight: mean = 0.01, stdDev = 0.1
 */
// Train each agent for 100 secs, train 100 agents
