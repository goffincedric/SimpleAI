using System;
using System.Collections.Generic;
using System.Linq;
using CartPoleWinForms.Controls;
using DirectedAcyclicGraph.Models;
using SkiaSharp;

namespace CartPoleWinForms.Helpers;

public static class CanvasHelper
{
    public static void DrawFpsCounter(double fps, SKCanvas canvas)
    {
        // Background rectangle
        var backgroundPaint = new SKPaint
        {
            Color = new SKColor(128, 128, 128, 128), // semi-transparent gray
        };
        var fpsRect = new SKRect(0, 0, 100, 30); // background rectangle dimensions
        fpsRect.Offset(canvas.LocalClipBounds.Width - fpsRect.Width, 0); // position at top right
        canvas.DrawRect(fpsRect, backgroundPaint); // draw the background

        // FPS counter text
        var fpsPaint = new SKPaint
        {
            Color = SKColors.Green,
            IsAntialias = true,
            TextSize = 20,
        };
        canvas.DrawText($"FPS: {fps}", fpsRect.Left + 5, fpsRect.Top + fpsPaint.TextSize, fpsPaint); // draw the FPS counter
    }

    public static class CartPole
    {
        private const float CartPoleYPos = 100; // y-axis position of the cart-pole relative to center of control

        public static void DrawCartTrack(RenderControl control, SKCanvas canvas)
        {
            // Move to center of control
            canvas.Save();
            canvas.Translate((float)control.Width / 2, (float)control.Height / 2);

            // Draw a horizontal line as the track of the CartPole
            var linePaint = new SKPaint
            {
                Color = control.CartPole.Track.Color,
                IsAntialias = true
            };
            canvas.DrawRect(
                ConvertMetersToCoordinate((float)control.CartPole.Track.Length / 2 / -2),
                CartPoleYPos - 1,
                ConvertMetersToCoordinate((float)control.CartPole.Track.Length / 2),
                2,
                linePaint
            );

            // Save the canvas state and revert translation
            canvas.Restore();
        }

        public static void DrawCartPole(RenderControl control, SKCanvas canvas)
        {
            // Move to center of control
            canvas.Save();
            canvas.Translate((float)control.Width / 2, (float)control.Height / 2);

            // Draw the cart
            var cartPaint = new SKPaint { Color = control.CartPole.Cart.Color, IsAntialias = true };
            var cartRect = GetRectangle(
                control.CartPole.X,
                control.CartPole.Cart.Width,
                control.CartPole.Cart.Height
            ); // rectangle dimensions in meters
            canvas.DrawRect(cartRect, cartPaint); // draw the cart

            // Draw the joint
            var jointPaint = new SKPaint
            {
                Color = control.CartPole.Joint.Color,
                IsAntialias = true
            };
            var jointPoint = new SKPoint(
                ConvertMetersToCoordinate((float)control.CartPole.X),
                CartPoleYPos
            ); // joint point
            canvas.DrawCircle(
                jointPoint,
                ConvertMetersToCoordinate(control.CartPole.Joint.Radius / 2),
                jointPaint
            ); // draw the joint

            // Draw the pole and rotate it
            var polePaint = new SKPaint { Color = control.CartPole.Pole.Color, IsAntialias = true };
            var poleRect = GetRectangle(
                control.CartPole.X,
                control.CartPole.Pole.Width,
                control.CartPole.Pole.Height,
                true
            ); // rectangle dimensions in meters
            canvas.Save();
            canvas.RotateDegrees(
                (float)(control.CartPole.Pole.AngleRad * (180 / Math.PI)),
                ConvertMetersToCoordinate((float)control.CartPole.X),
                CartPoleYPos
            ); // rotate around the center of the rectangle
            canvas.DrawRect(poleRect, polePaint); // draw the pole
            canvas.Restore();

            // Save the canvas state and revert translation
            canvas.Restore();
        }

