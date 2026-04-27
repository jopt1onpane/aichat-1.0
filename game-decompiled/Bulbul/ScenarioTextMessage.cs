using System;
using System.Collections.Generic;
using Febucci.UI;
using Febucci.UI.Core;
using NestopiSystem.DIContainers;
using TMPro;
using UnityEngine;
using VContainer;

namespace Bulbul;

[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(TypewriterCore))]
public class ScenarioTextMessage : MonoBehaviour
{
	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private LanguageSupplier _languageSupplier;

	[SerializeField]
	private TextMeshProUGUI _textMessage;

	[SerializeField]
	private TypewriterByCharacter _typewriterByCharacter;

	private float _initWaitForNormalChars;

	private readonly List<Action> _onTextShowedList = new List<Action>();

	private readonly List<Action> _callbacksForLoop = new List<Action>();

	private void Awake()
	{
		_initWaitForNormalChars = _typewriterByCharacter.waitForNormalChars;
		_typewriterByCharacter.onTextShowed.AddListener(OnTextShowed);
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		_onTextShowedList.Clear();
		_callbacksForLoop.Clear();
	}

	private void OnTextShowed()
	{
		_callbacksForLoop.Clear();
		_callbacksForLoop.AddRange(_onTextShowedList);
		foreach (Action item in _callbacksForLoop)
		{
			try
			{
				item();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public void ClearText()
	{
		_textMessage.text = string.Empty;
	}

	public void StartText(string text)
	{
		if (_masterDataLoader == null)
		{
			_masterDataLoader = ProjectLifetimeScope.Resolve<MasterDataLoader>();
		}
		if (_languageSupplier == null)
		{
			_languageSupplier = ProjectLifetimeScope.Resolve<LanguageSupplier>();
		}
		_typewriterByCharacter.waitForNormalChars = _initWaitForNormalChars;
		_typewriterByCharacter.waitForNormalChars = _masterDataLoader.TalkSpeedData.GetSpeed(_languageSupplier.Get());
		_textMessage.SetText(text);
	}

	public void DisplayAllText()
	{
		_typewriterByCharacter.waitForNormalChars = 0.01f;
	}

	public void AddOnTextShowedCallback(Action callback)
	{
		if (_onTextShowedList.Contains(callback))
		{
			Debug.LogError(callback?.Method.Name + " is already added");
		}
		else
		{
			_onTextShowedList.Add(callback);
		}
	}

	public void ClearOnTextShowedCallback()
	{
		_onTextShowedList.Clear();
	}
}
