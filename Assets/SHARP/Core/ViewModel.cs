using System;
using R3;
using UnityEngine;

namespace SHARP.Core
{
	public interface IViewModel : IDisposable { }

	public abstract class ViewModel : IViewModel
	{
		IDisposable _disposable = Disposable.Empty;
		bool _disposed;

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
			if (_disposed)
			{
				Debug.LogWarning($"Tried to dispose {GetType()} twice, ignoring this call.");
				return;
			}

			_disposable.Dispose();
		}
	}
}