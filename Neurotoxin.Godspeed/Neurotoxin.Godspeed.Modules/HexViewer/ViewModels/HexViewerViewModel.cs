using System;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Godspeed.Modules.HexViewer.ViewModels
{
    public class HexViewerViewModel : ModuleViewModelBase
    {
        #region Properties

        private const string BINARY = "Binary";
        private byte[] _binary;
        public byte[] Binary
        {
            get { return _binary; }
            set { _binary = value; NotifyPropertyChanged(BINARY); }
        }

        private const string MAP = "Map";
        private BinMap _map;
        public BinMap Map
        {
            get { return _map; }
            set { _map = value; NotifyPropertyChanged(MAP); }
        }

        private const string PERCENTAGE = "Percentage";
        private float _percentage;
        public float Percentage
        {
            get { return _percentage; }
            set
            {
                _percentage = value; 
                NotifyPropertyChanged(PERCENTAGE);
                LoadInfo.Title = String.Format("{0} ({1}%)", _title, value);
            }
        }

        #endregion

        public override bool HasDirty()
        {
            throw new NotImplementedException();
        }

        protected override void ResetDirtyFlags()
        {
            throw new NotImplementedException();
        }

        public override bool IsDirty(string propertyName)
        {
            throw new NotImplementedException();
        }

        private string _title;

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    _title = LoadInfo.Title;
                    var p = (Tuple<byte[], BinMap>) cmdParam;
                    Binary = p.Item1;
                    Map = p.Item2;
                    break;
            }
        }

    }
}