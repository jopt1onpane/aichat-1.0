using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class EnvironmentWindow : MonoBehaviour
{
	public Button CloseButton;

	public PlayerPointView PlayerPointView;

	public Button VolumeButton;

	public FacilityAnimationBase ActivateAnimation;

	public PresetSaveDialogView PresetSaveDialogView;

	public Button PresetSaveButton;

	public InteractableUI PresetSaveInteractableUI;
}
