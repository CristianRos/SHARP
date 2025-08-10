using Reflex.Core;
using UnityEngine;

namespace SHARP.Examples.ContextRebinding
{
	public class DI_ContextRebinding : MonoBehaviour, IInstaller
	{
		public void InstallBindings(ContainerBuilder builder)
		{
			builder.AddViewModel<VM_SimpleCounter>();
		}
	}
}