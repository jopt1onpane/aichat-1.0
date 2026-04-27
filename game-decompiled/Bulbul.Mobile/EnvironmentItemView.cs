using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class EnvironmentItemView : MonoBehaviour
{
	[SerializeField]
	private InteractableUI _mainButton;

	[SerializeField]
	private Image _mainButtonImage;

	[SerializeField]
	private Image _mainButtonImageActive;

	[SerializeField]
	private InteractableUI _windowOnlyButton;

	[SerializeField]
	private Slider _volumeSlider;

	[SerializeField]
	private TextLocalizationBehaviour _environmentNameText;

	[SerializeField]
	private Color _nameTextLockColor;

	[SerializeField]
	private GameObject _lockObj;

	[SerializeField]
	private NewItemIcon _newIcon;

	[SerializeField]
	private GameObject _shopObj;

	[SerializeField]
	private Button _shopThumbnailButton;

	[SerializeField]
	private PurchaseButtonWithPoint _purchaseButtonWithPoint;

	[SerializeField]
	private Image _shopThumbnailImage;

	[SerializeField]
	private GameObject[] _mobileDemoEditionLockedObjs;

	[SerializeField]
	private EnvironmentIconDBMobile _environmentIconDB;

	private Color _nameTextDefaultColor;

	private EnvironmentType _environmentType;

	private bool _isMobileDemoEditionLocked;

	public EnvironmentType EnvironmentType => _environmentType;

	public bool IsMobileDemoEditionLocked => _isMobileDemoEditionLocked;

	public Observable<Unit> OnClickMainButton => _mainButton.GetComponent<Button>().OnClickAsObservable();

	public Observable<Unit> OnClickWindowButton => _windowOnlyButton.GetComponent<Button>().OnClickAsObservable();

	public Observable<Unit> OnClickPurchaseButton => Observable.Merge<Unit>(_shopThumbnailButton.OnClickAsObservable(), _purchaseButtonWithPoint.OnClick);

	public void Init()
	{
		_mainButton.Setup();
		_windowOnlyButton.Setup();
		_purchaseButtonWithPoint.Initialize();
		_nameTextDefaultColor = _environmentNameText.Text.color;
	}

	public void SetModel(EnvironmentItemModel model, bool withAnimation)
	{
		_environmentType = model.EnvironmentType;
		EnvironmentItemModel.ItemLockState lockState = model.LockState;
		bool flag = lockState == EnvironmentItemModel.ItemLockState.Locked || lockState == EnvironmentItemModel.ItemLockState.LockedByPurchase;
		_environmentNameText.Set(flag ? "ui_lock_title" : model.NameLocalizeID);
		_environmentNameText.Text.color = (flag ? _nameTextLockColor : _nameTextDefaultColor);
		if (model.LockState == EnvironmentItemModel.ItemLockState.LockedByPurchase)
		{
			_environmentNameText.Text.text = "";
		}
		if (model.LockState == EnvironmentItemModel.ItemLockState.Unlocked)
		{
			SetActiveMobileDemoEditionLockedObjs(model.IsMobileDemoEditionLocked);
			_isMobileDemoEditionLocked = model.IsMobileDemoEditionLocked;
		}
		else
		{
			_isMobileDemoEditionLocked = false;
			SetActiveMobileDemoEditionLockedObjs(active: false);
		}
		var (sprite, sprite2) = _environmentIconDB.GetMainButtonIcon(_environmentType);
		_mainButtonImage.sprite = sprite;
		_mainButtonImageActive.sprite = sprite2;
		_mainButton.SetUseUI(model.IsWindowActive || model.IsSoundActive, !withAnimation);
		WindowViewType windowViewType;
		bool num = _environmentType.TryConvertToWindowViewType(out windowViewType);
		AmbientSoundType ambientSoundType;
		bool flag2 = _environmentType.TryConvertToAmbientSoundType(out ambientSoundType);
		bool flag3 = num && flag2 && !flag;
		_windowOnlyButton.gameObject.SetActive(flag3);
		if (flag3)
		{
			_windowOnlyButton.SetUseUI(model.IsWindowActive, !withAnimation);
		}
		bool flag4 = flag2 && !flag;
		_volumeSlider.gameObject.SetActive(flag4);
		if (flag4)
		{
			_volumeSlider.value = model.Volume;
		}
		_lockObj.SetActive(flag);
		_mainButton.gameObject.SetActive(!flag);
		_newIcon.SetIconActive(model.IsNew && !flag);
		bool flag5 = model.LockState == EnvironmentItemModel.ItemLockState.LockedByPurchase;
		_shopObj.SetActive(flag5);
		if (flag5)
		{
			_shopThumbnailImage.sprite = _environmentIconDB.GetShopIcon(_environmentType);
			_purchaseButtonWithPoint.Setup(model.Price, model.HasEnoughPoints);
		}
	}

	public void SetActiveMobileDemoEditionLockedObjs(bool active)
	{
		GameObject[] mobileDemoEditionLockedObjs = _mobileDemoEditionLockedObjs;
		foreach (GameObject gameObject in mobileDemoEditionLockedObjs)
		{
			if (gameObject.activeSelf != active)
			{
				gameObject.SetActive(active);
			}
		}
	}

	public void UnsetModel()
	{
	}
}
