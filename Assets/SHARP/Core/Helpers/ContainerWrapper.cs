using Reflex.Core;

namespace SHARP.Core
{
	public interface IContainer
	{
		T Resolve<T>();
		void Dispose();
	}

	public class ContainerWrapper : IContainer
	{
		readonly Container _container;

		public ContainerWrapper(Container container)
		{
			_container = container;
		}

		public T Resolve<T>()
		{
			return _container.Resolve<T>();
		}

		public void Dispose()
		{
			_container.Dispose();
		}
	}
}