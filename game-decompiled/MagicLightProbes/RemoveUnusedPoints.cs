using System.Collections;
using System.Collections.Generic;

namespace MagicLightProbes;

public class RemoveUnusedPoints
{
	public IEnumerator ExecutePass(MagicLightProbes parent)
	{
		parent.currentPass = "Romoving Unused Points...";
		parent.currentPassProgressCounter = 0;
		parent.currentPassProgressFrameSkipper = 0;
		List<MLPPointData> pointsToRemove = new List<MLPPointData>();
		if (parent.debugMode && parent.subVolumesDivided.Count > 0)
		{
			for (int i = 0; i < parent.debugAcceptedPoints.Count; i++)
			{
				if (parent.debugAcceptedPoints[i].col == parent.xPointsCount - 1 || parent.debugAcceptedPoints[i].depth == parent.zPointsCount)
				{
					pointsToRemove.Add(parent.debugAcceptedPoints[i]);
				}
				if (!parent.isInBackground && parent.UpdateProgress(parent.debugAcceptedPoints.Count))
				{
					yield return null;
				}
			}
			for (int j = 0; j < pointsToRemove.Count; j++)
			{
				parent.debugAcceptedPoints.Remove(pointsToRemove[j]);
			}
		}
		pointsToRemove.Clear();
		parent.calculatingVolumeSubPass = false;
	}
}
