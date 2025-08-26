using System;
using R3;

namespace SHARP.Core
{
	public interface IModel : IDisposable
	{
		Subject<Unit> OnDispose { get; }
	}

	public class Model : IModel
	{
		readonly IDisposable _disposable = Disposable.Empty;

		bool _isDisposed = false;
		public Subject<Unit> OnDispose { get; } = new();

		public void Dispose()
		{
			if (_isDisposed) return;
			_isDisposed = true;

			_disposable.Dispose();
			OnDispose.OnNext(Unit.Default);
		}
	}
}