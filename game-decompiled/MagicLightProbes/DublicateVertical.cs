using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLightProbes;

public class DublicateVertical
{
	public IEnumerator ExecutePass(MagicLightProbes parent)
	{
		parent.currentPass = "Check For Nearby Geometry...";
		parent.currentPassProgressCounter = 0;
		parent.currentPassProgressFrameSkipper = 0;
		int steps = Mathf.RoundToInt(parent.verticalDublicatingHeight / parent.verticalDublicatingStep);
		List<MLPPointData> candidates = new List<MLPPointData>();
		List<MLPPointData> savedNearGeometry = new List<MLPPointData>();
		parent.tmpSharedPointsArray.AddRange(parent.tmpNearbyGeometryPoints);
		for (int i = 0; i < parent.tmpSharedPointsArray.Count; i++)
		{
			if (parent.tmpSharedPointsArray[i].savedNearGeometry)
			{
				if (Vector3.Dot(-Vector3.up, (parent.tmpSharedPointsArray[i].contactPoint - parent.tmpSharedPointsArray[i].position).normalized) == 1f)
				{
					candidates.Add(parent.tmpSharedPointsArray[i]);
				}
				else
				{
					savedNearGeometry.Add(parent.tmpSharedPointsArray[i]);
				}
			}
			if (parent.UpdateProgress(parent.tmpSharedPointsArray.Count, 1000))
			{
				yield return null;
			}
		}
		parent.tmpSharedPointsArray.Clear();
		for (int i = 0; i < steps; i++)
		{
			parent.currentPass = "Vertical Dublicating Step " + i + "/" + steps;
			parent.currentPassProgressCounter = 0;
			parent.currentPassProgressFrameSkipper = 0;
			foreach (MLPPointData item in candidates)
			{
				MLPPointData mLPPointData = new MLPPointData();
				mLPPointData.position = new Vector3(item.position.x, item.position.y + parent.verticalDublicatingStep * (float)i, item.position.z);
				if (parent.probesVolume.GetComponent<MeshRenderer>().bounds.Contains(mLPPointData.position))
				{
					parent.tmpSharedPointsArray.Add(mLPPointData);
				}
				if (!parent.isInBackground && parent.UpdateProgress(parent.tmpSharedPointsArray.Count, 1000))
				{
					yield return null;
				}
			}
		}
		parent.tmpSharedPointsArray.AddRange(candidates);
		parent.tmpSharedPointsArray.AddRange(savedNearGeometry);
		parent.tmpSharedPointsArray.AddRange(parent.tmpPointsNearGeometryIntersections);
		parent.calculatingVolumeSubPass = false;
	}
}
