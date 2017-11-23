using System.Collections;
using System.Collections.Generic;
using Microsoft.Practices.ObjectBuilder2;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Io.Gpd;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    internal class GpdFileViewModel : ViewModelBase, IEnumerable<GpdFileEntryViewModel>
    {
        private GpdFile _model;
        private readonly List<GpdFileEntryViewModel> _entries;

        public GpdFileViewModel(GpdFile model)
        {
            _model = model;
            _entries = new List<GpdFileEntryViewModel>();
            model.Achievements.ForEach(a => _entries.Add(new GpdFileEntryViewModel(a.Entry.Type, a.Entry.Id, a.Name)));
            model.Images.ForEach(i => _entries.Add(new GpdFileEntryViewModel(i.Entry.Type, i.Entry.Id, null)));
            model.Settings.ForEach(s => _entries.Add(new GpdFileEntryViewModel(s.Entry.Type, s.Entry.Id, s.Id.ToString())));
            model.Strings.ForEach(s => _entries.Add(new GpdFileEntryViewModel(s.Entry.Type, s.Entry.Id, string.Format("[{0}] {1}", (SettingId)s.Entry.Id, s.Text))));
            model.TitlesPlayed.ForEach(t => _entries.Add(new GpdFileEntryViewModel(t.Entry.Type, t.Entry.Id, string.Format("[{0}] {1}", t.TitleCode, t.TitleName))));
            model.AvatarAwards.ForEach(a => _entries.Add(new GpdFileEntryViewModel(a.Entry.Type, a.Entry.Id, a.Name)));
            model.TheUnknowns.ForEach(u => _entries.Add(new GpdFileEntryViewModel(u.Entry.Type, u.Entry.Id, null)));
        }

        public IEnumerator<GpdFileEntryViewModel> GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}