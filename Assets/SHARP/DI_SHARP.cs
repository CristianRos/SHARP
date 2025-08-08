using Reflex.Core;
using SHARP.Core;
using UnityEngine;

namespace SHARP
{
	public class DI_SHARP : MonoBehaviour, IInstaller
	{
		public void InstallBindings(ContainerBuilder builder)
		{
			builder.AddSingleton(typeof(SharpCoordinator), typeof(ISharpCoordinator));
		}
	}
}