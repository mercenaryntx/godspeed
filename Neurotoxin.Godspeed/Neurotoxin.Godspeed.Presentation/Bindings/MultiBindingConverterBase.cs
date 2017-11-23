using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Neurotoxin.Godspeed.Presentation.Bindings
{
    public abstract class MultiBindingConverterBase<TViewModel, TOutput> : MultiBindingConverterBase<TViewModel, TOutput, object>
    {
        protected MultiBindingConverterBase(IEnumerable<string> properties) : base(properties)
        {
        }

        protected MultiBindingConverterBase()
        {
        }

        protected abstract TOutput Convert(TViewModel viewModel, Type targetType);

        protected override TOutput Convert(TViewModel viewModel, Type targetType, object parameter)
        {
            return Convert(viewModel, targetType);
        }
    }

    public abstract class MultiBindingConverterBase<TViewModel, TOutput, TParameter> : MultiBinding, IMultiValueConverter
    {
        protected MultiBindingConverterBase()
        {
            Converter = this;
            Bindings.Clear();
            Bindings.Add(new Binding(""));
            //PresentationTraceSources.SetTraceLevel(this, PresentationTraceLevel.High);
        }

        protected MultiBindingConverterBase(IEnumerable<string> properties) : this()
        {
            if (properties == null) return;
            foreach (var name in properties)
            {
                Bindings.Add(new Binding(name));
            }
            //Bindings.Add(new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
        }

        protected abstract TOutput Convert(TViewModel viewModel, Type targetType, TParameter parameter);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //HACK: In some weird cases the null DataContext becomes "" :S
            if (string.Empty.Equals(values[0])) return null;

            return Convert(values[0] != null && values[0] != DependencyProperty.UnsetValue
                            ? (TViewModel) values[0]
                            : default(TViewModel), 
                           targetType,
                           parameter != null ? (TParameter) parameter : default(TParameter));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
