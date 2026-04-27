using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLightProbes;

public class PartialVolumeFilling
{
	public enum TargetPoint
	{
		Unlit,
		Equivalent,
		Free
	}

	public IEnumerator ExecutePass(MagicLightProbes parent, TargetPoint targetPoint, MLPVolume currentVolume = null, bool realtimeEditing = false)
	{
		List<MLPPointData> tempList = new List<MLPPointData>();
		List<Vector3> realTimeEditingList = new List<Vector3>();
		List<Vector3> targetPoints = new List<Vector3>();
		float fillingRate = 0f;
		switch (targetPoint)
		{
		case TargetPoint.Unlit:
			parent.currentPass = "Unlit Probes Processing Pass 1/2";
			targetPoints.AddRange(currentVolume.localUnlitPointsPositions);
			currentVolume.resultLocalFreePointsPositions.Clear();
			currentVolume.resultLocalUnlitPointsPositions.Clear();
			fillingRate = parent.unlitVolumeFillingRate;
			tempList.AddRange(parent.tmpUnlitPoints);
			break;
		case TargetPoint.Equivalent:
			parent.currentPass = "Equivalent Probes Processing Pass 1/2";
			targetPoints.AddRange(currentVolume.localEquivalentPointsPositions);
			currentVolume.resultLocalFreePointsPositions.Clear();
			currentVolume.resultLocalEquivalentPointsPositions.Clear();
			fillingRate = parent.equivalentVolumeFillingRate;
			tempList.AddRange(parent.tmpEqualPoints);
			break;
		case TargetPoint.Free:
			parent.currentPass = "Free Probes Processing";
			targetPoints.AddRange(currentVolume.localFreePointsPositions);
			currentVolume.resultLocalFreePointsPositions.Clear();
			currentVolume.resultLocalEquivalentPointsPositions.Clear();
			currentVolume.resultLocalUnlitPointsPositions.Clear();
			fillingRate = parent.freeVolumeFillingRate;
			tempList.AddRange(parent.tmpFreePoints);
			break;
		}
		_ = realtimeEditing;
		realTimeEditingList.AddRange(targetPoints);
		parent.currentPassProgressCounter = 0;
		parent.currentPassProgressFrameSkipper = 0;
		if (targetPoints.Count > 0)
		{
			if (SystemInfo.supportsComputeShaders)
			{
				ComputeBuffer computeBuffer = new ComputeBuffer(targetPoints.Count, 12, ComputeBufferType.Default);
				ComputeBuffer computeBuffer2 = new ComputeBuffer(targetPoints.Count, 12, ComputeBufferType.Default);
				computeBuffer.SetData(targetPoints.ToArray());
				computeBuffer2.SetData(targetPoints.ToArray());
				parent.calculateVolumeFilling.SetBuffer(parent.calculateVolumeFilling.FindKernel("CSMain"), "inputArray", computeBuffer);
				parent.calculateVolumeFilling.SetBuffer(parent.calculateVolumeFilling.FindKernel("CSMain"), "exitArray", computeBuffer2);
				parent.calculateVolumeFilling.SetFloat("threshold", fillingRate);
				parent.calculateVolumeFilling.Dispatch(parent.calculateVolumeFilling.FindKernel("CSMain"), 256, 1, 1);
				Vector3[] exit = new Vector3[computeBuffer.count];
				computeBuffer2.GetData(exit);
				computeBuffer.Dispose();
				computeBuffer2.Dispose();
				for (int i = 0; i < exit.Length; i++)
				{
					if (!(exit[i] == Vector3.zero))
					{
						if (!realtimeEditing)
						{
							tempList[i].position = exit[i];
							parent.tmpSharedPointsArray.Add(tempList[i]);
						}
						switch (targetPoint)
						{
						case TargetPoint.Unlit:
							currentVolume.resultLocalUnlitPointsPositions.Add(exit[i]);
							break;
						case TargetPoint.Equivalent:
							currentVolume.resultLocalEquivalentPointsPositions.Add(exit[i]);
							break;
						case TargetPoint.Free:
							currentVolume.resultLocalFreePointsPositions.Add(exit[i]);
							break;
						}
						if (parent.UpdateProgress(exit.Length, 1000))
						{
							yield return null;
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < Mathf.RoundToInt((float)targetPoints.Count * (1f - fillingRate)); i++)
				{
					realTimeEditingList.Remove(realTimeEditingList[Random.Range(0, realTimeEditingList.Count)]);
					if (parent.UpdateProgress(Mathf.RoundToInt((float)targetPoints.Count * (1f - fillingRate))))
					{
						yield return null;
					}
				}
				if (!realtimeEditing)
				{
					parent.tmpSharedPointsArray.AddRange(tempList);
					for (int j = 0; j < tempList.Count; j++)
					{
						switch (targetPoint)
						{
						case TargetPoint.Unlit:
							currentVolume.resultLocalUnlitPointsPositions.Add(tempList[j].position);
							break;
						case TargetPoint.Equivalent:
							currentVolume.resultLocalEquivalentPointsPositions.Add(tempList[j].position);
							break;
						case TargetPoint.Free:
							currentVolume.resultLocalFreePointsPositions.Add(tempList[j].position);
							break;
						}
					}
				}
				else
				{
					for (int k = 0; k < realTimeEditingList.Count; k++)
					{
						switch (targetPoint)
						{
						case TargetPoint.Unlit:
							currentVolume.resultLocalUnlitPointsPositions.Add(realTimeEditingList[k]);
							break;
						case TargetPoint.Equivalent:
							currentVolume.resultLocalEquivalentPointsPositions.Add(realTimeEditingList[k]);
							break;
						case TargetPoint.Free:
							currentVolume.resultLocalFreePointsPositions.Add(realTimeEditingList[k]);
							break;
						}
					}
				}
			}
			if (targetPoint == TargetPoint.Unlit && !realtimeEditing)
			{
				parent.currentPass = "Unlit Probes Optimization Pass 2/2";
				parent.currentPassProgressCounter = 0;
				parent.currentPassProgressFrameSkipper = 0;
				for (int i = 0; i < tempList.Count; i++)
				{
					if (!tempList[i].lockForCull)
					{
						parent.CheckForNearContrast(tempList[i]);
					}
					if (!parent.isInBackground && parent.UpdateProgress(tempList.Count))
					{
						yield return null;
					}
				}
			}
		}
		parent.totalProbesInSubVolume = parent.tmpSharedPointsArray.Count;
		parent.calculatingVolumeSubPass = false;
	}
}
