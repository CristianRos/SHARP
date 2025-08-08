using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Core;

namespace SHARP.Core
{
	public class Coordinator<VM> : ICoordinator<VM>
		where VM : IViewModel
	{
		readonly List<VM> _viewModels = new();
		readonly Dictionary<string, VM> _vmByContext = new();
		readonly Dictionary<VM, string> _contextByVM = new();

		public void Register(VM viewModel, string context = null)
		{
			if (_viewModels.Contains(viewModel))
			{
				throw new InvalidOperationException($"ViewModel {viewModel.GetType()} already registered");
			}

			_viewModels.Add(viewModel);
			if (context != null) _vmByContext.Add(context, viewModel);
		}

		public void Unregister(VM viewModel)
		{
			if (!_viewModels.Contains(viewModel)) throw new InvalidOperationException($"ViewModel {viewModel.GetType()} not registered");

			_viewModels.Remove(viewModel);
			_vmByContext.Remove(_contextByVM[viewModel]);
			_contextByVM.Remove(viewModel);
		}

		public List<VM> GetAll() => _viewModels.ToList();

		public VM Get(Container container, string context = null)
		{
			if (context == null)
			{
				var viewModel = container.Resolve<VM>();
				_viewModels.Add(viewModel);

				return viewModel;
			}

			if (_vmByContext.TryGetValue(context, out var vm)) return vm;

			var contextViewModel = container.Resolve<VM>();

			_viewModels.Add(contextViewModel);
			_vmByContext.Add(context, contextViewModel);
			_contextByVM.Add(contextViewModel, context);

			return contextViewModel;
		}

		public void Dispose()
		{
			foreach (var vm in _viewModels) vm.Dispose();
			_viewModels.Clear();

			foreach (var vm in _vmByContext.Values) vm.Dispose();
			_vmByContext.Clear();

			_contextByVM.Clear();
		}
	}
}