using tairasoul.unity.common.events;
using tairasoul.unity.common.speedrunning.dsl;
using tairasoul.unity.common.speedrunning.dsl.eventbus;
using tairasoul.unity.common.speedrunning.dsl.internals;
using UnityEngine;

namespace speedrunningutils.impls;

record assoc(Bounds bounds, Action action);

class BoundsRegistry : IBoundsRegistry
{
	List<assoc> assocs = [];
	internal LineRenderer renderer;
	
	public void BoundCreated(BoundsPtrWrapper bounds)
	{
		renderer.enabled = true;
		GameObject s105 = GameObject.FindFirstObjectByType<Inventory>().gameObject;
		bool lastContained = false;
		assocs.Add(new(bounds, () =>
		{
			if (Plugin.cfg.VisualizeHitboxesByDefault.Value)
				BoundVisualization.VisualizeBound(bounds, renderer);
			if (bounds.bounds.Contains(s105.transform.position)) {
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

	// float speed = 1f;

	public void CheckBounds() {
		// float hue = Mathf.Repeat(Time.time * speed, 1f);
		// Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

		// renderer.startColor = rainbowColor;
		// renderer.endColor = rainbowColor;
		foreach (assoc assoc in assocs) {
			assoc.action();
		}
	}

	public void BoundDestroyed(BoundsPtrWrapper bounds)
	{
		foreach (assoc assoc in assocs) {
			if (assoc.bounds == bounds) {
				assocs.Remove(assoc);
				break;
			}
		}
		if (assocs.Count == 0)
			renderer.enabled = false;
	}
}