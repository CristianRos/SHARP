using System;
using System.Collections.Generic;
using Reflex.Core;

namespace SHARP.Core
{
	public interface ICoordinator : IDisposable { }
	public interface ICoordinator<VM> : ICoordinator
		where VM : IViewModel
	{
		void Register(VM viewModel, string context);
		void Unregister(VM viewModel);

		List<VM> GetAll();
		VM Get(Container container, string context = null);
	}
}