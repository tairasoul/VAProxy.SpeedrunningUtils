using tairasoul.unity.common.events;
using tairasoul.unity.common.speedrunning.dsl;
using tairasoul.unity.common.speedrunning.dsl.eventbus;
using UnityEngine;

namespace speedrunningutils.impls;

record assoc(Bounds bounds, Action action);

class BoundsRegistry : MonoBehaviour, IBoundsRegistry
{
	List<assoc> assocs = [];

	public void BoundCreated(Bounds bounds)
	{
		GameObject s105 = GameObject.FindFirstObjectByType<Inventory>().gameObject;
		bool lastContained = false;
		assocs.Add(new(bounds, () =>
		{
			if (Plugin.cfg.VisualizeHitboxesByDefault.Value)
				BoundVisualization.VisualizeBound(bounds);
			if (bounds.Contains(s105.transform.position)) {
				if (!lastContained)
				{
					lastContained = true;
					EventBus.Send(new DslPlayerEnteredBounds(), new DslBoundEntered(bounds));
				}
			}
			else if (lastContained) {
				lastContained = false;
				EventBus.Send(new DslPlayerLeftBounds(), new DslBoundLeft(bounds));
			}
		}));
	}

	public void Update() {
		foreach (assoc assoc in assocs) {
			assoc.action();
		}
	}

	public void BoundDestroyed(Bounds bounds)
	{
		foreach (assoc assoc in assocs) {
			if (assoc.bounds == bounds) {
				assocs.Remove(assoc);
				break;
			}
		}
	}
}