using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SHARP.Core
{
	public class DiscoveryQuery<VM> : IDiscoveryQuery<VM>
		where VM : IViewModel
	{
		#region Fields and Constructor

		readonly ICoordinator<VM> _coordinator;
		CoordinatorConstraint<VM> _coordinatorConstraints;
		SpatialConstraint<VM> _spatialConstraints;
		List<Func<VM, bool>> _predicates;

		public DiscoveryQuery(ICoordinator<VM> coordinator)
		{
			_coordinator = coordinator;
			_coordinatorConstraints = null;
			_spatialConstraints = null;
			_predicates = new();
		}

		#endregion

		#region Coordinator based filtering

		public IDiscoveryQuery<VM> InContext(string contextName)
		{
			_coordinatorConstraints ??= new();
			_coordinatorConstraints.ForContext(contextName);
			return this;
		}

		public IDiscoveryQuery<VM> WhereContext(Func<string, bool> contextMatcher)
		{
			_coordinatorConstraints ??= new();
			_coordinatorConstraints.WithContextMatcher(contextMatcher);
			return this;
		}

		public IDiscoveryQuery<VM> InAnyContext()
		{
			_coordinatorConstraints ??= new();
			_coordinatorConstraints.ThatRequiresAnyContext();
			return this;
		}

		public IDiscoveryQuery<VM> WithoutContext()
		{
			_coordinatorConstraints ??= new();
			_coordinatorConstraints.ThatExcludesContext();
			return this;
		}

		public IDiscoveryQuery<VM> WithOrWithoutContext()
		{
			_coordinatorConstraints ??= new();
			_coordinatorConstraints.WithOrWithoutContext();
			return this;
		}

		public IDiscoveryQuery<VM> ThatAreActive()
		{
			_coordinatorConstraints ??= new();
			_coordinatorConstraints.ThatAreActive();
			return this;
		}

		public IDiscoveryQuery<VM> ThatAreOrphaned()
		{
			_coordinatorConstraints ??= new();
			_coordinatorConstraints.ThatAreOrphaned();
			return this;
		}

		public IDiscoveryQuery<VM> ThatAreActiveOrOrphaned()
		{
			_coordinatorConstraints ??= new();
			_coordinatorConstraints.ThatExist();
			return this;
		}

		#endregion


		#region Spatial filtering

		public IDiscoveryQuery<VM> ChildrenOf(Transform reference)
		{
			_spatialConstraints ??= new();
			_spatialConstraints.ChildrenOf(reference);
			return this;
		}
		public IDiscoveryQuery<VM> DescendantsOf(Transform reference, int? maxDepth = null, bool withinDepth = true)
		{
			_spatialConstraints ??= new();
			_spatialConstraints.DescendantsOf(reference, maxDepth, withinDepth);
			return this;
		}
		public IDiscoveryQuery<VM> SiblingsOf(Transform reference)
		{
			_spatialConstraints ??= new();
			_spatialConstraints.SiblingsOf(reference);
			return this;
		}
		public IDiscoveryQuery<VM> SiblingsOfIncludingSelf(Transform reference)
		{
			_spatialConstraints ??= new();
			_spatialConstraints.SiblingsOfIncludingSelf(reference);
			return this;
		}

		#endregion


		#region Predicate filtering

		public IDiscoveryQuery<VM> Where(Func<VM, bool> predicate)
		{
			_predicates.Add(predicate);
			return this;
		}

		public IDiscoveryQuery<VM> WhereAll(params Func<VM, bool>[] predicates)
		{
			_predicates.AddRange(predicates);
			return this;
		}

		public IDiscoveryQuery<VM> WhereAny(params Func<VM, bool>[] predicates)
		{
			_predicates.Add(vm => predicates.Any(p => p(vm)));
			return this;
		}

		#endregion


		#region Query execution

		IEnumerable<VM> ExecuteQuery()
		{
			IEnumerable<VM> result;

			result = ExecuteCoordinatorQuery();
			result = ExecuteSpatialQuery(result);
			result = ExecutePredicateQuery(result);

			return result;
		}

		IEnumerable<VM> ExecuteCoordinatorQuery()
		{
			if (_coordinatorConstraints == null)
			{
				return _coordinator.GetAll();
			}

			HashSet<VM> results = new();

			switch (_coordinatorConstraints.ContextType)
			{
				case CoordinatorContextType.ContextName:
					VM vm = _coordinator.GetViewModel(_coordinatorConstraints.ContextName);
					if (vm != null) results.Add(vm);
					break;
				case CoordinatorContextType.ContextMatcher:
					results.UnionWith(_coordinator.GetViewModelsWithContext(_coordinatorConstraints.ContextMatcher));
					break;
				case CoordinatorContextType.WithAnyContext:
					results.UnionWith(_coordinator.GetViewModelsWithContext());
					break;
				case CoordinatorContextType.WithoutContext:
					results.UnionWith(_coordinator.GetViewModelsWithoutContext());
					break;
				case CoordinatorContextType.All:
					results.UnionWith(_coordinator.GetViewModelsWithContext());
					results.UnionWith(_coordinator.GetViewModelsWithoutContext());
					break;
				default:
					break;
			}

			switch (_coordinatorConstraints.StateType)
			{
				case CoordinatorStateType.Active:
					results.IntersectWith(_coordinator.GetActive());
					break;
				case CoordinatorStateType.Orphaned:
					results.IntersectWith(_coordinator.GetOrphan());
					break;
				case CoordinatorStateType.All:
					results.UnionWith(_coordinator.GetAll());
					break;
				default:
					break;
			}

			return results;
		}

		IEnumerable<VM> ExecuteSpatialQuery(IEnumerable<VM> result)
		{
			if (_spatialConstraints == null) return result;

			return _spatialConstraints.RelationType switch
			{
				SpatialRelationType.Children => ExecuteChildrenQuery(result),
				SpatialRelationType.Descendants => ExecuteDescendantsQuery(result),
				SpatialRelationType.Siblings => ExecuteSiblingsQuery(result),
				SpatialRelationType.SiblingsAndSelf => ExecuteSiblingsAndSelfQuery(result),
				_ => result
			};
		}

		IEnumerable<VM> ExecuteChildrenQuery(IEnumerable<VM> result)
		{
			Transform reference = _spatialConstraints.ReferenceTransform;
			if (reference == null) return new List<VM>();

			// Get direct children only
			List<Transform> children = new();
			for (int i = 0; i < reference.childCount; i++)
			{
				children.Add(reference.GetChild(i));
			}

			return FilterByTransforms(result, children);
		}

		IEnumerable<VM> ExecuteDescendantsQuery(IEnumerable<VM> result)
		{
			Transform reference = _spatialConstraints.ReferenceTransform;
			int? depthLimit = _spatialConstraints.DepthLimit;
			bool withinDepth = _spatialConstraints.WithinDepth;

			if (reference == null) return new List<VM>();

			IEnumerable<Transform> candidateTransforms = GetTransformsAtDepth(reference, depthLimit, withinDepth);
			return FilterByTransforms(result, candidateTransforms);
		}

		IEnumerable<VM> ExecuteSiblingsQuery(IEnumerable<VM> result)
		{
			Transform reference = _spatialConstraints.ReferenceTransform;

			if (reference == null || reference.parent == null) return new List<VM>();

			List<Transform> siblings = new();
			for (int i = 0; i < reference.parent.childCount; i++)
			{
				Transform child = reference.parent.GetChild(i);
				if (child != reference) // Exclude the reference itself
				{
					siblings.Add(child);
				}
			}

			return FilterByTransforms(result, siblings);
		}

		IEnumerable<VM> ExecuteSiblingsAndSelfQuery(IEnumerable<VM> result)
		{
			Transform reference = _spatialConstraints.ReferenceTransform;

			if (reference == null || reference.parent == null) return new List<VM>();

			// Get all children of the parent (siblings including self)
			List<Transform> siblingsAndSelf = new();
			for (int i = 0; i < reference.parent.childCount; i++)
			{
				siblingsAndSelf.Add(reference.parent.GetChild(i));
			}

			return FilterByTransforms(result, siblingsAndSelf);
		}

		IEnumerable<VM> ExecutePredicateQuery(IEnumerable<VM> result)
		{
			if (_predicates.Count == 0) return result;

			List<VM> filtered = new();
			foreach (var vm in result)
			{
				bool passesAll = true;
				for (int i = 0; i < _predicates.Count; i++)
				{
					if (!_predicates[i](vm))
					{
						passesAll = false;
						break;
					}
				}
				if (passesAll) filtered.Add(vm);
			}
			return filtered;
		}

		public IEnumerable<VM> All()
		{
			return ExecuteQuery();
		}
		public VM FirstOrDefault()
		{
			return ExecuteQuery().FirstOrDefault();
		}
		public VM Single()
		{
			return ExecuteQuery().Single();
		}
		public bool Any()
		{
			return ExecuteQuery().Any();
		}
		public int Count()
		{
			return ExecuteQuery().Count();
		}

		public DiscoveryQuery<VM> Clone()
		{
			return new DiscoveryQuery<VM>(_coordinator)
			{
				_coordinatorConstraints = _coordinatorConstraints?.Clone(),
				_spatialConstraints = _spatialConstraints?.Clone(),
				_predicates = new List<Func<VM, bool>>(_predicates)
			};
		}

		#endregion

		#region Spatial Helpers

		bool TryGetViewModelFromTransform(Transform transform, out VM viewModel)
		{
			if (transform.TryGetComponent<IView<VM>>(out var view))
			{
				viewModel = view.ViewModel.CurrentValue;
				return true;
			}
			viewModel = default;
			return false;
		}

		IEnumerable<VM> FilterByTransforms(IEnumerable<VM> candidates, IEnumerable<Transform> transforms)
		{
			HashSet<VM> matchingVMs = new();
			foreach (var transform in transforms)
			{
				if (TryGetViewModelFromTransform(transform, out var foundVM))
				{
					matchingVMs.Add(foundVM);
				}
			}

			List<VM> filtered = new();
			foreach (var vm in candidates)
			{
				if (matchingVMs.Contains(vm))
				{
					filtered.Add(vm);
				}
			}
			return filtered;
		}

		IEnumerable<Transform> GetTransformsAtDepth(Transform reference, int? depthLimit, bool withinDepth)
		{
			if (!depthLimit.HasValue)
			{
				return GetAllDescendants(reference);
			}

			List<Transform> results = new();
			CollectTransformsAtDepth(reference, 0, depthLimit.Value, withinDepth, results);
			return results;
		}

		IEnumerable<Transform> GetAllDescendants(Transform parent)
		{
			List<Transform> descendants = new();
			CollectAllDescendants(parent, descendants);
			return descendants;
		}

		void CollectAllDescendants(Transform current, List<Transform> results)
		{
			for (int i = 0; i < current.childCount; i++)
			{
				Transform child = current.GetChild(i);
				results.Add(child);

				// Recursively collect grandchildren, etc.
				CollectAllDescendants(child, results);
			}
		}

		void CollectTransformsAtDepth(Transform current, int currentDepth, int targetDepth, bool withinDepth, List<Transform> results)
		{
			if (currentDepth > targetDepth) return;

			for (int i = 0; i < current.childCount; i++)
			{
				Transform child = current.GetChild(i);
				int childDepth = currentDepth + 1;

				if (withinDepth ? childDepth <= targetDepth : childDepth == targetDepth)
				{
					results.Add(child);
				}

				CollectTransformsAtDepth(child, childDepth, targetDepth, withinDepth, results);
			}
		}

		#endregion
	}
}