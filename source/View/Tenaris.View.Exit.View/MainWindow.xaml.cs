using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;    

namespace Tenaris.View.Exit.View
{
    using Tenaris.View.Exit.ViewModel;
    using Tenaris.Library.Log;
    using System.Globalization;
    using System.Windows.Markup;
    using System.Threading;
   


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ExitViewModel exitViewModel;
        ResourceDictionary resourceDictionary;

        
        public MainWindow()
        {
            InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            exitViewModel = (ExitViewModel)this.DataContext;
            exitViewModel.GridHeight = (int)this.ActualHeight - exitViewModel.WHeight;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                LoadDynamicResources("Resources/ProductView.xaml");
                LoadDynamicResources("Resources/BundleView_TT32.xaml");
                LoadDynamicResources("Resources/BundleRView.xaml");
                LoadDynamicResources("Resources/ItemView.xaml");
                LoadDynamicResources("Resources/Filters.xaml");
                LoadDynamicResources("Resources/BundleHData.xaml");

                var cultureName = CultureInfo.CurrentCulture.Name;
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(cultureName)));

                Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;


            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message);
            }
        }


        private void LoadDynamicResources(string DynamicResourcesPath)
        {
            try
            {

                if (File.Exists(DynamicResourcesPath))
                {
                    using (var stream = new FileStream(DynamicResourcesPath, FileMode.Open))
                    {
                        resourceDictionary = (ResourceDictionary)System.Windows.Markup.XamlReader.Load(stream);
                    }
                }
                Resources.MergedDictionaries.Add(resourceDictionary);
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message);
            }
        }
    }
}
