using R3;
using SHARP.Core;

namespace SHARP.Examples.ViewModelRebinding
{
	public class VM_SimpleCounter : ViewModel
	{
		ReactiveProperty<int> _count = new(0);
		public ReactiveProperty<string> DisplayCount = new($"0");
		public ReactiveCommand Increase { get; private set; } = new();

		protected override void HandleSubscriptions(DisposableBuilder d)
		{
			_count
				.Subscribe(value => DisplayCount.Value = $"{value}")
				.AddTo(ref d);

			Increase
				.Subscribe(_ => _count.Value++)
				.AddTo(ref d);
		}
	}
}