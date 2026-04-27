using System.Collections;
using UnityEngine;

namespace MagicLightProbes;

public class SetDistanceFromGeometry
{
	public IEnumerator ExecutePass(MagicLightProbes parent, MLPVolume currentVolume)
	{
		currentVolume.resultLocalCornerPointsPositions.Clear();
		ComputeBuffer computeBuffer = new ComputeBuffer(currentVolume.localCornerPointsPositions.Count, 12, ComputeBufferType.Default);
		ComputeBuffer computeBuffer2 = new ComputeBuffer(currentVolume.localCornerPointsPositions.Count, 12, ComputeBufferType.Default);
		ComputeBuffer computeBuffer3 = new ComputeBuffer(currentVolume.localCornerPointsPositions.Count, 12, ComputeBufferType.Default);
		computeBuffer.SetData(currentVolume.localCornerPointsPositions.ToArray());
		computeBuffer2.SetData(currentVolume.localCornerPointsPositions.ToArray());
		computeBuffer3.SetData(currentVolume.localAvaragedDirections.ToArray());
		parent.calculateDistanceFromGeometry.SetBuffer(parent.calculateDistanceFromGeometry.FindKernel("CSMain"), "inputArray", computeBuffer);
		parent.calculateDistanceFromGeometry.SetBuffer(parent.calculateDistanceFromGeometry.FindKernel("CSMain"), "exitArray", computeBuffer2);
		parent.calculateDistanceFromGeometry.SetBuffer(parent.calculateDistanceFromGeometry.FindKernel("CSMain"), "directionsArray", computeBuffer3);
		parent.calculateDistanceFromGeometry.SetFloat("distance", parent.unlitVolumeFillingRate);
		parent.calculateDistanceFromGeometry.Dispatch(parent.calculateDistanceFromGeometry.FindKernel("CSMain"), 256, 1, 1);
		Vector3[] exit = new Vector3[computeBuffer.count];
		computeBuffer2.GetData(exit);
		computeBuffer.Dispose();
		computeBuffer2.Dispose();
		currentVolume.localCornerPointsPositions.Clear();
		for (int i = 0; i < exit.Length; i++)
		{
			if (!(exit[i] == Vector3.zero))
			{
				currentVolume.localCornerPointsPositions.Add(exit[i]);
				if (parent.UpdateProgress(exit.Length, 1000))
				{
					yield return null;
				}
			}
		}
	}
}
