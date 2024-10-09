﻿using CommunityToolkit.Maui;
using KoOrderRegister.Modules.Customer.Pages;
using KoOrderRegister.Modules.Customer.ViewModels;
using KoOrderRegister.Modules.Database.Services;
using KoOrderRegister.Modules.Order.Pages;
using KoOrderRegister.Modules.Order.List.Services;
using KoOrderRegister.Modules.Order.List.ViewModels;
using KoOrderRegister.Modules.Order.ViewModels;
using KoOrderRegister.Utility;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using KoOrderRegister.Modules.Settings.Pages;
using KoOrderRegister.Modules.Settings.ViewModels;
using System.Globalization;
using KoOrderRegister.Localization.SupportedLanguage;
using KoOrderRegister.Services;
using KoOrderRegister.Modules.DatabaseFile.Page;
using KoOrderRegister.Modules.DatabaseFile.ViewModel;
using KoOrderRegister.Modules.Export.Services;
using KoOrderRegister.Modules.Export.Excel.Services;

namespace KoOrderRegister
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureMopups()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddTransient<IDatabaseModel, DatabaseModel>();

            #region Order Modul

            builder.Services.AddTransient<OrderListPage>();
            builder.Services.AddTransient<OrderListViewModel>();

            builder.Services.AddTransient<OrderDetailsPage>();
            builder.Services.AddTransient<OrderDetailViewModel>();

            builder.Services.AddSingleton<IFileService, FileService>();

            #endregion

            #region Customer Modul

            builder.Services.AddTransient<CustomerListPage>();
            builder.Services.AddTransient<CustomerListViewModel>();

            builder.Services.AddTransient<PersonDetailsPage>();
            builder.Services.AddTransient<PersonDetailsViewModel>();


            #endregion

            #region Settings Modul
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddSingleton<SettingsViewModel>();
            #endregion

            #region Update
            builder.Services.AddHttpClient("GitHubClient", client =>
            {
                
            });
            builder.Services.AddSingleton<IAppUpdateService, AppUpdateService>();
#if WINDOWS
                builder.Services.AddSingleton<IVersionService, KoOrderRegister.Platforms.Windows.Services.VersionService>();
#elif ANDROID
            builder.Services.AddSingleton<IVersionService, KoOrderRegister.Platforms.Android.Services.VersionService>();
            builder.Services.AddSingleton<KoOrderRegister.Platforms.Android.Services.IPermissions, KoOrderRegister.Platforms.Android.Services.Permissions>();

#else
            builder.Services.AddSingleton<IAppUpdateService, AppUpdateService>();
#endif

            #endregion

            #region File Modul
            builder.Services.AddTransient<FilePropertiesPopup>();
            builder.Services.AddTransient<FilePropertiesViewModel>();
            #endregion

            #region DatabaseFile Export to File
            builder.Services.AddTransient<IExcelExportService, ExcelExportService>();
            #endregion

#if DEBUG
            builder.Logging.AddDebug();
#endif
            #region Language settings
            ILanguageSettings languageSettings = LanguageManager.GetCurrentLanguage();
            languageSettings.SetRegioSpecification();
            CultureInfo culture = new CultureInfo(languageSettings.GetCultureName());
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            #endregion

            return builder.Build();
        }
    }
}
