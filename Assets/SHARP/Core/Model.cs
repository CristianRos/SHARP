using System;
using R3;

namespace SHARP.Core
{
	public interface IModel : IDisposable { }

	public class Model : IModel
	{
		IDisposable _disposable = Disposable.Empty;

		public void Dispose()
		{
			_disposable.Dispose();
		}
	}
}