using KanKikuchi.AudioManager;
using NestopiSystem.DIContainers;
using UnityEngine;
using VContainer.Unity;

namespace Bulbul;

public class AssetLoadHolder : MonoBehaviour
{
	[SerializeField]
	private ProjectLifetimeScope projectLifetimeScopePrefab;

	[SerializeField]
	private VContainerSettings vContainerSettings;

	[SerializeField]
	private AudioManagerSetting audioManagerSetting;

	[SerializeField]
	private CommonPrefabSupplier commonPrefabSupplier;

	public ProjectLifetimeScope ProjectLifetimeScope => projectLifetimeScopePrefab;

	public VContainerSettings VContainerSettings => vContainerSettings;

	public AudioManagerSetting AudioManagerSetting => audioManagerSetting;

	public CommonPrefabSupplier CommonPrefabSupplier => commonPrefabSupplier;
}
