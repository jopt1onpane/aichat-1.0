using Cysharp.Text;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class EnvironmentVolumeItemView : MonoBehaviour
{
	[SerializeField]
	private InteractableUI _mainButton;

	[SerializeField]
	private Image _mainButtonImage;

	[SerializeField]
	private Image _mainButtonImageActive;

	[SerializeField]
	private Slider _volumeSlider;

	[SerializeField]
	private TextLocalizationBehaviour _environmentNameText;

	[SerializeField]
	private NewItemIcon _newIcon;

	[SerializeField]
	private EnvironmentIconDBMobile _environmentIconDB;

	[SerializeField]
	private TMP_Text _percentText;

	private EnvironmentType _environmentType;

	public EnvironmentType EnvironmentType => _environmentType;

	public Observable<Unit> OnClickMainButton => _mainButton.GetComponent<Button>().OnClickAsObservable();

	public Observable<float> OnVolumeSliderChange => _volumeSlider.OnValueChangedAsObservable().Skip(1);

	public void Init()
	{
		_mainButton.Setup();
	}

	public void SetModel(EnvironmentVolumeItemModel model, bool withAnimation)
	{
		_environmentType = model.EnvironmentType;
		_environmentNameText.Set(model.NameLocalizeID);
		var (sprite, sprite2) = _environmentIconDB.GetMainButtonIcon(_environmentType);
		_mainButtonImage.sprite = sprite;
		_mainButtonImageActive.sprite = sprite2;
		_mainButton.SetUseUI(model.IsSoundActive, !withAnimation);
		_volumeSlider.SetValueWithoutNotify(model.Volume);
		_newIcon.SetIconActive(model.IsNew);
		UpdateVolumeSliderPercent(model);
	}

	private void UpdateVolumeSliderPercent(EnvironmentVolumeItemModel model)
	{
		_percentText.SetTextFormat("{0}%", (int)(model.Volume * 100f));
	}

	public void UnsetModel()
	{
	}
}
