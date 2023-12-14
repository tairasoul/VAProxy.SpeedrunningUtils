
using UnityEngine;

namespace SpeedrunningUtils
{
    internal class VisualiserComponent : MonoBehaviour
    {
        Collider trigger;
        private LineRenderer lineRenderer;
        private Color color = Color.cyan;

        internal void Start()
        {
            trigger = gameObject.GetComponent<Collider>();
            if (!trigger)
            {
                trigger = gameObject.AddComponent<Collider>();
            }
        }

        private void Update()
        {
            DrawTrail();
        }

        private void DrawTrail()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false; // Use local space
            lineRenderer.loop = true; // Close the loop for trail
            lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Change the material
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            BoxCollider collider = trigger as BoxCollider;

            Vector3 center = collider.center;
            Vector3 size = collider.size;

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

        private void OnDestroy()
        {
            if (lineRenderer) Destroy(lineRenderer);
        }
    }
}
