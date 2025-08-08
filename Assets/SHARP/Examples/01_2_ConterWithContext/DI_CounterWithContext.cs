using Reflex.Core;
using UnityEngine;

namespace SHARP.Examples
{
	public class DI_CounterWithContext : MonoBehaviour, IInstaller
	{
		public void InstallBindings(ContainerBuilder builder)
		{
			builder.AddViewModel<VM_Counter>();
		}
	}
}