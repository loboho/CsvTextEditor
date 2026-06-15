namespace CsvTextEditor
{
    using System.Linq;
    using Models;
    using Orc.CsvTextEditor;

    public class CsvTextEditorInstanceProvider : ICsvTextEditorInstanceProvider
    {
        private readonly ICsvTextEditorInstanceManager _csvTextEditorInstanceManager;

        public CsvTextEditorInstanceProvider(ICsvTextEditorInstanceManager csvTextEditorInstanceManager)
        {
            _csvTextEditorInstanceManager = csvTextEditorInstanceManager;
        }

        public ICsvTextEditorInstance GetInstance(Project project)
        {
            if (project is null)
            {
                return null;
            }

            if (project.EditorId is not null)
            {
                return _csvTextEditorInstanceManager.GetInstance(project.EditorId);
            }

            // No EditorId set (file opened via command line / file association);
            // fall back to the first registered instance
            return _csvTextEditorInstanceManager.GetInstances().FirstOrDefault();
        }
    }
}
