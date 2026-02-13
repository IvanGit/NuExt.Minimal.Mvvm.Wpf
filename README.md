# NuExt.Minimal.Mvvm.Wpf

`NuExt.Minimal.Mvvm.Wpf` is an extension for the lightweight MVVM framework [NuExt.Minimal.Mvvm](https://github.com/IvanGit/NuExt.Minimal.Mvvm). This package is specifically designed to enhance development for WPF applications by providing additional components and utilities that simplify development, reduce routine work, and add functionality to your MVVM applications. A key focus of this package is to offer robust support for asynchronous operations, making it easier to manage complex scenarios involving asynchronous tasks and commands.

### Commonly Used Types

- **`Minimal.Mvvm.ModelBase`**: Base class for creating bindable models.
- **`Minimal.Mvvm.Wpf.ControlViewModel`**: Base class for control-specific ViewModels and designed for asynchronous disposal.
- **`Minimal.Mvvm.Wpf.DocumentContentViewModelBase`**: Base class for ViewModels that represent document content.
- **`Minimal.Mvvm.Wpf.WindowViewModel`**: Base class for window-specific ViewModels.
- **`Minimal.Mvvm.Wpf.IAsyncDialogService`**: Displays dialog windows asynchronously.
- **`Minimal.Mvvm.Wpf.IAsyncDocument`**: Asynchronous document created with `IAsyncDocumentManagerService`.
- **`Minimal.Mvvm.Wpf.IAsyncDocumentContent`**: Asynchronous document content that represent a view model.
- **`Minimal.Mvvm.Wpf.IAsyncDocumentManagerService`**: Manages asynchronous documents.
- **`Minimal.Mvvm.Wpf.InputDialogService`**: Shows modal dialogs asynchronously.
- **`Minimal.Mvvm.Wpf.OpenWindowsService`**: Manages open window ViewModels within the application.
- **`Minimal.Mvvm.Wpf.SettingsService`**: Facilitates saving and loading settings.
- **`Minimal.Mvvm.Wpf.TabbedDocumentService`**: Manages tabbed documents within a UI.
- **`Minimal.Mvvm.Wpf.ViewLocator`**: Locates and initializes views based on view models.
- **`Minimal.Mvvm.Wpf.WindowedDocumentService`**: Manages windowed documents within a UI.
- **`Minimal.Mvvm.Wpf.WindowPlacementService`**: Saves and restores window placement between runs.

### Recommended Companion Package

For an enhanced development experience, we highly recommend using the [`NuExt.Minimal.Mvvm.SourceGenerator`](https://www.nuget.org/packages/NuExt.Minimal.Mvvm.SourceGenerator) package alongside this framework. It provides a source generator that produces boilerplate code for your ViewModels at compile time, significantly reducing the amount of repetitive coding tasks and allowing you to focus more on the application-specific logic.

### Installation

You can install `NuExt.Minimal.Mvvm.Wpf` via [NuGet](https://www.nuget.org/):

```sh
dotnet add package NuExt.Minimal.Mvvm.Wpf
```

Or through the Visual Studio package manager:

1. Go to `Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution...`.
2. Search for `NuExt.Minimal.Mvvm.Wpf`.
3. Click "Install".

### Usage Examples

For comprehensive examples of how to use the package, refer to the [samples](samples) directory in this repository and the [NuExt.Minimal.Mvvm.MahApps.Metro](https://github.com/IvanGit/NuExt.Minimal.Mvvm.MahApps.Metro) repository. These samples illustrate best practices for using these extensions.

### Contributing

Contributions are welcome! Feel free to submit issues, fork the repository, and send pull requests. Your feedback and suggestions for improvement are highly appreciated.

### Acknowledgements

Special thanks to the creators and maintainers of the [DevExpress MVVM Framework](https://github.com/DevExpress/DevExpress.Mvvm.Free). The author has been inspired by its advanced features and design philosophy for many years. However, as technology evolves, the DevExpress MVVM Framework has started to resemble more of a legacy framework, falling behind modern asynchronous best practices and contemporary development paradigms. The need for better support for asynchronous programming, greater simplicity, and improved performance led to the creation of these projects.

### License

Licensed under the MIT License. See the LICENSE file for details.