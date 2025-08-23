using System;
using System.Collections.Generic;
using Reflex.Core;

namespace SHARP.Core
{
	public interface ICoordinator : IDisposable
	{ }
	public interface ICoordinator<VM> : ICoordinator
		where VM : IViewModel
	{
		IEnumerable<VM> GetActive();
		IEnumerable<VM> GetOrphan();
		IEnumerable<VM> GetAll();

		IEnumerable<IView<VM>> GetViewsWithoutContext();
		IEnumerable<IView<VM>> GetViewsWithContext();

		IEnumerable<VM> GetViewModelsWithoutContext();
		IEnumerable<VM> GetViewModelsWithContext();
		IEnumerable<VM> GetViewModelsWithContext(Func<string, bool> contextMatcher);
		VM GetViewModel(string context);

		string GetContext(IView<VM> view);
		string GetContext(VM viewModel);
		IEnumerable<string> GetAllContexts();

		VM Get(IView<VM> view, string withContext, IContainer withContainer);
		VM CoordinateRebind<V>(V view, VM toVM, IContainer withContainer)
			where V : IView<VM>;
		VM RebindToContext(IView<VM> view, string fromContext, string toContext, IContainer withContainer);
		void UnregisterView(IView<VM> view, string context);
	}
}