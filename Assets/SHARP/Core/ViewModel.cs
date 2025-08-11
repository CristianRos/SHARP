using System;
using R3;
using UnityEngine;

namespace SHARP.Core
{
	public interface IViewModel : IDisposable { }

	public abstract class ViewModel : IViewModel
	{
		#region Fields

		IDisposable _disposable = Disposable.Empty;
		bool _disposed;

		#endregion


		public ViewModel()
		{
			Subscribe();
		}


		#region Subscriptions

		void Subscribe()
		{
			var d = Disposable.CreateBuilder();

			HandleSubscriptions(ref d);

			_disposable = d.Build();
		}

		protected abstract void HandleSubscriptions(ref DisposableBuilder d);

		#endregion


		#region Cleanup

		public void Dispose()
		{
			if (_disposed)
			{
				Debug.LogWarning($"Tried to dispose {GetType()} twice, ignoring this call.");
				return;
			}

			_disposable.Dispose();
		}

		#endregion
	}
}