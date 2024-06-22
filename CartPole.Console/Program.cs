using System.Collections.Concurrent;
using CartPoleConsole.Training;
using CartPolePhysics.SinglePole.DoublePrecision;
using CartPoleShared.Constants;
using CartPoleShared.Extensions;
using CartPoleShared.Functions;
using CartPoleShared.Graph;
using CartPoleShared.Helpers;
using CartPoleShared.Models.Environment;
using CartPoleShared.Models.Graph;
using DirectedAcyclicGraph.Models;
using SimpleAI.Constants;
using SimpleAI.Models;

// Define episode function
var runEpisode = (
    DirectedAcyclicGraph<WeightedNode> graph,
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

        // Calculate score of new physics state
        var fitnessScore = FitnessFunctions.PoleTopAboveThreshold(
            cartPole.Pole.Height,
            cartPole.Pole.AngleRadians,
            RewardConstants.MaxPoleHeightReward
        );
        agent.AddFitness(fitnessScore);
    }

    // Return fitness score of graph for provided starting state
    return agent.Fitness;
};

// Create 10 new starting graphs
var grapsInGeneration = new List<DirectedAcyclicGraph<WeightedNode>>();
for (var i = 0; i < 10; i++)
    grapsInGeneration.Add(GraphFunctions.GenerateStartingGraph());

// Run generations until ????
var generationCount = 0;

// TODO: Define episode end (ctrl-c, episode limit, etc.)
while (generationCount <= 10)
{
    generationCount++;
    Console.WriteLine("=====================================");
    Console.WriteLine("Generation: " + generationCount);
    // Create random pole angle in radians
    var randomPoleAngleForEpisode = AngleHelpers.DegreesToRadians(
        Math.Round(new Random().NextDouble() * 360)
    );

    // Run episodes for agents in parallel
    var threads = new List<Thread>();
    ConcurrentDictionary<DirectedAcyclicGraph<WeightedNode>, double> agentGraphFitnessPairs = new();
    foreach (var graph in grapsInGeneration)
    {
        // Create cartPole and physics
        var cartPole = EnvironmentHelpers.CreateNewCartPole(
            // Changes to perform: Add random track coordinate
            randomPoleAngleForEpisode
        );
        var physics = EnvironmentHelpers.CreatePhysics(cartPole);

        // Create a thread for the agent
        var agentThread = new Thread(() =>
        {
            // Run episode and save fitness
            var fitness = runEpisode(graph, cartPole, physics);
            agentGraphFitnessPairs.TryAdd(graph, fitness);
        });
        threads.Add(agentThread);
        agentThread.Start();
    }

    // Wait for threads to finish
    foreach (var thread in threads)
        thread.Join();

    // Create next generation of agents
    var newGeneration = new List<DirectedAcyclicGraph<WeightedNode>>();

    // Select top performers and add to next generation
    var topPerformers = TrainingHelpers.Selection.SelectTopPerformers(
        agentGraphFitnessPairs,
        TrainingConstants.TopPerformersPercentage
    );
    newGeneration.AddRange(topPerformers);

    // Print scores of top performers
    topPerformers.ForEach(graph => Console.WriteLine($"Score: {agentGraphFitnessPairs[graph]}"));

    // Select random agents for remaining amount and mutate them
    var randomGraphsToSelect = grapsInGeneration.Count - topPerformers.Count;
    var randomGraphs = TrainingHelpers.Selection.SelectRandomCollection(
        agentGraphFitnessPairs.Keys.ToList(),
        randomGraphsToSelect
    );

    // Mutate random agents and add to next generation
    var mutatedGraphs = TrainingHelpers.MutationHelpers.MutateGraphs(randomGraphs);
    newGeneration.AddRange(mutatedGraphs);

    // Set new generation as current generation
    grapsInGeneration = newGeneration;
}

// Save graphs to file next to current working directory
for (var i = 0; i < grapsInGeneration.Count; i++)
    grapsInGeneration[i].SaveGraph($"V1/graph-{i}.bin");





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
