using DG.Tweening;
using UnityEngine;

namespace Bulbul.Mobile;

public class FacilitiesUIGroupContentsView : MonoBehaviour
{
	public enum ContentType
	{
		Note,
		Todo,
		Calendar,
		HabitTracker
	}

	private static readonly float _fadeDurationSec = 0.25f;

	[SerializeField]
	[Header("タブで切り替える要素 enumの順に入れてください")]
	private CanvasGroup[] _contents;

	private ITabChangedSetuper[] _contentSetupers;

	private ContentType _currentContentType;

	private CanvasGroup _currentContent;

	private Sequence _fadeSequence;

	private bool _isChanging;

	private GameObject _currentFadeInObj;

	private GameObject _currentFadeOutObj;

	public bool IsChanging => _isChanging;

	public void Setup()
	{
		_contentSetupers = new ITabChangedSetuper[_contents.Length];
		for (int i = 0; i < _contents.Length; i++)
		{
			_contents[i].TryGetComponent<ITabChangedSetuper>(out _contentSetupers[i]);
		}
	}

	public void ChangeContents(ContentType type, bool isChangedFromTab = true)
	{
		_isChanging = true;
		_fadeSequence?.Kill();
		_fadeSequence = DOTween.Sequence();
		int hashCode = type.GetHashCode();
		int hashCode2 = _currentContentType.GetHashCode();
		_contentSetupers[hashCode]?.SetupBeforeTabChanged(isChangedFromTab);
		_contents[hashCode].alpha = 0f;
		_fadeSequence.Join(_contents[hashCode].DOFade(1f, _fadeDurationSec));
		if (_currentContent != null && _currentContentType != type)
		{
			_fadeSequence.Join(_contents[hashCode2].DOFade(0f, _fadeDurationSec));
			_currentContent.blocksRaycasts = false;
			_currentFadeOutObj = _currentContent.gameObject;
		}
		else
		{
			_currentFadeOutObj = null;
		}
		_currentContentType = type;
		_currentContent = _contents[hashCode];
		_currentContent.blocksRaycasts = false;
		_currentContent.alpha = 0f;
		_currentContent.gameObject.SetActive(value: true);
		_currentFadeInObj = _contents[hashCode].gameObject;
		_fadeSequence.OnComplete(delegate
		{
			_isChanging = false;
			_currentContent.blocksRaycasts = true;
			if (_currentFadeOutObj != null)
			{
				_currentFadeOutObj.SetActive(value: false);
			}
		});
	}

	public void ChangeContentsImmediate(ContentType type, bool isChangedFromTab = false)
	{
		_fadeSequence?.Kill();
		CanvasGroup[] contents = _contents;
		foreach (CanvasGroup obj in contents)
		{
			obj.alpha = 0f;
			obj.blocksRaycasts = false;
			obj.gameObject.SetActive(value: false);
		}
		_contentSetupers[type.GetHashCode()]?.SetupBeforeTabChanged(isChangedFromTab);
		_currentContentType = type;
		_currentContent = _contents[type.GetHashCode()];
		_currentContent.alpha = 1f;
		_currentContent.blocksRaycasts = true;
		_currentContent.gameObject.SetActive(value: true);
		_isChanging = false;
		_currentFadeInObj = null;
		_currentFadeOutObj = null;
	}
}
