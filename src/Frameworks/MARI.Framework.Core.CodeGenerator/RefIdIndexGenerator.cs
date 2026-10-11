// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.CodeGenerator
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    [Generator]
    public sealed class RefIdIndexGenerator : ISourceGenerator
    {
        #region Fields

        private static readonly HashSet<string> s_attributesSNI;

        private static readonly string s_assemblyInfoTemplate;
        private static readonly string s_providerTemplate;

        #endregion

        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        static RefIdIndexGenerator()
        {
            var attrSNI = new[]
            {
                "VGrainMetaDataAttribute",
                "VGrainMetaDataMethodAttribute",
                "RefSimpleNameIdentifierAttribute",
            };

            s_attributesSNI = new HashSet<string>(attrSNI.SelectMany(a => new[] { a, a.Replace(nameof(Attribute), "") }));

            var assembly = typeof(RefIdIndexGenerator).Assembly;
            var tmpl = assembly.GetManifestResourceNames()
                               .FirstOrDefault(f => f.EndsWith("RegistryProvider.tpl"));

            using (var stream = assembly.GetManifestResourceStream(tmpl))
            using (var reader = new StreamReader(stream))
            {
                s_providerTemplate = reader.ReadToEnd();
            }

            var assemblyInfoTmpl = assembly.GetManifestResourceNames()
                                           .FirstOrDefault(f => f.EndsWith("RegistryProvider.AssemblyInfo.tpl"));

            using (var stream = assembly.GetManifestResourceStream(assemblyInfoTmpl))
            using (var reader = new StreamReader(stream))
            {
                s_assemblyInfoTemplate = reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefIdIndexGenerator"/> class.
        /// </summary>
        public RefIdIndexGenerator()
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Execute(GeneratorExecutionContext context)
        {
            // A fresh receiver is created by Roslyn for each compilation pass
            var vgrainRefToBuild = (IEnumerable<MemberDeclarationSyntax>)(context.SyntaxContextReceiver as RefIdSyntaxReceiver)?.Members ?? Array.Empty<MemberDeclarationSyntax>();

            var errors = new List<Diagnostic>();

            var sourceNamespace = context.Compilation.AssemblyName;

            var indexedByType = vgrainRefToBuild.GroupBy(s => (s.Kind(), IsVGrain: IsVGrain(s, context.Compilation)))
                                                .ToDictionary(k => k.Key, v => v.ToArray());

            var registryLines = new StringBuilder();

            foreach (var byType in indexedByType.OrderBy(i => i.Key.Item1 == SyntaxKind.InterfaceDeclaration
                                                                         ? 0
                                                                         : i.Key.Item1 == SyntaxKind.ClassDeclaration
                                                                                    ? 1
                                                                                    : i.Key.Item1 == SyntaxKind.StructDeclaration
                                                                                                ? 2
                                                                                                : 3))
            {
                registryLines.Append("/* --- ");
                registryLines.Append(byType.Key.ToString().Replace("Declaration", ""));
                registryLines.AppendLine(" --- */");

                var definitionType = ConvertKindTORefType(byType.Key.Item1, byType.Key.IsVGrain);

                foreach (var member in byType.Value)
                {
                    var attr = member.AttributeLists.SelectMany(a => a.Attributes)
                                                    .FirstOrDefault(a => s_attributesSNI.Any(n => a.Name is IdentifierNameSyntax id && id.Identifier.ValueText == n));

                    if (attr is null)
                        continue;

                    var attrData = member.AttributeLists.SelectMany(a => GetAttributes(a, context.Compilation))
                                                        .FirstOrDefault(a => a.ApplicationSyntaxReference?.GetSyntax() == attr);

                    if (attrData?.AttributeConstructor is null)
                    {
                        errors.Add(Diagnostic.Create(ErrorManager.EDG999, attr.GetLocation(), "attribute '" + attr.Name + "' could not be resolved"));
                        continue;
                    }

                    var ctrParameters = attrData.AttributeConstructor.Parameters.Select(p => p.Name).ToArray();

                    // Semantic constructor arguments are already ordered as the constructor parameters (named "param:" and default values resolved)
                    var args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    for (int i = 0; i < attrData.ConstructorArguments.Length && i < ctrParameters.Length; i++)
                        args[ctrParameters[i]] = attrData.ConstructorArguments[i].ToCSharpString();

                    // Property/field assignments (Prop = value)
                    foreach (var namedArg in attrData.NamedArguments)
                        args[namedArg.Key] = namedArg.Value.ToCSharpString();

                    var memberIdentifier = (member as ClassDeclarationSyntax)?.Identifier ??
                                           (member as InterfaceDeclarationSyntax)?.Identifier ??
                                           (member as StructDeclarationSyntax)?.Identifier ??
                                           (member as RecordDeclarationSyntax)?.Identifier ??
                                           (member as EnumDeclarationSyntax)?.Identifier ??
                                           (member as MethodDeclarationSyntax)?.Identifier;

                    if (memberIdentifier is null)
                    {
                        errors.Add(Diagnostic.Create(ErrorManager.EDG002, member.GetLocation(), member.GetType()));
                        continue;
                    }

                    var model = context.Compilation.GetSemanticModel(memberIdentifier.Value.SyntaxTree).GetDeclaredSymbol(member);

                    registryLines.Append(' ', 12);
                    List<string> arguments = null;
                    int nbGenericArgs = 0;

                    if (byType.Key.Item1 == SyntaxKind.MethodDeclaration)
                    {
                        // Method could be declared in class, struct, interface or record
                        var parentTypeDeclaration = member.Parent as TypeDeclarationSyntax;
                        if (parentTypeDeclaration is null)
                        {
                            errors.Add(Diagnostic.Create(ErrorManager.EDG002, member.GetLocation(), member.Parent?.GetType()));
                            continue;
                        }

                        registryLines.Append("registry.RegisterMethod(");
                        model = context.Compilation.GetSemanticModel(parentTypeDeclaration.SyntaxTree).GetDeclaredSymbol(parentTypeDeclaration);

                        var mthd = (MethodDeclarationSyntax)member;

                        arguments = new List<string>();

                        nbGenericArgs = mthd.TypeParameterList?.Parameters.Count ?? 0;

                        // Parameters belong to the method syntax tree, use its semantic model directly
                        var methodSemanticModel = context.Compilation.GetSemanticModel(mthd.SyntaxTree);

                        arguments.AddRange(mthd.ParameterList.Parameters
                                                             .Select(p => (p.Type?.GetTypeFullNameWithNamespace(t => methodSemanticModel.GetDeclaredSymbol(t)) ?? "global::MARI.Toolbox.AnyType"))
                                                             .Select(s => "typeof(" + s + ")"));
                    }
                    else
                    {
                        registryLines.Append("registry.Register(");
                    }

                    var type = (ITypeSymbol)model;

                    if (byType.Key.Item1 != SyntaxKind.MethodDeclaration)
                    {
                        registryLines.Append("RefTypeEnum.");
                        registryLines.Append(definitionType);
                        registryLines.Append(", ");
                    }

                    if (args.TryGetValue("simpleNameIdentifier", out var sni))
                    {
                        registryLines.Append(sni.Trim());
                    }
                    else
                    {
                        errors.Add(Diagnostic.Create(ErrorManager.EDG001, attr.GetLocation()));
                        continue;
                    }

                    registryLines.Append(", ");

                    if (byType.Key.Item1 != SyntaxKind.MethodDeclaration)
                    {
                        if (args.TryGetValue("namespaceIdentifier", out var @namespace))
                            registryLines.Append(@namespace.Trim());
                        else
                            registryLines.Append("null");
                    }
                    else
                    {
                        registryLines.Append("\"");
                        registryLines.Append(memberIdentifier.Value.Text.Trim());
                        registryLines.Append("\"");
                    }

                    var typeName = type.ToString().Trim('{', '}');

                    if (type.IsType && type is INamedTypeSymbol namedType && namedType.IsGenericType)
                    {
                        // Search if type is generic
                        if (namedType.TypeArguments.Any(t => t.IsDefinition))
                        {
                            var supIndexOf = typeName.IndexOf('<');
                            if (supIndexOf > 0)
                                typeName = typeName.Substring(0, supIndexOf) + "<" + new string(',', namedType.TypeParameters.Length - 1) + ">";
                        }
                    }

                    registryLines.Append(", typeof(global::" + typeName + ")");

                    if (byType.Key.Item1 == SyntaxKind.MethodDeclaration)
                    {
                        registryLines.Append(", ");
                        registryLines.Append(nbGenericArgs);
                    }

                    if (arguments != null && arguments.Any())
                    {
                        foreach (var arg in arguments)
                        {
                            registryLines.Append(',');
                            registryLines.Append(arg);
                        }
                    }

                    registryLines.AppendLine(");");
                }

                registryLines.AppendLine();
                registryLines.Append(' ', 12);
            }

            foreach (var error in errors)
                context.ReportDiagnostic(error);

            var registrySource = s_providerTemplate.Replace("{{ASSEMBLY_NAMESPACE}}", sourceNamespace)
                                                   .Replace("{{ASSEMBLY_NAMESPACE_CLASS_NAME}}", sourceNamespace.Replace(".", "_"))
                                                   .Replace("{{REGISTRIES}}", registryLines.ToString());

            context.AddSource("MARIReferenceRegistry.g.cs", registrySource);
            context.AddSource("MARIReferenceRegistry.AssemblyInfo.g.cs", s_assemblyInfoTemplate.Replace("{{ASSEMBLY_NAMESPACE}}", sourceNamespace)
                                                                                                    .Replace("{{ASSEMBLY_NAMESPACE_CLASS_NAME}}", sourceNamespace.Replace(".", "_")));
        }

        public static IReadOnlyList<AttributeData> GetAttributes(AttributeListSyntax attributes, Compilation compilation)
        {
            // Collect pertinent syntax trees from these attributes
            var acceptedTrees = new HashSet<SyntaxTree>();
            foreach (var attribute in attributes.Attributes)
                acceptedTrees.Add(attribute.SyntaxTree);

            var parentSymbol = GetDeclaredSymbol(attributes.Parent, compilation);
            var parentAttributes = parentSymbol.GetAttributes();
            var ret = new List<AttributeData>();
            foreach (var attribute in parentAttributes)
            {
                if (acceptedTrees.Contains(attribute.ApplicationSyntaxReference.SyntaxTree))
                    ret.Add(attribute);
            }

            return ret;
        }

        public static ISymbol GetDeclaredSymbol(SyntaxNode node, Compilation compilation)
        {
            var model = compilation.GetSemanticModel(node.SyntaxTree);
            return model.GetDeclaredSymbol(node);
        }

        /// <inheritdoc />
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new RefIdSyntaxReceiver());
        }

        #region Tools

        private string ConvertKindTORefType(SyntaxKind key, bool isVGrain)
        {
            switch (key)
            {
                case SyntaxKind.ClassDeclaration:
                    return isVGrain ? "VGrainImplementation" : "Type";

                case SyntaxKind.StructDeclaration:
                case SyntaxKind.RecordDeclaration:
                case SyntaxKind.RecordStructDeclaration:
                case SyntaxKind.EnumDeclaration:
                    return "Type";

                case SyntaxKind.InterfaceDeclaration:
                    return "VGrain";
            }

            return "Other";
        }

        /// <summary>
        /// Detect if the type is a vgrain
        /// </summary>
        private static bool IsVGrain(MemberDeclarationSyntax s, Compilation compilation)
        {
            if (!(s is ClassDeclarationSyntax || s is InterfaceDeclarationSyntax))
                return false;

            // Use semantic model to also detect IVGrain inherited indirectly (base class or interface inheritance)
            var symbol = compilation.GetSemanticModel(s.SyntaxTree).GetDeclaredSymbol(s) as INamedTypeSymbol;
            if (symbol is null)
                return false;

            return symbol.AllInterfaces.Any(i => i.Name == "IVGrain" &&
                                                 i.ContainingNamespace?.ToDisplayString() == "MARI.Framework.Core.Abstractions");
        }

        #endregion

        #endregion

        #region Nested

        /// <summary>
        /// Syntax receiver collecting members decorated with reference attributes; one instance per compilation pass
        /// </summary>
        private sealed class RefIdSyntaxReceiver : ISyntaxContextReceiver
        {
            /// <summary>
            /// Gets the members to index.
            /// </summary>
            public HashSet<MemberDeclarationSyntax> Members { get; } = new HashSet<MemberDeclarationSyntax>();

            /// <inheritdoc />
            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is MemberDeclarationSyntax classDeclaration)
                {
                    if (s_attributesSNI.Any(a => classDeclaration.HasAttribute(a)))
                        this.Members.Add(classDeclaration);
                }
            }
        }

        #endregion
    }
}
