using NestopiSystem.DIContainers;
using R3;
using TMPro;
using UnityEngine;
using VContainer;

namespace Bulbul;

[DefaultExecutionOrder(-1)]
public class FontLocalizationBehaviour : MonoBehaviour
{
	[Inject]
	private LanguageSupplier languageSupplier;

	[Inject]
	private FontSupplier fontSupplier;

	private TMP_Text _text;

	private TMP_InputField _inputField;

	private TMP_InputField InputField
	{
		get
		{
			if (!(_inputField != null))
			{
				return _inputField = GetComponent<TMP_InputField>();
			}
			return _inputField;
		}
	}

	private void Awake()
	{
		TMP_InputField component2;
		if (TryGetComponent<TMP_Text>(out var component))
		{
			_text = component;
		}
		else if (TryGetComponent<TMP_InputField>(out component2))
		{
			_inputField = component2;
		}
		else
		{
			Debug.LogError("フォント変更対象が存在しません。Text、もしくはInputFieldをアタッチしてください。");
		}
		if (languageSupplier == null)
		{
			languageSupplier = ProjectLifetimeScope.Resolve<LanguageSupplier>();
		}
		if ((object)fontSupplier == null)
		{
			fontSupplier = ProjectLifetimeScope.Resolve<FontSupplier>();
		}
		languageSupplier.Language.Subscribe(this, delegate(GameLanguageType _, FontLocalizationBehaviour @this)
		{
			@this.Set();
		}).AddTo(this);
	}

	private void Set()
	{
		if (languageSupplier == null)
		{
			languageSupplier = ProjectLifetimeScope.Resolve<LanguageSupplier>();
		}
		if ((object)fontSupplier == null)
		{
			fontSupplier = ProjectLifetimeScope.Resolve<FontSupplier>();
		}
		if (_text != null)
		{
			GameLanguageType language = languageSupplier.Get();
			Material fontMaterial = fontSupplier.GetFontMaterial(_text, language);
			_text.font = fontSupplier.GetFontAsset(language);
			_text.fontMaterial = fontMaterial;
		}
		else if (_inputField != null)
		{
			GameLanguageType language2 = languageSupplier.Get();
			Material fontMaterial2 = fontSupplier.GetFontMaterial(_inputField.textComponent, language2);
			_inputField.fontAsset = fontSupplier.GetFontAsset(language2);
			_inputField.textComponent.fontMaterial = fontMaterial2;
		}
	}
}
