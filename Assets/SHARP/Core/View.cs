using System;
using R3;
using Reflex.Attributes;
using Reflex.Extensions;
using UnityEngine;

namespace SHARP.Core
{
	public abstract class View<VM> : MonoBehaviour, IDisposable
		where VM : IViewModel
	{
		[Inject] protected readonly VM _viewModel;
		[Inject] readonly ISharpCoordinator _coordinator;

		IDisposable _disposable = Disposable.Empty;

		protected virtual void OnEnable()
		{
			_coordinator
				.For<VM>()
				.Get(gameObject.scene.GetSceneContainer());

			_disposable = Subscribe();
		}

		protected virtual void OnDisable()
		{
			_disposable.Dispose();
		}

		protected abstract void HandleSubscriptions(VM viewModel, DisposableBuilder d);

		IDisposable Subscribe()
		{
			var d = Disposable.CreateBuilder();

			HandleSubscriptions(_viewModel, d);

			return d.Build();
		}

		public void Dispose()
		{
			_disposable.Dispose();
		}
	}
}