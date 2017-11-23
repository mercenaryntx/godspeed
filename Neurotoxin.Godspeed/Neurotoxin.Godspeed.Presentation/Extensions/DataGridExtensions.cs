using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Neurotoxin.Godspeed.Presentation.Extensions
{
    public static class DataGridExtensions
    {
        public static DataGridCell FirstCell(this DataGridRow row)
        {
            var root = VisualTreeHelper.GetChild(row, 0);
            var cellsPresenter = (DataGridCellsPresenter)VisualTreeHelper.GetChild(root, 0);
            return cellsPresenter.ItemContainerGenerator.ContainerFromIndex(0) as DataGridCell;
        }

        public static DataGridRow FindRowByValue(this DataGrid grid, object value)
        {
            return grid.ItemContainerGenerator.ContainerFromItem(value) as DataGridRow;
        }
    }
}