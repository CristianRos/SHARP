namespace SHARP.Core
{
	public interface ISharpDiscovery
	{
		public IDiscoveryQuery<VM> For<VM>()
			where VM : IViewModel;
	}
}