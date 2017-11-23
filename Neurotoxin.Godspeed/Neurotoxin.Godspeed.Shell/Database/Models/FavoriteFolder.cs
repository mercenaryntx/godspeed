using System.ComponentModel;
using Neurotoxin.Godspeed.Shell.Database.Attributes;
using ServiceStack.DataAnnotations;

namespace Neurotoxin.Godspeed.Shell.Database.Models
{
    public class FavoriteFolder : ModelBase
    {
        [Index]
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        private int? _connectionId;
        public int? ConnectionId
        {
            get { return _connectionId; }
            set { _connectionId = value; SetDirtyFlag("ConnectionId"); }
        }

        private string _name;
        [StringLength(255)]
        [OrderBy(ListSortDirection.Ascending)]
        public string Name
        {
            get { return _name; }
            set { _name = value; SetDirtyFlag("Name"); }
        }

        private string _path;
        [StringLength(255)]
        public string Path
        {
            get { return _path; }
            set { _path = value; SetDirtyFlag("Path"); }
        }
    }
}