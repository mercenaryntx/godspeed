using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.Practices.Composite;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using Neurotoxin.Godspeed.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    /// <summary>
    /// Class <see cref="ModuleViewBase"/> is a base class for user controls presenting views.
    /// </summary>
    public class ModuleViewBase : UserControl, IView<ModuleViewModelBase>, IActiveAware
    {
        #region Fields
        
        protected IUnityContainer container;
        protected IEventAggregator eventAggregator;
        private bool isActive;
        private bool isInitalized;
        
        #endregion

        #region Properties

        public ModuleLoadInfo LoadInfo
        {
            get
            {
                if (ViewModel == null) return null;
                return ViewModel.LoadInfo;
            }
        }

        public ImageSource Icon { get; set; }

        public ModuleViewModelBase ViewModel
        {
            get { return this.DataContext as ModuleViewModelBase; }
            set { this.DataContext = value; }
        }

        public event EventHandler Closing;

        #endregion

        #region Constructor

        public ModuleViewBase()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.container = UnityInstance.Container;
                this.eventAggregator = container.Resolve<IEventAggregator>();
                Activated += ViewActivated;
            }
            this.IsVisibleChanged += OnIsVisibleChanged;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!((bool)e.OldValue) && (bool)e.NewValue && eventAggregator != null)
            {
                eventAggregator.GetEvent<ModuleViewShownEvent>().Publish(this);
            }
        }

        #endregion

        #region IActiveAware Members

        /// <summary>
        /// Gets or sets a value indicating whether the view is active.
        /// </summary>
        /// <value><see langword="true" /> if the object is active; otherwise <see langword="false" />.</value>
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    OnIsActiveChanged();

                    if (isActive) OnActivated();
                }
            }
        }

        /// <summary>
        /// Notifies that the value for <see cref="IsActive"/> property has changed.
        /// </summary>
        public event EventHandler IsActiveChanged;

        /// <summary>
        /// Notifies that the view is activated.
        /// </summary>
        public event EventHandler Activated;

        /// <summary>
        /// Raises the <see cref="IsActiveChanged"/> event.
        /// </summary>
        protected virtual void OnIsActiveChanged()
        {
            if (IsActiveChanged != null) IsActiveChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="Activated"/> event.
        /// </summary>
        protected virtual void OnActivated()
        {
            if (Activated != null) Activated(this, EventArgs.Empty);
        }
        #endregion

        #region Methods
        
        /// <summary>
        /// Virtual method invoked each time the View is activated, and creates a new Rollover to that view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void ViewActivated(object sender, EventArgs e)
        {
            var c = (FrameworkElement)Content;
            if (isInitalized) return;
            isInitalized = true;
            var sv = this.FindAncestor<ScrollViewer>();
            if (sv != null) c.SizeChanged += OnContentSizeChanged;
        }

        private void OnContentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //HACK zsbangha 2010-08-26: Lightweight hack to prevent the ScrollViewer to shrink the content to its own image (which is a by design behavior in other cases)
            var sv = this.FindAncestor<ScrollViewer>();
            if (sv == null) return;
            if (e.NewSize.Width > this.ActualWidth) sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            if (e.NewSize.Height > this.ActualHeight) sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        /// <summary>
        /// Adds progress indicating feature to the ViewModel's dataloader.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameter"></param>
        public virtual void LoadData(LoadCommand cmd, object parameter)
        {
            if (ViewModel == null) return;
            ViewModel.LoadDataAsync(cmd, parameter);
        }

        /// <summary>
        /// Call this directly before you want to Close the current module
        /// </summary>
        /// <returns></returns>
        public bool Close(bool force)
        {
            if (force) return true;
            return Close();
        }

        public virtual bool Close()
        {
            OnClosing();
            return true;
        }

        /// <summary>
        /// Raises the Closing event
        /// </summary>
        protected virtual void OnClosing()
        {
            if (Closing != null) Closing.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Catches Enter button pressings, and forces the ViewModel to Submit
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Enter && e.OriginalSource is TextBoxBase) ViewModel.Submit();
        }

        /// <summary>
        /// Gets the Model behind the ViewModel
        /// </summary>
        /// <returns></returns>
        public object GetModel()
        {
            return ViewModel == null ? null : ViewModel.GetModel();
        }

        #endregion
    }
}