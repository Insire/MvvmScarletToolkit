using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Markup;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MvvmScarletToolkit.Implementations")]
[assembly: AssemblyDescription("MvvmScarletToolkit.Implementations is part of the MvvmScarletToolkit framework, containing concrete implementations for WPF that have been abstracted away in the MvvmScarletToolkit.Abstractions library.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("9bd9b759-8c12-4e08-971b-9ba34d8247ff")]

// Set the recommended prefix for xaml namespaces
[assembly: XmlnsPrefix("http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared", "mvvm")]

// map clr namespaces to a single xaml namespace
[assembly: XmlnsDefinition("http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared", "MvvmScarletToolkit")]
[assembly: XmlnsDefinition("http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared", "MvvmScarletToolkit.Wpf")]
[assembly: XmlnsDefinition("http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared", "MvvmScarletToolkit.Wpf.FileSystemBrowser")]
