using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class TalkSpeedData
{
	[Header("テキストの文字表示待機秒数")]
	[SerializeField]
	[Header("日本")]
	private float TalkTextWaitSecondsJp;

	[SerializeField]
	[Header("英語")]
	private float TalkTextWaitSecondsEn;

	[SerializeField]
	[Header("簡体字")]
	private float TalkTextWaitSecondsCS;

	[SerializeField]
	[Header("繁体字")]
	private float TalkTextWaitSecondsCT;

	[SerializeField]
	[Header("ポルトガル語")]
	private float TalkTextWaitSecondsPt;

	[SerializeField]
	[Header("韓国語")]
	private float TalkTextWaitSecondsKo;

	[SerializeField]
	[Header("ロシア語")]
	private float TalkTextWaitSecondsRu;

	public float GetSpeed(GameLanguageType language)
	{
		float result = TalkTextWaitSecondsJp;
		switch (language)
		{
		case GameLanguageType.Japanese:
			result = TalkTextWaitSecondsJp;
			break;
		case GameLanguageType.English:
			result = TalkTextWaitSecondsEn;
			break;
		case GameLanguageType.ChineseSimplified:
			result = TalkTextWaitSecondsCS;
			break;
		case GameLanguageType.ChineseTraditional:
			result = TalkTextWaitSecondsCT;
			break;
		case GameLanguageType.Portuguese:
			result = TalkTextWaitSecondsPt;
			break;
		case GameLanguageType.Korean:
			result = TalkTextWaitSecondsKo;
			break;
		case GameLanguageType.Russian:
			result = TalkTextWaitSecondsRu;
			break;
		default:
			Debug.LogError($"{language}のテキスト速度は定義されていません。");
			break;
		}
		return result;
	}
}
