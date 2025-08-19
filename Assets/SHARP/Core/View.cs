using System;
using System.Reflection;
using R3;
using Reflex.Extensions;
using UnityEngine;

namespace SHARP.Core
{
	public interface IView
	{ }

	public interface IView<VM> : IView, IDisposable
		where VM : IViewModel
	{
		ReactiveProperty<VM> ViewModel { get; }
		string Context { get; set; }
	}

	public abstract class View<VM> : MonoBehaviour, IView<VM>
		where VM : IViewModel
	{
		#region Fields

		public ReactiveProperty<VM> ViewModel { get; } = new();
		protected ISharpCoordinator _coordinator;
		protected ISharpDiscovery _discovery;

		[SerializeField] string _context;
		public string Context { get => _context; set { _context = value; } }

		IDisposable _disposable = Disposable.Empty;
		bool _disposed = false;

		#endregion


		#region Unity Messages

		protected virtual void Awake()
		{
			var sceneContainer = gameObject.scene.GetSceneContainer();
			_coordinator = sceneContainer.Resolve<ISharpCoordinator>();
			_discovery = sceneContainer.Resolve<ISharpDiscovery>();

			ViewModel.Value = _coordinator.For<VM>().Get(this, Context, sceneContainer);
		}

		protected virtual void OnEnable()
		{
			ViewModel
				.Subscribe(_ => RefreshSubscriptions())
				.AddTo(this);
		}

		protected virtual void OnDisable()
		{
			_disposable.Dispose();
			_disposable = Disposable.Empty;
		}

		protected void Reset()
		{
			Context = GetType().GetCustomAttribute<ContextAttribute>()?.DefaultContext ?? "";
		}

		void OnDestroy() => Dispose();

		#endregion


		#region Subscriptions

		protected abstract void HandleSubscriptions(VM viewModel, ref DisposableBuilder d);

		void RefreshSubscriptions()
		{
			_disposable.Dispose();
			_disposable = Subscribe();
		}

		IDisposable Subscribe()
		{

			var d = Disposable.CreateBuilder();

			HandleSubscriptions(ViewModel.Value, ref d);

			return d.Build(); ;
		}

		#endregion


		#region Rebind

		protected virtual void RebindToContext(string toContext)
		{
			if (string.IsNullOrEmpty(toContext))
			{
				throw new ArgumentException("Context cannot be empty");
			}

			var sceneContainer = gameObject.scene.GetSceneContainer();

			ViewModel.Value = _coordinator.For<VM>()
				.RebindToContext(this, Context, toContext, sceneContainer);
		}

		protected virtual void Rebind(VM viewModel)
		{
			if (viewModel == null)
			{
				throw new ArgumentNullException(nameof(viewModel));
			}

			var sceneContainer = gameObject.scene.GetSceneContainer();

			ViewModel.Value = _coordinator.For<VM>().CoordinateRebind(this, viewModel, sceneContainer);
		}

		protected virtual void UnsafeRebind(VM viewModel)
		{
			if (viewModel == null)
			{
				throw new ArgumentNullException(nameof(viewModel));
			}

			ViewModel.Value = viewModel;
		}

		#endregion


		#region Cleanup

		public void Dispose()
		{
			if (_disposed)
			{
				Debug.LogWarning($"Tried to dispose {GetType()} twice, ignoring this call");
				return;
			}
			_disposed = true;

			_disposable.Dispose();
			_coordinator.For<VM>().UnregisterView(this, Context);
		}

		#endregion
	}
}