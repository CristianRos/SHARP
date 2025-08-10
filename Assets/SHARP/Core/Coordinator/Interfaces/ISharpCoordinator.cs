namespace SHARP.Core
{
	public interface ISharpCoordinator
	{
		public ICoordinator<VM> For<VM>()
			where VM : IViewModel;
		public ICoordinator<VM> ForViewModel<VM>(VM viewModel)
			where VM : IViewModel;
		public void Clear<VM>()
			where VM : IViewModel;
		public void ClearEverything<VM>()
			where VM : IViewModel;
	}
}