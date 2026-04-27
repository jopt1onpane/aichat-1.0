using DG.Tweening;
using UnityEngine;

namespace Bulbul.Mobile;

public class RemovingModeUI : MonoBehaviour, ITwoStateUITransition
{
	private static float _transitionDurationSec = 0.25f;

	[SerializeField]
	[Header("必ず表示されるオブジェクトのルート")]
	private RectTransform _requiredDisplayObjRoot;

	[SerializeField]
	[Header("通常モード\u3000_requiredDisplayTextObjRoot 位置")]
	private Vector2 _normalModeRequiredDisplayTextObjRootAnchoredPos;

	[SerializeField]
	[Header("削除モード _requiredDisplayTextObjRoot\u3000位置")]
	private Vector2 _removingModeRequiredDisplayTextObjRootAnchoredPos;

	[SerializeField]
	[Header("削除ボタンオブジェクトルート")]
	private CanvasGroup _removingButtonObjRoot;

	[SerializeField]
	[Header("削除モードで消すCanvasGroup")]
	private CanvasGroup[] _removingModeDeactivationRoots;

	private bool _isTransitioning;

	private Sequence _sequence;

	public bool IsTransitioning => _isTransitioning;

	public void Transition(bool isRemoving)
	{
		_isTransitioning = true;
		if (isRemoving)
		{
			TransitionRemoving();
		}
		else
		{
			TransitionNormal();
		}
	}

	private void TransitionNormal()
	{
		_sequence?.Kill();
		_sequence = DOTween.Sequence();
		ExecBeforeTransitioning();
		_sequence.Join(_requiredDisplayObjRoot.DOAnchorPos(_normalModeRequiredDisplayTextObjRootAnchoredPos, _transitionDurationSec));
		_sequence.Join(_removingButtonObjRoot.DOFade(0f, _transitionDurationSec));
		CanvasGroup[] removingModeDeactivationRoots = _removingModeDeactivationRoots;
		foreach (CanvasGroup target in removingModeDeactivationRoots)
		{
			_sequence.Join(target.DOFade(1f, _transitionDurationSec));
		}
		_sequence.OnComplete(delegate
		{
			_removingButtonObjRoot.gameObject.SetActive(value: false);
			_isTransitioning = false;
			ExecAfterTransitioning();
		});
	}

	private void TransitionRemoving()
	{
		_sequence?.Kill();
		_sequence = DOTween.Sequence();
		ExecBeforeTransitioning();
		_sequence.Join(_requiredDisplayObjRoot.DOAnchorPos(_removingModeRequiredDisplayTextObjRootAnchoredPos, _transitionDurationSec));
		_sequence.Join(_removingButtonObjRoot.DOFade(1f, _transitionDurationSec));
		CanvasGroup[] removingModeDeactivationRoots = _removingModeDeactivationRoots;
		foreach (CanvasGroup target in removingModeDeactivationRoots)
		{
			_sequence.Join(target.DOFade(0f, _transitionDurationSec));
		}
		_sequence.OnComplete(delegate
		{
			SetActiveRemovingModeDeactivationRoots(active: false);
			_isTransitioning = false;
			ExecAfterTransitioning();
		});
	}

	private void ExecBeforeTransitioning()
	{
		SetActiveRemovingModeDeactivationRoots(active: true);
		_removingButtonObjRoot.gameObject.SetActive(value: true);
		SetBlockRaycastRemovingModeDeactivationRoots(isBlock: false);
		_removingButtonObjRoot.blocksRaycasts = false;
	}

	private void ExecAfterTransitioning()
	{
		SetBlockRaycastRemovingModeDeactivationRoots(isBlock: true);
		_removingButtonObjRoot.blocksRaycasts = true;
		_sequence = null;
	}

	private void SetActiveRemovingModeDeactivationRoots(bool active)
	{
		CanvasGroup[] removingModeDeactivationRoots = _removingModeDeactivationRoots;
		foreach (CanvasGroup canvasGroup in removingModeDeactivationRoots)
		{
			if (canvasGroup.gameObject.activeSelf != active)
			{
				canvasGroup.gameObject.SetActive(active);
			}
		}
	}

	private void SetBlockRaycastRemovingModeDeactivationRoots(bool isBlock)
	{
		CanvasGroup[] removingModeDeactivationRoots = _removingModeDeactivationRoots;
		for (int i = 0; i < removingModeDeactivationRoots.Length; i++)
		{
			removingModeDeactivationRoots[i].blocksRaycasts = isBlock;
		}
	}

	public void TransitionImmediate(bool isRemoving)
	{
		Transition(isRemoving);
		_sequence.Complete();
	}
}
