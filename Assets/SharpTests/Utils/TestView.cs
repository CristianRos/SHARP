using R3;
using SHARP.Core;

namespace SHARP.Tests.Utils
{
	public class TestView : View<ITestViewModel>
	{
		public void InitView(ICoordinator<ITestViewModel> coordinator, IContainer container, string context = null)
		{
			_coordinator = null;
			Context = context;
			ViewModel.Value = coordinator.Get(this, Context, container);
		}

		protected override void Awake() { }
		protected override void OnEnable() { }
		protected override void OnDisable() { }
		protected override void HandleSubscriptions(ITestViewModel viewModel, ref DisposableBuilder d) { }
	}
}