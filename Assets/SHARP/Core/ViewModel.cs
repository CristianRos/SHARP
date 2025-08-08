using System;
using R3;

namespace SHARP.Core
{
	public interface IViewModel : IDisposable { }

	public abstract class ViewModel : IViewModel
	{
		IDisposable _disposable = Disposable.Empty;

		public ViewModel()
		{
			Subscribe();
		}

		void Subscribe()
		{
			var d = Disposable.CreateBuilder();

			HandleSubscriptions(d);

			_disposable = d.Build();
		}

		protected abstract void HandleSubscriptions(DisposableBuilder d);

		public void Dispose()
		{
			_disposable.Dispose();
		}
	}
}