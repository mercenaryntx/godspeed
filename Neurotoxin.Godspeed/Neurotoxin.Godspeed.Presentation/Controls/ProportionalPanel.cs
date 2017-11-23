using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Neurotoxin.Godspeed.Presentation.Controls
{
    public class ProportionalPanel : Grid
    {
        #region ValueMemberPath

        public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register("ValueMemberPath", typeof(string), typeof(ProportionalPanel), new FrameworkPropertyMetadata("Value", OnValueMemberPathChanged));
        
        public string ValueMemberPath
        {
            get { return (string)GetValue(ValueMemberPathProperty); }
            set { SetValue(ValueMemberPathProperty, value); }
        }

        private static void OnValueMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProportionalPanel)d).OnValueMemberPathChanged();
        }

        protected virtual void OnValueMemberPathChanged()
        {
            Render();
        }

        #endregion

        #region DisplayMemberPath

        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(ProportionalPanel), new FrameworkPropertyMetadata("Text", OnDisplayMemberPathChanged));

        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProportionalPanel)d).OnDisplayMemberPathChanged();
        }

        protected virtual void OnDisplayMemberPathChanged()
        {
            Render();
        }

        #endregion

        #region ItemsSource

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ProportionalPanel), new FrameworkPropertyMetadata(null, OnItemsSourceChanged));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProportionalPanel)d).OnItemsSourceChanged();
        }

        protected virtual void OnItemsSourceChanged()
        {
            Render();
        }

        #endregion

        #region ItemTemplate

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ProportionalPanel), new FrameworkPropertyMetadata(null, OnItemTemplateChanged));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProportionalPanel)d).OnItemTemplateChanged();
        }

        protected virtual void OnItemTemplateChanged()
        {
            Render();
        }

        #endregion

        private void Render()
        {
            ColumnDefinitions.Clear();
            Children.Clear();

            if (ItemsSource == null) return;

            var i = 0;
            foreach (var item in ItemsSource)
            {
                var type = item.GetType();
                var valueProperty = type.GetProperty(ValueMemberPath);
                var valueField = type.GetField(ValueMemberPath);
                var value = valueProperty != null ? valueProperty.GetValue(item, null) : valueField != null ? valueField.GetValue(item) : 1;

                var column = new ColumnDefinition {Width = new GridLength(Convert.ToInt64(value), GridUnitType.Star)};
                ColumnDefinitions.Add(column);

                var bar = new ContentPresenter {ContentTemplate = ItemTemplate, Content = item};
                Children.Add(bar);
                bar.SetValue(ColumnProperty, i);
                i++;
            }
        }

    }
}