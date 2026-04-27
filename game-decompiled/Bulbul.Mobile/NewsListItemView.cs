using Bulbul.Web;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class NewsListItemView : MonoBehaviour
{
	[SerializeField]
	private Button _cellButton;

	[SerializeField]
	private TextMeshProUGUI _releaseDateText;

	[SerializeField]
	private TextMeshProUGUI _noticeTitleText;

	[SerializeField]
	[Header("Using状態を表示する用\u3000なければ実行しない")]
	private InteractableUI _interactableUI;

	private Subject<NewsData> onClickNotice = new Subject<NewsData>();

	private NewsListItemModel itemModel;

	public Subject<NewsData> OnClickNotice => onClickNotice;

	private void OnDestroy()
	{
		onClickNotice?.Dispose();
	}

	public void Setup()
	{
		ObservableSubscribeExtensions.Subscribe(_cellButton.OnClickAsObservable(), delegate
		{
			onClickNotice?.OnNext(itemModel.NewsData);
		}).AddTo(this);
	}

	public void SetModel(NewsListItemModel model)
	{
		itemModel = model;
		_releaseDateText.SetText(itemModel.NewsData.StartDate.ToString("yyyy/MM/dd HH:mm"));
		_noticeTitleText.SetText(itemModel.NewsData.Title);
		SetActiveUseImage(model.IsSelected);
	}

	public void SetActiveUseImage(bool active)
	{
		if (!(_interactableUI == null))
		{
			_interactableUI.SetUseUI(active, isDoComplete: true);
		}
	}
}
