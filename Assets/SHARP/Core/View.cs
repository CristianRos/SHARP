using System;
using System.Reflection;
using R3;
using Reflex.Attributes;
using Reflex.Extensions;
using UnityEngine;

namespace SHARP.Core
{
	public abstract class View<VM> : MonoBehaviour, IDisposable
		where VM : IViewModel
	{
		protected VM _viewModel;
		[Inject] readonly ISharpCoordinator _coordinator;

		[SerializeField]
		string _context;
		public string Context { get => _context; set { _context = value; } }

		IDisposable _disposable = Disposable.Empty;

		protected virtual void Awake()
		{
			_viewModel = _coordinator
							.For<VM>()
							.Get(gameObject.scene.GetSceneContainer(), Context);
		}

		protected virtual void OnEnable()
		{
			_disposable = Subscribe();
		}

		protected virtual void OnDisable()
		{
			_disposable.Dispose();
		}

		protected abstract void HandleSubscriptions(VM viewModel, DisposableBuilder d);

		IDisposable Subscribe()
		{
			var d = Disposable.CreateBuilder();

			HandleSubscriptions(_viewModel, d);

			return d.Build();
		}

		protected void Reset()
		{
			_context = GetType().GetCustomAttribute<ContextAttribute>()?.DefaultContext ?? "";
		}

		public void Dispose()
		{
			_disposable.Dispose();
		}
	}
}