using System.Collections.Generic;
using System.Windows;

namespace test
{
    public partial class BreakColumnsView : Window
    {
        public BreakColumnsView(List<ColumnToBreak> columnsToBreak)
        {
            InitializeComponent();
            ColumnsGrid.ItemsSource = columnsToBreak;
        }
    }
}
