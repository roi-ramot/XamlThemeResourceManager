using System.Collections.Generic;
using System.Collections.ObjectModel;
using ThemeResourceManager.BaseClass;

namespace ThemeResourceManager.DataModel
{
    public class Theme:BaseViewModel
    {
        private string _key;
        private ObservableCollection<Resource> _resources;

        public Theme()
        {
            Resources = new ObservableCollection<Resource>();
        }

        public string Key
        {
            get { return _key; }
            set
            {
                _key = value; 
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Resource> Resources
        {
            get { return _resources; }
            set
            {
                _resources = value; 
                OnPropertyChanged();
            }
        }
    }
}
