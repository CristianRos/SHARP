using System;
using UnityEngine;

namespace SHARP.Core
{
	public interface IContextDiscovery<VM> : IDiscoveryQuery<VM>
		where VM : IViewModel
	{
		new IContextDiscovery<VM> Where(Func<VM, bool> predicate);
		IContextDiscovery<VM> WithViewType<V>() where V : IView<VM>;
		IContextDiscovery<VM> WithViewInParent(Transform parent);
		IContextDiscovery<VM> Active();
		IContextDiscovery<VM> Orphaned();
	}
}