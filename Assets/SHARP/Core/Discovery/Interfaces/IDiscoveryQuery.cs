using System;
using System.Collections.Generic;
using UnityEngine;

namespace SHARP.Core
{
	/// <summary>
	/// Main entry point for discovery queries. Provides fluent interface for building
	/// complex queries that combine contextual, spatial, and predicate-based filtering.
	/// </summary>
	public interface IDiscoveryQuery<VM> where VM : IViewModel
	{
		// ===== STEP 1: COORDINATOR-BASED FILTERING =====
		// These operations use the coordinator's existing indexes for efficient filtering
		// They should execute first to narrow down the candidate set before expensive operations

		/// <summary>
		/// Filter to ViewModels that exist within the specified named context.
		/// </summary>
		IDiscoveryQuery<VM> InContext(string contextName);

		/// <summary>
		/// Filter to ViewModels whose context names match the provided predicate.
		/// </summary>
		IDiscoveryQuery<VM> WhereContext(Func<string, bool> contextMatcher);

		/// <summary>
		/// Filter to ViewModels that exist in any context (excludes contextless ViewModels).
		/// </summary>
		IDiscoveryQuery<VM> InAnyContext();

		/// <summary>
		/// Filter to ViewModels that exist without any context.
		/// </summary>
		IDiscoveryQuery<VM> WithoutContext();

		/// <summary>
		/// Filter to ViewModels that are currently active in the coordinator.
		/// </summary>
		IDiscoveryQuery<VM> ThatAreActive();

		/// <summary>
		/// Filter to ViewModels that are orphaned (ViewModel exists but associated View is destroyed).
		/// </summary>
		IDiscoveryQuery<VM> ThatAreOrphaned();


		// ===== STEP 2: SPATIAL FILTERING =====
		// These operations require Unity hierarchy traversal and should execute after
		// coordinator-based filtering to minimize the number of spatial checks needed

		/// <summary>
		/// Filter to ViewModels whose associated Views are direct children of the reference transform.
		/// </summary>
		/// <param name="reference">The parent transform to search within</param>
		/// <param name="depth">Optional: Only include children at exactly this depth (relative to reference)</param>
		/// <param name="withinDepth">If true, include all children within the specified depth; if false, only exact depth</param>
		IDiscoveryQuery<VM> ChildrenOf(Transform reference);

		/// <summary>
		/// Filter to ViewModels whose associated Views are descendants (children, grandchildren, etc.) of the reference transform.
		/// </summary>
		/// <param name="reference">The ancestor transform to search within</param>
		/// <param name="maxDepth">Optional: Maximum depth to search (prevents full tree traversal)</param>
		IDiscoveryQuery<VM> DescendantsOf(Transform reference, int? maxDepth = null, bool WithinDepth = true);

		/// <summary>
		/// Filter to ViewModels whose associated Views are siblings of the reference transform.
		/// </summary>
		/// <param name="reference">The sibling reference transform</param>
		IDiscoveryQuery<VM> SiblingsOf(Transform reference);

		/// <summary>
		/// Filter to ViewModels whose associated Views are siblings of the reference transform, including the reference itself.
		/// </summary>
		/// <param name="reference">The sibling reference transform</param>
		IDiscoveryQuery<VM> SiblingsOfIncludingSelf(Transform reference);

		// ===== STEP 3: PREDICATE FILTERING =====
		// These operations apply custom logic and should execute last on the final candidate set

		/// <summary>
		/// Filter ViewModels using a custom predicate. Applied to the ViewModel instances.
		/// </summary>
		IDiscoveryQuery<VM> Where(Func<VM, bool> predicate);


		/// <summary>
		/// Filter ViewModels that match ALL of the provided predicates (AND logic).
		/// </summary>
		IDiscoveryQuery<VM> WhereAll(params Func<VM, bool>[] predicates);

		/// <summary>
		/// Filter ViewModels that match ANY of the provided predicates (OR logic).
		/// </summary>
		IDiscoveryQuery<VM> WhereAny(params Func<VM, bool>[] predicates);

		// ===== QUERY EXECUTION =====
		// These methods execute the built query and return results

		/// <summary>
		/// Execute the query and return all matching ViewModels.
		/// </summary>
		IEnumerable<VM> All();

		/// <summary>
		/// Execute the query and return the first matching ViewModel, or default if none found.
		/// </summary>
		VM FirstOrDefault();

		/// <summary>
		/// Execute the query and return the single matching ViewModel. Throws if zero or multiple results.
		/// </summary>
		VM Single();

		/// <summary>
		/// Execute the query and return whether any ViewModels match the criteria.
		/// </summary>
		bool Any();

		/// <summary>
		/// Execute the query and return the count of matching ViewModels.
		/// </summary>
		int Count();
	}
}