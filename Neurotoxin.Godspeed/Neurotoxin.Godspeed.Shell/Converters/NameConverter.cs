using System;
using Neurotoxin.Godspeed.Presentation.Bindings;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class NameConverter : MultiBindingConverterBase<FileSystemItemViewModel, string, NameConverterParameter>
    {
        public NameConverter() : base(new [] { "Title", "Name" })
        {
        }

        protected override string Convert(FileSystemItemViewModel viewModel, Type targetType, NameConverterParameter parameter)
        {
            if (viewModel == null) return null;
            var title = viewModel.Title;
            var name = viewModel.Name;

            switch (parameter)
            {
                case NameConverterParameter.Secondary:
                    return name == title || viewModel.IsUpDirectory
                        ? null
                        : (name != null && title != null ? string.Format(" [{0}]", name) : null);
                case NameConverterParameter.Primary:
                    if (viewModel.IsUpDirectory) name = Strings.UpDirectory;
                    var format = viewModel.Type != ItemType.File ? "[{0}]" : "{0}";
                    return string.Format(format, name);
                case NameConverterParameter.Plain:
                    return name == title || viewModel.IsUpDirectory
                        ? null
                        : (name != null && title != null ? name : null);
                default:
                    throw new NotSupportedException("Invalid parameter value: " + parameter);
            }
        }
    }
}