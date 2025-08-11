using System.Collections.Generic;
using System.Linq;

namespace SHARP.Core
{
	public static class CoordinatorDiscoveryExtensions
	{
		public static string GetContextForViewModel<VM>(this ICoordinator<VM> coordinator, VM viewModel)
			where VM : IViewModel
		{
			return coordinator.ContextByViewModel.TryGetValue(viewModel, out var context) ? context : null;
		}

		public static bool IsActive<VM>(this ICoordinator<VM> coordinator, VM viewModel)
			where VM : IViewModel
		{
			return coordinator.Active_ViewModels.Contains(viewModel);
		}

		public static bool IsOrphaned<VM>(this ICoordinator<VM> coordinator, VM viewModel)
			where VM : IViewModel
		{
			return coordinator.Orphan_ViewModels.Contains(viewModel);
		}

		public static IEnumerable<IView<VM>> GetViewsForViewModel<VM>(this ICoordinator<VM> coordinator, VM viewModel)
			where VM : IViewModel
		{
			var result = new List<IView<VM>>();

			// Check single view
			if (coordinator.ViewsByViewModel_WithoutContext.TryGetValue(viewModel, out var singleView))
			{
				if (singleView is IView<VM> typedView) result.Add(typedView);
			}

			// Check multiple views
			var context = coordinator.GetContextForViewModel(viewModel);
			if (!string.IsNullOrEmpty(context) && coordinator.ViewsByContext.TryGetValue(context, out var contextViews))
			{
				result.AddRange(contextViews.OfType<IView<VM>>());
			}

			return result;
		}
	}
}