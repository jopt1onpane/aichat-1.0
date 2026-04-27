using Bulbul.Web;

namespace Bulbul.Mobile;

public class NewsListItemModel
{
	public NewsData NewsData { get; private set; }

	public bool IsSelected { get; set; }

	public NewsListItemModel(NewsData newsData)
	{
		NewsData = newsData;
	}
}
