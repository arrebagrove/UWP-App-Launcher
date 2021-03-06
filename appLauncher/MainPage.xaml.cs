﻿using appLauncher.Control;
using appLauncher.Helpers;
using appLauncher.Model;
using appLauncher.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace appLauncher
{
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int maxRows;
        public ObservableCollection<finalAppItem> finalApps;
        public static FlipViewItem flipViewTemplate;
        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        bool pageIsLoaded = false;

        public MainPage()
        {
            this.InitializeComponent();
            //allApps = packageHelper.getAllApps(); 
            finalApps = packageHelper.getAllApps();
            Debug.WriteLine("Successfully got all app packages");
            
        }


        

        private async void appGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedApp = (finalAppItem)e.ClickedItem;
            await clickedApp.appEntry.LaunchAsync();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
            maxRows = (int)(appGridView.ActualHeight / 124);
            int appsPerScreen = 4 * maxRows;
            int additionalPagesToMake = calculateExtraPages(appsPerScreen) - 1;
            int fullPages = additionalPagesToMake;
            int appsLeftToAdd = finalApps.Count() - (fullPages * appsPerScreen);
            DataTemplate theTemplate = appGridView.ItemTemplate;
            GridView defaultGridView = appGridView;
            defaultGridView.Name = string.Empty;

            if (additionalPagesToMake > 0)
            {
                ControlTemplate template = new appControl().Template;

                for (int i = 0; i < additionalPagesToMake; i++)
                {
                    screensContainerFlipView.Items.Add(new FlipViewItem()
                    {
                        Content = new GridView()
                        {
                            ItemTemplate = theTemplate,
                            ItemsPanel = appGridView.ItemsPanel,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            IsItemClickEnabled = true,
                            Margin = new Thickness(0,10,0,0)

                        }


                    });


                    int j = i + 2;
                    {
                        var flipViewItem = (FlipViewItem)screensContainerFlipView.Items[j];
                        var gridView = (GridView)flipViewItem.Content;
                        //disableScrollViewer(gridView);
                        gridView.ItemClick += appGridView_ItemClick;
                    }

                }
                int start = 0;
                int end = appsPerScreen;

                for (int j = 1; j < fullPages + 1; j++)
                {

                    FlipViewItem screen = (FlipViewItem)screensContainerFlipView.Items[j];
                    GridView gridOfApps = (GridView)screen.Content;
                    addItemsToGridViews(gridOfApps, start, end);
                    if (j == 1)
                    {
                        start = appsPerScreen + 1;
                        end += appsPerScreen + 1;
                    }
                    else
                    {
                        start += appsPerScreen;
                        end += appsPerScreen;
                    }
                }


                int startOfLastAppsToAdd = finalApps.Count() - appsLeftToAdd;


                FlipViewItem finalScreen = (FlipViewItem)screensContainerFlipView.Items[additionalPagesToMake + 1];
                GridView finalGridOfApps = (GridView)finalScreen.Content;
                addItemsToGridViews(finalGridOfApps, startOfLastAppsToAdd, finalApps.Count());
                screensContainerFlipView.SelectedItem = screensContainerFlipView.Items[1];
            }
            else
            {
                for (int i = 0; i < finalApps.Count() - 1; i++)
                {
                    appGridView.Items.Add(finalApps[i]);
                }
            }
            loadSettings();
            pageIsLoaded = true;
            screensContainerFlipView.SelectionChanged += screensContainerFlipView_SelectionChanged;

           
            

        }

        private async void loadSettings()
        {
            if ((string)App.localSettings.Values["bgImageAvailable"] == "1")
            {
                //load background image
                var bgImageFolder = await localFolder.GetFolderAsync("backgroundImage");
                var filesInBgImageFolder = await bgImageFolder.GetFilesAsync();
                var bgImageFile = filesInBgImageFolder.First();
                rootGrid.Background = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(bgImageFile.Path)),
                    Stretch = Stretch.UniformToFill
                };

            }




        }

        private void disableScrollViewer(GridView gridView)
        {
            try
            {
                var border = (Border)VisualTreeHelper.GetChild(gridView, 0);
                var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.IsVerticalRailEnabled = false;
                scrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }

            catch(Exception e)
            {

            }
        }

        private int calculateExtraPages(int appsPerScreen)
        {
            double appsPerScreenAsDouble = appsPerScreen;
            double numberOfApps = finalApps.Count();
            int pagesToMake = (int)Math.Ceiling(numberOfApps / appsPerScreenAsDouble);
            return pagesToMake;
        }

        private void addItemsToGridViews(GridView gridOfApps, int start, int end)
        {
            for (int k = start; k < end; k++)
            {
                gridOfApps.Items.Add(finalApps[k]);
            }
        }

        private void dockGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //TODO: try cast object as appItem then launch the app
        }

        private void settingsItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.WriteLine("You clicked on the settings icon");
            Frame.Navigate(typeof(settings));
        }

        private async void screensContainerFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pageIsLoaded)
            {
                if (screensContainerFlipView.SelectedIndex == 0)
                {                
                    await Launcher.LaunchUriAsync(new Uri("ms-cortana://"));
                    screensContainerFlipView.SelectedIndex = 1;
                }
            }
        }

        private void Page_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            for (int i = 1; i < screensContainerFlipView.Items.Count; i++)
            {
                
                FlipViewItem screen = (FlipViewItem)screensContainerFlipView.Items[i];
                GridView gridOfApps = (GridView)screen.Content;
                disableScrollViewer(gridOfApps);
            }
        }
    }
}
