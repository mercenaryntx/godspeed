using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Neurotoxin.Godspeed.Presentation.Formatters.HtmlConverter;
using Xceed.Wpf.Toolkit;

namespace Neurotoxin.Godspeed.Shell.Views.Formatters
{
    public class HtmlFormatter : ITextFormatter
    {
        public string GetText(FlowDocument document)
        {
            string html = null;
            TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
            MemoryStream ms = new MemoryStream();
            try
            {
                tr.Save(ms, DataFormats.Xaml);
                html = HtmlFromXamlConverter.ConvertXamlToHtml(Encoding.Default.GetString(ms.ToArray()));
            }
            finally
            {
                if (ms != null)
                {
                    ((IDisposable)ms).Dispose();
                }
            }
            return html;
        }

        public void SetText(FlowDocument document, string text)
        {
            text = HtmlToXamlConverter.ConvertHtmlToXaml(text, false);
            try
            {
                TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
                try
                {
                    tr.Load(ms, DataFormats.Xaml);
                }
                finally
                {
                    if (ms != null)
                    {
                        ((IDisposable)ms).Dispose();
                    }
                }
            }
            catch
            {
                throw new InvalidDataException("data provided is not in the correct Html format.");
            }
        }
    }
}