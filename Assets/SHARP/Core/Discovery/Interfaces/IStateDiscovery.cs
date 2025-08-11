using System;

namespace SHARP.Core
{
	public interface IStateDiscovery<VM> : IDiscoveryQuery<VM>
		where VM : IViewModel
	{
		new IStateDiscovery<VM> Where(Func<VM, bool> predicate);
	}
}