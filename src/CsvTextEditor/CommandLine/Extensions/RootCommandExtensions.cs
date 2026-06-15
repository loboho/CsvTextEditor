namespace CsvTextEditor.CommandLine
{
    public static class RootCommandExtensions
    {
        public static ProjectCommandContext GetProjectCommandContext(this IRootCommand rootCommand)
        {
            var parseResult = rootCommand.Parse();

            // First try --project option, then fall back to positional argument (used by file association / double-click)
            var project = parseResult.GetValue<string?>("--project")
                ?? parseResult.GetValue<string?>("project");

            var context = new ProjectCommandContext
            {
                Project = project
            };

            return context;
        }
    }
}
