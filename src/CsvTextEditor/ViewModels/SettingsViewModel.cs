namespace CsvTextEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel.Configuration;
    using Catel.MVVM;
    using Catel.Services;
    using Orc.Squirrel;
    using Orchestra;

    public class LanguageItem
    {
        public string CultureCode { get; set; }
        public string DisplayName { get; set; }
    }

    public class SettingsViewModel : ViewModelBase
    {
        private readonly IConfigurationService _configurationService;
        private readonly IManageAppDataService _manageAppDataService;
        private readonly IUpdateService _updateService;
        private readonly IOpenFileService _openFileService;
        private readonly ILanguageService _languageService;

        public SettingsViewModel(IConfigurationService configurationService, 
            IManageAppDataService manageAppDataService, IUpdateService updateService, 
            IOpenFileService openFileService, ILanguageService languageService,
            IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _configurationService = configurationService;
            _manageAppDataService = manageAppDataService;
            _updateService = updateService;
            _openFileService = openFileService;
            _languageService = languageService;

            PickEditor = new TaskCommand(serviceProvider, PickEditorExecuteAsync);
            OpenApplicationDataDirectory = new Command(serviceProvider, OnOpenApplicationDataDirectoryExecute);
            BackupUserData = new TaskCommand(serviceProvider, OnBackupUserDataExecuteAsync);

            Title = "Settings";

            AvailableLanguages = new List<LanguageItem>
            {
                new() { CultureCode = "en-US", DisplayName = "English" },
                new() { CultureCode = "zh-CN", DisplayName = "中文 (简体)" },
            };
        }

        public bool IsUpdateSystemAvailable { get; private set; }
        public bool CheckForUpdates { get; set; }
        public bool AutoSaveEditor { get; set; }
        public string CustomEditor { get; private set; }
        public List<UpdateChannel> AvailableUpdateChannels { get; private set; }
        public UpdateChannel UpdateChannel { get; set; }

        public List<LanguageItem> AvailableLanguages { get; private set; }

        public LanguageItem SelectedLanguage { get; set; }

        public Command OpenApplicationDataDirectory { get; private set; }

        private void OnOpenApplicationDataDirectoryExecute()
        {
            _manageAppDataService.OpenApplicationDataDirectory(Catel.IO.ApplicationDataTarget.UserRoaming);
        }

        public TaskCommand PickEditor { get; private set; }

        private async Task PickEditorExecuteAsync()
        {
            var result = await _openFileService.DetermineFileAsync(new DetermineOpenFileContext
            {
                Filter = "Program Files (*.exe)|*exe",
                IsMultiSelect = false
            });

            if (result.Result)
            {
                CustomEditor = result.FileName;
            }       
        }

        public TaskCommand BackupUserData { get; private set; }

        private async Task OnBackupUserDataExecuteAsync()
        {
            await _manageAppDataService.BackupUserDataAsync(Catel.IO.ApplicationDataTarget.UserRoaming);
        }

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            IsUpdateSystemAvailable = _updateService.IsUpdateSystemAvailable;
            CheckForUpdates = _updateService.IsCheckForUpdatesEnabled;
            AvailableUpdateChannels = new List<UpdateChannel>(_updateService.AvailableChannels);
            UpdateChannel = _updateService.CurrentChannel;

            CustomEditor = _configurationService.GetRoamingValue<string>(Configuration.CustomEditor);
            AutoSaveEditor = _configurationService.GetRoamingValue(Configuration.AutoSaveEditor, Configuration.AutoSaveEditorDefaultValue);

            var savedLanguage = _configurationService.GetRoamingValue(Configuration.Language, Configuration.LanguageDefaultValue);
            SelectedLanguage = AvailableLanguages.FirstOrDefault(x => x.CultureCode == savedLanguage) ?? AvailableLanguages[0];
        }

        protected override async Task<bool> SaveAsync()
        {
            _updateService.IsCheckForUpdatesEnabled = CheckForUpdates;
            _updateService.CurrentChannel = UpdateChannel;

            _configurationService.SetRoamingValue(Configuration.CustomEditor, CustomEditor);
            _configurationService.SetRoamingValue(Configuration.AutoSaveEditor, AutoSaveEditor);

            if (SelectedLanguage is not null)
            {
                _configurationService.SetRoamingValue(Configuration.Language, SelectedLanguage.CultureCode);
                _languageService.PreferredCulture = new CultureInfo(SelectedLanguage.CultureCode);
            }

            return await base.SaveAsync();
        }
    }
}
