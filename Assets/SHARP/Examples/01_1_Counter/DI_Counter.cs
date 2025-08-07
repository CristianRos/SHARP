using Reflex.Core;
using UnityEngine;

namespace SHARP.Examples
{
	public class DI_Counter : MonoBehaviour, IInstaller
	{
		public void InstallBindings(ContainerBuilder builder)
		{
			builder.AddTransient(typeof(VM_Counter));
		}
	}
}