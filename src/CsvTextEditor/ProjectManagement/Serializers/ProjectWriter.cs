namespace CsvTextEditor.ProjectManagement
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Models;
    using Orc.FileSystem;
    using Orc.ProjectManagement;

    public class ProjectWriter : ProjectWriterBase<Project>
    {
        private readonly IFileService _fileService;
        private readonly ICsvTextEditorInstanceProvider _csvTextEditorInstanceProvider;

        public ProjectWriter(IFileService fileService, ICsvTextEditorInstanceProvider csvTextEditorInstanceProvider)
        {
            ArgumentNullException.ThrowIfNull(fileService);
            ArgumentNullException.ThrowIfNull(csvTextEditorInstanceProvider);

            _fileService = fileService;
            _csvTextEditorInstanceProvider = csvTextEditorInstanceProvider;
        }

        protected override Task<bool> WriteToLocationAsync(Project project, string location)
        {
            var encoding = GetEncoding(project);
            var bytes = encoding.GetBytes(project.Text);
            _fileService.WriteAllBytes(location, bytes);

            var csvTextEditorInstance = _csvTextEditorInstanceProvider.GetInstance(project);
            csvTextEditorInstance.ResetIsDirty();

            return Task.FromResult<bool>(true);
        }

        private static Encoding GetEncoding(Project project)
        {
            if (project.CodePage > 0)
            {
                try
                {
                    return Encoding.GetEncoding(project.CodePage);
                }
                catch
                {
                    // Fall through to default
                }
            }

            return Encoding.UTF8;
        }
    }
}
