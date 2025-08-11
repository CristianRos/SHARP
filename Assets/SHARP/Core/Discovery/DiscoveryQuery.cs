using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SHARP.Core
{
	public class DiscoveryQuery<VM> : IDiscoveryQuery<VM>, IContextDiscovery<VM>, IHierarchyDiscovery<VM>, IStateDiscovery<VM>
	where VM : IViewModel
	{

		#region Fields

		private readonly ICoordinator<VM> _coordinator;
		private readonly List<Func<VM, bool>> _filters = new();
		private readonly List<Func<IView<VM>, bool>> _viewFilters = new();
		private readonly HierarchyQuery _hierarchyQuery = new();

		#endregion

		public DiscoveryQuery(ICoordinator<VM> coordinator)
		{
			_coordinator = coordinator;
		}

		#region Context Discovery public APIs

		public IContextDiscovery<VM> InContext(string context)
		{
			_filters.Add(vm => _coordinator.GetContextForViewModel(vm) == context);
			return this;
		}

		public IContextDiscovery<VM> InAnyContext()
		{
			_filters.Add(vm => !string.IsNullOrEmpty(_coordinator.GetContextForViewModel(vm)));
			return this;
		}

		public IContextDiscovery<VM> WithoutContext()
		{
			_filters.Add(vm => string.IsNullOrEmpty(_coordinator.GetContextForViewModel(vm)));
			return this;
		}

		public IContextDiscovery<VM> WithViewType<V>() where V : IView<VM>
		{
			_viewFilters.Add(view => view is V);
			return this;
		}

		public IContextDiscovery<VM> WithViewInParent(Transform parent)
		{
			_viewFilters.Add(view =>
			{
				if (view is Component comp)
					return comp.transform.IsChildOf(parent);
				return false;
			});
			return this;
		}

		public IContextDiscovery<VM> Active()
		{
			_filters.Add(vm => _coordinator.IsActive(vm));
			return this;
		}

		public IContextDiscovery<VM> Orphaned()
		{
			_filters.Add(vm => _coordinator.IsOrphaned(vm));
			return this;
		}

		#endregion


		#region Hierarchy Discovery public APIs

		// Setting the reference transform

		public IHierarchyDiscovery<VM> FromTransform(Transform reference)
		{
			_hierarchyQuery.ReferenceTransform = reference;
			return this;
		}

		// Direct children
		public IHierarchyDiscovery<VM> Children()
		{
			return Descendants().AtDepth(1);
		}

		public IHierarchyDiscovery<VM> Descendants()
		{
			_viewFilters.Add(view =>
			{
				if (view is Component comp && _hierarchyQuery.ReferenceTransform != null)
				{
					if (!comp.transform.IsChildOf(_hierarchyQuery.ReferenceTransform))
						return false;
					return true;
				}
				return false;
			});
			return this;
		}

		public IHierarchyDiscovery<VM> Siblings()
		{
			var parent = _hierarchyQuery.ReferenceTransform?.parent;

			_viewFilters.Add(view =>
			{
				if (view is Component comp && parent != null)
					return comp.transform.parent == parent &&
						   comp.transform != _hierarchyQuery.ReferenceTransform;
				return false;
			});
			return this;
		}

		public IHierarchyDiscovery<VM> SiblingsIncludingSelf()
		{
			var parent = _hierarchyQuery.ReferenceTransform?.parent;

			_viewFilters.Add(view =>
			{
				if (view is Component comp && parent != null)
					return comp.transform.parent == parent;
				return false;
			});
			return this;
		}


		public IHierarchyDiscovery<VM> AtDepth(int depth)
		{
			_hierarchyQuery.ExactDepth = depth;

			_viewFilters.Add(view =>
			{
				if (view is Component comp)
				{

					int actualDepth = GetDepthFromReference(comp.transform, _hierarchyQuery.ReferenceTransform);
					return actualDepth == depth && actualDepth > 0;

				}
				return false;
			});
			return this;
		}


		public IHierarchyDiscovery<VM> WithMaxDepth(int maxDepth)
		{
			_hierarchyQuery.MaxDepth = maxDepth;

			_viewFilters.Add(view =>
			{
				if (view is Component comp)
				{

					int actualDepth = GetDepthFromReference(comp.transform, _hierarchyQuery.ReferenceTransform);
					return actualDepth <= maxDepth && actualDepth > 0;
				}
				return false;
			});
			return this;
		}


		#endregion


		#region Where implementations

		public IDiscoveryQuery<VM> Where(Func<VM, bool> filter)
		{
			_filters.Add(filter);
			return this;
		}

		IContextDiscovery<VM> IContextDiscovery<VM>.Where(Func<VM, bool> filter)
		{
			_filters.Add(filter);
			return this;
		}

		IHierarchyDiscovery<VM> IHierarchyDiscovery<VM>.Where(Func<VM, bool> filter)
		{
			_filters.Add(filter);
			return this;
		}

		IStateDiscovery<VM> IStateDiscovery<VM>.Where(Func<VM, bool> filter)
		{
			_filters.Add(filter);
			return this;
		}

		#endregion


		#region Execution Methods

		public IEnumerable<VM> All()
		{
			var viewModels = _coordinator.GetAll().AsEnumerable();

			// Apply ViewModel filters
			foreach (var filter in _filters)
			{
				viewModels = viewModels.Where(filter);
			}

			// Apply View filters if any
			if (_viewFilters.Any())
			{
				viewModels = viewModels.Where(vm =>
				{
					var views = _coordinator.GetViewsForViewModel(vm);
					return views.Any(view => _viewFilters.All(filter => filter(view)));
				});
			}

			return viewModels;
		}

		public VM FirstOrDefault() => All().FirstOrDefault();
		public VM Single() => All().Single();
		public bool Any() => All().Any();

		#endregion


		#region Helper methods

		// How many steps from child to parent?
		// Returns: 0 if child == parent, 1 if direct child, 2 if grandchild, etc.
		// Returns: -1 if no relationship exists
		private int GetDepthFromReference(Transform child, Transform parent)
		{
			if (parent == null || child == null) return -1;
			if (child == parent) return 0;

			int depth = 0;
			var current = child;

			while (current != null)
			{
				if (current == parent) return depth;
				current = current.parent;
				depth++;
			}

			return -1;
		}

		#endregion
	}
}