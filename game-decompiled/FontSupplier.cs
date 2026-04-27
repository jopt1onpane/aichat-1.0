using Bulbul;
using TMPro;
using UnityEngine;

public class FontSupplier : MonoBehaviour
{
	[SerializeField]
	[Header("日本語フォント")]
	private TMP_FontAsset fontAssetJapanese;

	[SerializeField]
	[Header("英語フォント")]
	private TMP_FontAsset fontAssetEnglish;

	[SerializeField]
	[Header("中国語（簡体字）フォント")]
	private TMP_FontAsset fontAssetChineseSimplified;

	[SerializeField]
	[Header("中国語（繁体字）フォント")]
	private TMP_FontAsset fontAssetChineseTraditional;

	[SerializeField]
	[Header("韓国語フォント")]
	private TMP_FontAsset fontAssetKorean;

	[SerializeField]
	[Header("ロシア語フォント")]
	private TMP_FontAsset fontAssetRussian;

	[SerializeField]
	[Header("日本語通常マテリアル")]
	private Material materialNormalForJapanese;

	[SerializeField]
	[Header("英語通常マテリアル")]
	private Material materialNormalForEnglish;

	[SerializeField]
	[Header("中国語（簡体字）通常マテリアル")]
	private Material materialNormalForChineseSimplified;

	[SerializeField]
	[Header("中国語（繁体字）通常マテリアル")]
	private Material materialNormalForChineseTraditional;

	[SerializeField]
	[Header("韓国語通常マテリアル")]
	private Material materialNormalForKorean;

	[SerializeField]
	[Header("ロシア語通常マテリアル")]
	private Material materialNormalForRussian;

	[SerializeField]
	[Header("日本語グラデーションマテリアル")]
	private Material materialGradationForJapanese;

	[SerializeField]
	[Header("英語グラデーションマテリアル")]
	private Material materialGradationForEnglish;

	[SerializeField]
	[Header("中国語（簡体字）グラデーションマテリアル")]
	private Material materialGradationForChineseSimplified;

	[SerializeField]
	[Header("中国語（繁体字）グラデーションマテリアル")]
	private Material materialGradationForChineseTraditional;

	[SerializeField]
	[Header("韓国語グラデーションマテリアル")]
	private Material materialGradationForKorean;

	[SerializeField]
	[Header("ロシア語グラデーションマテリアル")]
	private Material materialGradationForRussian;

	[SerializeField]
	[Header("日本語アンダーレイマテリアル")]
	private Material materialUnderlayForJapanese;

	[SerializeField]
	[Header("英語アンダーレイマテリアル")]
	private Material materialUnderlayForEnglish;

	[SerializeField]
	[Header("中国語（簡体字）アンダーレイマテリアル")]
	private Material materialUnderlayForChineseSimplified;

	[SerializeField]
	[Header("中国語（繁体字）アンダーレイマテリアル")]
	private Material materialUnderlayForChineseTraditional;

	[SerializeField]
	[Header("韓国語アンダーレイマテリアル")]
	private Material materialUnderlayForKorean;

	[SerializeField]
	[Header("ロシア語アンダーレイマテリアル")]
	private Material materialUnderlayForRussian;

	public TMP_FontAsset GetFontAsset(GameLanguageType language)
	{
		TMP_FontAsset result = null;
		switch (language)
		{
		case GameLanguageType.Japanese:
			result = fontAssetJapanese;
			break;
		case GameLanguageType.English:
			result = fontAssetEnglish;
			break;
		case GameLanguageType.ChineseSimplified:
			result = fontAssetChineseSimplified;
			break;
		case GameLanguageType.ChineseTraditional:
			result = fontAssetChineseTraditional;
			break;
		case GameLanguageType.Portuguese:
			result = fontAssetEnglish;
			break;
		case GameLanguageType.Korean:
			result = fontAssetKorean;
			break;
		case GameLanguageType.Russian:
			result = fontAssetRussian;
			break;
		default:
			Debug.LogError($"{language}のフォントアセットは設定されていません。設定してください。");
			break;
		}
		return result;
	}

