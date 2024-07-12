using Microsoft.Extensions.DependencyInjection;
using SaleFarm.Services;
using SaleFarm.ViewModels;
using SaleFarm.Views;
using System;
using System.Windows;

namespace SaleFarm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddHttpClient();
            services.AddSingleton<AsfHttpService>();

            services.AddSingleton<MainWindowViewModel>();

            services.AddSingleton<MainWindowView>(s => new MainWindowView(s.GetRequiredService<MainWindowViewModel>()));
            //{
            //    DataContext = s.GetRequiredService<MainWindowViewModel>()
            //});

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow = _serviceProvider.GetRequiredService<MainWindowView>();
            MainWindow.Show();

            base.OnStartup(e);
        }
    }
}
