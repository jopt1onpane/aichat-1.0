using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLightProbes;

public class EquivalentProbesOptimization
{
	public IEnumerator ExecutePass(MagicLightProbes parent, MLPVolume currentVolume = null, bool realtimeEditing = false)
	{
		parent.currentPass = "Equivalent Probes Optimization...";
		parent.currentPassProgressCounter = 0;
		parent.currentPassProgressFrameSkipper = 0;
		if (parent.debugMode)
		{
			parent.tmpSharedPointsArray.Clear();
			parent.debugAcceptedPoints.Clear();
		}
		if (currentVolume.localEquivalentPointsPositions.Count > 0)
		{
			currentVolume.resultLocalEquivalentPointsPositions.Clear();
			ComputeBuffer computeBuffer = new ComputeBuffer(currentVolume.localEquivalentPointsPositions.Count, 12, ComputeBufferType.Default);
			ComputeBuffer computeBuffer2 = new ComputeBuffer(currentVolume.localEquivalentPointsPositions.Count, 12, ComputeBufferType.Default);
			computeBuffer.SetData(currentVolume.localEquivalentPointsPositions.ToArray());
			computeBuffer2.SetData(currentVolume.localEquivalentPointsPositions.ToArray());
			parent.calculateVolumeFilling.SetBuffer(parent.calculateVolumeFilling.FindKernel("CSMain"), "inputArray", computeBuffer);
			parent.calculateVolumeFilling.SetBuffer(parent.calculateVolumeFilling.FindKernel("CSMain"), "exitArray", computeBuffer2);
			parent.calculateVolumeFilling.SetFloat("threshold", parent.equivalentVolumeFillingRate);
			parent.calculateVolumeFilling.Dispatch(parent.calculateVolumeFilling.FindKernel("CSMain"), 256, 1, 1);
			Vector3[] exit = new Vector3[computeBuffer.count];
			computeBuffer2.GetData(exit);
			computeBuffer.Dispose();
			computeBuffer2.Dispose();
			List<MLPPointData> tempList = new List<MLPPointData>();
			tempList.AddRange(parent.tmpEqualPoints);
			for (int i = 0; i < exit.Length; i++)
			{
				if (!(exit[i] == Vector3.zero))
				{
					if (!realtimeEditing)
					{
						tempList[i].position = exit[i];
						parent.tmpSharedPointsArray.Add(tempList[i]);
					}
					currentVolume.resultLocalEquivalentPointsPositions.Add(exit[i]);
					if (!parent.isInBackground && parent.UpdateProgress(exit.Length, 1000))
					{
						yield return null;
					}
				}
			}
			if (parent.debugMode)
			{
				parent.debugAcceptedPoints.AddRange(parent.tmpSharedPointsArray);
			}
		}
		parent.totalProbesInSubVolume = parent.tmpSharedPointsArray.Count;
		parent.calculatingVolumeSubPass = false;
	}
}
