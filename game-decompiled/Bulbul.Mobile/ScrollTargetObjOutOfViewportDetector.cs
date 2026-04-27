using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class ScrollTargetObjOutOfViewportDetector : MonoBehaviour
{
	[SerializeField]
	[Header("スクロール範囲外検知対象\u3000回転・スケーリングは未対応必ず1にしておくこと")]
	private RectTransform _target;

	[SerializeField]
	[Header("スクロールのビューポート\u3000スケーリングは未対応必ず１にしておくこと")]
	private RectTransform _viewport;

	[SerializeField]
	[Header("スクロールのコンテンツ\u3000スケーリングは未対応必ず１にしておくこと")]
	private RectTransform _content;

	[SerializeField]
	[Header("動いている間は検知しないか trueで検知しない")]
	private bool _isDetectionScrollStopped;

	[SerializeField]
	[Header("動いていると判定する閾値")]
	private float _stoppedThresold;

	[SerializeField]
	private bool isCheckHorizontal;

	[SerializeField]
	private bool isCheckVertical;

	private Subject<bool> _onNoticeDetetion = new Subject<bool>();

	private Vector2 _contentPrevPos;

	private bool? _isCurrentState;

	public Observable<bool> OnNoticeDetetion => _onNoticeDetetion;

	private void OnDestroy()
	{
		_onNoticeDetetion.Dispose();
	}

	public void Update()
	{
		if (CheckMoving())
		{
			return;
		}
		Vector3 position = _target.position;
		Vector3 position2 = _viewport.position;
		Vector2 vector = new Vector2(_target.rect.width * _target.lossyScale.x, _target.rect.height * _target.lossyScale.y);
		Vector2 vector2 = new Vector2(_viewport.rect.width * _viewport.lossyScale.x, _viewport.rect.height * _viewport.lossyScale.y);
		if (isCheckVertical)
		{
			float num = position2.y + vector2.y * (1f - _viewport.pivot.y);
			float num2 = position2.y - vector2.y * _viewport.pivot.y;
			float num3 = position.y + vector.y * (1f - _target.pivot.y);
			if (position.y - vector.y * _target.pivot.y > num || num3 < num2)
			{
				if (!_isCurrentState.HasValue)
				{
					_isCurrentState = true;
					_onNoticeDetetion.OnNext(value: true);
				}
				else if (!_isCurrentState.Value)
				{
					_isCurrentState = true;
					_onNoticeDetetion.OnNext(value: true);
				}
				return;
			}
		}
		if (isCheckHorizontal)
		{
			float num4 = position2.x - vector2.x * _viewport.pivot.x;
			float num5 = position2.x + vector2.x * (1f - _viewport.pivot.x);
			float num6 = position.x - vector.x * _target.pivot.x;
			if (position.x + vector.x * (1f - _target.pivot.x) > num4 || num6 < num5)
			{
				if (!_isCurrentState.HasValue)
				{
					_isCurrentState = true;
					_onNoticeDetetion.OnNext(value: true);
				}
				else if (!_isCurrentState.Value)
				{
					_isCurrentState = true;
					_onNoticeDetetion.OnNext(value: true);
				}
				return;
			}
		}
		if (!_isCurrentState.HasValue)
		{
			_isCurrentState = false;
			_onNoticeDetetion.OnNext(value: false);
		}
		else if (_isCurrentState.Value)
		{
			_isCurrentState = false;
			_onNoticeDetetion.OnNext(value: false);
		}
	}

	private bool CheckMoving()
	{
		if (!_isDetectionScrollStopped)
		{
			return false;
		}
		float num = _content.anchoredPosition.x - _contentPrevPos.x;
		float num2 = _content.anchoredPosition.y - _contentPrevPos.y;
		_contentPrevPos = _content.anchoredPosition;
		if (num > _stoppedThresold || num < 0f - _stoppedThresold)
		{
			return true;
		}
		if (num2 > _stoppedThresold || num2 < 0f - _stoppedThresold)
		{
			return true;
		}
		return false;
	}
}
