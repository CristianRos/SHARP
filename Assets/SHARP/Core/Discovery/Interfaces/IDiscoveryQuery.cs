using System;
using System.Collections.Generic;
using UnityEngine;

namespace SHARP.Core
{
	public interface IDiscoveryQuery<VM> where VM : IViewModel
	{
		// Context-based discovery
		IContextDiscovery<VM> InContext(string context);
		IContextDiscovery<VM> InAnyContext();
		IContextDiscovery<VM> WithoutContext();

		// Hierarchy-based discovery
		IHierarchyDiscovery<VM> FromTransform(Transform reference);

		// State-based discovery
		IDiscoveryQuery<VM> Where(Func<VM, bool> filter);

		// Execution
		IEnumerable<VM> All();
		VM FirstOrDefault();
		VM Single();
		bool Any();
	}
}