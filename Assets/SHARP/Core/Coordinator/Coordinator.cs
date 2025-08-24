using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace SHARP.Core
{
	public class Coordinator<VM> : ICoordinator<VM>
		where VM : IViewModel
	{
		#region Fields

		readonly HashSet<VM> _active_ViewModels = new();
		readonly HashSet<VM> _orphan_ViewModels = new();

		// Without Context
		readonly HashSet<IView<VM>> _views_WithoutContext = new();
		readonly HashSet<VM> _viewModels_WithoutContext = new();

		readonly BiDirectionalMap<IView<VM>, VM> _bi_views_viewModels_WithoutContext = new();

		// With Context
		readonly BiDirectionalSetMap<string, IView<VM>> _bi_contexts_setViews = new();

		readonly BiDirectionalMap<string, VM> _bi_contexts_viewModels = new();
		// TODO: Look at this too
		readonly HashSet<string> _keepAliveContexts = new();


		readonly ReaderWriterLockSlim _coordinatorLock = new(LockRecursionPolicy.SupportsRecursion);
		public bool IsDisposed = false;

		#endregion


		#region Public API Methods

		public IEnumerable<VM> GetActive()
		{
			_coordinatorLock.EnterReadLock();
			try
			{
				return _active_ViewModels.ToArray();
			}
			finally
			{
				_coordinatorLock.ExitReadLock();
			}
		}
		public IEnumerable<VM> GetOrphan()
		{
			_coordinatorLock.EnterReadLock();
			try
			{
				return _orphan_ViewModels.ToArray();
			}
			finally
			{
				_coordinatorLock.ExitReadLock();
			}
		}
		public IEnumerable<VM> GetAll()
		{
			_coordinatorLock.EnterReadLock();
			try
			{
				return _active_ViewModels.Concat(_orphan_ViewModels).ToArray();
			}
			finally
			{
				_coordinatorLock.ExitReadLock();
			}
		}

		public IEnumerable<IView<VM>> GetViewsWithoutContext()
		{
			_coordinatorLock.EnterReadLock();
			try
			{
				return _views_WithoutContext.ToArray();
			}
			finally
			{
				_coordinatorLock.ExitReadLock();
			}
		}
		public IEnumerable<IView<VM>> GetViewsWithContext()
		{
			_coordinatorLock.EnterReadLock();

			try
			{
				return _bi_contexts_setViews.ForwardValues
					.SelectMany(set => set)
					.ToArray();
			}
			finally
			{
				_coordinatorLock.ExitReadLock();
			}
		}

		public IEnumerable<VM> GetViewModelsWithoutContext()
		{
			_coordinatorLock.EnterReadLock();
			try
			{
				return _viewModels_WithoutContext.ToArray();
			}
			finally
			{
				_coordinatorLock.ExitReadLock();
			}
		}
		public IEnumerable<VM> GetViewModelsWithContext()
		{
			_coordinatorLock.EnterReadLock();
			try
			{
				return _bi_contexts_viewModels.Values.ToArray();
			}
			finally
			{
				_coordinatorLock.ExitReadLock();
			}
		}
		public IEnumerable<VM> GetViewModelsWithContext(Func<string, bool> contextMatcher)
		{
			_coordinatorLock.EnterReadLock();

			try
			{
				List<VM> viewModels = new();
				foreach (var (context, vm) in _bi_contexts_viewModels.Forward)
				{
					if (contextMatcher(context))
					{
						viewModels.Add(vm);
					}
				}

				return viewModels;
			}
			finally
			{
				_coordinatorLock.ExitReadLock();
			}
		}
		public VM GetViewModel(string context)
		{
			_coordinatorLock.EnterReadLock();
			try
			{
				return _bi_contexts_viewModels.TryGetValue(context, out var vm) ? vm : default;
			}
			finally
			{
				_coordinatorLock.ExitReadLock();
			}
		}

		public string GetContext(IView<VM> view)
		{
			_coordinatorLock.EnterReadLock();

			try
			{
				if (_bi_contexts_setViews.TryGetKey(view, out var ctx))
				{
					return ctx;
				}
				return null;
			}
			finally
			{
				_coordinatorLock.ExitReadLock();
			}
		}
		public string GetContext(VM viewModel)
		{
			_coordinatorLock.EnterReadLock();
			try
			{
				if (_bi_contexts_viewModels.TryGetKey(viewModel, out var ctx))
				{
					return ctx;
				}
				return null;
			}
			finally
			{
				_coordinatorLock.ExitReadLock();
			}
		}
		public IEnumerable<string> GetAllContexts()
		{
			_coordinatorLock.EnterReadLock();
			try
			{
				return _bi_contexts_viewModels.Keys.ToArray();

			}
			finally
			{
				_coordinatorLock.ExitReadLock();
			}
		}

		public VM Get(IView<VM> view, string withContext, IContainer withContainer)
		{
			_coordinatorLock.EnterWriteLock();

			try
			{
				// Return a new instance of a ViewModel if there is no context
				if (string.IsNullOrEmpty(withContext))
				{
					Debug.Log($"Getting a new instance of {typeof(VM)}");
					return CreateViewModelWithoutContext(view, withContainer);
				}

				// Return the ViewModel with the context if it exists
				if (_bi_contexts_viewModels.TryGetValue(withContext, out var vm))
				{
					Debug.Log($"Getting {typeof(VM)} with context {withContext}");
					AddViewToContext(view, withContext);
					return vm;
				}

				// Return a new instance of a ViewModel with the specified context
				Debug.Log($"Getting a new instance of {typeof(VM)} with context {withContext}");
				return CreateViewModelWithContext(view, withContext, withContainer);
			}
			finally
			{
				_coordinatorLock.ExitWriteLock();
			}
		}

		// Handles if the view should be rebound to a new context or an existing context
		public virtual void CoordinateRebind<V>(V view, VM toVM, IContainer withContainer)
			where V : IView<VM>
		{
			_coordinatorLock.EnterWriteLock();

			try
			{

				var fromContext = view.Context;

				if (_bi_contexts_viewModels.TryGetKey(toVM, out var toContext))
				{
					RebindToContext(view, fromContext, toContext, withContainer);
					return;
				}

				ConvertToContextualViewModel(view, toVM, fromContext, withContainer);
			}
			finally
			{
				_coordinatorLock.ExitWriteLock();
			}
		}

		public void RebindToContext(IView<VM> view, string fromContext, string toContext, IContainer withContainer)
		{
			_coordinatorLock.EnterWriteLock();

			try
			{

				var currentViewModel = view.ViewModel.CurrentValue;

				if (string.IsNullOrEmpty(fromContext))
				{
					OrphanViewModel(currentViewModel);

					view.ViewModel.Value = Get(view, toContext, withContainer);
					return;
				}

				if (fromContext == toContext)
				{
					Debug.LogWarning($"Trying to rebind to the same context {toContext} for {view.GetType()}, returning current view model");
					return;
				}

				UnregisterView(view, fromContext);
				view.ViewModel.Value = Get(view, toContext, withContainer);
			}
			finally
			{
				_coordinatorLock.ExitWriteLock();
			}
		}

		public void UnregisterView(IView<VM> view, string context)
		{
			_coordinatorLock.EnterWriteLock();

			try
			{
				if (IsDisposed) return;

				UnregisterFromContextTracking(view, context);
				UnregisterFromWithoutContextTracking(view);
			}
			finally
			{
				_coordinatorLock.ExitWriteLock();
			}
		}

		public virtual void Dispose()
		{
			_coordinatorLock.EnterWriteLock();

			try
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
			finally
			{
				_coordinatorLock.ExitWriteLock();
			}
		}

		#endregion


		#region ViewModel Creation Methods

		VM CreateViewModelWithoutContext(IView<VM> view, IContainer withContainer)
		{
			var viewModel = withContainer.Resolve<VM>();
			_active_ViewModels.Add(viewModel);

			_views_WithoutContext.Add(view);
			_viewModels_WithoutContext.Add(viewModel);
			_bi_views_viewModels_WithoutContext.Add(view, viewModel);

			return viewModel;
		}

		VM CreateViewModelWithContext(IView<VM> view, string withContext, IContainer withContainer)
		{
			var contextViewModel = withContainer.Resolve<VM>();

			_active_ViewModels.Add(contextViewModel);
			_bi_contexts_viewModels.Add(withContext, contextViewModel);
			AddViewToContext(view, withContext);

			return contextViewModel;
		}

		void ConvertToContextualViewModel(IView<VM> view, VM toVM, string fromContext, IContainer withContainer)
		{
			if (!_bi_views_viewModels_WithoutContext.TryGetKey(toVM, out var targetView))
			{
				throw new InvalidOperationException($"Cannot find View associated with ViewModel {toVM.GetType()}");
			}

			var newContext = $"__TransientContext__{Guid.NewGuid()}";

			RemoveFromWithoutContextTracking(toVM, targetView);
			AddToContextTracking(toVM, targetView, newContext);

			RebindToContext(view, fromContext, newContext, withContainer);
			return;
		}

		#endregion


		#region Context Management Methods

		void AddViewToContext(IView<VM> view, string toContext)
		{
			if (_bi_contexts_setViews.TryGetKey(view, out var currentContext))
			{
				throw new InvalidOperationException($"View {view.GetType()} already registered with context {currentContext}");
			}

			_bi_contexts_setViews.TryAdd(toContext, view);

			view.Context = toContext;
		}

		void AddToContextTracking(VM viewModel, IView<VM> targetView, string context)
		{
			_bi_contexts_setViews.Add(context, targetView);
			_bi_contexts_viewModels.Add(context, viewModel);

			targetView.Context = context;
		}

		void RemoveFromWithoutContextTracking(VM viewModel, IView<VM> view)
		{
			_views_WithoutContext.Remove(view);
			_viewModels_WithoutContext.Remove(viewModel);
			_bi_views_viewModels_WithoutContext.RemoveByValue(viewModel);
		}

		#endregion


		#region Cleanup Methods

		private void UnregisterFromContextTracking(IView<VM> view, string context)
		{
			if (_bi_contexts_setViews.RemoveByValue(view))
			{
				if (!_bi_contexts_setViews.TryGetValues(context, out var _))
				{
					if (_bi_contexts_viewModels.TryGetValue(context, out var vm))
					{
						OrphanViewModel(vm);
					}

					_bi_contexts_viewModels.RemoveByKey(context);
				}
			}
		}

		void UnregisterFromWithoutContextTracking(IView<VM> view)
		{
			if (_views_WithoutContext.Contains(view))
			{
				_views_WithoutContext.Remove(view);
				if (_bi_views_viewModels_WithoutContext.TryGetValue(view, out var vm))
				{
					_bi_views_viewModels_WithoutContext.RemoveByKey(view);
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
			if (_bi_views_viewModels_WithoutContext.TryGetKey(viewModel, out var view))
			{
				_views_WithoutContext.Remove(view);
			}
			_bi_views_viewModels_WithoutContext.RemoveByValue(viewModel);
		}

		void ClearAllTrackingData()
		{
			// ViewModel lifecycle
			_active_ViewModels.Clear();
			_orphan_ViewModels.Clear();

			// Context-based tracking
			_bi_contexts_setViews.Clear();
			_bi_contexts_viewModels.Clear();
			_bi_views_viewModels_WithoutContext.Clear();

			// Without-context tracking
			_views_WithoutContext.Clear();
			_viewModels_WithoutContext.Clear();
			_bi_views_viewModels_WithoutContext.Clear();
		}

		#endregion
	}
}