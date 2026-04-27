using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulbul;

[RequireComponent(typeof(Button))]
public class ButtonEventObservable : MonoBehaviour
{
	private Button button;

	private Subject<Unit> onClick;

	private Subject<BaseEventData> onSubmit;

	private Subject<BaseEventData> onSelect;

	private Subject<BaseEventData> onDeselect;

	private Subject<AxisEventData> onMove;

	private Subject<PointerEventData> onPointerClick;

	private Subject<PointerEventData> onPointerEnter;

	private Subject<PointerEventData> onPointerExit;

	private Subject<PointerEventData> onPointerDown;

	private Subject<PointerEventData> onPointerUp;

	public Button View => button ?? (button = GetComponent<Button>());

	public Observable<Unit> OnClick => OnClickAsObservable();

	public Observable<BaseEventData> OnSubmit => OnSubmitAsObservable();

	public Observable<BaseEventData> OnSelect => OnSelectAsObservable();

	public Observable<BaseEventData> OnDeselect => OnDeselectAsObservable();

	public Observable<AxisEventData> OnMove => OnMoveAsObservable();

	public Observable<PointerEventData> OnPointerClick => OnPointerClickAsObservable();

	public Observable<PointerEventData> OnPointerEnter => OnPointerEnterAsObservable();

	public Observable<PointerEventData> OnPointerExit => OnPointerExitAsObservable();

	public Observable<PointerEventData> OnPointerDown => OnPointerDownAsObservable();

	public Observable<PointerEventData> OnPointerUp => OnPointerUpAsObservable();

	private Observable<Unit> OnClickAsObservable()
	{
		if (onClick != null)
		{
			return onClick;
		}
		onClick = new Subject<Unit>();
		View.OnClickAsObservable().Subscribe(onClick).AddTo(this);
		return onClick;
	}

	private Observable<BaseEventData> OnSubmitAsObservable()
	{
		if (onSubmit != null)
		{
			return onSubmit;
		}
		onSubmit = new Subject<BaseEventData>();
		View.OnSubmitAsObservable().Subscribe(onSubmit).AddTo(this);
		return onSubmit;
	}

	private Observable<BaseEventData> OnSelectAsObservable()
	{
		if (onSelect != null)
		{
			return onSelect;
		}
		onSelect = new Subject<BaseEventData>();
		View.OnSelectAsObservable().Subscribe(onSelect).AddTo(this);
		return onSelect;
	}

	private Observable<BaseEventData> OnDeselectAsObservable()
	{
		if (onDeselect != null)
		{
			return onDeselect;
		}
		onDeselect = new Subject<BaseEventData>();
		View.OnDeselectAsObservable().Subscribe(onDeselect).AddTo(this);
		return onDeselect;
	}

	private Observable<AxisEventData> OnMoveAsObservable()
	{
		if (onMove != null)
		{
			return onMove;
		}
		onMove = new Subject<AxisEventData>();
		View.OnMoveAsObservable().Subscribe(onMove).AddTo(this);
		return onMove;
	}

	private Observable<PointerEventData> OnPointerClickAsObservable()
	{
		if (onPointerClick != null)
		{
			return onPointerClick;
		}
		onPointerClick = new Subject<PointerEventData>();
		View.OnPointerClickAsObservable().Subscribe(onPointerClick).AddTo(this);
		return onPointerClick;
	}

	private Observable<PointerEventData> OnPointerEnterAsObservable()
	{
		if (onPointerEnter != null)
		{
			return onPointerEnter;
		}
		onPointerEnter = new Subject<PointerEventData>();
		View.OnPointerEnterAsObservable().Subscribe(onPointerEnter).AddTo(this);
		return onPointerEnter;
	}

	private Observable<PointerEventData> OnPointerExitAsObservable()
	{
		if (onPointerExit != null)
		{
			return onPointerExit;
		}
		onPointerExit = new Subject<PointerEventData>();
		View.OnPointerExitAsObservable().Subscribe(onPointerExit).AddTo(this);
		return onPointerExit;
	}

	private Observable<PointerEventData> OnPointerDownAsObservable()
	{
		if (onPointerDown != null)
		{
			return onPointerDown;
		}
		onPointerDown = new Subject<PointerEventData>();
		View.OnPointerDownAsObservable().Subscribe(onPointerDown).AddTo(this);
		return onPointerDown;
	}

	private Observable<PointerEventData> OnPointerUpAsObservable()
	{
		if (onPointerUp != null)
		{
			return onPointerUp;
		}
		onPointerUp = new Subject<PointerEventData>();
		View.OnPointerUpAsObservable().Subscribe(onPointerUp).AddTo(this);
		return onPointerUp;
	}

	private void OnDestroy()
	{
		onClick?.Dispose();
		onSubmit?.Dispose();
		onSelect?.Dispose();
		onDeselect?.Dispose();
		onMove?.Dispose();
		onPointerClick?.Dispose();
		onPointerEnter?.Dispose();
		onPointerExit?.Dispose();
		onPointerDown?.Dispose();
		onPointerUp?.Dispose();
	}
}
