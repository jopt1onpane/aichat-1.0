using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class EnvironmentTimeItemView : MonoBehaviour
{
	[SerializeField]
	private Image _mainImage;

	[SerializeField]
	private Image _mainImageActive;

	[SerializeField]
	private InteractableUI _interactableUI;

	[SerializeField]
	private EnvironmentIconDBMobile _iconDB;

	[SerializeField]
	private GameObject _lockObj;

	[SerializeField]
	private NewItemIcon _newIcon;

	private EnvironmentTimeItemModel _model;

	public Sprite ActiveSprite => _mainImageActive.sprite;

	public EnvironmentType EnvironmentType => _model.EnvironmentType;

	public bool IsUse => _model.IsUse;

	public Observable<Unit> OnClick => _interactableUI.GetComponent<Button>().OnClickAsObservable();

	public void Initialize(EnvironmentTimeItemModel model)
	{
		_model = model;
		var (sprite, sprite2) = _iconDB.GetMainButtonIcon(model.EnvironmentType);
		_mainImage.sprite = sprite;
		_mainImageActive.sprite = sprite2;
		ReapplyModel();
	}

	public void ReapplyModel(bool isForceDeactivation = false)
	{
		_lockObj.SetActive(_model.IsLocked);
		_interactableUI.gameObject.SetActive(!_model.IsLocked);
		bool isUse = !isForceDeactivation && _model.IsUse;
		_interactableUI.SetUseUI(isUse);
		_newIcon.SetIconActive(_model.IsNew && !_model.IsLocked);
	}
}
