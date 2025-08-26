using Reflex.Core;
using SHARP.Core;

namespace SHARP
{
	public static class ReflexBuilderExtensions
	{
		#region Models

		public static void AddModel<TConcrete>(this ContainerBuilder builder)
			where TConcrete : IModel
		{
			builder.AddTransient(typeof(TConcrete));
		}

		public static void AddModel<TConcrete, TInterface>(this ContainerBuilder builder)
			where TConcrete : TInterface
			where TInterface : IModel
		{
			builder.AddTransient(typeof(TConcrete), typeof(TInterface));
		}

		#endregion

		#region ViewModels

		public static void AddViewModel<TConcrete>(this ContainerBuilder builder)
			where TConcrete : IViewModel
		{
			builder.AddTransient(typeof(TConcrete));
		}

		public static void AddViewModel<TConcrete, TInterface>(this ContainerBuilder builder)
			where TConcrete : TInterface
			where TInterface : IViewModel
		{
			builder.AddTransient(typeof(TConcrete), typeof(TInterface));
		}

		#endregion
	}
}