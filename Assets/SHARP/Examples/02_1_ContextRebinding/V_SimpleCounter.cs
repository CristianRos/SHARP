using R3;
using SHARP.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SHARP.Examples.ContextRebinding
{
	public class V_SimpleCounter : View<VM_SimpleCounter>
	{
		[SerializeField] TMP_Text _countText;
		[SerializeField] Button _button;

		[SerializeField] TMP_InputField _inputField;
		[SerializeField] Button _rebindButton;

		protected override void HandleSubscriptions(VM_SimpleCounter viewModel, ref DisposableBuilder d)
		{
			viewModel.DisplayCount
				.Subscribe(value => _countText.text = value)
				.AddTo(ref d);

			_button.OnClickAsObservable()
				.Subscribe(viewModel.Increase.Execute)
				.AddTo(ref d);

			_rebindButton.OnClickAsObservable()
				.Subscribe(_ =>
				{
					Debug.Log($"Rebinding to context {_inputField.text}");
					RebindToContext(_inputField.text);
				})
				.AddTo(ref d);
		}
	}
}