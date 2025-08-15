using R3;
using SHARP.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SHARP.Examples.DiscoveryDemo
{
	public class V_DiscoveryHub : View<VM_DiscoveryHub>
	{
		[SerializeField] TMP_InputField _contextInputField;
		[SerializeField] Button _discoverByContextButton;

		[SerializeField] TMP_InputField _greaterThanInputField;
		[SerializeField] Button _discoverGreaterThanButton;

		[SerializeField] Button _discoverMixedButton;

		[SerializeField] Transform _referenceTransform;

		[SerializeField] Button _discoverParentDepth0Button;
		[SerializeField] Button _discoverParentDepth1Button;
		[SerializeField] Button _discoverParentDepth2Button;

		protected override void HandleSubscriptions(VM_DiscoveryHub viewModel, ref DisposableBuilder d)
		{
			_discoverByContextButton.OnClickAsObservable()
				.Subscribe(_ => viewModel.DiscoverContextCommand.Execute(_contextInputField.text))
				.AddTo(ref d);

			_discoverGreaterThanButton.OnClickAsObservable()
				.Subscribe(_ => viewModel.DiscoverGreaterThanCommand.Execute(int.Parse(_greaterThanInputField.text)))
				.AddTo(ref d);

			_discoverMixedButton.OnClickAsObservable()
				.Subscribe(_ => viewModel.DiscoverMixedCommand.Execute((_contextInputField.text, int.Parse(_greaterThanInputField.text))))
				.AddTo(ref d);

			_discoverParentDepth0Button.OnClickAsObservable()
				.Subscribe(_ => viewModel.DiscoverParentCommand.Execute((_referenceTransform, 1)))
				.AddTo(ref d);

			_discoverParentDepth1Button.OnClickAsObservable()
				.Subscribe(_ => viewModel.DiscoverParentCommand.Execute((_referenceTransform, 2)))
				.AddTo(ref d);

			_discoverParentDepth2Button.OnClickAsObservable()
				.Subscribe(_ => viewModel.DiscoverParentCommand.Execute((_referenceTransform, 3)))
				.AddTo(ref d);
		}
	}
}