using System;
using Neurotoxin.Godspeed.Presentation.Bindings;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class TitleConverter : MultiBindingConverterBase<FileSystemItemViewModel, string>
    {
        public TitleConverter() : base(new [] { "Title", "Name" })
        {
        }

        protected override string Convert(FileSystemItemViewModel viewModel, Type targetType)
        {
            if (viewModel == null) return null;
            if (!string.IsNullOrEmpty(viewModel.Title) && !viewModel.IsUpDirectory) return viewModel.Title;
            var name = viewModel.Name;
            if (viewModel.IsUpDirectory) name = Strings.UpDirectory;
            var format = viewModel.Type != ItemType.File ? "[{0}]" : "{0}";
            return string.Format(format, name);
        }
    }
}