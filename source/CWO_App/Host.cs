using CWO_App.config;
using CWO_App.UI.Commands;
using CWO_App.UI.Services;
using CWO_App.UI.ViewModels;
using CWO_App.UI.ViewModels.ApartmentParameters;
using CWO_App.UI.ViewModels.ApartmentValidation;
using CWO_App.UI.ViewModels.KeynotesCreationViewModel;
using CWO_App.UI.ViewModels.SharedParametersViewModels;
using CWO_App.UI.Views;
using CWO_App.UI.Views.ApartmentParametersViews;
using CWO_App.UI.Views.ApartmentValidationViews;
using CWO_App.UI.Views.KeynotesCreationViews;
using CWO_App.UI.Views.SharedParameterViews;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Reflection;


namespace CWO_App
{
    /// <summary>
    ///     Provides a host for the application's services and manages their lifetimes
    /// </summary>
    public static class Host
    {
        private static IHost _host;

        public static void Start()
        {
            var builder = new HostApplicationBuilder(new HostApplicationBuilderSettings
            {
                ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                DisableDefaults = true
            });

            //logging
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilogConfiguration();

            builder.Services.AddSerializerOptions();

            //add here services like views and VM
             builder.Services.AddTransient<IWindowService, WindowService>();


            builder.Services.AddTransient<BrickEvaluatorShowWindow>();
            builder.Services.AddTransient<BrickWindow>();
            builder.Services.AddTransient<BrickEvaluator_ViewModel>();


            builder.Services.AddTransient<FamilyParameters_ViewModel>();
            builder.Services.AddTransient<FamilyParameters_Window>();
            builder.Services.AddTransient<FamilyParametersShowWindow>();

            builder.Services.AddTransient<ApartmentValidation_ViewModel>();
            builder.Services.AddTransient<ApartmentValidation_Window>();
            builder.Services.AddTransient<ApartmentValidationShowWindow>();

            builder.Services.AddTransient<KeynotesCreation_ViewModel>();
            builder.Services.AddTransient<KeynotesCreation_Window>();
            builder.Services.AddTransient<KeynotesCreationShowWindow>();

            builder.Services.AddTransient<ApartmentParameters_ViewModel>();
            builder.Services.AddTransient<ApartmentParameters_Window>();
            builder.Services.AddTransient<ApartmentParametersShowWindow>();

            _host = builder.Build();
            _host.Start();
        }

        /// <summary>
        ///     Stops the host
        /// </summary>
        public static void Stop()
        {
            _host.StopAsync();
        }

        /// <summary>
        ///     Gets a service of the specified type
        /// </summary>
        /// <typeparam name="T">The type of service object to get</typeparam>
        /// <returns>A service object of type T or null if there is no such service</returns>
        public static T GetService<T>() where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }
    }
}
