using System;

namespace SHARP.Core
{
	public interface IHierarchyDiscovery<VM> : IDiscoveryQuery<VM>
		where VM : IViewModel
	{
		new IHierarchyDiscovery<VM> Where(Func<VM, bool> filter);
		IHierarchyDiscovery<VM> AtDepth(int depth);
		IHierarchyDiscovery<VM> WithMaxDepth(int maxDepth);
		IHierarchyDiscovery<VM> Siblings();
		IHierarchyDiscovery<VM> SiblingsIncludingSelf();
		IHierarchyDiscovery<VM> Children();
		IHierarchyDiscovery<VM> Descendants();
	}
}