using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MagicLightProbes;

[ExecuteInEditMode]
[HelpURL("https://motion-games-studio.gitbook.io/magic-light-probes/system-components/main-component")]
public class MagicLightProbes : MonoBehaviour
{
	public struct VolumeParameters(int _volumeIndex, Vector3 _position, Vector3 _demensions)
	{
		public int volumeIndex = _volumeIndex;

		public Vector3 position = _position;

		public Vector3 demensions = _demensions;
	}

	public enum FillingMode
	{
		VerticalDublicating,
		FullFilling,
		SeparateFilling
	}

	public enum ExcludingLightsMode
	{
		AllInList,
		AllExceptInList
	}

	public enum Workflow
	{
		Simple,
		Advanced
	}

	public enum BoundsDisplayMode
	{
		Always,
		OnSelection
	}

	public enum DebugPasses
	{
		MaximumHeight,
		GeometryCollision,
		GeometryIntersections,
		NearGeometry,
		OutOfRange,
		OutOfRangeBorders,
		ShadingBorders,
		ContrastAreas,
		NearLights,
		LightIntensity,
		UnlitProbes,
		EqualProbes,
		GeometryEdges,
		EqualColor
	}

	public enum DrawModes
	{
		Accepted,
		Culled,
		Both
	}

	public enum CalculationTarget
	{
		GeometryEdges,
		GeneralCalculation
	}

	[Serializable]
	private struct TempPointData(Vector3 _position)
	{
		public float xPos = _position.x;

		public float yPos = _position.y;

		public float zPos = _position.z;
	}

	[Serializable]
	public class WorkPathFoundEvent : UnityEvent<string>
	{
	}

	private const string COMPUTE_SHADERS_FOLDER = "/Passes/Compute Shaders/";

	public IEnumerator colorThresholdRecalculationRoutine;

	public IEnumerator lightProbesVolumeCalculatingRoutine;

	public IEnumerator lightProbesVolumeCalculatingSubRoutine;

	public IEnumerator executingPassesRoutine;

	public IEnumerator volumeDivideingRoutine;

	public bool autoSaveSettings;

	public MLPSettings lastSettingsAsset;

	public List<string> groundAndFloorObjects = new List<string>();

	public List<string> storedGroundAndFloorKeywords = new List<string>();

	public GameObject probesVolume;

	public bool useDynamicDensity;

	public float volumeSpacing = 0.4f;

	public float volumeSpacingMin = 0.4f;

	public float volumeSpacingMax = 5f;

	public float cornersDetectionThreshold = 0.4f;

	public float cornersDetectionThresholdMin = 0.4f;

	public float cornersDetectionThresholdMax = 5f;

	public float lastCornersDetectionThreshold;

	public float lastCornersDetectionThresholdMin;

	public float lastCornersDetectionThresholdMax;

	public int lastMaxProbesInVolume;

	public int maxProbesInVolume = 10000;

	public int defaultMaxProbesCount;

	public float lastVolumeSpacing;

	public float lastVolumeSpacingMin;

	public float lastVolumeSpacingMax;

	public bool volumeSpacingChanged;

	public bool tooManySubVolumes;

	public FillingMode fillingMode = FillingMode.SeparateFilling;

	public ExcludingLightsMode excludingLightsMode;

	public MLPSettings settingsAsset;

	public Workflow workflow;

	public float maxHeightAboveGeometry = 3.5f;

	public float lastMaxHeightAboveGeometry;

	public float maxHeightAboveTerrain = 3.5f;

	public bool considerDistanceToLights = true;

	public float lightIntensityTreshold = 0.5f;

	public float colorTreshold = 0.01f;

	public float collisionDetectionRadius = 0.1f;

	public bool saveProbesNearbyGeometry = true;

	public float cornerProbesSpacing = 0.5f;

	public float nearbyGeometryDetectionRadius = 0.5f;

	public float nearbyGeometryDetectionRadiusMin;

	public float nearbyGeometryDetectionRadiusMax;

