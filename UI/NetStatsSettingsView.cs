using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using WindowSill.API;
using System;

namespace NetStats;

internal static class NetStatsSettingsView
{
    public static SillSettingsView Create(NetworkStatsService service, ISettingsProvider settingsProvider)
    {
        // Create main container
        var container = new StackPanel { Spacing = 16 };

        // Add unit selector
        var unitPanel = new StackPanel { Spacing = 8 };
        var unitLabel = new TextBlock { Text = "Speed Unit:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold };
        var unitComboBox = new ComboBox
        {
            ItemsSource = new[] { "Mbps", "Kbps", "MBps", "KBps" },
            SelectedItem = settingsProvider.GetSetting(NetStatsSill.SpeedUnitSetting) ?? "Mbps",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        // Handle selection changes
        unitComboBox.SelectionChanged += (s, e) =>
        {
            if (unitComboBox.SelectedItem is string selectedUnit)
            {
                settingsProvider.SetSetting(NetStatsSill.SpeedUnitSetting, selectedUnit);
            }
        };
        
        unitPanel.Children.Add(unitLabel);
        unitPanel.Children.Add(unitComboBox);
        container.Children.Add(unitPanel);

        // Add separator
        container.Children.Add(new Microsoft.UI.Xaml.Shapes.Rectangle 
        { 
            Height = 1, 
            Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray) { Opacity = 0.2 },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 8, 0, 8)
        });

        // Add interfaces label
        var interfacesLabel = new TextBlock 
        { 
            Text = "Network Interfaces:", 
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        };
        container.Children.Add(interfacesLabel);

        var listView = new ListView();
        listView.ItemsSource = service.Interfaces;

        // Create DataTemplate using XamlReader
        // Note: We use StringFormat to add Rx:/Tx: prefixes. 
        // In XAML string, {} are special, so we escape them as \{ \}.
        string xaml = @"
            <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                          xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                <Grid ColumnDefinitions='32, *, Auto'>
                    <CheckBox Grid.Column='0' IsChecked='{Binding IsSelected, Mode=TwoWay}' VerticalAlignment='Center' Width='32' HorizontalAlignment='Left' Margin='0,0,8,0'/>
                    <StackPanel Grid.Column='1' VerticalAlignment='Center'>
                        <TextBlock Text='{Binding Name}' FontWeight='SemiBold'/>
                        <TextBlock Text='{Binding Description}' FontSize='12' Opacity='0.7'/>
                    </StackPanel>
                    <StackPanel Grid.Column='2' VerticalAlignment='Center' HorizontalAlignment='Right'>
                        <TextBlock Text='{Binding CurrentSpeed}' HorizontalAlignment='Right' FontSize='12'/>
                        <StackPanel Orientation='Horizontal' HorizontalAlignment='Right' Spacing='8'>
                            <TextBlock Text='{Binding RxTotal}' FontSize='10' Opacity='0.6'/>
                            <TextBlock Text='{Binding TxTotal}' FontSize='10' Opacity='0.6'/>
                        </StackPanel>
                    </StackPanel>
                </Grid>
            </DataTemplate>";

        listView.ItemTemplate = (DataTemplate)XamlReader.Load(xaml);
        listView.SelectionMode = ListViewSelectionMode.None;
        container.Children.Add(listView);

        return new SillSettingsView(
            "NetStats Settings",
            new Lazy<FrameworkElement>(() => container)
        );
    }
}
