using System.Collections.Generic;
using VContainer;

[Preserve]
internal class TutorialServiceGeneratedInjector : IInjector
{
	public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
		TutorialService tutorialService = new TutorialService();
		Inject(tutorialService, resolver, parameters);
		return tutorialService;
	}

	public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)
	{
	}
}
