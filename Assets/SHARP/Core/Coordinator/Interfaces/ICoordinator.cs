using System;
using System.Collections.Generic;
using Reflex.Core;

namespace SHARP.Core
{
	public interface ICoordinator : IDisposable { }
	public interface ICoordinator<VM> : ICoordinator
		where VM : IViewModel
	{
		List<VM> GetAll();
		VM Get(IView<VM> view, string withContext, Container withContainer);
		VM CoordinateRebind<V>(V view, VM toVM, Container withContainer)
			where V : IView<VM>;
		VM RebindToContext(IView<VM> view, string fromContext, string toContext, Container withContainer);
		void UnregisterView(IView<VM> view, string context);

	}
}