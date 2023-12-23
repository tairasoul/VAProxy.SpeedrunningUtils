
using System.Collections.Generic;
using UnityEngine;

namespace SpeedrunningUtils

{
    internal class VisualiserComponent : MonoBehaviour
    {
        private List<Bounds> boundColliders = new List<Bounds>(); // List to store bounds
        private void Update()
        {
            if (Plugin.VisualisingHitboxes)
            {
                foreach (Bounds bounds in boundColliders)
                {
                    VisualizeBound(bounds);
                }
            }
            else
            {
                Destroy(GetComponent<LineRenderer>());
            }
        }

        internal void AddBounds(Bounds bounds)
        {
            boundColliders.Add(bounds);
        }

        internal void RemoveBounds(Bounds bounds)
        {
            boundColliders.Remove(bounds);
        }

        private void VisualizeBound(Bounds bounds)
        {
            LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (!lineRenderer)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                // Set LineRenderer properties (same as in your code)
            }

            Vector3 center = bounds.center;
            Vector3 size = bounds.size;

            Vector3[] corners =
            {
                center + new Vector3(size.x, size.y, size.z) * 0.5f,
                center + new Vector3(size.x, size.y, -size.z) * 0.5f,
                center + new Vector3(-size.x, size.y, -size.z) * 0.5f,
                center + new Vector3(-size.x, size.y, size.z) * 0.5f,
                center + new Vector3(size.x, -size.y, size.z) * 0.5f,
                center + new Vector3(size.x, -size.y, -size.z) * 0.5f,
                center + new Vector3(-size.x, -size.y, -size.z) * 0.5f,
                center + new Vector3(-size.x, -size.y, size.z) * 0.5f
            };

            // Define indices to draw the edges of the box
            int[] indices =
            {
                0, 1, 1, 2, 2, 3, 3, 0, // Top face
                4, 5, 5, 6, 6, 7, 7, 4, // Bottom face
                0, 4, 1, 5, 2, 6, 3, 7  // Vertical edges
            };

            lineRenderer.positionCount = indices.Length;

            for (int i = 0; i < indices.Length; i++)
            {
                lineRenderer.SetPosition(i, corners[indices[i]]);
            }
        }
    }
}