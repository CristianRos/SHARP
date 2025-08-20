using System.Linq;
using R3;
using SHARP.Core;
using UnityEngine;

namespace SHARP.Examples.DiscoveryDemo
{
	public class VM_DiscoveryHub : ViewModel
	{
		readonly ISharpDiscovery _discovery;

		public VM_DiscoveryHub(ISharpDiscovery discovery)
		{
			_discovery = discovery;
		}

		public ReactiveCommand<string> DiscoverContextCommand { get; private set; } = new();
		public ReactiveCommand<int> DiscoverGreaterThanCommand { get; private set; } = new();
		public ReactiveCommand<(string, int)> DiscoverMixedCommand { get; private set; } = new();

		public ReactiveCommand<(Transform, int)> DiscoverParentCommand { get; private set; } = new();


		protected override void HandleSubscriptions(ref DisposableBuilder d)
		{
			DiscoverContextCommand
				.Subscribe(context =>
				{
					var allCounters = _discovery.For<VM_SimpleCounter>()
						.All();

					foreach (var counter in allCounters)
					{
						counter.Unhighlight.Execute(Unit.Default);
					}

					var query = _discovery.For<VM_SimpleCounter>()
						.InContext(context)
						.All();

					Debug.Log($"Found {query.Count()} counters in context {context}");

					foreach (var vm in query)
					{
						vm.Highlight.Execute(Unit.Default);
					}
				})
				.AddTo(ref d);

			DiscoverGreaterThanCommand
				.Subscribe(min =>
				{
					var allCounters = _discovery.For<VM_SimpleCounter>()
						.All();

					foreach (var counter in allCounters)
					{
						counter.Unhighlight.Execute(Unit.Default);
					}

					var query = _discovery.For<VM_SimpleCounter>()
						.Where(vm => vm.Count.Value > min);

					var results = query.All().ToList();

					Debug.Log($"Found {results.Count()} counters with count > {min}");

					foreach (var vm in results)
					{
						vm.Highlight.Execute(Unit.Default);
					}
				})
				.AddTo(ref d);

			DiscoverMixedCommand
				.Subscribe(tuple =>
				{
					var (context, min) = tuple;

					var allCounters = _discovery.For<VM_SimpleCounter>()
						.All();

					foreach (var counter in allCounters)
					{
						counter.Unhighlight.Execute(Unit.Default);
					}

					var mixedQuery = _discovery.For<VM_SimpleCounter>()
						.InContext(context)
						.Where(vm => vm.Count.Value > min)
						.All().ToList();

					Debug.Log($"Found {mixedQuery.Count()} counters in context {context} with count > {min}");

					foreach (var vm in mixedQuery)
					{
						vm.Highlight.Execute(Unit.Default);
					}
				})
				.AddTo(ref d);

			DiscoverParentCommand
				.Subscribe(tuple =>
				{
					var (reference, depth) = tuple;

					var allCounters = _discovery.For<VM_SimpleCounter>()
						.All();

					foreach (var counter in allCounters)
					{
						counter.Unhighlight.Execute(Unit.Default);
					}

					Transform ancestorReference = reference;
					for (int i = 0; i < depth; i++)
					{
						ancestorReference = ancestorReference.parent;
					}

					var counters = _discovery.For<VM_SimpleCounter>()
						.WithoutContext()
						.DescendantsOf(reference, depth, false)
						.Where(vm => vm.Count.Value > 1)
						.All();

					Debug.Log($"Found {counters.Count()} counters at depth {depth}");

					foreach (var vm in counters)
					{
						vm.Highlight.Execute(Unit.Default);
					}
				})
				.AddTo(ref d);

		}
	}
}