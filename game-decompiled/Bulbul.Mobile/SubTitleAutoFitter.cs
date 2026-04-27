using System;
using Cysharp.Text;
using R3;
using TMPro;
using UnityEngine;

namespace Bulbul.Mobile;

public class SubTitleAutoFitter : MonoBehaviour
{
	[Serializable]
	public struct FontSetting
	{
		public string name;

		public float MaxFontSize;

		public float MinFontSize;

		public float CharacterSpace;
	}

	private readonly string _init = "";

	[SerializeField]
	private TextMeshProUGUI _targetText;

	[SerializeField]
	private FontSetting[] _fontSettings;

	[SerializeField]
	private float _defaultTextBoxHeight;

	[SerializeField]
	private float _checkFontSizeInterval;

	[SerializeField]
	[Header("計算用 描画されないような位置に配置する")]
	private TextMeshProUGUI _dummyText;

	private string _diffCheckStr;

	private Subject<Vector2> _onLayoutChangedSize = new Subject<Vector2>();

	private RectTransform _targetTextRectTransform;

	private FontSetting _currentFontSetting;

	private bool _isRequestedLayout;

	public Observable<Vector2> OnLayoutChangedSize => _onLayoutChangedSize;

	private void Awake()
	{
		_diffCheckStr = _init;
		_targetTextRectTransform = _targetText.GetComponent<RectTransform>();
		_targetText.autoSizeTextContainer = false;
		_targetText.overflowMode = TextOverflowModes.Overflow;
		_targetText.enableWordWrapping = true;
	}

	private void OnDestroy()
	{
		_onLayoutChangedSize.Dispose();
	}

	private void Update()
	{
		if (_isRequestedLayout || !_diffCheckStr.Equals(_targetText.text))
		{
			_isRequestedLayout = false;
			ChangeFontSetting();
			_targetTextRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1000f);
			_targetText.fontSize = _currentFontSetting.MaxFontSize;
			_targetText.characterSpacing = _currentFontSetting.CharacterSpace;
			_diffCheckStr = _targetText.text;
			Layout();
		}
	}

	private void ChangeFontSetting()
	{
		switch (SaveDataManager.Instance.SettingData.GameLanguage.Value)
		{
		case GameLanguageType.Japanese:
		case GameLanguageType.ChineseSimplified:
		case GameLanguageType.ChineseTraditional:
		case GameLanguageType.Korean:
			_currentFontSetting = _fontSettings[0];
			break;
		case GameLanguageType.English:
		case GameLanguageType.Portuguese:
		case GameLanguageType.Russian:
			_currentFontSetting = _fontSettings[1];
			break;
		}
	}

	private void Layout()
	{
		float defaultTextBoxHeight = _defaultTextBoxHeight;
		float num = _targetText.fontSize;
		float num2 = 0f;
		while (true)
		{
			float preferredHeight = _targetText.preferredHeight;
			if (preferredHeight <= defaultTextBoxHeight)
			{
				num2 = preferredHeight;
				break;
			}
			num -= _checkFontSizeInterval;
			if (num <= _currentFontSetting.MinFontSize)
			{
				_targetText.fontSize = num;
				num2 = _targetText.preferredHeight;
				break;
			}
			_targetText.fontSize = num;
		}
		_targetTextRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num2);
		float x = CalcWidth();
		_onLayoutChangedSize.OnNext(new Vector2(x, _targetText.preferredHeight));
	}

	private float CalcWidth()
	{
		using Utf8ValueStringBuilder arg = ZString.CreateUtf8StringBuilder(notNested: true);
		Span<TMP_CharacterInfo> span = MemoryExtensions.AsSpan(_targetText.textInfo.characterInfo);
		_dummyText.fontSize = _targetText.fontSize;
		_dummyText.characterSpacing = _targetText.characterSpacing;
		_targetText.ForceMeshUpdate();
		float num = 0f;
		for (int i = 0; i < _targetText.textInfo.lineCount; i++)
		{
			TMP_LineInfo tMP_LineInfo = _targetText.textInfo.lineInfo[i];
			int firstCharacterIndex = tMP_LineInfo.firstCharacterIndex;
			int characterCount = tMP_LineInfo.characterCount;
			Span<TMP_CharacterInfo> span2 = span.Slice(firstCharacterIndex, characterCount);
			arg.Clear();
			Span<TMP_CharacterInfo> span3 = span2;
			for (int j = 0; j < span3.Length; j++)
			{
				TMP_CharacterInfo tMP_CharacterInfo = span3[j];
				arg.Append(tMP_CharacterInfo.character);
			}
			_dummyText.SetText(arg);
			float preferredWidth = _dummyText.preferredWidth;
			if (preferredWidth > num)
			{
				num = preferredWidth;
			}
		}
		return num;
	}

	public void RequestLayout()
	{
		_isRequestedLayout = true;
	}
}
