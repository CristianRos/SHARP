using System;
using R3;

namespace SHARP.Examples
{
	public class VM_Counter : IDisposable
	{
		IDisposable _disposable = Disposable.Empty;
		public ReactiveProperty<int> Count = new(0);

		public ReactiveCommand Increase { get; private set; } = new();
		public ReactiveCommand Decrease { get; private set; } = new();


		public VM_Counter()
		{
			var d = Disposable.CreateBuilder();

			Increase.Subscribe(_ => Count.Value++).AddTo(ref d);
			Decrease.Subscribe(_ => Count.Value--).AddTo(ref d);

			_disposable = d.Build();
		}

		public void Dispose()
		{
			_disposable.Dispose();
		}
	}
}