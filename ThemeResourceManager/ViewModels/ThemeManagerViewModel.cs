using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ThemeResourceManager.BaseClass;
using ThemeResourceManager.DataModel;
using MessageBox = System.Windows.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace ThemeResourceManager.ViewModels
{
    public class ThemeManagerViewModel:BaseViewModel
    {
        private ObservableCollection<Theme> _themes;
        private bool _showAddResourcePopup;
        private string _addedResourceKey;
        private ResourceType _addedResourceSelectedType;
        private List<ResourceType> _types;
        private ObservableCollection<Resource> _addedResourceValues;

        public ThemeManagerViewModel()
        {
            Themes=new ObservableCollection<Theme>();
            Types=new List<ResourceType> {ResourceType.Bursh,ResourceType.Geometry,ResourceType.Image};
            AddedResourceSelectedType=ResourceType.Bursh;
            LoadThemes();
        }

        private void LoadThemes()
        {
            Themes.Add(new Theme {Key = "Test"});
        }

        public ObservableCollection<Theme> Themes
        {
            get { return _themes; }
            set
            {
                _themes = value;
                OnPropertyChanged();
            }
        }


        public bool ShowAddResourcePopup
        {
            get { return _showAddResourcePopup; }
            set
            {
                _showAddResourcePopup = value;
                OnPropertyChanged();
            }
        }

        public string AddedResourceKey
        {
            get { return _addedResourceKey; }
            set
            {
                _addedResourceKey = value; 
                OnPropertyChanged();
            }
        }

        public ResourceType AddedResourceSelectedType
        {
            get { return _addedResourceSelectedType; }
            set
            {
                _addedResourceSelectedType = value; 
                OnPropertyChanged();
            }
        }

        public List<ResourceType> Types
        {
            get { return _types; }
            set
            {
                _types = value; 
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Resource> AddedResourceValues
        {
            get { return _addedResourceValues; }
            set
            {
                _addedResourceValues = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand AddThemeCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var newTheme = new Theme {Key = "New theme"};
                    if (Themes.Any())
                    {
                        foreach (var resource in Themes.First().Resources)
                        {
                            newTheme.Resources.Add(new Resource(resource));
                        }
                    }
                    Themes.Add(newTheme);
                });
            }
        }

        public DelegateCommand ImportResourceDictionaries
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var openDialog = new FolderBrowserDialog();
                    var result = openDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                      var files=  Directory.GetFiles(openDialog.SelectedPath);
                        var xamlFiles = files.Where(x => Path.GetExtension(x) == ".xaml").ToList();
                        var allBrushes = new List<Resource>();
                        Themes=new ObservableCollection<Theme>();
                        foreach (var file in xamlFiles)
                        {
                            Themes.Add(new Theme { Key = Path.GetFileName(file) });
                        }
                        foreach (var file in xamlFiles)
                        {
                            var brushes = new List<Resource>();
                            var data = File.ReadAllLines(file);
                            if (!data.Any(x => x.Contains("ResourceDictionary")))
                            {
                                break;
                            }
                            foreach (var resource in data.Where(x => x.Contains("SolidColorBrush")))
                            {
                                var segments = resource.Split('"').ToList();
                                var keyIndex = segments.IndexOf("Key") + 2;
                                var key = segments[keyIndex];
                                var value = segments.FirstOrDefault(x => x.Contains("#"));
                                if (allBrushes.All(x => x.Key != key))
                                {
                                    allBrushes.Add(new Resource(value, key, ResourceType.Bursh));
                                }
                                brushes.Add(new Resource(value, key, ResourceType.Bursh));
                            }
                            foreach (var resource in data.Where(x => x.Contains("Path")))
                            {
                                var segments = resource.Split('"').ToList();
                                var keyIndex = segments.IndexOf("Key") + 2;
                                var key = segments[keyIndex];
                                var f = segments.FirstOrDefault(x => x.Contains("Data"));
                                var valueIndex = segments.IndexOf(f)+1;
                                var value = segments[valueIndex];
                                if (allBrushes.All(x => x.Key != key))
                                {
                                    allBrushes.Add(new Resource(value, key, ResourceType.Image));
                                }
                                brushes.Add(new Resource(value, key, ResourceType.Image));

                            }
                            foreach (var resource in data.Where(x => x.Contains("ImageBrush")))
                            {
                                var segments = resource.Split('"').ToList();
                                var keyIndex = segments.IndexOf("Key") + 2;
                                var key = segments[keyIndex];
                                var f = segments.FirstOrDefault(x => x.Contains("Source"));
                                var valueIndex = segments.IndexOf(f) + 1;
                                var value = segments[valueIndex];
                                if (allBrushes.All(x => x.Key != key))
                                {
                                    allBrushes.Add(new Resource(value, key, ResourceType.Image));
                                }
                                brushes.Add(new Resource(value, key, ResourceType.Image));

                            }
                            foreach (var theme in Themes)
                            {
                                theme.Resources = new ObservableCollection<Resource>(brushes.OrderBy(x => x.Type));
                            }
                        }
                        foreach (var theme in Themes)
                        {
                            var missignResources = allBrushes.Where(x => theme.Resources.All(y => y.Key != x.Key)).ToList();
                            if (!missignResources.Any()) continue;
                            foreach (var missignResource in missignResources)
                            {
                                theme.Resources.Add(new Resource("",missignResource.Key,missignResource.Type));

                            }
                        }
                    }
                });
            }
        }

        public DelegateCommand AddResourceCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (!ValidateParams())
                    {
                        return;
                    }
                    foreach (var theme in Themes)
                    {
                        var firstOrDefault = AddedResourceValues.FirstOrDefault(x => x.Key == theme.Key);
                        if (firstOrDefault != null)
                            theme.Resources.Add(
                                new Resource(firstOrDefault.Value,
                                    AddedResourceKey, AddedResourceSelectedType));
                    }
                    ShowAddResourcePopup = false;
                });
            }
        }

        public DelegateCommand ShowAddResourceCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    AddedResourceKey = "";
                    AddedResourceSelectedType=ResourceType.Bursh;
                    AddedResourceValues=new ObservableCollection<Resource>();
                    foreach (var theme in Themes)
                    {
                        AddedResourceValues.Add(new Resource("",theme.Key,ResourceType.Bursh));
                    }
                    ShowAddResourcePopup = true;
                });
            }
        }

        public DelegateCommand ExportCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var saveWin = new SaveFileDialog();
                    var result = saveWin.ShowDialog();
                    if (result == null || !result.Value) return;
                    foreach (var theme in Themes)
                    {
                            var doc = new StringBuilder();
                            doc.AppendLine("<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
                            doc.AppendLine("xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
                            foreach (var res in theme.Resources)
                            {
                                var str = GetResourceString(res);
                                doc.AppendLine(str);
                            }
                            doc.AppendLine("</ResourceDictionary>");
                            File.WriteAllText(saveWin.FileName+theme.Key+".xaml",doc.ToString());
                    }
                });
            }
        }
        private string GetResourceString(Resource res)
        {
            switch (res.Type)
            {
                case ResourceType.Bursh:
                    return "<" + "SolidColorBrush" + " x:Key=" + "\"" + res.Key + "\"" + " Color=" + "\"" + res.Value + "\"" + "/>";
                case ResourceType.Image:
                    return "<" + "ImageBrush" + " x:Key=" + "\"" + res.Key + "\""+" Strech=\"Uniform\"" + " ImageSource=" + "\"" + res.Value + "\"" + "/>";
                case ResourceType.Geometry:
                    return "<" + "Path" + " x:Key=" + "\"" + res.Key + "\"" + " Strech=\"Uniform\"" + " Data=" + "\"" + res.Value + "\"" + "/>";
                default:
                    return null;
            }
        }

        private bool ValidateParams()
        {
            if (Themes.Any(theme=>theme.Resources.Any(x=>x.Key==AddedResourceKey)))
            {
                MessageBox.Show("This key already exists");
                return false;
            }
            if (AddedResourceValues==null||AddedResourceValues.Any(x => string.IsNullOrEmpty(x.Value)))
            {
                MessageBox.Show("Plaese add missing values");
                return false;
            }
            return true;
        }
    }
}
