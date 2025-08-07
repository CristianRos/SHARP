using System;
using R3;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SHARP.Examples
{
	public class V_Counter : MonoBehaviour, IDisposable
	{
		[Inject] VM_Counter _viewModel;

		[SerializeField] TMP_Text _countText;
		[SerializeField] Button _increaseButton;
		[SerializeField] Button _decreaseButton;

		IDisposable _disposable = Disposable.Empty;

		void OnEnable()
		{
			var d = Disposable.CreateBuilder();

			_viewModel.Count
				.Subscribe(count => _countText.text = $"Count: {count}")
				.AddTo(ref d);

			_increaseButton.OnClickAsObservable()
				.Subscribe(_viewModel.Increase.Execute)
				.AddTo(ref d);

			_decreaseButton.OnClickAsObservable()
				.Subscribe(_viewModel.Decrease.Execute)
				.AddTo(ref d);

			_disposable = d.Build();
		}

		void OnDisable() => _disposable.Dispose();
		public void Dispose() => _disposable.Dispose();
	}
}