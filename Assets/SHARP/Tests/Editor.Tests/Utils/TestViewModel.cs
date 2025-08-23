using R3;
using SHARP.Core;

namespace SHARP.Tests.Utils
{
	public interface ITestViewModel : IViewModel
	{
		ReactiveProperty<int> TestProperty { get; }

		ReactiveCommand<Unit> IncrementCommand { get; }
	}

	public class TestViewModel : ViewModel, ITestViewModel
	{
		public ReactiveProperty<int> TestProperty { get; } = new(0);

		public ReactiveCommand<Unit> IncrementCommand { get; } = new();

		protected override void HandleSubscriptions(ref DisposableBuilder d)
		{
			IncrementCommand
				.Subscribe(_ => TestProperty.Value++)
				.AddTo(ref d);
		}
	}
}