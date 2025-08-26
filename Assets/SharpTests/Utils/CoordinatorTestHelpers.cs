using System.Linq;
using NUnit.Framework;
using SHARP.Core;

namespace SHARP.Tests.Utils
{
	public static class CoordinatorTestHelpers
	{
		public static void InitView(this IView<ITestViewModel> view, ICoordinator<ITestViewModel> coordinator, IContainer container)
		{
			view.ViewModel.Value = coordinator.Get(view, view.Context, container);
		}

		public static void AssertStateConsistent(this ICoordinator<ITestViewModel> coordinator)
		{
			var allActive = coordinator.GetActive().ToHashSet();
			var allWithContext = coordinator.GetViewModelsWithContext().ToHashSet();
			var allWithoutContext = coordinator.GetViewModelsWithoutContext().ToHashSet();

			// No ViewModel should be in both context and without-context
			var intersection = allWithContext.Intersect(allWithoutContext);
			Assert.That(intersection, Is.Empty,
				"ViewModels found in both context and without-context tracking");

			// All context and without-context VMs should be in active
			var allTracked = allWithContext.Union(allWithoutContext);
			Assert.That(allTracked.All(vm => allActive.Contains(vm)), Is.True,
				"Some tracked ViewModels are not in active set");
		}

		public static void AssertEmptyState(this ICoordinator<ITestViewModel> coordinator)
		{
			Assert.That(coordinator.GetActive(), Is.Empty, "Active ViewModels should be empty");
			Assert.That(coordinator.GetOrphan(), Is.Empty, "Orphan ViewModels should be empty");
			Assert.That(coordinator.GetViewsWithoutContext(), Is.Empty, "Views without context should be empty");
			Assert.That(coordinator.GetViewsWithContext(), Is.Empty, "Views with context should be empty");
			Assert.That(coordinator.GetViewModelsWithoutContext(), Is.Empty, "ViewModels without context should be empty");
			Assert.That(coordinator.GetViewModelsWithContext(), Is.Empty, "ViewModels with context should be empty");
			Assert.That(coordinator.GetAllContexts(), Is.Empty, "Contexts should be empty");
		}

		public static void AssertSingleActive(this ICoordinator<ITestViewModel> coordinator, ITestViewModel vm)
		{
			var active = coordinator.GetActive().ToHashSet();
			Assert.That(active, Has.Count.EqualTo(1));
			Assert.That(active.Single(), Is.EqualTo(vm));
		}

		public static void AssertSingleWithContext(this ICoordinator<ITestViewModel> coordinator, ITestViewModel vm)
		{
			var withContext = coordinator.GetViewModelsWithContext().ToHashSet();
			Assert.That(withContext, Has.Count.EqualTo(1));
			Assert.That(withContext.Single(), Is.EqualTo(vm));
		}

		public static void AssertContextExists(this ICoordinator<ITestViewModel> coordinator, string context)
		{
			Assert.That(coordinator.GetAllContexts(), Contains.Item(context));
			Assert.That(coordinator.GetViewModel(context), Is.Not.Null);
		}

		public static void AssertContextEmpty(this ICoordinator<ITestViewModel> coordinator, string context)
		{
			Assert.That(coordinator.GetViewModel(context), Is.Null.Or.Empty);
		}

		public static void AssertViewsSharingContext(this ICoordinator<ITestViewModel> coordinator,
			string context, params IView<ITestViewModel>[] views)
		{
			Assert.That(coordinator.GetAllContexts(), Contains.Item(context));

			var viewsWithContext = coordinator.GetViewsWithContext().ToHashSet();
			foreach (var view in views)
			{
				Assert.That(viewsWithContext, Contains.Item(view));
				Assert.That(view.Context, Is.EqualTo(context));
			}
		}
	}
}