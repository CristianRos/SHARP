using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace SHARP.Core
{
	public class DiscoveryQuery<VM> : IDiscoveryQuery<VM>
		where VM : IViewModel
	{
		#region Fields and Constructor

		readonly ICoordinator<VM> _coordinator;
		readonly CoordinatorConstraint<VM> _coordinatorConstraints;
		readonly SpatialConstraint<VM> _spatialConstraints;
		readonly IReadOnlyList<PredicateConstraint<VM>> _predicateConstraints;

		public DiscoveryQuery(ICoordinator<VM> coordinator)
		{
			_coordinator = coordinator;
			_coordinatorConstraints = null;
			_spatialConstraints = null;
			_predicateConstraints = new List<PredicateConstraint<VM>>();
		}

		DiscoveryQuery(
			ICoordinator<VM> coordinator,
			CoordinatorConstraint<VM> contextConstraints,
			SpatialConstraint<VM> spatialConstraint,
			IReadOnlyList<PredicateConstraint<VM>> predicateConstraints)
		{
			_coordinator = coordinator;
			_coordinatorConstraints = contextConstraints;
			_spatialConstraints = spatialConstraint;
			_predicateConstraints = predicateConstraints;
		}

		#endregion

		#region Coordinator based filtering

		DiscoveryQuery<VM> NewContextualConstrainedQuery(CoordinatorConstraint<VM> constraint)
		{
			return new(
				_coordinator,
				constraint,
				_spatialConstraints,
				_predicateConstraints
			);
		}

		public IDiscoveryQuery<VM> InContext(string contextName)
		{
			return NewContextualConstrainedQuery(
				CoordinatorConstraint<VM>.ForContext(contextName)
			);
		}

		public IDiscoveryQuery<VM> WhereContext(Func<string, bool> contextMatcher)
		{
			return NewContextualConstrainedQuery(
				CoordinatorConstraint<VM>.WithContextMatcher(contextMatcher)
			);
		}

		public IDiscoveryQuery<VM> InAnyContext()
		{
			return NewContextualConstrainedQuery(
				CoordinatorConstraint<VM>.ThatRequiresAnyContext()
			);
		}

		public IDiscoveryQuery<VM> WithoutContext()
		{
			return NewContextualConstrainedQuery(
				CoordinatorConstraint<VM>.ThatExcludesContext()
			);
		}

		public IDiscoveryQuery<VM> ThatAreActive()
		{
			return NewContextualConstrainedQuery(
				CoordinatorConstraint<VM>.ThatAreActive()
			);
		}

		public IDiscoveryQuery<VM> ThatAreOrphaned()
		{
			return NewContextualConstrainedQuery(
				CoordinatorConstraint<VM>.ThatAreOrphaned()
			);
		}

		#endregion


		#region Spatial filtering

		DiscoveryQuery<VM> NewSpatialConstrainedQuery(SpatialConstraint<VM> constraint)
		{
			return new(
				_coordinator,
				_coordinatorConstraints,
				constraint,
				_predicateConstraints
			);
		}

		public IDiscoveryQuery<VM> ChildrenOf(Transform reference, int? depth = null, bool withinDepth = true)
		{
			return NewSpatialConstrainedQuery(
				SpatialConstraint<VM>.ChildrenOf(reference, depth, withinDepth)
			);
		}
		public IDiscoveryQuery<VM> DescendantsOf(Transform reference, int? maxDepth = null)
		{
			return NewSpatialConstrainedQuery(
				SpatialConstraint<VM>.DescendantsOf(reference, maxDepth)
			);
		}
		public IDiscoveryQuery<VM> SiblingsOf(Transform reference)
		{
			return NewSpatialConstrainedQuery(
				SpatialConstraint<VM>.SiblingsOf(reference)
			);
		}
		public IDiscoveryQuery<VM> SiblingsOfIncludingSelf(Transform reference)
		{
			return NewSpatialConstrainedQuery(
				SpatialConstraint<VM>.SiblingsOfIncludingSelf(reference)
			);
		}


		#endregion


		#region Predicate filtering

		DiscoveryQuery<VM> NewPredicateConstrainedQuery(PredicateConstraint<VM> constraint)
		{
			var predicateConstraints = new List<PredicateConstraint<VM>>(_predicateConstraints) { constraint };

			return new(
				_coordinator,
				_coordinatorConstraints,
				_spatialConstraints,
				predicateConstraints
			);
		}

		public IDiscoveryQuery<VM> Where(Func<VM, bool> predicate)
		{
			return NewPredicateConstrainedQuery(
				PredicateConstraint<VM>.Where(predicate)
			);
		}

		public IDiscoveryQuery<VM> WhereAll(params Func<VM, bool>[] predicates)
		{
			return NewPredicateConstrainedQuery(
				PredicateConstraint<VM>.WhereAll(predicates)
			);
		}
		public IDiscoveryQuery<VM> WhereAny(params Func<VM, bool>[] predicates)
		{
			return NewPredicateConstrainedQuery(
				PredicateConstraint<VM>.WhereAny(predicates)
			);
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

			IEnumerable<VM> queryResult = Enumerable.Empty<VM>();

			switch (_coordinatorConstraints.ContextType)
			{
				case CoordinatorContextType.ContextName:
					queryResult = queryResult.Append(_coordinator.GetViewModel(_coordinatorConstraints.ContextName));
					break;
				case CoordinatorContextType.ContextMatcher:
					queryResult = queryResult.Concat(_coordinator.GetViewModelsWithContext(_coordinatorConstraints.ContextMatcher));
					break;
				case CoordinatorContextType.WithAnyContext:
					queryResult = queryResult.Concat(_coordinator.GetViewModelsWithContext().ToList());
					break;
				case CoordinatorContextType.WithoutContext:
					queryResult = queryResult.Concat(_coordinator.GetViewModelsWithoutContext().ToList());
					break;
				default:
					break;
			}

			switch (_coordinatorConstraints.StateType)
			{
				case CoordinatorStateType.Active:
					var activeViewModels = _coordinator.GetActive().ToList();
					queryResult = queryResult.Intersect(activeViewModels);
					break;
				case CoordinatorStateType.Orphaned:
					var orphanedViewModels = _coordinator.GetOrphan().ToList();
					queryResult = queryResult.Intersect(orphanedViewModels);
					break;
				default:
					break;
			}

			return queryResult;
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
			return result.Where(vm =>
			{
				var reference = _spatialConstraints.ReferenceTransform;
				var depthLimit = _spatialConstraints.DepthLimit;
				var withinDepth = _spatialConstraints.WithinDepth;

				if (reference == null) return false;

				var candidateTransforms = GetTransformsAtDepth(reference, depthLimit, withinDepth);
				return candidateTransforms.Any(t => DoesTransformBelongToViewModel(t, vm));
			});
		}

		IEnumerable<VM> ExecuteDescendantsQuery(IEnumerable<VM> result)
		{
			return result.Where(vm =>
			{
				var reference = _spatialConstraints.ReferenceTransform;
				var depthLimit = _spatialConstraints.DepthLimit;

				if (reference == null) return false;

				var candidateTransforms = GetTransformsAtDepth(reference, depthLimit, true); // true = withinDepth for descendants
				return candidateTransforms.Any(t => DoesTransformBelongToViewModel(t, vm));
			});
		}

		IEnumerable<VM> ExecuteSiblingsQuery(IEnumerable<VM> result)
		{
			return result.Where(vm =>
			{
				var reference = _spatialConstraints.ReferenceTransform;

				if (reference == null || reference.parent == null) return false;

				// Get all children of the parent (siblings)
				var siblings = new List<Transform>();
				for (int i = 0; i < reference.parent.childCount; i++)
				{
					var child = reference.parent.GetChild(i);
					if (child != reference) // Exclude the reference itself
					{
						siblings.Add(child);
					}
				}

				return siblings.Any(sibling => DoesTransformBelongToViewModel(sibling, vm));
			});
		}

		IEnumerable<VM> ExecuteSiblingsAndSelfQuery(IEnumerable<VM> result)
		{
			return result.Where(vm =>
			{
				var reference = _spatialConstraints.ReferenceTransform;

				if (reference == null || reference.parent == null) return false;

				// Get all children of the parent (siblings including self)
				var siblingsAndSelf = new List<Transform>();
				for (int i = 0; i < reference.parent.childCount; i++)
				{
					siblingsAndSelf.Add(reference.parent.GetChild(i));
				}

				return siblingsAndSelf.Any(sibling => DoesTransformBelongToViewModel(sibling, vm));
			});
		}

		IEnumerable<VM> ExecutePredicateQuery(IEnumerable<VM> result)
		{
			IEnumerable<VM> queryResult = result;

			if (_predicateConstraints.Any())
			{
				queryResult = queryResult.Where(vm => _predicateConstraints.All(c => c.Predicate(vm)));
			}

			return queryResult;
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

		#endregion

		#region Spatial Helpers

		bool DoesTransformBelongToViewModel(Transform transform, VM viewModel)
		{
			if (!transform.TryGetComponent<IView<VM>>(out var view)) return false;

			return EqualityComparer<VM>.Default.Equals(view.ViewModel.CurrentValue, viewModel);
		}

		IEnumerable<Transform> GetTransformsAtDepth(Transform reference, int? depthLimit, bool withinDepth)
		{
			if (!depthLimit.HasValue)
			{
				return GetAllDescendants(reference);
			}

			var results = new List<Transform>();
			CollectTransformsAtDepth(reference, 0, depthLimit.Value, withinDepth, results);
			return results;
		}

		IEnumerable<Transform> GetAllDescendants(Transform parent)
		{
			var descendants = new List<Transform>();
			CollectAllDescendants(parent, descendants);
			return descendants;
		}

		void CollectAllDescendants(Transform current, List<Transform> results)
		{
			for (int i = 0; i < current.childCount; i++)
			{
				var child = current.GetChild(i);
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
				var child = current.GetChild(i);
				int childDepth = currentDepth + 1;

				if (withinDepth ? childDepth <= targetDepth : childDepth == targetDepth)
				{
					results.Add(child);
				}

				if (childDepth < targetDepth)
				{
					CollectTransformsAtDepth(child, childDepth, targetDepth, withinDepth, results);
				}
			}
		}

		#endregion
	}
}