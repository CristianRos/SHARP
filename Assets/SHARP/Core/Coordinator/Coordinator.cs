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
		protected List<VM> _viewModels = new();
		protected Dictionary<string, VM> _vmByContext = new();
		protected Dictionary<VM, string> _contextByVM = new();

		public virtual void Register(VM viewModel, string context = null)
		{
			if (_viewModels.Contains(viewModel))
			{
				throw new InvalidOperationException($"ViewModel {viewModel.GetType()} already registered");
			}
			Debug.Log($"Registering {viewModel.GetType()} with context {context}");

			_viewModels.Add(viewModel);
			if (context != null) _vmByContext.Add(context, viewModel);
		}

		public virtual void Unregister(VM viewModel)
		{
			if (!_viewModels.Contains(viewModel)) throw new InvalidOperationException($"ViewModel {viewModel.GetType()} not registered");

			Debug.Log($"Unregistering {viewModel.GetType()} with context {_contextByVM[viewModel]}");

			_viewModels.Remove(viewModel);
			_vmByContext.Remove(_contextByVM[viewModel]);
			_contextByVM.Remove(viewModel);
		}

		public virtual List<VM> GetAll() => _viewModels.ToList();

		public virtual VM Get(Container container, string context = null)
		{
			if (string.IsNullOrEmpty(context))
			{
				Debug.Log($"Getting a new instance of {typeof(VM)}");

				var viewModel = container.Resolve<VM>();
				_viewModels.Add(viewModel);

				return viewModel;
			}

			if (_vmByContext.TryGetValue(context, out var vm))
			{
				Debug.Log($"Getting {typeof(VM)} with context {context}");
				return vm;
			}

			Debug.Log($"Getting a new instance of {typeof(VM)} with context {context}");

			var contextViewModel = container.Resolve<VM>();

			_viewModels.Add(contextViewModel);
			_vmByContext.Add(context, contextViewModel);
			_contextByVM.Add(contextViewModel, context);

			return contextViewModel;
		}

		public virtual void Dispose()
		{
			Debug.Log($"Disposing Coordinator<{typeof(VM)}>");
			Debug.Log($"{_viewModels.Count} view models registered");
			Debug.Log($"{_vmByContext.Count} view models by context registered");

			foreach (var vm in _viewModels) vm.Dispose();
			_viewModels.Clear();

			foreach (var vm in _vmByContext.Values) vm.Dispose();
			_vmByContext.Clear();

			_contextByVM.Clear();
		}
	}
}