using System;
using System.Collections.Generic;
using Reflex.Core;

namespace SHARP.Core
{
	public interface ICoordinator : IDisposable { }
	public interface ICoordinator<VM> : ICoordinator
		where VM : IViewModel
	{
		#region Fields

		List<VM> Active_ViewModels { get; }
		List<VM> Orphan_ViewModels { get; }

		// Without Context
		List<IView> Views_WithoutContext { get; }
		List<VM> ViewModels_WithoutContext { get; }

		Dictionary<IView, VM> ViewModelsByView_WithoutContext { get; }
		Dictionary<VM, IView> ViewsByViewModel_WithoutContext { get; }

		// With Context
		Dictionary<string, List<IView>> ViewsByContext { get; }
		Dictionary<IView, string> ContextByView { get; }

		Dictionary<string, VM> ViewModelByContext { get; }
		Dictionary<VM, string> ContextByViewModel { get; }

		#endregion

		List<VM> GetAll();
		VM Get(IView<VM> view, string withContext, Container withContainer);
		VM CoordinateRebind<V>(V view, VM toVM, Container withContainer)
			where V : IView<VM>;
		VM RebindToContext(IView<VM> view, string fromContext, string toContext, Container withContainer);
		void UnregisterView(IView<VM> view, string context);

	}
}