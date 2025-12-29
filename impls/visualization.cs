using UnityEngine;

namespace speedrunningutils.impls;

static class BoundVisualization {
	public static void VisualizeBound(Bounds bounds, LineRenderer renderer) {
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
    Vector3[] corners =
		[
			new Vector3(min.x, min.y, min.z),
			new Vector3(max.x, min.y, min.z),
			new Vector3(max.x, min.y, max.z),
			new Vector3(min.x, min.y, max.z),
			new Vector3(min.x, max.y, min.z),
			new Vector3(max.x, max.y, min.z),
			new Vector3(max.x, max.y, max.z),
			new Vector3(min.x, max.y, max.z),
		];
		int[] indices =
		[
			0, 1, 1, 2, 2, 3, 3, 0,
			4, 5, 5, 6, 6, 7, 7, 4,
			0, 4, 1, 5, 2, 6, 3, 7 
		];
		renderer.positionCount = indices.Length;
		for (int i = 0; i < indices.Length; i++)
		{
			renderer.SetPosition(i, corners[indices[i]]);
		}
	}
}