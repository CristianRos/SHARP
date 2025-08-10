using R3;
using SHARP.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SHARP.Examples.ViewModelRebinding
{
	public class V_SimpleCounter : View<VM_SimpleCounter>
	{
		[SerializeField] TMP_Text _countText;
		[SerializeField] Button _button;

		protected override void HandleSubscriptions(VM_SimpleCounter viewModel, ref DisposableBuilder d)
		{
			viewModel.DisplayCount
				.Subscribe(value => _countText.text = value)
				.AddTo(ref d);

			_button.OnClickAsObservable()
				.Subscribe(viewModel.Increase.Execute)
				.AddTo(ref d);
		}
	}
}