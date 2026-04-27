using System.Collections.Generic;
using UnityEngine;

namespace MagicLightProbes;

[ExecuteInEditMode]
public class MLPVolume : MonoBehaviour
{
	[HideInInspector]
	public MagicLightProbes parentRootComponent;

	[HideInInspector]
	public MeshRenderer selfRenderer;

	[HideInInspector]
	public bool showGizmo;

	public bool showGizmoSelected;

	public bool isPartVolume;

	public bool isSubdividedPart;

	public bool isCalculated;

	public bool isInProcess;

	public bool skipped;

	public int id;

	public Color colorOnSelection;

	public List<MLPPointData> localAcceptedPoints = new List<MLPPointData>();

	public List<MLPPointData> localNearbyGeometryPoints = new List<MLPPointData>();

	public List<MLPPointData> localContrastPoints = new List<MLPPointData>();

	public List<MLPPointData> localCornerPoints = new List<MLPPointData>();

	public List<Vector3> localNearbyGeometryPointsPositions = new List<Vector3>();

	public List<Vector3> resultNearbyGeometryPointsPositions = new List<Vector3>();

	public List<Vector3> localCornerPointsPositions = new List<Vector3>();

	public List<Vector3> resultLocalCornerPointsPositions = new List<Vector3>();

	public List<Vector3> localEquivalentPointsPositions = new List<Vector3>();

	public List<Vector3> resultLocalEquivalentPointsPositions = new List<Vector3>();

	public List<Vector3> resultLocalFreePointsPositions = new List<Vector3>();

	public List<Vector3> localUnlitPointsPositions = new List<Vector3>();

	public List<Vector3> localFreePointsPositions = new List<Vector3>();

	public List<Vector3> resultLocalUnlitPointsPositions = new List<Vector3>();

	public List<Vector3> localDirections = new List<Vector3>();

	public List<Vector3> localAvaragedDirections = new List<Vector3>();

	public List<MLPPointData> localColorThresholdEditingPoints = new List<MLPPointData>();

	public int objectsInside;
}
