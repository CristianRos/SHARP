using R3;
using SHARP.Core;

namespace SHARP.Examples.Counter
{
	public class VM_Counter : ViewModel
	{
		ReactiveProperty<int> _count = new(0);
		public ReactiveProperty<string> DisplayCount = new($"Count: 0");

		public ReactiveCommand Increase { get; private set; } = new();
		public ReactiveCommand Decrease { get; private set; } = new();

		protected override void HandleSubscriptions(ref DisposableBuilder d)
		{
			_count
				.Subscribe(value => DisplayCount.Value = $"Count: {value}")
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