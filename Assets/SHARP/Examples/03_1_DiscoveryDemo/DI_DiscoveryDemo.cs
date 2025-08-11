using Reflex.Core;
using UnityEngine;

namespace SHARP.Examples.DiscoveryDemo
{
	public class DI_DiscoveryDemo : MonoBehaviour, IInstaller
	{
		public void InstallBindings(ContainerBuilder builder)
		{
			builder.AddViewModel<VM_SimpleCounter>();
			builder.AddViewModel<VM_DiscoveryHub>();
		}
	}
}