using System;
using System.Reflection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace Tripaui.Views.Controlls;

public partial class TopSheet : ContentView
{
    private const uint shortDuration = 250u;
    private const uint regularDuration = shortDuration * 2u;

    public IList<Microsoft.Maui.IView> TopSheetContent => TopSheetContentGrid.Children;

    #region Bindable Properties

    public static readonly BindableProperty SheetHeightProperty = BindableProperty.Create(
        nameof(SheetHeight),
        typeof(double),
        typeof(TopSheet),
        360d,
        BindingMode.OneWay,
        validateValue: (_, value) => value != null);

    public double SheetHeight
    {
        get => (double)GetValue(SheetHeightProperty);
        set => SetValue(SheetHeightProperty, value);
    }

    public static readonly BindableProperty HeaderTextProperty = BindableProperty.Create(
        nameof(HeaderText),
        typeof(string),
        typeof(TopSheet),
        string.Empty,
        BindingMode.OneWay,
        validateValue: (_, value) => value != null);

    public string HeaderText
    {
        get => (string)GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }

    public static readonly BindableProperty HeaderStyleProperty = BindableProperty.Create(
        nameof(HeaderStyle),
        typeof(Style),
        typeof(TopSheet),
        new Style(typeof(Label))
        {
            Setters =
            {
                new Setter
                {
                    Property = Label.FontSizeProperty,
                    Value = 24
                }
            }
        },
        BindingMode.OneWay,
        validateValue: (_, value) => value != null);

    public Style HeaderStyle
    {
        get => (Style)GetValue(HeaderStyleProperty);
        set => SetValue(HeaderStyleProperty, value);
    }

    public static readonly BindableProperty ThemeProperty = BindableProperty.Create(
        nameof(Theme),
        typeof(DisplayTheme),
        typeof(TopSheet),
        DisplayTheme.Light,
        BindingMode.OneWay,
        validateValue: (_, value) => value != null,
        propertyChanged:
        (bindableObject, oldValue, newValue) =>
        {
            if (newValue is not null && bindableObject is TopSheet sheet && newValue != oldValue)
            {
                sheet.UpdateTheme();
            }
        });

    public DisplayTheme Theme
    {
        get => (DisplayTheme)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    private void UpdateTheme()
    {
        MainContent.BackgroundColor = Color.FromArgb(Theme == DisplayTheme.Dark ? "#333333" : "#FFFFFF");
        MainContent.Stroke = Color.FromArgb(Theme == DisplayTheme.Dark ? "#333333" : "#FFFFFF");
    }

    #endregion

    public TopSheet()
    {
        InitializeComponent();

        //Set the Theme
        UpdateTheme();

        this.Margin = new Thickness(0, -1 * SheetHeight, 0, 0);
        //Set Close Icon from Local Resource
        //CloseTopSheetButton.Source = ImageSource.FromResource($"Tripaui.Resources.Images.icnmenuclose.png");
    }

    public async Task OpenTopSheet()
    {
        this.InputTransparent = false;
        BackgroundFader.IsVisible = true;

        _ = BackgroundFader.FadeTo(1, shortDuration, Easing.SinInOut);
        await 
            MainContent.TranslateTo(0, SheetHeight, regularDuration, Easing.SinInOut);
    }

    public async Task CloseTopSheet()
    {
        _ = MainContent.TranslateTo(-1 * SheetHeight, 0, shortDuration, Easing.SinInOut);

        await BackgroundFader.FadeTo(0, shortDuration, Easing.SinInOut);

        BackgroundFader.IsVisible = true;
        this.InputTransparent = true;
    }

}