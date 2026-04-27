using System;
using System.Collections.Generic;

namespace Bulbul.Web;

public class NewsDataEqualityComparer : IEqualityComparer<NewsData>
{
	public static readonly NewsDataEqualityComparer Default = new NewsDataEqualityComparer();

	public bool Equals(NewsData x, NewsData y)
	{
		if (x.ID == y.ID && x.StartDate.Equals(y.StartDate))
		{
			return x.EndDate.Equals(y.EndDate);
		}
		return false;
	}

	public int GetHashCode(NewsData obj)
	{
		return HashCode.Combine(obj.ID, obj.StartDate, obj.EndDate);
	}
}
