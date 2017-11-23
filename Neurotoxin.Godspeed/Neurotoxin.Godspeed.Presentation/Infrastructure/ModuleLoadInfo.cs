using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Regions;
using System.Xml.Serialization;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    /// <summary>
    /// Describes load informations about Prism modules.
    /// </summary>
    public class ModuleLoadInfo : INotifyPropertyChanged
    {
        public string ModuleName { get; set; }
        public object LoadParameter { get; set; }
        public LoadCommand LoadCommand { get; set; }
        public bool Singleton { get; set; }
        public ModuleLoadInfo Opener { get; set; }

        public ModuleViewModelBase ViewModel
        {
            get { return RenderedView != null ? RenderedView.ViewModel : null; }
            set { if (RenderedView != null) RenderedView.DataContext = value; }
        }

        public string ModuleType
        {
            get { return "Neurotoxin.Godspeed.Modules." + ModuleName; }
        }
        public string ViewType
        {
            get { return ModuleType + ".Views." + ViewName; }
        }

        private const string DefaultTitle = "[N/A]";
        private string _title;
        public string Title
        {
            get { return String.IsNullOrEmpty(_title) ? DefaultTitle : _title; }
            set
            {
                _title = value;
                NotifyPropertyChange("Title");
            }
        }

        public string ViewName { get; set; }
        public string RegionName { get; set; }

        private ModuleViewBase _renderedView;
        public ModuleViewBase RenderedView
        {
            get { return _renderedView; }
            set
            {
                _renderedView = value;
                NotifyPropertyChange("RenderedView");
            }
        }

        public ModuleLoadInfo()
        {
            Singleton = true;
        }

        public ModuleLoadInfo Clone()
        {
            return new ModuleLoadInfo
            {
                ModuleName = ModuleName,
                LoadParameter = LoadParameter,
                LoadCommand = LoadCommand,
                RegionName = RegionName,
                ViewName = ViewName,
                Singleton = Singleton,
                Title = Title
            };
        }

        public string GetUniqueRegionName(IRegionManager regionManager)
        {
            var prefix = RegionName ?? ModuleName;
            var i = 0;
            do
            {
                i++;
                RegionName = prefix + i;
            } while (regionManager.Regions.ContainsRegionWithName(RegionName));
            return RegionName;
        }

        public void Update()
        {
            NotifyPropertyChange("Title");
        }

        /// <summary>
        /// Tries to create new ModuleLoadInfo instance from the given input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException">Input type isn't ModuleInfo neither string.</exception>
        public static ModuleLoadInfo Parse(object input)
        {
            var moduleInfo = input as ModuleInfo;
            if (moduleInfo != null)
                return new ModuleLoadInfo { ModuleName = moduleInfo.ModuleName };

            var moduleName = input as string;
            if (moduleName != null)
                return new ModuleLoadInfo { ModuleName = moduleName };

            throw new InvalidDataException("Unknown data type.");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj == null || GetType() != obj.GetType()) return false;
            var mli = (ModuleLoadInfo)obj;
            if (mli.Singleton)
                return ModuleName == mli.ModuleName && ViewName == mli.ViewName && Singleton;

            if (ModuleName != mli.ModuleName)
                return false;

            return Same(mli);
        }

        /// <summary>
        /// Compares two ModuleLoadInfos to determine they are pretty much the same or not
        /// </summary>
        /// <param name="mli"></param>
        /// <returns></returns>
        public bool Same(ModuleLoadInfo mli)
        {
            return (ViewName == mli.ViewName
                    && LoadCommand == mli.LoadCommand
                    && ((LoadParameter == null && mli.LoadParameter == null)
                        || (LoadParameter != null && LoadParameter.Equals(mli.LoadParameter))));
        }

        public override int GetHashCode()
        {
            var hash = 0;
            if (ModuleName != null)
                hash ^= ModuleName.GetHashCode();

            if (ViewName != null)
                hash ^= ViewName.GetHashCode();

            hash ^= LoadCommand.GetHashCode();

            if (LoadParameter != null)
                hash ^= LoadParameter.GetHashCode();

            return hash;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChange(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}