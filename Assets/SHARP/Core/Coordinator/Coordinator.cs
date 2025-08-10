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
		protected Dictionary<string, List<IView>> _viewsByContext = new();
		protected Dictionary<IView, string> _contextByView = new();

		protected List<VM> _activeViewModels = new();
		protected List<VM> _orphanViewModels = new();
		protected Dictionary<string, VM> _viewModelByContext = new();
		protected Dictionary<VM, string> _contextByViewModel = new();

		protected List<IView> _viewsWithoutContext = new();
		protected List<VM> _activeViewModelsWithoutContext = new();
		public Dictionary<IView, VM> _viewModelsByViewWithoutContext = new();
		public Dictionary<VM, IView> _viewByViewModelWithoutContext = new();

		public bool IsDisposed { get; private set; } = false;


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

		void OrphanViewModel(VM viewModel)
		{
			_activeViewModels.Remove(viewModel);
			_orphanViewModels.Add(viewModel);
		}

		public virtual List<VM> GetAll() => _activeViewModels.ToList();

		public virtual VM Get(IView<VM> view, string withContext, Container withContainer)
		{
			// Return a new instance of a ViewModel if there is no context
			if (string.IsNullOrEmpty(withContext))
			{
				Debug.Log($"Getting a new instance of {typeof(VM)}");

				var viewModel = withContainer.Resolve<VM>();
				_activeViewModels.Add(viewModel);

				_viewsWithoutContext.Add(view);
				_activeViewModelsWithoutContext.Add(viewModel);
				_viewModelsByViewWithoutContext.Add(view, viewModel);
				_viewByViewModelWithoutContext.Add(viewModel, view);

				return viewModel;
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

			var contextViewModel = withContainer.Resolve<VM>();

			_activeViewModels.Add(contextViewModel);
			_viewModelByContext.Add(withContext, contextViewModel);
			_contextByViewModel.Add(contextViewModel, withContext);
			AddViewToContext(view, withContext);

			return contextViewModel;
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

			if (!_viewByViewModelWithoutContext.TryGetValue(toVM, out var targetView))
			{
				throw new InvalidOperationException($"Cannot find view associated with ViewModel {toVM.GetType()}");
			}

			var newContext = $"__TransientContext__{Guid.NewGuid()}";

			// Clear toVM and the related view from the coordinator
			_activeViewModelsWithoutContext.Remove(toVM);
			_viewByViewModelWithoutContext.Remove(toVM);
			_viewModelsByViewWithoutContext.Remove(targetView);

			// Give new context to toVM and the related view
			_contextByView.Add(targetView, newContext);
			_viewsByContext.Add(newContext, new List<IView> { targetView });

			_viewModelByContext.Add(newContext, toVM);
			_contextByViewModel.Add(toVM, newContext);

			return RebindToContext(view, fromContext, newContext, withContainer);
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

			_contextByView.Remove(view);
			if (_viewsByContext.TryGetValue(context, out var views))
			{
				views.Remove(view);
				return;
			}
			if (_viewsWithoutContext.Contains(view))
			{
				_viewsWithoutContext.Remove(view);
				_viewModelsByViewWithoutContext.Remove(view);
				_viewByViewModelWithoutContext.Remove(view.ViewModel.CurrentValue);
			}
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

			_activeViewModels.Clear();
			_orphanViewModels.Clear();

			_viewModelByContext.Clear();
			_contextByViewModel.Clear();

			_viewsByContext.Clear();
			_contextByView.Clear();
		}
	}
}