        /// <summary>
        /// Gets the bounding box of the cart in pixels. Pixels are converted from x, width, and height in meters.
        /// </summary>
        /// <param name="x">X-axis in meters</param>
        /// <param name="width">Width in meters</param>
        /// <param name="height">Height in meters</param>
        /// <param name="onlyShowTopHalf">Flag indicating whether to only show top half of the rectangle</param>
        /// <returns></returns>
        private static SKRect GetRectangle(
            double x,
            float width,
            float height,
            bool onlyShowTopHalf = false
        )
        {
            var left = ConvertMetersToCoordinate((float)x - width / 2);
            var top = CartPoleYPos - ConvertMetersToCoordinate(height / 2);
            var right = ConvertMetersToCoordinate((float)x + width / 2);
            float bottom;
            if (onlyShowTopHalf)
                bottom = CartPoleYPos;
            else
                bottom = CartPoleYPos + ConvertMetersToCoordinate(height / 2);

            return new SKRect(left, top, right, bottom);
        }

        /// <summary>
        /// Method that converts the meters to coordinates (x or y), to make 1 meter equal 200 pixels on screen.
        /// </summary>
        /// <returns></returns>
        private static float ConvertMetersToCoordinate(float meters) => meters * 200;
    }

    public static class AI
    {
        private const int BorderPadding = 10;

        private const int LayerSpacing = 30;
        private const int NodeRadius = 7;
        private const int NodeSpacing = 10;
        private const int EdgeWidth = 3;

        public static void DrawGraph(RenderControl control, SKCanvas canvas)
        {
            // Calculate height and width of background box
            var largestLayerNodeCount = control.SortedLayers.Max(nodes => nodes.Count);
            var backgroundWidth =
                BorderPadding * 2 // padding on both sides
                + control.SortedLayers.Count * NodeRadius * 2 // Width of all nodes
                + (control.SortedLayers.Count - 1) * LayerSpacing; // Spacing between all layers
            var backgroundHeight =
                BorderPadding * 2 // padding on both sides
                + largestLayerNodeCount * NodeRadius * 2 // Height of the layer with the most nodes
                + (largestLayerNodeCount - 1) * NodeSpacing; // Spacing between all nodes in the layer with the most nodes

            // Move to bottom center of control
            canvas.Save();
            canvas.Translate(
                (float)control.Width / 2 - (float)backgroundWidth / 2,
                (float)control.Height - backgroundHeight - 50
            );

            // Draw background
            var backgroundPaint = new SKPaint
            {
                Color = new SKColor(240, 240, 240),
                IsAntialias = true
            };
            canvas.DrawRect(0, 0, backgroundWidth, backgroundHeight, backgroundPaint);

            // Draw the edges
            DrawEdges(control.SortedLayers, canvas);

            // Draw the nodes over the edges
            DrawNodes(control.SortedLayers, canvas);

            // Save the canvas state and revert translation
            canvas.Restore();
        }

        /// <summary>
        /// Draws the nodes from top to bottom for each layer.
        /// </summary>
        /// <param name="nodes">Sorted layers of nodes</param>
        /// <param name="canvas"></param>
        private static void DrawNodes(List<List<DirectedNode>> nodes, SKCanvas canvas)
        {
            // Draw a circle for each node
            var nodePaint = new SKPaint { Color = new SKColor(40, 40, 40), IsAntialias = true };
            foreach (var layer in nodes)
            {
                var layerIndex = nodes.IndexOf(layer);

                foreach (var node in layer)
                {
                    // Calculate position of the node
                    var nodeIndex = layer.IndexOf(node);

                    // Draw the node
                    var nodePoint = GetNodePoint(layerIndex, nodeIndex);
                    canvas.DrawCircle(nodePoint, NodeRadius, nodePaint);
                }
            }
        }

        private static void DrawEdges(List<List<DirectedNode>> nodes, SKCanvas canvas)
        {
            var edgePaint = new SKPaint { Color = new SKColor(80, 80, 80), IsAntialias = true };
        }

        private static SKPoint GetNodePoint(int layerIndex, int nodeIndex)
        {
            var nodeCenterX =
                BorderPadding // Padding
                + layerIndex * NodeRadius * 2 // Space for the nodes in the layers before current layer
                + layerIndex * LayerSpacing // Spacing between layers
                + NodeRadius; // Space for the node in the current layer
            var nodeCenterY =
                BorderPadding // Padding
                + nodeIndex * NodeRadius * 2 // Space for the nodes before the current node
                + nodeIndex * NodeSpacing // Spacing between nodes
                + NodeRadius; // Space for the current node
            return new SKPoint(nodeCenterX, nodeCenterY);
        }
    }
}
