namespace CsvTextEditor.ViewModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel.Fody;
    using Catel.MVVM;
    using Models;
    using Orc.CsvTextEditor;
    using Orc.ProjectManagement;

    public class MainViewModel : FeaturedViewModelBase
    {
        private readonly IProjectManager _projectManager;
        private readonly ICsvTextEditorInstanceProvider _csvTextEditorInstanceProvider;
        private readonly ICsvTextEditorInstanceManager _csvTextEditorInstanceManager;
        private bool _pendingTextSet;

        public MainViewModel(IProjectManager projectManager, IServiceProvider serviceProvider,
            ICsvTextEditorInstanceProvider csvTextEditorInstanceProvider, 
            ICsvTextEditorInstanceManager csvTextEditorInstanceManager)
            : base(serviceProvider)
        {
            _projectManager = projectManager;
            _csvTextEditorInstanceProvider = csvTextEditorInstanceProvider;
            _csvTextEditorInstanceManager = csvTextEditorInstanceManager;
        }
        
        [Model]
        [Expose(nameof(Models.Project.Text))]
        [Expose(nameof(Models.Project.Separator))]
        public Project Project { get; set; }

        protected override Task InitializeAsync()
        {
            _projectManager.ProjectActivationAsync += OnProjectActivationAsync;
            _csvTextEditorInstanceManager.InstanceRegistered += OnInstanceRegistered;

            var existingInstance = _csvTextEditorInstanceManager.GetInstances().FirstOrDefault();
            if (existingInstance is not null)
            {
                OnInstanceRegistered(null, new CsvTextEditorEventArgs(existingInstance));
            }

            return base.InitializeAsync();
        }

        protected override Task OnClosedAsync(bool? result)
        {
            _projectManager.ProjectActivationAsync -= OnProjectActivationAsync;

            return base.OnClosedAsync(result);
        }

        private async Task OnProjectActivationAsync(object sender, ProjectUpdatingCancelEventArgs e)
        {
            var newProject = (Project)e.NewProject;
            Project = newProject;

            _pendingTextSet = false;

            try
            {
                TrySetInitialText();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void OnInstanceRegistered(object sender, CsvTextEditorEventArgs e)
        {
            TrySetInitialText();
        }

        private void TrySetInitialText()
        {
            if (_pendingTextSet || Project is null)
            {
                return;
            }

#pragma warning disable IDISP001 //: Dispose created
            var instance = _csvTextEditorInstanceProvider.GetInstance(Project);
#pragma warning restore IDISP001 //: Dispose created

            if (instance?.GetEditor() is not null)
            {
                instance.SetInitialText(Project.Text ?? string.Empty);
                _pendingTextSet = true;
            }
        }

        protected override Task CloseAsync()
        {
            _projectManager.ProjectActivationAsync -= OnProjectActivationAsync;
            _csvTextEditorInstanceManager.InstanceRegistered -= OnInstanceRegistered;

            return base.CloseAsync();
        }
    }
}
