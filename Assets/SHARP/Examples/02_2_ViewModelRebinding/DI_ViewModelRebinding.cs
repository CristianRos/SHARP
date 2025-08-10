using Reflex.Core;
using UnityEngine;

namespace SHARP.Examples.ViewModelRebinding
{
	public class DI_ViewModelRebinding : MonoBehaviour, IInstaller
	{
		public void InstallBindings(ContainerBuilder builder)
		{
			builder.AddViewModel<VM_SimpleCounter>();
		}
	}
}