using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class NoteContentListItemView : MonoBehaviour, IAnimationView, IRemovableItemView
{
	public static Action<ulong, bool, bool> OnClickSelectButtonAction;

	public static Action<ulong> OnClickRemoveButtonAction;

	[SerializeField]
	private RectTransform _contents;

	[SerializeField]
	private TextMeshProUGUI _noteTitleText;

	[SerializeField]
	private Button _selectButton;

	[SerializeField]
	private Button _removeButton;

	[SerializeField]
	private ItemDragReorderHandle _dragHandle;

	[SerializeField]
	private float _draggingScale = 0.8f;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private GameObject _blockRaycaster;

	[SerializeField]
	private GameObject[] _draggingActives;

	private ITwoStateUITransition __removingModeUI;

	private Vector2 _initAncPos;

	private RectTransform _rectTransform;

	private CancellationTokenSource _removingAnimationCancellationTokenSoruce = new CancellationTokenSource();

	public ITwoStateUITransition RemovingModeUI
	{
		get
		{
			if (__removingModeUI == null)
			{
				__removingModeUI = GetComponent<ITwoStateUITransition>();
			}
			return __removingModeUI;
		}
	}

	public Observable<Unit> OnClickSelectButton => _selectButton.OnClickAsObservable();

	public ulong PageUniqueId { get; set; }

	public RectTransform RectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = base.transform as RectTransform;
			}
			return _rectTransform;
		}
	}

	public ItemDragReorderHandle DragHandle => _dragHandle;

	CanvasGroup IAnimationView.AnimationCanvasGroup => _canvasGroup;

	RectTransform IAnimationView.AnimationRectTransform => RectTransform;

	CancellationToken IRemovableItemView.CancellationToken => _removingAnimationCancellationTokenSoruce.Token;

	private void Awake()
	{
		_initAncPos = RectTransform.anchoredPosition;
	}

	public void InitPos()
	{
		RectTransform.anchoredPosition = _initAncPos;
	}

	public void InitAlpha()
	{
		_canvasGroup.alpha = 1f;
	}

	public void ButtonSetup()
	{
		_selectButton.onClick.RemoveAllListeners();
		_selectButton.onClick.AddListener(delegate
		{
			OnClickSelectButtonAction?.Invoke(PageUniqueId, arg2: false, arg3: false);
		});
		_removeButton.onClick.RemoveAllListeners();
		_removeButton.onClick.AddListener(delegate
		{
			OnClickRemoveButtonAction?.Invoke(PageUniqueId);
		});
	}

	public void CreateRemovingAnimationCancellatonToken()
	{
		_removingAnimationCancellationTokenSoruce = new CancellationTokenSource();
	}

	public void SetActive(bool active)
	{
		if (_contents.gameObject.activeSelf != active)
		{
			_contents.gameObject.SetActive(active);
		}
	}

	public void SetTitleText(string title)
	{
		_noteTitleText.SetText(title);
	}

	public void SetActiveRaycastBlocker(bool isActive)
	{
		if (_blockRaycaster.activeSelf != isActive)
		{
			_blockRaycaster.SetActive(isActive);
		}
	}

	public void ChangeRemovingMode(bool isRemovingMode, bool isImmediate = false)
	{
		if (!isImmediate)
		{
			RemovingModeUI.Transition(isRemovingMode);
		}
		else
		{
			RemovingModeUI.TransitionImmediate(isRemovingMode);
		}
		_selectButton.enabled = !isRemovingMode;
		_selectButton.interactable = !isRemovingMode;
	}

	public void CancelRemovingAnimation()
	{
		if (_removingAnimationCancellationTokenSoruce != null)
		{
			_removingAnimationCancellationTokenSoruce.Cancel();
			_removingAnimationCancellationTokenSoruce.Dispose();
			_removingAnimationCancellationTokenSoruce = null;
		}
	}

	async UniTask IRemovableItemView.Play(ListItemViewAnimations.RemoveAnimationDirection direction, CancellationToken token)
	{
		try
		{
			await ListItemViewAnimations.PlayRemovingAnimation(this, token, TweenCancelBehaviour.Kill, direction);
		}
		catch (OperationCanceledException ex)
		{
			throw ex;
		}
	}

	public void SetActiveDraggingObjs(bool active)
	{
		GameObject[] draggingActives = _draggingActives;
		for (int i = 0; i < draggingActives.Length; i++)
		{
			draggingActives[i].SetActive(active);
		}
	}

	public void SetDraggingScale(bool isDragging)
	{
		float num = (isDragging ? _draggingScale : 1f);
		base.transform.localScale = new Vector3(num, num, 1f);
	}
}
