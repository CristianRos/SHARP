using R3;
using SHARP.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SHARP.Tests.Utils
{
	public class TestView : View<ITestViewModel>
	{
		public Button button;

		protected override void Awake()
		{
			base.Awake();
			button = gameObject.AddComponent<Button>();
		}

		protected override void HandleSubscriptions(ITestViewModel viewModel, ref DisposableBuilder d)
		{
			viewModel.TestProperty
				.Subscribe(value => Debug.Log($"TestProperty: {value}"))
				.AddTo(ref d);

			button.OnClickAsObservable()
				.Subscribe(viewModel.IncrementCommand.Execute)
				.AddTo(ref d);
		}
	}
}