using R3;
using SHARP.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SHARP.Examples.DiscoveryDemo
{
	[Context("Counter")]
	public class V_SimpleCounter : View<VM_SimpleCounter>
	{
		[SerializeField] TMP_Text _countText;
		[SerializeField] Button _increaseButton;
		[SerializeField] Image _buttonImage;

		Color _initialColor = Color.white;

		protected override void Awake()
		{
			base.Awake();

			_initialColor = _buttonImage.color;
		}

		protected override void HandleSubscriptions(VM_SimpleCounter viewModel, ref DisposableBuilder d)
		{
			viewModel.DisplayCount
				.Subscribe(value => _countText.text = value)
				.AddTo(ref d);

			viewModel.Highlight
				.Subscribe(_ =>
				{
					_buttonImage.color = Color.yellow;
				})
				.AddTo(ref d);

			viewModel.Unhighlight
				.Subscribe(_ =>
				{
					_buttonImage.color = _initialColor;
				})
				.AddTo(ref d);

			_increaseButton.OnClickAsObservable()
				.Subscribe(viewModel.Increase.Execute)
				.AddTo(ref d);
		}
	}
}