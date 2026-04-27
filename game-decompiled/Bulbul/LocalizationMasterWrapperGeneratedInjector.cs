using System.Collections.Generic;
using VContainer;

namespace Bulbul;

[Preserve]
internal class LocalizationMasterWrapperGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		LocalizationMasterWrapper localizationMasterWrapper = new LocalizationMasterWrapper((MasterDataLoader)resolver.ResolveOrParameter(typeof(MasterDataLoader), "masterDataLoader", parameters), (LanguageSupplier)resolver.ResolveOrParameter(typeof(LanguageSupplier), "languageSupplier", parameters));
		Inject(localizationMasterWrapper, resolver, parameters);
		return localizationMasterWrapper;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
