using System;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NestopiSystem;

public static class ObservableExtensions
{
	public static IDisposable BindGate(this Button button, ReactiveProperty<bool> gate)
	{
		return gate.Subscribe(button, delegate(bool interactable, Button button2)
		{
			button2.interactable = interactable;
		});
	}

	public static IDisposable BindGate(this TMP_InputField inputField, ReactiveProperty<bool> gate)
	{
		return gate.Subscribe(inputField, delegate(bool interactable, TMP_InputField button)
		{
			button.interactable = interactable;
		});
	}

	public static Observable<PointerEventData> OnPointerEnterAsObservable(this IPointerEnterHandler pointerEnterHandler)
	{
		if (!(pointerEnterHandler is Component component) || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return component.GetOrAddComponent<ObservablePointerEnterTrigger>().OnPointerEnterAsObservable();
	}

	public static Observable<PointerEventData> OnPointerExitAsObservable(this IPointerExitHandler pointerExitHandler)
	{
		if (!(pointerExitHandler is Component component) || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return component.GetOrAddComponent<ObservablePointerExitTrigger>().OnPointerExitAsObservable();
	}

	public static Observable<PointerEventData> OnPointerDownAsObservable(this IPointerEnterHandler pointerEnterHandler)
	{
		if (!(pointerEnterHandler is Component component) || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return component.GetOrAddComponent<ObservablePointerDownTrigger>().OnPointerDownAsObservable();
	}

	public static Observable<PointerEventData> OnPointerUpAsObservable(this IPointerEnterHandler pointerEnterHandler)
	{
		if (!(pointerEnterHandler is Component component) || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return component.GetOrAddComponent<ObservablePointerUpTrigger>().OnPointerUpAsObservable();
	}

	public static Observable<PointerEventData> OnDragAsObservable(this IBeginDragHandler beginDragHandler)
	{
		if (!(beginDragHandler is Component component) || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return component.GetOrAddComponent<ObservableDragTrigger>().OnDragAsObservable();
	}

	public static Observable<PointerEventData> OnBeginDragAsObservable(this IBeginDragHandler beginDragHandler)
	{
		if (!(beginDragHandler is Component component) || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return component.GetOrAddComponent<ObservableBeginDragTrigger>().OnBeginDragAsObservable();
	}

	public static Observable<PointerEventData> OnEndDragAsObservable(this IBeginDragHandler beginDragHandler)
	{
		if (!(beginDragHandler is Component component) || component.gameObject == null)
		{
			return Observable.Empty<PointerEventData>();
		}
		return component.GetOrAddComponent<ObservableEndDragTrigger>().OnEndDragAsObservable();
	}
}
