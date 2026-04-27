using Bulbul.Mobile;
using Bulbul.Web;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class SettingNewsView : MonoBehaviour
{
	[SerializeField]
	private NewsListView _newsListView;

	[SerializeField]
	private GameObject _noNewsObj;

	[SerializeField]
	private GameObject[] _noNewstHideObjs;

	[Header("ここから選択されたお知らせのUI")]
	[SerializeField]
	private TextMeshProUGUI _titleText;

	[SerializeField]
	private TextMeshProUGUI _releaseDateText;

	[SerializeField]
	private TextMeshProUGUI _mainText;

	[SerializeField]
	private ScrollRect _scroll;

	[SerializeField]
	private CanvasGroup _contentsCanvasGroup;

	private bool _isInitialized;

	public void Setup()
	{
		if (!_isInitialized)
		{
			Hide();
			_newsListView.OnClickNewsCell.Subscribe(delegate(NewsData data)
			{
				_scroll.verticalNormalizedPosition = 1f;
				_releaseDateText.SetText(data.StartDate.ToString("yyyy/MM/dd HH:mm"));
				_titleText.text = data.Title;
				_mainText.text = data.MainText;
			}).AddTo(this);
			_isInitialized = true;
		}
	}

	public void Show()
	{
		_contentsCanvasGroup.alpha = 1f;
		_contentsCanvasGroup.blocksRaycasts = true;
	}

	public void Hide()
	{
		_contentsCanvasGroup.alpha = 0f;
		_contentsCanvasGroup.blocksRaycasts = false;
	}

	public void SetNewsData(NewsData[] newsData)
	{
		_newsListView.SetNewsData(newsData);
		_newsListView.SelectTop();
		bool flag = newsData == null || newsData.Length == 0;
		SetActiveNoAnnouncementsObj(flag);
		bool flag2 = !flag;
		GameObject[] noNewstHideObjs = _noNewstHideObjs;
		foreach (GameObject gameObject in noNewstHideObjs)
		{
			if (gameObject.activeSelf != flag2)
			{
				gameObject.SetActive(flag2);
			}
		}
	}

	private void SetActiveNoAnnouncementsObj(bool active)
	{
		if (_noNewsObj.activeSelf != active)
		{
			_noNewsObj.SetActive(active);
		}
	}
}
