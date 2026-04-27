using Bulbul.Web;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class SettingNewsView : MonoBehaviour
{
	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private FacilityAnimationBase _animation;

	[SerializeField]
	private GameObject _newsNothingText;

	[SerializeField]
	private NewsListView _newsListView;

	[SerializeField]
	private NewsDialog _newsDialog;

	public Observable<Unit> OnClickClose => _closeButton.OnClickAsObservable();

	public Observable<Unit> OnActivateAnimationCompleted => _animation.OnCompleteActivate;

	public Observable<Unit> OnDeactivateAnimationCompleted => _animation.OnCompleteDeactivate;

	public Observable<NewsData> OnOpenDialog => _newsListView.OnClickNewsCell;

	public Observable<Unit> OnCloseDialog => _newsDialog.OnClose;

	public void Setup()
	{
		_animation.Setup();
		_newsDialog.Setup();
		_newsNothingText.SetActive(value: false);
	}

	public void Activate()
	{
		if (_animation == null)
		{
			base.gameObject.SetActive(value: true);
			return;
		}
		_animation.Activate().Forget();
		_newsListView.ResetNewsData();
	}

	public void Deactivate()
	{
		if (_animation == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			_animation.Deactivate().Forget();
		}
	}

	public void SetNewsData(NewsData[] newsDatas)
	{
		if (newsDatas.Length == 0)
		{
			_newsNothingText.SetActive(value: true);
			_newsListView.ResetNewsData();
		}
		else
		{
			_newsNothingText.SetActive(value: false);
			_newsListView.SetNewsData(newsDatas);
		}
	}

	public void OpenNewsDialog(NewsData newsData)
	{
		_newsDialog.SetNewsData(newsData);
		_newsDialog.Activate();
	}

	public void CloseNewsDialog()
	{
		_newsDialog.Deactivate();
	}
}
