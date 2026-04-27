using System.Collections.Generic;
using Bulbul;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class SpecialSelectCellView : MonoBehaviour
{
	private Color32 GrayOutColor = new Color32(124, 124, 124, 124);

	protected const float ProgressBarMaxWidth = 406f;

	[SerializeField]
	[Header("共通Serialize要素")]
	protected SpecialSelectViewBaseSerializeVariable _baseVariable;

	[SerializeField]
	[Header("アイコン通常")]
	private Sprite _iconNormalSprite;

	[SerializeField]
	[Header("アイコン使用時")]
	private Sprite _iconActiveSprite;

	[SerializeField]
	[Header("アイコン グレーアウト用")]
	private Sprite _iconGrayOutSprite;

	private List<InteractableUI> _readIconInteractableUIs = new List<InteractableUI>();

	public Observable<Unit> OnSubmit => _baseVariable.SelectButton.OnClickAsObservable();

	public void Setup(int storyMaxCount, ReactiveProperty<int> readEpisodeCount)
	{
		_baseVariable.FontMaterialChanger.Setup();
		_baseVariable.InteractableUI.Setup();
		_baseVariable.IconNormalImage.sprite = _iconNormalSprite;
		_baseVariable.IconActiveImage.sprite = _iconActiveSprite;
		_readIconInteractableUIs.Clear();
		for (int i = 0; i < storyMaxCount; i++)
		{
			InteractableUI component = Object.Instantiate(_baseVariable.ReadInteractableUIPrefab, _baseVariable.ReadInteractableUIParent).GetComponent<InteractableUI>();
			component.Setup();
			_readIconInteractableUIs.Add(component);
		}
		_baseVariable.ProgressBar.Setup(GetCurrentProgress(), GetMaxProgress());
		readEpisodeCount.Subscribe(delegate(int readCount)
		{
			UpdateReadIcon(readCount);
		}).AddTo(this);
	}

	public void OnAddValue()
	{
		_baseVariable.ProgressBar.OnAddValue(GetCurrentProgress()).Forget();
	}

	protected virtual float GetCurrentProgress()
	{
		return 0f;
	}

	protected virtual float GetMaxProgress()
	{
		return 0f;
	}

	public void UpdateReadIcon(int readCount)
	{
		for (int i = 0; i < _readIconInteractableUIs.Count; i++)
		{
			if (i < readCount)
			{
				_readIconInteractableUIs[i].ActivateUseUI(isUseDoComplete: true);
			}
			else
			{
				_readIconInteractableUIs[i].DeactivateUseUI(isUseDoComplete: true);
			}
		}
	}

	public void Activate()
	{
		ViewToNormal();
		_baseVariable.InteractableUI.ActivateUseUI();
		_baseVariable.FontMaterialChanger.ActivateUse();
	}

	public void Deactivate()
	{
		ViewToNormal();
		_baseVariable.InteractableUI.DeactivateAllUI();
		_baseVariable.FontMaterialChanger.DeactivateUse();
	}

	public void GrayOut()
	{
		_baseVariable.BackImage.sprite = _baseVariable.BackgroundGrayOutSprite;
		_baseVariable.IconNormalImage.sprite = _iconGrayOutSprite;
		_baseVariable.LocalizationText.Text.color = GrayOutColor;
		_baseVariable.InteractableUI.enabled = false;
		_baseVariable.HoldButtonAnim.enabled = false;
		_baseVariable.FontMaterialChanger.enabled = false;
	}

	private void ViewToNormal()
	{
		_baseVariable.BackImage.sprite = _baseVariable.BackgroundNormalSprite;
		_baseVariable.IconNormalImage.sprite = _iconNormalSprite;
		_baseVariable.LocalizationText.Text.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		_baseVariable.InteractableUI.enabled = true;
		_baseVariable.HoldButtonAnim.enabled = true;
		_baseVariable.FontMaterialChanger.enabled = true;
	}
}
