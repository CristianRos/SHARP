using R3;
using SHARP.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SHARP.Examples.CounterSlider
{
	public class V_Slider : View<VM_Counter>
	{
		[SerializeField] TMP_Text _countText;
		[SerializeField] Slider _slider;

		protected override void HandleSubscriptions(VM_Counter viewModel, ref DisposableBuilder d)
		{
			viewModel.DisplayCount
				.Subscribe(value => _countText.text = value)
				.AddTo(ref d);

			_slider.OnValueChangedAsObservable()
				.Subscribe(value => viewModel.Set.Execute((int)value))
				.AddTo(ref d);
		}
	}
}