using System;
using NestopiSystem;
using NestopiSystem.DIContainers;
using R3;
using TMPro;
using UnityEngine;
using VContainer;

namespace Bulbul;

[RequireComponent(typeof(TMP_Text))]
[DefaultExecutionOrder(-1)]
public class TextLocalizationBehaviour : MonoBehaviour
{
	[SerializeField]
	private bool autoBind = true;

	[Inject]
	private LanguageSupplier languageSupplier;

	[Inject]
	private LocalizationMasterWrapper localizationMaster;

	[Inject]
	private FontSupplier fontSupplier;

	private bool isInitialized;

	private string localizeID;

	private TMP_Text _text;

	private Func<string, string> formatter;

	public TMP_Text Text
	{
		get
		{
			if (!(_text != null))
			{
				return _text = GetComponent<TMP_Text>();
			}
			return _text;
		}
	}

	public string LocalizeID => localizeID;

	private void Awake()
	{
		InitializeIfNeeded();
		if (autoBind)
		{
			if (localizeID.IsNullOrEmpty())
			{
				localizeID = Text.text;
			}
			Set();
		}
	}

	private void InitializeIfNeeded()
	{
		if (!isInitialized)
		{
			if (languageSupplier == null)
			{
				languageSupplier = ProjectLifetimeScope.Resolve<LanguageSupplier>();
			}
			if (localizationMaster == null)
			{
				localizationMaster = ProjectLifetimeScope.Resolve<LocalizationMasterWrapper>();
			}
			if ((object)fontSupplier == null)
			{
				fontSupplier = ProjectLifetimeScope.Resolve<FontSupplier>();
			}
			FontSet(languageSupplier.Get());
			languageSupplier.Language.Skip(1).Subscribe(this, delegate(GameLanguageType _, TextLocalizationBehaviour @this)
			{
				@this.Set();
			}).AddTo(this);
			isInitialized = true;
		}
	}

	private void Set()
	{
		if (!string.IsNullOrEmpty(localizeID))
		{
			Set(languageSupplier.Get(), localizeID);
		}
	}

	private void Set(GameLanguageType language, string localizeID)
	{
		localizationMaster.Bind(Text, localizeID, language, formatter);
		FontSet(language);
		this.localizeID = localizeID;
	}

	public bool Set(string localizeID, Func<string, string> formatter = null)
	{
		InitializeIfNeeded();
		bool result = localizationMaster.Bind(Text, localizeID, formatter);
		this.localizeID = localizeID;
		this.formatter = formatter;
		return result;
	}

	private void FontSet(GameLanguageType language)
	{
		Material fontMaterial = fontSupplier.GetFontMaterial(Text, language);
		Text.font = fontSupplier.GetFontAsset(language);
		Text.fontMaterial = fontMaterial;
	}
}
