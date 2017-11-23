using System;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Shell.Controllers
{
    /// <summary>
    /// Class <see cref="ModuleController"/> is used to display modules.
    /// </summary>
    public class ModuleController : IGeneralController
    {
        #region String constants

        public static string STATUSBAR = "StatusBar";

        #endregion

        #region Private fields
        
        private readonly IUnityContainer container;
        private readonly IRegionManager regionManager;
        private readonly IModuleCatalog moduleCatalog;
        private readonly IModuleManager moduleManager;
        private readonly IEventAggregator eventAggregator;
        
        #endregion

        #region Constructor

        public ModuleController(IUnityContainer container, IRegionManager regionManager, IModuleCatalog moduleCatalog,
            IModuleManager moduleManager, IEventAggregator eventAggregator)
        {
            this.container = container;
            this.regionManager = regionManager;
            this.moduleCatalog = moduleCatalog;
            this.moduleManager = moduleManager;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region IGeneralController Members

        /// <summary>
        /// Initializer method of the Controller, subscribes the DisplayModule method to the EventAggregator's ModuleChangeEvent.
        /// </summary>
        public void Run()
        {
            //Commented on purpose: The ShellView's TabControl will take care of this
            //eventAggregator.GetEvent<ModuleChangeEvent>().Subscribe(DisplayModule, ThreadOption.UIThread, true);
        }

        #endregion

        /// <summary>
        /// Display a module the specified region with the given view.
        /// </summary>
        /// <param name="loadInfo">module load descriptor</param>
        /// <exception cref="ArgumentNullException">Empty module name.</exception>
        public void DisplayModule(ModuleLoadInfo loadInfo)
        {
            string moduleName = loadInfo.ModuleName;
            string regionName = loadInfo.RegionName;
            string viewName = loadInfo.ViewName ?? loadInfo.ModuleName;
            if (string.IsNullOrEmpty(moduleName)) throw new ArgumentNullException(moduleName, "Illegal module name to load.");
            ModuleViewBase currentView;
            try
            {
                moduleManager.LoadModule(moduleName);
                IModulePresentation module = TryResolve<IModulePresentation>(moduleName);

                if (module != null)
                {
                    IRegion region = regionManager.Regions[regionName];
                    currentView = (ModuleViewBase)region.GetView(viewName);

                    if (currentView == null)
                    {
                        currentView = (ModuleViewBase)module.GetView(viewName);
                        if (currentView.ViewModel != null)
                            currentView.ViewModel.LoadInfo = loadInfo;
                        else
                        {
                            // TODO rerdelyi 100705: some action should be taken, e.g. throw exception
                        }
                        loadInfo.RenderedView = currentView;
                        region.Add(currentView, viewName);
                        region.Activate(currentView);
                        currentView.LoadData(loadInfo.LoadCommand, loadInfo.LoadParameter);
                    }
                    else
                    {
                        // TODO rerdelyi 100705: some action should be taken, e.g. throw exception
                    }

                    //string statusBarRegionName = regionName + STATUSBAR;
                    //string statusBarViewName = viewName + STATUSBAR;

                    //if (regionManager.Regions.ContainsRegionWithName(statusBarRegionName))
                    //{
                    //    IRegion statusBarRegion = regionManager.Regions[statusBarRegionName];
                    //    if (statusBarRegion != null && statusBarRegion.GetView(statusBarViewName) == null)
                    //    {
                    //        var statusBar = module.GetStatusBar(viewName);
                    //        statusBar.DataContext = currentView.ViewModel;
                    //        statusBarRegion.Add(statusBar, statusBarViewName);
                    //        statusBarRegion.Activate(statusBar);
                    //    }
                    //}
                }
                else
                {
                    throw new MissingMemberException(string.Format("Unable to display \"{0}\" module.", moduleName));
                }
            }

            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to display \"{0}\" module.", moduleName), ex);
            }
        }

        /// <summary>
        /// Gets the current View of the given module and sets its ViewModel as the data context of the ModuleProperties module
        /// </summary>
        /// <param name="loadInfo"></param>
        public void SetModuleContext(ModuleLoadInfo loadInfo)
        {
            ModuleViewBase view = null;
            if (loadInfo != null)
            {
                view = loadInfo.RenderedView;
                if (view == null)
                {
                    string moduleName = loadInfo.ModuleName;
                    string viewName = loadInfo.ViewName ?? moduleName;

                    IModulePresentation module = TryResolve<IModulePresentation>(moduleName);
                    if (module != null) view = module.GetView(viewName) as ModuleViewBase;
                }
            }
            ModuleViewModelBase viewModel = view == null ? null : view.DataContext as ModuleViewModelBase;

            eventAggregator.GetEvent<ActivateModuleContextEvent>().Publish(viewModel);
        }

        /// <summary>
        /// Resolve an Object by Type and Registration Name.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="name">Registration Name.</param>
        /// <returns>Instances of registered object in the Unity Container.</returns>
        private T TryResolve<T>(string name)
        {
            try
            {
                return container.Resolve<T>(name);
            }
            catch
            {
                return default(T);
            }
        }


        public void Reset()
        {
            // no action must be taken
        }
    }
}