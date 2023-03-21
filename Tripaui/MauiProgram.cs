using CommunityToolkit.Maui;
using DevExpress.Maui;
using GoogleApi.Extensions;
using Microsoft.Extensions.Configuration;
using Repositories;
using Services;
using System.Reflection;
using Tripaui.Abstractions;
using Tripaui.Platforms;
using Tripaui.Services;
using Tripaui.ViewModels;
using Tripaui.ViewModels.Places;
using Tripaui.ViewModels.Trips;
using Tripaui.Views;
using Tripaui.Views.Places;
using Tripaui.Views.Trips;

namespace Tripaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        var a = Assembly.GetExecutingAssembly();
        using var stream = a.GetManifestResourceStream("Tripaui.appsettings.json");
        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();
        builder.Configuration.AddConfiguration(config);
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .UseDevExpress()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Kalam-Regular.ttf", "KalamRegular");
                fonts.AddFont("Kalam-Bold.ttf", "KalamBold");
                fonts.AddFont("Kalam-Light.ttf", "KalamLight");
                fonts.AddFont("fa_solid.ttf", "FontAwesome");
                fonts.AddFont("materialdesignicons-webfont.ttf", "MaterialDesignIcons");
            });

        builder.Services.AddSingleton(Connectivity.Current);
        builder.Services.ConfigureViews();
        builder.Services.ConfigureViewModels();
        builder.Services.ConfigureServices();
        builder.Services.ConfigureRepositories();
        builder.Services.AddSingleton<ISpeechToText, SpeechToText>();
        builder.Services.AddGoogleApiClients();

        return builder.Build();
    }

    private static void ConfigureViews(this IServiceCollection services)
    {
        services.AddTransient<LoginPage>();
        services.AddTransient<RegisterPage>();

        services.AddSingleton<PlacesListPage>();
        services.AddTransient<PlaceDetailsPage>();

        services.AddSingleton<MyTripsPage>();
        services.AddTransient<ChooseTripPage>();
        services.AddTransient<MapPage>();
        services.AddTransient<TripDetailsPage>();
        services.AddTransient<AddTripPage>();
        services.AddTransient<EditTripPage>();
        services.AddTransient<PlanTripPage>();
        services.AddTransient<ReviewsPage>();
        services.AddTransient<ArchiveTripsPage>();
    }

    private static void ConfigureViewModels(this IServiceCollection services)
    {
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();

        services.AddSingleton<PlaceListViewModel>();
        services.AddTransient<PlaceDetailsViewModel>();

        services.AddSingleton<MyTripsViewModel>();
        services.AddSingleton<ChooseTripViewModel>();
        services.AddTransient<MapViewModel>();
        services.AddTransient<TripDetailsViewModel>();
        services.AddTransient<AddTripViewModel>();
        services.AddTransient<EditTripViewModel>();
        services.AddTransient<PlanTripViewModel>();
        services.AddTransient<ReviewsViewModel>();
        services.AddTransient<ArchiveTripsViewModel>();
    }
    private static void ConfigureServices(this IServiceCollection services)
    {
        services.AddSingleton<AuthService>();
        services.AddSingleton<UsersService>();
        services.AddSingleton<PlacesService>();
        services.AddSingleton<TripsService>();
        services.AddSingleton<DirectionsService>();
        services.AddSingleton<ReviewsService>();
        services.AddSingleton<GeolocationService>();
        services.AddSingleton<RecommendationService>();
    }

    private static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddSingleton<UsersRepository>();
        services.AddSingleton<TripsRepository>();
        services.AddSingleton<PlanRepository>();
        services.AddSingleton<ReviewsRepository>();
        services.AddSingleton<RecoRepository>();
        services.AddSingleton<IRepositoryContext>((e) => new RepositoryContext());
    }
}