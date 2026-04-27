using MagicLightProbes;
using UnityEngine;

[CreateAssetMenu(fileName = "New MLP Settings Asset", menuName = "Magic Tools/Magic Light Probes/Create New Settings Asset", order = 1)]
public class MLPSettings : ScriptableObject
{
	public global::MagicLightProbes.MagicLightProbes.Workflow workflow;

	public float probesSpacing;

	public float cornersDetectionThreshold;

	public float cornerProbesSpacing;

	public float distanceFromGeometry;

	public int divideVolumeOnProbesCount;

	public bool useVolumeBottom;

	public bool selectedObjectsAsGlobal;

	public float maxHeightAboveGeometry;

	public float maxHeightAboveTerrain;

	public LayerMask collisionDetectionLayers;

	public float collisionDetectionRadius;

	public bool considerDistanceToLights;

	public global::MagicLightProbes.MagicLightProbes.FillingMode fillingMode;

	public float lightIntensityThreshold;

	public bool cullByColor;

	public float colorThreshold;

	public bool fillEquivalentVolume;

	public float equivalentVolumeFillingRate;

	public bool fillUnlitVolume;

	public float unlitVolumeFillingRate;

	public bool optimizeForMixedLighting;

	public bool tryPreventLightLeakage;

	public global::MagicLightProbes.MagicLightProbes.BoundsDisplayMode boundsDisplayMode;
}
