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

	internal class CoreSharpCoordinator : IDisposable
	{
		private readonly Dictionary<Type, object> _coordinators = new();

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}