using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class LanguageSupplierGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		LanguageSupplier languageSupplier = new LanguageSupplier((IDefaultLanguageSupplier)resolver.ResolveOrParameter(typeof(IDefaultLanguageSupplier), "defaultLanguageSupplier", parameters));
		Inject(languageSupplier, resolver, parameters);
		return languageSupplier;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
