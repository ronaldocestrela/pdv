using System;

namespace Pdv.Maui;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; set; }

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ServiceProvider = serviceProvider;
        MainPage = new MainPage();
    }
}
