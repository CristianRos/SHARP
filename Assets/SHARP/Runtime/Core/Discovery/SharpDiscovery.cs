namespace SHARP.Core
{
	public class SharpDiscovery : ISharpDiscovery
	{
		readonly ISharpCoordinator _sharpCoordinator;

		public SharpDiscovery(ISharpCoordinator sharpCoordinator)
		{
			_sharpCoordinator = sharpCoordinator;
		}

		public IDiscoveryQuery<VM> For<VM>()
			where VM : IViewModel
		{
			ICoordinator<VM> coordinator = _sharpCoordinator.For<VM>();
			return new DiscoveryQuery<VM>(coordinator);
		}
	}
}