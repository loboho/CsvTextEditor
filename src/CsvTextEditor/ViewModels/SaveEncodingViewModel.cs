namespace CsvTextEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel.MVVM;

    /// <summary>
    /// Represents a single encoding option shown in the save encoding dialog.
    /// </summary>
    public class SaveEncodingOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveEncodingOption"/> class.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <param name="codePage">The code page identifier.</param>
        public SaveEncodingOption(string displayName, int codePage)
        {
            DisplayName = displayName;
            CodePage = codePage;
        }

        /// <summary>
        /// Gets the display name shown in the UI.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the .NET code page identifier.
        /// </summary>
        public int CodePage { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return DisplayName;
        }
    }

    /// <summary>
    /// ViewModel for the encoding selection dialog shown during Save / Save As.
    /// </summary>
    public class SaveEncodingViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveEncodingViewModel"/> class.
        /// </summary>
        public SaveEncodingViewModel(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Title = "Select Encoding";
        }

        /// <summary>
        /// Gets the list of available encoding options.
        /// </summary>
        public List<SaveEncodingOption> AvailableEncodings { get; private set; }

        /// <summary>
        /// Gets or sets the currently selected encoding option.
        /// </summary>
        public SaveEncodingOption SelectedEncoding { get; set; }

        /// <summary>
        /// Gets or sets the default code page to pre-select.
        /// Set this before the dialog is shown to pre-select the file's current encoding.
        /// </summary>
        public int DefaultCodePage { get; set; } = 65001;

        /// <inheritdoc />
        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            AvailableEncodings = new List<SaveEncodingOption>
            {
                new("UTF-8", 65001),
                new("ANSI", CultureInfo.CurrentCulture.TextInfo.ANSICodePage)
            };

            SelectedEncoding = AvailableEncodings.FirstOrDefault(x => x.CodePage == DefaultCodePage)
                               ?? AvailableEncodings[0];
        }
    }
}
