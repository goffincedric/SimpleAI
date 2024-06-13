# SimpleAI

This is a simple AI project that uses Reinforcement Learning to balance a cart-pole system. An explanation of the cart-pole system by OpenAI can be found [here](https://gymnasium.farama.org/environments/classic_control/cart_pole/).
The equations used are explained [here](https://sharpneat.sourceforge.io/research/cart-pole/cart-pole-equations.html).

This solution contains multiple projects:
- CartPole.Physics (.net8.0)
- CartPole.WinForms (.net8.0-windows)
- DirectedAcyclicGraph (.net8.0)
- SimpleAI (.net8.0)

## Description
### CartPole.Physics
A physics simulation of the cart-pole system. Credits to [colgreen](https://github.com/colgreen/cartpole-physics).

### CartPole.WinForms
A simple Windows Forms application to visualize the cart-pole system and AI.

### DirectedAcyclicGraph
A simple implementation of a directed acyclic graph.

### SimpleAI
An AI to balance the cart-pole system. The AI uses Reinforcement Learning with a Directed Acyclic Graph.


## How to run
Pre-requisites:
- .NET 8.0 SDK
- Windows OS

How to run:
1. Open the solution in DotNet Rider or an IDE of your choice.
2. Set CartPole.WinForms as the startup project.
3. Run the project.