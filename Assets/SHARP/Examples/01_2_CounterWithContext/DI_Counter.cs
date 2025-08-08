using Reflex.Core;
using UnityEngine;

namespace SHARP.Examples.CounterWithContext
{
	public class DI_Counter : MonoBehaviour, IInstaller
	{
		public void InstallBindings(ContainerBuilder builder)
		{
			builder.AddViewModel<VM_Counter>();
		}
	}
}