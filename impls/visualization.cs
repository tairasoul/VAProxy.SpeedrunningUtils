using UnityEngine;

namespace speedrunningutils.impls;

static class BoundVisualization {
	public static void VisualizeBound(Bounds bounds) {
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
		Debug.DrawLine(corners[0], corners[1]);
		Debug.DrawLine(corners[1], corners[2]);
		Debug.DrawLine(corners[2], corners[3]);
		Debug.DrawLine(corners[3], corners[0]);
		Debug.DrawLine(corners[4], corners[5]);
		Debug.DrawLine(corners[5], corners[6]);
		Debug.DrawLine(corners[6], corners[7]);
		Debug.DrawLine(corners[7], corners[4]);
		Debug.DrawLine(corners[0], corners[4]);
		Debug.DrawLine(corners[1], corners[5]);
		Debug.DrawLine(corners[2], corners[6]);
		Debug.DrawLine(corners[3], corners[7]);
	}
}