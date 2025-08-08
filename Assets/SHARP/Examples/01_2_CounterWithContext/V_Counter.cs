using R3;
using SHARP.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SHARP.Examples.CounterWithContext
{
	[Context("Counter")]
	public class V_Counter : View<VM_Counter>
	{
		[SerializeField] TMP_Text _countText;
		[SerializeField] Button _increaseButton;
		[SerializeField] Button _decreaseButton;

		protected override void HandleSubscriptions(VM_Counter viewModel, DisposableBuilder d)
		{
			viewModel.DisplayCount
				.Subscribe(value => _countText.text = value)
				.AddTo(ref d);

			_increaseButton.OnClickAsObservable()
				.Subscribe(_viewModel.Increase.Execute)
				.AddTo(ref d);

			_decreaseButton.OnClickAsObservable()
				.Subscribe(_viewModel.Decrease.Execute)
				.AddTo(ref d);
		}
	}
}