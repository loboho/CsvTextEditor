namespace CsvTextEditor
{
    using System;
    using System.Threading.Tasks;
    using Catel.MVVM;
    using Models;
    using Orc.ProjectManagement;

    public class FileSaveCommandContainer : ProjectCommandContainerBase
    {
        public FileSaveCommandContainer(ICommandManager commandManager, 
            IProjectManager projectManager, IServiceProvider serviceProvider)
            : base(Commands.File.Save, commandManager, projectManager, serviceProvider)
        {
        }

        public override async Task ExecuteAsync(object parameter)
        {
            await base.ExecuteAsync(parameter);

            if (_projectManager.ActiveProject is Project project)
            {
                if (string.IsNullOrEmpty(project.Location))
                {
                    // First-time save: redirect to Save As flow (file dialog + encoding selection)
                    _commandManager.ExecuteCommand(Commands.File.SaveAs);
                    return;
                }

                await _projectManager.SaveAsync(project);
            }
        }
    }
}
