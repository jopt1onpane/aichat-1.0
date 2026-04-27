using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class EnvironmentTimeAutoItemView : MonoBehaviour
{
	[Inject]
	private FontSupplier _fontSupplier;

	[Inject]
	private LanguageSupplier _languageSupplier;

	[SerializeField]
	private TextMeshProUGUI _autoText;

	[SerializeField]
	private Button _autoButton;

	[SerializeField]
	private Color _activeColor;

	[SerializeField]
	private Color _deactiveColor;

	[SerializeField]
	private FontMaterialType _activeMat;

	[SerializeField]
	private FontMaterialType _deactiveMat;

	public Observable<Unit> OnClickAuto => _autoButton.OnClickAsObservable();

	public void SetUse(bool isUse)
	{
		_autoText.color = (isUse ? _activeColor : _deactiveColor);
		_ = _languageSupplier.Language.CurrentValue;
		_autoText.fontMaterial = GetMaterial(isUse ? _activeMat : _deactiveMat);
	}

	private Material GetMaterial(FontMaterialType materialType)
	{
		return materialType switch
		{
			FontMaterialType.Normal => _fontSupplier.GetFontNormalMaterial(_languageSupplier.Language.CurrentValue), 
			FontMaterialType.Gradation => _fontSupplier.GetFontGradationMaterial(_languageSupplier.Language.CurrentValue), 
			_ => null, 
		};
	}
}
