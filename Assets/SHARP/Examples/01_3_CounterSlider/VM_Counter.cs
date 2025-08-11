using R3;
using SHARP.Core;

namespace SHARP.Examples.CounterSlider
{
	public class VM_Counter : ViewModel
	{
		ReactiveProperty<int> _count = new(0);
		public ReactiveProperty<string> DisplayCount = new($"Count: 0");

		public ReactiveProperty<bool> CanIncrease = new();
		public ReactiveProperty<bool> CanDecrease = new();

		public ReactiveCommand<int> Set { get; private set; } = new();

		public ReactiveCommand Increase { get; private set; } = new();
		public ReactiveCommand Decrease { get; private set; } = new();

		protected override void HandleSubscriptions(ref DisposableBuilder d)
		{
			_count
				.Subscribe(value => DisplayCount.Value = $"Count: {value}")
				.AddTo(ref d);

			_count
				.Subscribe(value => CanIncrease.Value = value < 10)
				.AddTo(ref d);

			_count
				.Subscribe(value => CanDecrease.Value = value > 0)
				.AddTo(ref d);

			Set
				.Subscribe(value => _count.Value = value)
				.AddTo(ref d);

			Increase
				.Subscribe(_ => _count.Value++)
				.AddTo(ref d);

			Decrease
				.Subscribe(_ => _count.Value--)
				.AddTo(ref d);
		}
	}
}