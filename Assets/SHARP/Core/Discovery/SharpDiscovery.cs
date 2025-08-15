namespace SHARP.Core
{
	public class SharpDiscovery : ISharpDiscovery
	{
		ISharpCoordinator _sharpCoordinator;

		public SharpDiscovery(ISharpCoordinator sharpCoordinator)
		{
			_sharpCoordinator = sharpCoordinator;
		}

		public IExecutableQuery<VM> For<VM>()
			where VM : IViewModel
		{
			return new DiscoveryQuery<VM>(_sharpCoordinator.For<VM>());
		}
	}
}