// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace {{ASSEMBLY_NAMESPACE}}.SourceGen
{
	using global::MARI.Framework.Core.Abstractions.Attributes.MetaData;
	using global::MARI.Framework.Core.Abstractions.References;
	using global::MARI.Framework.Core.Abstractions.Enums;

	/// <summary>
	///		Reference provider for assembly {{ASSEMBLY_NAMESPACE}}
	/// </summary>
	internal sealed class {{ASSEMBLY_NAMESPACE_CLASS_NAME}}_MARIReferenceProviderAttribute : MARIReferenceProviderAttribute
	{
		/// <inheritdoc />
		public override void Populate(MARIReferenceRegistry registry)
		{
			{{REGISTRIES}}
		}
	}
}