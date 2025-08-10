using System;
using System.Collections.Generic;

namespace SHARP.Core
{
	public class SharpCoordinator : ISharpCoordinator, IDisposable
	{
		protected Dictionary<Type, ICoordinator> Coordinators = new();

		public virtual ICoordinator<VM> For<VM>()
			where VM : IViewModel
		{
			if (Coordinators.TryGetValue(typeof(VM), out var coordinator)) return coordinator as ICoordinator<VM>;

			var @new = new Coordinator<VM>();
			Coordinators.Add(typeof(VM), @new);

			return @new;
		}

		public virtual ICoordinator<VM> ForViewModel<VM>(VM viewModel)
			where VM : IViewModel
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			var type = viewModel.GetType();

			var forMethod = typeof(ISharpCoordinator)
				.GetMethod(nameof(For))
				.MakeGenericMethod(type);

			return forMethod.Invoke(this, null) as ICoordinator<VM>;
		}

		public virtual void Clear<VM>()
			where VM : IViewModel
		{
			if (!Coordinators.ContainsKey(typeof(VM))) throw new InvalidOperationException($"No coordinator for {typeof(VM)}");

			Coordinators.Remove(typeof(VM));
		}

		public virtual void ClearEverything<VM>()
			where VM : IViewModel
		{
			if (!Coordinators.TryGetValue(typeof(VM), out var coordinator)) throw new InvalidOperationException($"No coordinator for {typeof(VM)}");

			var c = coordinator as ICoordinator<VM>;
			c.Dispose();
			Coordinators.Remove(typeof(VM));
		}

		public virtual void Dispose()
		{
			foreach (var c in Coordinators.Values) c.Dispose();
			Coordinators.Clear();
		}
	}
}