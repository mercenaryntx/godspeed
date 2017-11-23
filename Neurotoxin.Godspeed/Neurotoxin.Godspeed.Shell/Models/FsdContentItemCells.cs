using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Exceptions;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class FsdContentItemCells
    {
        private readonly string _innerHtml;
        private readonly Dictionary<FsdContentItemProperty, string> _cells;

        public FsdContentItemCells(HtmlNodeCollection nodes)
        {
            _innerHtml = string.Join(string.Empty, nodes.Select(node => node.OuterHtml));
            _cells = nodes.Select((c, i) => new { key = (FsdContentItemProperty)i, value = c.InnerText.Trim() }).ToDictionary(k => k.key, k => k.value);
        }

        public string this[FsdContentItemProperty key]
        {
            get
            {
                if (_cells.ContainsKey(key)) return _cells[key];
                throw new FsdMissingContentItemCellException(key, _innerHtml);
            }
        }
    }
}
