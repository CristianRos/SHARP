using R3;
using SHARP.Core;

namespace SHARP.Examples.DiscoveryDemo
{
	public class VM_SimpleCounter : ViewModel
	{
		public ReactiveProperty<int> Count = new(0);
		public ReactiveProperty<string> DisplayCount = new("0");

		public ReactiveCommand Increase { get; private set; } = new();
		public ReactiveCommand Highlight { get; private set; } = new();
		public ReactiveCommand Unhighlight { get; private set; } = new();

		protected override void HandleSubscriptions(ref DisposableBuilder d)
		{
			Count
				.Subscribe(value => DisplayCount.Value = $"{value}")
				.AddTo(ref d);

			Increase
				.Subscribe(_ => Count.Value++)
				.AddTo(ref d);
		}
	}
}