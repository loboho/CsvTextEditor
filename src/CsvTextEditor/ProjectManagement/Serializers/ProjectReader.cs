namespace CsvTextEditor.ProjectManagement
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Models;
    using Orc.FileSystem;
    using Orc.Notifications;
    using Orc.ProjectManagement;

    public class ProjectReader : ProjectReaderBase
    {
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;

        public ProjectReader(IFileService fileService, INotificationService notificationService)
        {
            _fileService = fileService;
            _notificationService = notificationService;
        }

        protected override async Task<IProject> ReadFromLocationAsync(string location)
        {
            try
            {
                var bytes = await _fileService.ReadAllBytesAsync(location);
                var encoding = DetectEncoding(bytes);

                var text = encoding.GetString(bytes);
                if (HasBom(bytes))
                {
                    var preamble = encoding.GetPreamble();
                    if (preamble.Length > 0 && bytes.Length >= preamble.Length)
                    {
                        text = encoding.GetString(bytes, preamble.Length, bytes.Length - preamble.Length);
                    }
                }

                var project = new Project(location)
                {
                    Text = text,
                    CodePage = encoding.CodePage,
                    Separator = Project.DetectSeparatorFromExtension(location)
                };

                return project;
            }
            catch (IOException ex)
            {
                _notificationService.ShowNotification("Could not open file", ex.Message);
            }

            return null;
        }

        private static Encoding DetectEncoding(byte[] bytes)
        {
            // UTF-8 BOM: EF BB BF
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                return Encoding.UTF8;
            }

            // UTF-16 LE BOM: FF FE
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                return Encoding.Unicode;
            }

            // UTF-16 BE BOM: FE FF
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                return Encoding.BigEndianUnicode;
            }

            // No BOM: try UTF-8 first (check round-trip fidelity)
            try
            {
                var utf8Text = Encoding.UTF8.GetString(bytes);
                var roundTrip = Encoding.UTF8.GetBytes(utf8Text);

                if (bytes.AsSpan().SequenceEqual(roundTrip))
                {
                    return Encoding.UTF8;
                }
            }
            catch
            {
                // Invalid UTF-8 sequence, fall through to ANSI
            }

            // Fall back to system ANSI encoding (e.g., GBK on Chinese Windows)
            try
            {
                return Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
            }
            catch
            {
                return Encoding.UTF8;
            }
        }

        private static bool HasBom(byte[] bytes)
        {
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                return true;
            }

            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                return true;
            }

            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                return true;
            }

            return false;
        }
    }
}