	public float distanceFromNearbyGeometry = 0.1f;

	public bool fillEquivalentVolume;

	public float equivalentVolumeFillingRate;

	public bool fillUnlitVolume;

	public bool fillFreeVolume = true;

	public float unlitVolumeFillingRate;

	public float freeVolumeFillingRate = 0.01f;

	public bool cullAcceptedVolume;

	public float acceptedVolumeFillingRate;

	public float nearbyGeometryVolumeFillingRate;

	public float verticalDublicatingHeight = 3.5f;

	public float verticalDublicatingStep = 1f;

	public LayerMask raycastFilter;

	public List<MLPLight> excludedLights = new List<MLPLight>();

	public LayerMask layerMask = 1;

	public int firstCollisionLayer;

	public bool useMultithreading = true;

	public GameObject previousSelection;

	public bool unloaded;

	public bool sceneChanging;

	public bool waitForPrevious;

	public MagicLightProbes previousVolume;

	public bool optimizeForMixedLighting;

	public bool lastOptimizeForMixedLightingValue;

	public bool lastUseDynamicDensityValue;

	public bool preventLeakageThroughWalls;

	public bool useVolumeBottom;

	public bool placeProbesOnGeometryEdges = true;

	public bool queryHitBackfaces;

	public float lastColorThreshold;

	public float lastLightIntensityThreshold;

	public float lastEquivalentVolumeFillingRate;

	public float lastUnlitVolumeFillingRate;

	public float lastFreeVolumeFillingRate;

	public float lastCornerProbesSpacing;

	public float lastDistanceFromGeometry;

	public bool debugMode;

	public float debugObjectScale = 0.1f;

	public BoundsDisplayMode boundsDisplayMode;

	public DebugPasses debugPass;

	public DrawModes drawMode;

	public bool debugShowLightIntensity;

	public bool showPreviewGrid;

	public bool nextStep;

	public bool cullByColor = true;

	public bool forceSaveProbesOnShadingBorders = true;

	public string dataPath;

	public string workPath;

	public bool workPathFound;

	public List<Vector3> localFinishedPositions = new List<Vector3>();

	public ComputeShader calculateVolumeFilling;

	public ComputeShader calculateProbeSpacing;

	public ComputeShader calculateDistanceFromGeometry;

	public MLPVolume currentVolume;

	public bool recalculationRequired;

	private List<Collider> lightColliders = new List<Collider>();

	private List<Collider> objectColliders = new List<Collider>();

	private List<MLPPointData> finalDebugAcceptedPoints = new List<MLPPointData>();

	private List<MLPPointData> finalDebugCulledPoints = new List<MLPPointData>();

	private List<GameObject> tempObjects = new List<GameObject>();

	private List<GameObject> temporarilyDisabledDynamicObjects = new List<GameObject>();

	private List<GameObject> staticObjectsWithoutCollider = new List<GameObject>();

	public List<GameObject> finalStaticGameObjectsList = new List<GameObject>();

	private GameObject combinedVolumeObject;

	private VolumeParameters currentEditingVolume;

	private Vector3 currentSelectedObjectLastPosition;

	private GameObject lastSelectedObject;

	public bool cancelCombination;

	public List<IEnumerator> passesToExecute = new List<IEnumerator>();

	public List<GameObject> staticObjects = new List<GameObject>();

	public List<MLPLight> lights = new List<MLPLight>();

	public List<MLPPointData> tmpSharedPointsArray = new List<MLPPointData>();

	public List<MLPPointData> tmpOutOfRangePoints = new List<MLPPointData>();

	public List<MLPPointData> tmpOutOfMaxHeightPoints = new List<MLPPointData>();

	public List<MLPPointData> tmpGeometryCollisionPoints = new List<MLPPointData>();

	public List<MLPPointData> tmpContrastOnOutOfRangePoints = new List<MLPPointData>();

	public List<MLPPointData> tmpContrastShadingBordersPoints = new List<MLPPointData>();

	public List<MLPPointData> tmpUnlitPoints = new List<MLPPointData>();

