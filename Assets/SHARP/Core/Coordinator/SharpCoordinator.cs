using System;
using System.Collections.Generic;

namespace SHARP.Core
{
	public class SharpCoordinator : ISharpCoordinator, IDisposable
	{
		Dictionary<Type, ICoordinator> _coordinators = new();

		public ICoordinator<VM> For<VM>()
			where VM : IViewModel
		{
			if (_coordinators.TryGetValue(typeof(VM), out var coordinator)) return coordinator as ICoordinator<VM>;

			var @new = new Coordinator<VM>();
			_coordinators.Add(typeof(VM), @new);

			return @new;
		}

		public void Clear<VM>()
			where VM : IViewModel
		{
			if (!_coordinators.ContainsKey(typeof(VM))) throw new InvalidOperationException($"No coordinator for {typeof(VM)}");

			_coordinators.Remove(typeof(VM));
		}

		public void ClearEverything<VM>()
			where VM : IViewModel
		{
			if (!_coordinators.TryGetValue(typeof(VM), out var coordinator)) throw new InvalidOperationException($"No coordinator for {typeof(VM)}");

			var c = coordinator as ICoordinator<VM>;
			c.Dispose();
			_coordinators.Remove(typeof(VM));
		}

		public void Dispose()
		{
			foreach (var c in _coordinators.Values) c.Dispose();
			_coordinators.Clear();
		}
	}
}