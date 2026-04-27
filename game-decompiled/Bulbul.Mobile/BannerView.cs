using DG.Tweening;
using UnityEngine;

namespace Bulbul.Mobile;

public class BannerView : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup[] _contents;

	[SerializeField]
	private CanvasGroup[] _pageDots;

	[SerializeField]
	private Vector2 _toAnchoredPos;

	[SerializeField]
	private Vector2 _fromAnchoredPos;

	private RectTransform[] _contentRects;

	private GameObject[] _contentGameObjs;

	private Sequence _sequence;

	private int _curIdx;

	private int _length;

	private Sequence _dotSequence;

	public int ContentsSum => _contents.Length;

	public void Setup()
	{
		_length = _contents.Length;
		_contentRects = new RectTransform[_contents.Length];
		_contentGameObjs = new GameObject[_contents.Length];
		for (int i = 0; i < _contents.Length; i++)
		{
			_contentRects[i] = _contents[i].transform as RectTransform;
			_contentGameObjs[i] = _contents[i].gameObject;
		}
	}

	public void SetBannerIdx(int idx)
	{
		_sequence?.Kill();
		for (int i = 0; i < _contents.Length; i++)
		{
			_contents[i].alpha = 0f;
			_contents[i].blocksRaycasts = false;
			if (_contentGameObjs[i].activeSelf)
			{
				_contentGameObjs[i].SetActive(value: false);
			}
		}
		_contents[idx].alpha = 1f;
		_contents[idx].blocksRaycasts = true;
		_contentGameObjs[idx].SetActive(value: true);
		_contentRects[idx].anchoredPosition = _toAnchoredPos;
		_curIdx = idx;
		_dotSequence?.Kill();
		CanvasGroup[] pageDots = _pageDots;
		for (int j = 0; j < pageDots.Length; j++)
		{
			pageDots[j].alpha = 0f;
		}
		_pageDots[idx].alpha = 1f;
	}

	public void MoveNextBanner()
	{
		_sequence.Complete();
		_sequence = DOTween.Sequence();
		int nextIdx = _curIdx + 1;
		nextIdx %= _length;
		UpdateDots(_curIdx, nextIdx);
		_contentRects[nextIdx].anchoredPosition = _fromAnchoredPos;
		_contents[nextIdx].alpha = 0f;
		_contents[nextIdx].blocksRaycasts = false;
		_contentGameObjs[nextIdx].SetActive(value: true);
		_sequence.Join(_contentRects[nextIdx].DOAnchorPosX(_toAnchoredPos.x, 0.2f).SetEase(Ease.InQuart));
		_sequence.Join(_contents[nextIdx].DOFade(1f, 0.2f).SetEase(Ease.InQuart));
		_sequence.Join(_contents[_curIdx].DOFade(0f, 0.2f).SetEase(Ease.OutQuint));
		_sequence.OnComplete(delegate
		{
			_contents[nextIdx].blocksRaycasts = true;
			_contentGameObjs[_curIdx].SetActive(value: false);
			_contents[_curIdx].blocksRaycasts = false;
			_curIdx = nextIdx;
		});
	}

	public void MoveBackBanner()
	{
		_sequence.Complete();
		_sequence = DOTween.Sequence();
		int backIdx = _curIdx - 1;
		if (backIdx < 0)
		{
			backIdx = _length - 1;
		}
		UpdateDots(_curIdx, backIdx);
		_contentRects[backIdx].anchoredPosition = -_fromAnchoredPos;
		_contents[backIdx].alpha = 0f;
		_contents[backIdx].blocksRaycasts = false;
		_contentGameObjs[backIdx].SetActive(value: true);
		_sequence.Join(_contentRects[backIdx].DOAnchorPosX(_toAnchoredPos.x, 0.2f).SetEase(Ease.InQuart));
		_sequence.Join(_contents[backIdx].DOFade(1f, 0.2f).SetEase(Ease.InQuart));
		_sequence.Join(_contents[_curIdx].DOFade(0f, 0.2f).SetEase(Ease.OutQuint));
		_sequence.OnComplete(delegate
		{
			_contents[backIdx].blocksRaycasts = true;
			_contentGameObjs[_curIdx].SetActive(value: false);
			_contents[_curIdx].blocksRaycasts = false;
			_curIdx = backIdx;
		});
	}

	private void UpdateDots(int beforeIdx, int afterIdx)
	{
		_dotSequence?.Complete();
		_dotSequence = DOTween.Sequence();
		_dotSequence.Join(_pageDots[beforeIdx].DOFade(0f, 0.2f));
		_dotSequence.Join(_pageDots[afterIdx].DOFade(1f, 0.2f));
	}
}
