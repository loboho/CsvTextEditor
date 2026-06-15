namespace CsvTextEditor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel.MVVM;
    using Catel.Reflection;
    using Catel.Services;
    using CsvTextEditor.ViewModels;
    using Models;
    using Orc.ProjectManagement;

    public class FileSaveAsCommandContainer : ProjectCommandContainerBase
    {
        private static readonly Type EncodingViewModelType;

        private readonly ISaveFileService _saveFileService;
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IViewModelFactory _viewModelFactory;

        static FileSaveAsCommandContainer()
        {
            EncodingViewModelType = TypeCache.GetTypes(x => string.Equals(x.Name, "SaveEncodingViewModel")).FirstOrDefault();
        }

        public FileSaveAsCommandContainer(ICommandManager commandManager, IProjectManager projectManager, 
            ISaveFileService saveFileService, IUIVisualizerService uiVisualizerService,
            IViewModelFactory viewModelFactory, IServiceProvider serviceProvider)
            : base(Commands.File.SaveAs, commandManager, projectManager, serviceProvider)
        {
            _saveFileService = saveFileService;
            _uiVisualizerService = uiVisualizerService;
            _viewModelFactory = viewModelFactory;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            if (!(_projectManager.ActiveProject is Project project))
            {
                return;
            }

            var result = await _saveFileService.DetermineFileAsync(new DetermineSaveFileContext
            {
                Filter = "Text Files (*.csv)|*csv",
                AddExtension = true
            });

            if (result.Result)
            {
                var fileName = result.FileName;

                // Note: manually ensure we are using correct extension
                fileName = Path.ChangeExtension(fileName, "csv");

                // Show encoding selection dialog
                if (EncodingViewModelType is null)
                {
                    throw new InvalidOperationException("Cannot find type 'SaveEncodingViewModel'");
                }

                var encodingViewModel = _viewModelFactory.CreateViewModel(EncodingViewModelType, null, null) as SaveEncodingViewModel;
                if (encodingViewModel is not null)
                {
                    // Pre-select the file's current encoding if known
                    encodingViewModel.DefaultCodePage = project.CodePage > 0 ? project.CodePage : 65001;

                    var dialogResult = await _uiVisualizerService.ShowDialogAsync(encodingViewModel);
                    if (dialogResult?.DialogResult != true)
                    {
                        return;
                    }

                    project.CodePage = encodingViewModel.SelectedEncoding?.CodePage ?? 65001;
                }

                await _projectManager.SaveAsync(project, fileName);
            }

            await base.ExecuteAsync(parameter);
        }
    }
}
