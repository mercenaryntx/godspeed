using System.Windows;

namespace Neurotoxin.Godspeed.Shell.Primitives
{
    /// <summary>
    /// Interaction logic for CaptionButtons.xaml
    /// </summary>
    public partial class CaptionButtons
    {
        /// <summary>
        /// The parent Window of the control.
        /// </summary>
        private Window _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaptionButtons"/> class.
        /// </summary>
        public CaptionButtons()
        {
            InitializeComponent();
            this.Loaded += CaptionButtonsLoaded;
        }

        /// <summary>
        /// Event when the control is loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void CaptionButtonsLoaded(object sender, RoutedEventArgs e)
        {
            _parent = GetTopParent();
        }

        /// <summary>
        /// Action on the button to close the window.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            _parent.Close();
        }

        /// <summary>
        /// Changes the view of the window (maximized or normal).
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void RestoreButtonClick(object sender, RoutedEventArgs e)
        {
            _parent.WindowState = _parent.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        /// <summary>
        /// Minimizes the Window.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            _parent.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Gets the top parent (Window).
        /// </summary>
        /// <returns>The parent Window.</returns>
        private Window GetTopParent()
        {
            return Window.GetWindow(this);
        }

        /// <summary>
        /// Gets or sets the margin button.
        /// </summary>
        /// <value>The margin button.</value>
        public Thickness MarginButton
        {
            get { return (Thickness)GetValue(MarginButtonProperty); }
            set
            {
                base.SetValue(MarginButtonProperty, value);
            }
        }

        /// <summary>
        /// The dependency property for the Margin between the buttons.
        /// </summary>
        public static DependencyProperty MarginButtonProperty = DependencyProperty.Register("MarginButton", typeof(Thickness), typeof(BorderlessWindow));

        /// <summary>
        /// Enum of the types of caption buttons
        /// </summary>
        public enum CaptionType
        {
            /// <summary>
            /// All the buttons
            /// </summary>
            Full,
            /// <summary>
            /// Only the close button
            /// </summary>
            Close,
            /// <summary>
            /// Reduce and close buttons
            /// </summary>
            ReduceClose
        }

        /// <summary>
        /// Gets or sets the visibility of the buttons.
        /// </summary>
        /// <value>The visible buttons.</value>
        public CaptionType Type
        {
            get { return (CaptionType)GetValue(TypeProperty); }
            set
            {
                base.SetValue(TypeProperty, value);
            }
        }

        /// <summary>
        /// The dependency property for the Margin between the buttons.
        /// </summary>
        public static DependencyProperty TypeProperty = DependencyProperty.Register("Type",typeof(CaptionType),typeof(BorderlessWindow),new PropertyMetadata(CaptionType.Full));
    }
}
