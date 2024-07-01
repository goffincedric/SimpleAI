using System;
using System.Collections.Generic;
using System.Linq;
using CartPoleShared.Models;
using CartPoleShared.Models.Environment;
using CartPoleWinForms.Controls;
using Graphs.Models;
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

    public static class CartPoleDrawer
    {
        private const float CartPoleYPos = 100; // y-axis position of the cart-pole relative to center of control
        private static SKColor CartColor { get; set; } = new(81, 97, 114);
        private static SKColor JointColor { get; set; } = new(40, 40, 40);
        private static SKColor PoleColor { get; set; } = new(178, 149, 91);
        private static SKColor TrackColor { get; set; } = SKColors.Black;

        public static void DrawCartTrack(RenderControl control, Track track, SKCanvas canvas)
        {
            // Move to center of control
            canvas.Save();
            canvas.Translate((float)control.Width / 2, (float)control.Height / 2);

            // Draw a horizontal line as the track of the CartPole
            var linePaint = new SKPaint { Color = TrackColor, IsAntialias = true };
            canvas.DrawRect(
                ConvertMetersToPixels(
                    (float)control.CartPole.Track.Length / 2 * -1,
                    track,
                    control
                ),
                CartPoleYPos - 1,
                ConvertMetersToPixels((float)control.CartPole.Track.Length, track, control),
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
            DrawCart(control.CartPole, control, canvas);

            // Draw the joint
            DrawJoint(control.CartPole, control, canvas);

            // Draw the pole
            DrawPole(control.CartPole, control, canvas);

            // Save the canvas state and revert translation
            canvas.Restore();
        }

        private static void DrawCart(CartPole cartPole, RenderControl control, SKCanvas canvas)
        {
            var cartPaint = new SKPaint { Color = CartColor, IsAntialias = true };
            var cartRect = GetRectangle(
                cartPole.Cart.X,
                cartPole.Cart.Width,
                cartPole.Cart.Height,
                cartPole.Track,
                control
            ); // rectangle dimensions in meters
            canvas.DrawRect(cartRect, cartPaint); // draw the cart
        }

        private static void DrawJoint(CartPole cartPole, RenderControl control, SKCanvas canvas)
        {
            var jointPaint = new SKPaint { Color = JointColor, IsAntialias = true };
            var jointPoint = new SKPoint(
                ConvertMetersToPixels((float)cartPole.Cart.X, cartPole.Track, control),
                CartPoleYPos
            ); // joint point
            canvas.DrawCircle(
                jointPoint,
                ConvertMetersToPixels(cartPole.Pole.Width, cartPole.Track, control),
                jointPaint
            ); // draw the joint
        }

        private static void DrawPole(CartPole cartPole, RenderControl control, SKCanvas canvas)
        {
            var polePaint = new SKPaint { Color = PoleColor, IsAntialias = true };
            var poleRect = GetRectangle(
                cartPole.Cart.X,
                cartPole.Pole.Width,
                cartPole.Pole.Height,
                cartPole.Track,
                control,
                true
            ); // rectangle dimensions in meters
            canvas.Save();
            canvas.RotateDegrees(
                (float)(cartPole.Pole.AngleRadians * (180 / Math.PI)),
                ConvertMetersToPixels((float)cartPole.Cart.X, cartPole.Track, control),
                CartPoleYPos
            ); // rotate around the center of the rectangle
            canvas.DrawRect(poleRect, polePaint); // draw the pole
            // Draw a circle on top of the pole in the same color as the joint
            var ballPoint = new SKPoint(
                ConvertMetersToPixels((float)cartPole.Cart.X, cartPole.Track, control),
                CartPoleYPos
                    - ConvertMetersToPixels(cartPole.Pole.Height / 2, cartPole.Track, control)
            ); // ball point
            canvas.DrawCircle(
                ballPoint,
                ConvertMetersToPixels(cartPole.Pole.Width * 2, cartPole.Track, control),
                new SKPaint { Color = new SKColor(171, 144, 199), IsAntialias = true }
            ); // draw the ball
            canvas.Restore();
        }

        /// <summary>
        /// Gets the bounding box of the cart in pixels. Pixels are converted from x, width, and height in meters.
        /// </summary>
        /// <param name="x">X-axis in meters</param>
        /// <param name="width">Width in meters</param>
        /// <param name="height">Height in meters</param>
        /// <param name="track"></param>
        /// <param name="control"></param>
        /// <param name="onlyShowTopHalf">Flag indicating whether to only show top half of the rectangle</param>
        /// <returns></returns>
        private static SKRect GetRectangle(
            double x,
            float width,
            float height,
            Track track,
            RenderControl control,
            bool onlyShowTopHalf = false
        )
        {
            var left = ConvertMetersToPixels((float)x - width / 2, track, control);
            var top = CartPoleYPos - ConvertMetersToPixels(height / 2, track, control);
            var right = ConvertMetersToPixels((float)x + width / 2, track, control);
            float bottom;
            if (onlyShowTopHalf)
                bottom = CartPoleYPos;
            else
                bottom = CartPoleYPos + ConvertMetersToPixels(height / 2, track, control);

            return new SKRect(left, top, right, bottom);
        }

        /// <summary>
        /// Method that converts the meters to coordinates (x or y). Draws the entire track, with 50px of paddling left and right of the track
        /// </summary>
        /// <returns></returns>
        private static float ConvertMetersToPixels(float meters, Track track, RenderControl control)
        {
            var trackWidth = control.Width - 100; // 50px padding on both sides
            var pixelsPerMeter = trackWidth / (float)track.Length;
            return meters * pixelsPerMeter;
        }
    }

    public static class AIDrawer
    {
        private const int BorderPadding = 10;

        private const int LayerSpacing = 30;
        private const int NodeRadius = 7;
        private const int NodeSpacing = 10;
        private const int EdgeWidth = 2;

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
            DrawEdges(control.SortedLayers, backgroundHeight, canvas);

            // Draw the nodes over the edges
            DrawNodes(control.SortedLayers, backgroundHeight, canvas);

            // Save the canvas state and revert translation
            canvas.Restore();
        }

        /// <summary>
        /// Draws the nodes from top to bottom for each layer.
        /// </summary>
        /// <param name="nodes">Sorted layers of nodes</param>
        /// <param name="canvas"></param>
        /// <param name="backgroundHeight"></param>
        private static void DrawNodes(
            List<List<WeightedNode>> nodes,
            float backgroundHeight,
            SKCanvas canvas
        )
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
                    var nodePoint = GetNodePoint(
                        layerIndex,
                        nodeIndex,
                        layer.Count,
                        backgroundHeight
                    );
                    canvas.DrawCircle(nodePoint, NodeRadius, nodePaint);
                }
            }
        }

        private static void DrawEdges(
            List<List<WeightedNode>> nodes,
            int backgroundHeight,
            SKCanvas canvas
        )
        {
            var edgePaint = new SKPaint
            {
                Color = new SKColor(80, 80, 80),
                IsAntialias = true,
                StrokeWidth = EdgeWidth
            };
            // Loop over all layers
            for (var layerIndex = 0; layerIndex < nodes.Count; layerIndex++)
            {
                // Loop over nodes in layer
                var layer = nodes[layerIndex];
                for (var nodeIndex = 0; nodeIndex < layer.Count; nodeIndex++)
                {
                    // Loop over children of node
                    var node = layer[nodeIndex];
                    var fromNodePoint = GetNodePoint(
                        layerIndex,
                        nodeIndex,
                        layer.Count,
                        backgroundHeight
                    );
                    foreach (var childNode in node.Children)
                    {
                        // Find the layer the child node belongs to
                        var childLayerIndex = nodes.FindIndex(layer => layer.Contains(childNode));
                        if (childLayerIndex == -1)
                            continue; // If not found, must be detached node
                        var childNodeLayer = nodes[childLayerIndex];
                        var childNodeIndex = childNodeLayer.IndexOf(childNode);

                        // Get the destination point of the edge
                        var toNodePoint = GetNodePoint(
                            childLayerIndex,
                            childNodeIndex,
                            childNodeLayer.Count,
                            backgroundHeight
                        );

                        // Draw a line between the nodes
                        canvas.DrawLine(fromNodePoint, toNodePoint, edgePaint);
                    }
                }
            }
        }

        private static SKPoint GetNodePoint(
            int layerIndex,
            int nodeIndex,
            int totalNodesInLayer,
            float backgroundHeight
        )
        {
            var nodeCenterX =
                BorderPadding // Padding
                + layerIndex * NodeRadius * 2 // Space for the nodes in the layers before current layer
                + layerIndex * LayerSpacing // Spacing between layers
                + NodeRadius; // Space for the node in the current layer

            var nodeCenterY =
                backgroundHeight / 2 // Center of the background
                - (float)(totalNodesInLayer - 1) * NodeSpacing / 2 // Subtract half the total spacing between the nodes
                - totalNodesInLayer * NodeRadius // Subtract half the total height of the nodes (radius is half of height of a node)
                + nodeIndex * NodeRadius * 2 // Space for the nodes before the current node
                + nodeIndex * NodeSpacing // Spacing between nodes
                + NodeRadius; // Space for the current node
            return new SKPoint(nodeCenterX, nodeCenterY);
        }
    }
}
