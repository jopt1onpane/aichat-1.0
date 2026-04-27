using System.Collections.Generic;
using UnityEngine;

namespace MagicLightProbes;

[DisallowMultipleComponent]
[ExecuteInEditMode]
[HelpURL("https://motion-games-studio.gitbook.io/magic-light-probes/system-components/mlp-light")]
public class MLPLight : MonoBehaviour
{
	public enum CalculationMode
	{
		AccurateShadows,
		LightIntensity
	}

	public enum MLPLightType
	{
		Spot,
		Directional,
		Point,
		Area,
		Mesh
	}

	public enum MLPLightTypeMA
	{
		Area = 3,
		Mesh
	}

	public enum TracePointSettingMode
	{
		Auto,
		Custom
	}

	public enum ShadowmaskMode
	{
		Shadowmask,
		DistanceShadowmask
	}

	public MLPLightType lightType;

	public MLPLightType lastLightType;

	public MLPLightTypeMA lightTypeMA;

	public CalculationMode calculationMode;

	public TracePointSettingMode tracePointSettingType;

	public LightmapBakeType lightMode;

	public ShadowmaskMode shadowmaskMode;

	public Light targetLight;

	public GameObject parentGameObject;

	public Vector3 position;

	public Vector3 forward;

	public bool saveNearbyProbes;

	public float saveRadius;

	public float range;

	public bool useSourceParameters;

	public bool reverseDirection;

	public float angle;

	public bool customTracePoints;

	public bool accurateTrace;

	public int accuracy;

	public int lastAccuracy;

	public bool isDirectional;

	public float tracePointSize = 0.3f;

	public float lastTracePointSize;

	public MeshFilter lastMesh;

	public List<GameObject> tracePoints = new List<GameObject>();

	public List<MLPTracePoint> tracePointsData = new List<MLPTracePoint>();

	public MLPTracePoint mainTracePoint;

	public MagicLightProbes parentVolume;

	public bool showOptionsInManagerWindow;

	public float intensity;

	public bool resetEditor;

	public bool showLightOnScene;

	public bool saveOnOutOfRange;

	public bool isHDRP;

	public Vector2 hdrpAreaSize;
}
