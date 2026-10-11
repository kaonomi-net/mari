// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

using System.Windows;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://mari.toolbox.ui/", "MARI.Toolbox.WPF.UI.Controls")]
[assembly: XmlnsDefinition("http://mari.toolbox.ui/", "MARI.Toolbox.WPF.UI.Helpers")]
[assembly: XmlnsDefinition("http://mari.toolbox.ui/", "MARI.Toolbox.WPF.UI.Converters")]
[assembly: XmlnsDefinition("http://mari.toolbox.ui/", "MARI.Toolbox.WPF.UI.Resources")]
[assembly: XmlnsDefinition("http://mari.toolbox.ui/", "MARI.Toolbox.WPF.UI.Behaviors")]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,            //where theme specific resource dictionaries are located
                                                //(used if a resource is not found in the page,
                                                // or application resource dictionaries)

    ResourceDictionaryLocation.SourceAssembly   //where the generic resource dictionary is located
                                                //(used if a resource is not found in the page,
                                                // app, or any theme specific resource dictionaries)
)]