	public List<MLPPointData> tmpFreePoints = new List<MLPPointData>();

	public List<MLPPointData> tmpNearbyGeometryPoints = new List<MLPPointData>();

	public List<MLPPointData> tmpPointsNearGeometryIntersections = new List<MLPPointData>();

	public List<MLPPointData> tmpNearbyLightsPoints = new List<MLPPointData>();

	public List<MLPPointData> tmpEqualPoints = new List<MLPPointData>();

	public List<MLPPointData> debugCulledPoints = new List<MLPPointData>();

	public List<MLPPointData> debugAcceptedPoints = new List<MLPPointData>();

	public List<GameObject> subVolumesDivided = new List<GameObject>();

	public List<Vector3> points = new List<Vector3>();

	public List<VolumeParameters> innerVolumes = new List<VolumeParameters>();

	public List<VolumeParameters> subVolumesParameters = new List<VolumeParameters>();

	public List<MagicLightProbes> innerVolumesObjects = new List<MagicLightProbes>();

	public List<LayerMask> layerMasks = new List<LayerMask>();

	public List<Vector3> transformedPoints = new List<Vector3>();

	public string assetEditorPath;

	public MagicLightProbes parentVolume;

	public static bool operationalDataLost = true;

	public bool localOperationalDataLost = true;

	public bool recombinationNeeded;

	public bool isInBackground;

	public bool realtimeEditing;

	public bool calculated;

	public bool calculatingError;

	public int xPointsCount;

	public int yPointsCount;

	public int zPointsCount;

	public float prevVolumeScaleX;

	public float prevVolumeScaleY;

	public float prevVolumeScaleZ;

	public int totalProbes;

	public int totalProbesInSubVolume;

	public int totalProbesInVolume;

	public bool calculatingVolume;

	public bool calculatingVolumeSubPass;

	public string currentPass;

	public int currentPassProgressCounter;

	public int currentPassProgressFrameSkipper;

	public float totalProgress;

	public float currentPassProgress;

	public int selectedTab;

	public bool showOptionsInManagerWindow;

	public bool restored = true;

	public int currentVolumePart;

	public float eta;

	public bool changed;

	public bool redivideParts;

	public bool combinedVolumeError;

	public bool isInPrefab;

	public Vector3 prefabPosition;

	public GameObject prefabRoot;

	public MLPPrefab prefabConnectionObject;

	public string prefabRootName;

	public bool calculatedFromPrefab;

	public string prefabUID;

	private bool passesExecuting;

	private int totalProgressCounter;

	private int totalProgressFrameSkipper;

	private float startTime;

	private float endTime;

	private bool scenePreparing;

	public void CheckForNearContrast(MLPPointData pointForCheck)
	{
		bool flag = true;
		List<MLPPointData> list = new List<MLPPointData>();
		foreach (MLPPointData item in tmpSharedPointsArray)
		{
			if (Vector3.Distance(item.position, pointForCheck.position) <= 2f)
			{
				list.Add(item);
			}
		}
		foreach (MLPPointData item2 in list)
		{
			if (!item2.contrastOnOutOfRangeArea && !item2.contrastOnShadingArea)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			tmpSharedPointsArray.Add(pointForCheck);
		}
	}

	public bool UpdateProgress(int count, int period = 100)
	{
		currentPassProgress = (float)currentPassProgressCounter / (float)count * 100f;
		currentPassProgressCounter++;
		currentPassProgressFrameSkipper++;
		if (currentPassProgressFrameSkipper == period)
		{
			currentPassProgressFrameSkipper = 0;
			return true;
		}
		return false;
	}

	private bool UpdateTotalProgress(int count, int period = 100)
	{
		totalProgress = (float)totalProgressCounter / (float)count * 100f;
		totalProgressCounter++;
		totalProgressFrameSkipper++;
		if (totalProgressFrameSkipper == period)
		{
			totalProgressFrameSkipper = 0;
			return true;
		}
		return false;
	}
}
