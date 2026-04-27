using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace Bulbul;

public class PlayerPointGetUI : MonoBehaviour, IPlayerPointGetUI
{
	[Header("全体")]
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private RectTransform _fadeInTransform;

	[SerializeField]
	private float _fadeFromPosY;

	[SerializeField]
	private float _fadeToPosY;

	[SerializeField]
	private float _fadeDuration;

	[Header("合計ポイントテキスト")]
	[SerializeField]
	private TMP_Text _pointText;

	[SerializeField]
	private float _accumulatePointDelaySeconds;

	[SerializeField]
	[Header("取得量UI")]
	private AddPointValueUI _addPointValueUIOriginal;

	[SerializeField]
	private float _addPointUIHeight;

	[SerializeField]
	private float _addPointUISpace;

	[SerializeField]
	private float _addPointFromPosY;

	[SerializeField]
	private float _addPointToPosY;

	[SerializeField]
	private float _hideDelay;

	private bool isPlaying;

	private readonly List<int> _addPoints = new List<int>();

	private int _basePoint;

	private List<AddPointValueUI> _addPointValueUIList;

	private ObjectPool<AddPointValueUI> _addPointValueUIPool;

	[Inject]
	private ScenarioReader _scenarioReader;

	public void Setup(int initialPoint)
	{
		base.gameObject.SetActive(value: false);
		_canvasGroup.alpha = 0f;
		_fadeInTransform.anchoredPosition = new Vector2(_fadeInTransform.anchoredPosition.x, _fadeFromPosY);
		_pointText.SetText("{0}", initialPoint);
		_addPointValueUIList = new List<AddPointValueUI>();
		_addPointValueUIOriginal.gameObject.SetActive(value: false);
		_addPointValueUIPool = new ObjectPool<AddPointValueUI>(() => UnityEngine.Object.Instantiate(_addPointValueUIOriginal, _addPointValueUIOriginal.transform.parent), delegate(AddPointValueUI x)
		{
			x.gameObject.SetActive(value: true);
			_addPointValueUIList.Add(x);
		}, delegate(AddPointValueUI x)
		{
			x.Reset();
			x.gameObject.SetActive(value: false);
			_addPointValueUIList.Remove(x);
		}, delegate(AddPointValueUI x)
		{
			_addPointValueUIList.Remove(x);
		}, collectionCheck: true, 10, 5);
	}

	public void ChangePoint(int pointDiff, int newPoint)
	{
		if (_addPoints.Count == 0)
		{
			_basePoint = newPoint - pointDiff;
		}
		if (pointDiff > 0)
		{
			_addPoints.Add(pointDiff);
			CheckPlayQueue();
		}
	}

	private void CheckPlayQueue()
	{
		if (!isPlaying && _addPoints.Count > 0)
		{
			PlayAddAsync().Forget();
		}
	}

	private async UniTask PlayAddAsync()
	{
		isPlaying = true;
		_pointText.SetText("{0}", _basePoint);
		_fadeInTransform.anchoredPosition = new Vector2(_fadeInTransform.anchoredPosition.x, _fadeFromPosY);
		_canvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: true);
		await DOTween.Sequence().Append(_canvasGroup.DOFade(1f, _fadeDuration)).Join(_fadeInTransform.DOAnchorPosY(_fadeToPosY, _fadeDuration));
		int totalPointAdd = 0;
		float elapsed = 0f;
		while (elapsed < _accumulatePointDelaySeconds)
		{
			await UniTask.Yield();
			if (_addPoints.Count > 0)
			{
				foreach (int addPoint in _addPoints)
				{
					totalPointAdd += addPoint;
					AddPointValueUI(addPoint);
				}
				_addPoints.Clear();
				elapsed = 0f;
			}
			else
			{
				elapsed += Time.deltaTime;
			}
		}
		foreach (AddPointValueUI valueUI in _addPointValueUIList)
		{
			valueUI.EndAnim(delegate
			{
				_addPointValueUIPool.Release(valueUI);
			});
		}
		int result;
		int num = (int.TryParse(_pointText.text, out result) ? result : 0);
		await DOVirtual.Int(num, num + totalPointAdd, 0.5f, delegate(int val)
		{
			_pointText.SetText("{0}", val);
		});
		await UniTask.Delay(TimeSpan.FromSeconds(_hideDelay));
		await DOTween.Sequence().Append(_canvasGroup.DOFade(0f, _fadeDuration)).Join(_fadeInTransform.DOAnchorPosY(_fadeFromPosY, _fadeDuration));
		base.gameObject.SetActive(value: false);
		isPlaying = false;
		CheckPlayQueue();
	}

	private void AddPointValueUI(int point)
	{
		float num = (_addPointUIHeight + _addPointUISpace) * (float)_addPointValueUIList.Count;
		num *= -1f;
		_addPointValueUIPool.Get().StartAnim(toPosY: _addPointToPosY + num, point: point, fromPosY: _addPointFromPosY);
	}
}