	public Material GetFontMaterial(TMP_Text text, GameLanguageType language)
	{
		FontMaterialType fontMaterialType = (IsGradationCurrentText(text) ? FontMaterialType.Gradation : FontMaterialType.Normal);
		if (fontMaterialType == FontMaterialType.Normal && IsUndarlayCurrentText(text))
		{
			fontMaterialType = FontMaterialType.Underlay;
		}
		Material result = null;
		switch (fontMaterialType)
		{
		case FontMaterialType.Normal:
			result = GetFontNormalMaterial(language);
			break;
		case FontMaterialType.Gradation:
			result = GetFontGradationMaterial(language);
			break;
		case FontMaterialType.Underlay:
			result = GetFontUnderlayMaterial(language);
			break;
		default:
			Debug.LogError($"{fontMaterialType}は定義されていません。");
			break;
		}
		return result;
	}

	public Material GetFontNormalMaterial(GameLanguageType language)
	{
		Material result = null;
		switch (language)
		{
		case GameLanguageType.Japanese:
			result = materialNormalForJapanese;
			break;
		case GameLanguageType.English:
			result = materialNormalForEnglish;
			break;
		case GameLanguageType.ChineseSimplified:
			result = materialNormalForChineseSimplified;
			break;
		case GameLanguageType.ChineseTraditional:
			result = materialNormalForChineseTraditional;
			break;
		case GameLanguageType.Portuguese:
			result = materialNormalForEnglish;
			break;
		case GameLanguageType.Korean:
			result = materialNormalForKorean;
			break;
		case GameLanguageType.Russian:
			result = materialNormalForRussian;
			break;
		default:
			Debug.LogError($"{language}のフォントアセットは設定されていません。設定してください。");
			break;
		}
		return result;
	}

	public Material GetFontGradationMaterial(GameLanguageType language)
	{
		Material result = null;
		switch (language)
		{
		case GameLanguageType.Japanese:
			result = materialGradationForJapanese;
			break;
		case GameLanguageType.English:
			result = materialGradationForEnglish;
			break;
		case GameLanguageType.ChineseSimplified:
			result = materialGradationForChineseSimplified;
			break;
		case GameLanguageType.ChineseTraditional:
			result = materialGradationForChineseTraditional;
			break;
		case GameLanguageType.Portuguese:
			result = materialGradationForEnglish;
			break;
		case GameLanguageType.Korean:
			result = materialGradationForKorean;
			break;
		case GameLanguageType.Russian:
			result = materialGradationForRussian;
			break;
		default:
			Debug.LogError($"{language}のフォントアセットは設定されていません。設定してください。");
			break;
		}
		return result;
	}

	public Material GetFontUnderlayMaterial(GameLanguageType language)
	{
		Material result = null;
		switch (language)
		{
		case GameLanguageType.Japanese:
			result = materialUnderlayForJapanese;
			break;
		case GameLanguageType.English:
			result = materialUnderlayForEnglish;
			break;
		case GameLanguageType.ChineseSimplified:
			result = materialUnderlayForChineseSimplified;
			break;
		case GameLanguageType.ChineseTraditional:
			result = materialUnderlayForChineseTraditional;
			break;
		case GameLanguageType.Portuguese:
			result = materialUnderlayForEnglish;
			break;
		case GameLanguageType.Korean:
			result = materialUnderlayForKorean;
			break;
		case GameLanguageType.Russian:
			result = materialUnderlayForRussian;
			break;
		default:
			Debug.LogError($"{language}のフォントアセットは設定されていません。設定してください。");
			break;
		}
		return result;
	}

	private bool IsGradationCurrentText(TMP_Text text)
	{
		if (text.fontMaterial.name.Contains("FX"))
		{
			return true;
		}
		return false;
	}

	private bool IsUndarlayCurrentText(TMP_Text text)
	{
		if (text.fontMaterial.name.Contains("Underlay"))
		{
			return true;
		}
		return false;
	}
}
