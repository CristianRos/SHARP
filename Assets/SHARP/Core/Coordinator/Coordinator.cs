using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Core;
using UnityEngine;

namespace SHARP.Core
{
	public class Coordinator<VM> : ICoordinator<VM>
		where VM : IViewModel
	{
		#region Fields

		protected List<VM> _active_ViewModels = new();
		protected List<VM> _orphan_ViewModels = new();

		// Without Context
		protected List<IView> _views_WithoutContext = new();
		protected List<VM> _viewModels_WithoutContext = new();
		public Dictionary<IView, VM> _viewModelsByView_WithoutContext = new();
		public Dictionary<VM, IView> _viewsByViewModel_WithoutContext = new();

		// With Context
		protected Dictionary<string, List<IView>> _viewsByContext = new();
		protected Dictionary<IView, string> _contextByView = new();

		protected Dictionary<string, VM> _viewModelByContext = new();
		protected Dictionary<VM, string> _contextByViewModel = new();

		public bool IsDisposed { get; private set; } = false;

		#endregion


		#region Public API Methods

		public virtual List<VM> GetAll() => _active_ViewModels.ToList();

		public virtual VM Get(IView<VM> view, string withContext, Container withContainer)
		{
			// Return a new instance of a ViewModel if there is no context
			if (string.IsNullOrEmpty(withContext))
			{
				Debug.Log($"Getting a new instance of {typeof(VM)}");
				return CreateViewModelWithoutContext(view, withContainer);
			}

			// Return the ViewModel with the context if it exists
			if (_viewModelByContext.TryGetValue(withContext, out var vm))
			{
				Debug.Log($"Getting {typeof(VM)} with context {withContext}");
				AddViewToContext(view, withContext);
				return vm;
			}

			// Return a new instance of a ViewModel with the specified context
			Debug.Log($"Getting a new instance of {typeof(VM)} with context {withContext}");
			return CreateViewModelWithContext(view, withContext, withContainer);
		}

		// Handles if the view should be rebound to a new context or an existing context
		public virtual VM CoordinateRebind<V>(V view, VM toVM, Container withContainer)
			where V : IView<VM>
		{
			var fromContext = view.Context;

			if (_contextByViewModel.TryGetValue(toVM, out var toContext))
			{
				return RebindToContext(view, fromContext, toContext, withContainer);
			}

			return ConvertToContextualViewModel(view, toVM, fromContext, withContainer);
		}

		public VM RebindToContext(IView<VM> view, string fromContext, string toContext, Container withContainer)
		{
			var currentViewModel = view.ViewModel.CurrentValue;

			if (string.IsNullOrEmpty(fromContext))
			{
				OrphanViewModel(currentViewModel);
				return Get(view, toContext, withContainer);
			}

			if (fromContext == toContext)
			{
				Debug.LogWarning($"Trying to rebind to the same context {toContext} for {view.GetType()}, returning current view model");
				return view.ViewModel.CurrentValue;
			}

			UnregisterView(view, fromContext);
			return Get(view, toContext, withContainer);
		}

		public void UnregisterView(IView<VM> view, string context)
		{
			if (IsDisposed) return;

			UnregisterFromContextTracking(view, context);
			UnregisterFromWithoutContextTracking(view);
		}

		public virtual void Dispose()
		{
			if (IsDisposed)
			{
				Debug.LogWarning($"Tried to dispose the coordinator twice, ignoring this call");
				return;
			}

			Debug.Log($"Disposing Coordinator<{typeof(VM)}>");
			IsDisposed = true;

			ClearAllTrackingData();
		}

		#endregion


		#region ViewModel Creation Methods

		VM CreateViewModelWithoutContext(IView<VM> view, Container withContainer)
		{
			var viewModel = withContainer.Resolve<VM>();
			_active_ViewModels.Add(viewModel);

			_views_WithoutContext.Add(view);
			_viewModels_WithoutContext.Add(viewModel);
			_viewModelsByView_WithoutContext.Add(view, viewModel);
			_viewsByViewModel_WithoutContext.Add(viewModel, view);

			return viewModel;
		}

		VM CreateViewModelWithContext(IView<VM> view, string withContext, Container withContainer)
		{
			var contextViewModel = withContainer.Resolve<VM>();

			_active_ViewModels.Add(contextViewModel);
			_viewModelByContext.Add(withContext, contextViewModel);
			_contextByViewModel.Add(contextViewModel, withContext);
			AddViewToContext(view, withContext);

			return contextViewModel;
		}

		VM ConvertToContextualViewModel(IView<VM> view, VM toVM, string fromContext, Container withContainer)
		{
			if (!_viewsByViewModel_WithoutContext.TryGetValue(toVM, out var targetView))
			{
				throw new InvalidOperationException($"Cannot find View associated with ViewModel {toVM.GetType()}");
			}

			var newContext = $"__TransientContext__{Guid.NewGuid()}";

			RemoveFromWithoutContextTracking(toVM, targetView);
			AddToContextTracking(toVM, targetView, newContext);

			return RebindToContext(view, fromContext, newContext, withContainer);
		}

		#endregion


		#region Context Management Methods

		void AddViewToContext(IView<VM> view, string toContext)
		{
			if (_contextByView.TryGetValue(view, out var currentContext))
			{
				throw new InvalidOperationException($"View {view.GetType()} already registered with context {currentContext}");
			}

			if (_viewsByContext.TryGetValue(toContext, out var views))
			{
				views.Add(view);
			}
			else
			{
				_viewsByContext.Add(toContext, new List<IView> { view });
			}

			_contextByView.Add(view, toContext);
			view.Context = toContext;
		}

		void AddToContextTracking(VM viewModel, IView targetView, string context)
		{
			_contextByView.Add(targetView, context);
			_viewsByContext.Add(context, new List<IView> { targetView });
			_viewModelByContext.Add(context, viewModel);
			_contextByViewModel.Add(viewModel, context);
		}

		// TODO: Check if properly removing from lists and not the whole list
		void RemoveFromWithoutContextTracking(VM viewModel, IView view)
		{
			_viewModels_WithoutContext.Remove(viewModel);
			_viewsByViewModel_WithoutContext.Remove(viewModel);
			_viewModelsByView_WithoutContext.Remove(view);
			_views_WithoutContext.Remove(view);
			_contextByView.Remove(view);
		}

		#endregion


		#region Cleanup Methods

		private void UnregisterFromContextTracking(IView<VM> view, string context)
		{
			_contextByView.Remove(view);
			if (_viewsByContext.TryGetValue(context, out var views))
			{
				views.Remove(view);

				// If no more views in this context, cleanup the ViewModel
				if (views.Count == 0)
				{
					_viewsByContext.Remove(context);
					if (_viewModelByContext.TryGetValue(context, out var vm))
					{
						_viewModelByContext.Remove(context);
						_contextByViewModel.Remove(vm);
						OrphanViewModel(vm);
					}
				}
			}
		}

		void UnregisterFromWithoutContextTracking(IView<VM> view)
		{
			if (_views_WithoutContext.Contains(view))
			{
				_views_WithoutContext.Remove(view);
				if (_viewModelsByView_WithoutContext.TryGetValue(view, out var vm))
				{
					_viewModelsByView_WithoutContext.Remove(view);
					_viewsByViewModel_WithoutContext.Remove(vm);
					OrphanViewModel(vm);
				}
			}
		}

		void OrphanViewModel(VM viewModel)
		{
			if (!_active_ViewModels.Remove(viewModel))
				return;

			_orphan_ViewModels.Add(viewModel);

			// Clean up without context references
			if (_viewsByViewModel_WithoutContext.TryGetValue(viewModel, out var view))
			{
				_viewsByViewModel_WithoutContext.Remove(viewModel);
				_viewModelsByView_WithoutContext.Remove(view);
				_views_WithoutContext.Remove(view);
			}
		}

		void ClearAllTrackingData()
		{
			// ViewModel lifecycle
			_active_ViewModels.Clear();
			_orphan_ViewModels.Clear();

			// Context-based tracking
			_viewModelByContext.Clear();
			_contextByViewModel.Clear();
			_viewsByContext.Clear();
			_contextByView.Clear();

			// Without-context tracking
			_views_WithoutContext.Clear();
			_viewModels_WithoutContext.Clear();
			_viewModelsByView_WithoutContext.Clear();
			_viewsByViewModel_WithoutContext.Clear();
		}

		#endregion
	}
}