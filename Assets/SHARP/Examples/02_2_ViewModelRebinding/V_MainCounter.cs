using R3;
using SHARP.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SHARP.Examples.ViewModelRebinding
{
	public class V_MainCounter : View<VM_SimpleCounter>
	{
		[SerializeField] TMP_Text _countText;
		[SerializeField] Button _button;


		[SerializeField] V_SimpleCounter _viewA;
		[SerializeField] Button _buttonA;

		[SerializeField] V_SimpleCounter _viewB;
		[SerializeField] Button _buttonB;

		[SerializeField] V_SimpleCounter _viewC;
		[SerializeField] Button _buttonC;

		protected override void HandleSubscriptions(VM_SimpleCounter viewModel, ref DisposableBuilder d)
		{
			viewModel.DisplayCount
				.Subscribe(value => _countText.text = value)
				.AddTo(ref d);

			_button.OnClickAsObservable()
				.Subscribe(viewModel.Increase.Execute)
				.AddTo(ref d);



			_buttonA.OnClickAsObservable()
				.Subscribe(_ => Rebind(_viewA.ViewModel.CurrentValue))
				.AddTo(ref d);

			_buttonB.OnClickAsObservable()
				.Subscribe(_ => Rebind(_viewB.ViewModel.CurrentValue))
				.AddTo(ref d);

			_buttonC.OnClickAsObservable()
				.Subscribe(_ => Rebind(_viewC.ViewModel.CurrentValue))
				.AddTo(ref d);
		}
	}
}