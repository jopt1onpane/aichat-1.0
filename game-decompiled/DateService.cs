using System;
using R3;

public class DateService : IDisposable
{
	private IDisposable _disposable;

	private DateTime _currentDateTime;

	private Subject<DateTime> _onChangeDate = new Subject<DateTime>();

	private Subject<DateTime> _onChangeTime = new Subject<DateTime>();

	public Observable<DateTime> OnChangeDate => _onChangeDate;

	public Observable<DateTime> OnChangeTime => _onChangeTime;

	public void Dispose()
	{
		_disposable.Dispose();
	}

	public void Setup()
	{
		_disposable = ObservableSubscribeExtensions.Subscribe(Observable.Interval(TimeSpan.FromSeconds(0.20000000298023224)), delegate
		{
			if (_currentDateTime.Day != DateTime.Now.Day)
			{
				_onChangeDate.OnNext(DateTime.Now);
			}
			_currentDateTime = DateTime.Now;
			_onChangeTime.OnNext(_currentDateTime);
		});
	}
}
