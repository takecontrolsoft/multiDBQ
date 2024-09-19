using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MultiDBQ
{
    /// <summary>
    /// Interaction logic for ScriptSelection.xaml
    /// </summary>
    [ContentProperty("Query")]
    public partial class ScriptSelection : INotifyPropertyChanged
    {
        public static readonly DependencyProperty QueryProperty =
        DependencyProperty.Register("Query",
        typeof(string),
        typeof(ScriptSelection));

       public static readonly DependencyProperty ScriptNameProperty =
        DependencyProperty.Register("ScriptName",
        typeof(string),
        typeof(ScriptSelection));

        public event PropertyChangedEventHandler PropertyChanged;

        public string Query
        {
            get
            {
                return (string)GetValue(QueryProperty);
            }
            set
            {
                SetValue(QueryProperty, value);
            }
        }
      
        public string ScriptName
        {
            get
            {
                return (string)GetValue(ScriptNameProperty);
            }
            set
            {
                SetValue(ScriptNameProperty, value);
            }
        }

        public ScriptSelection()
        {
            InitializeComponent();
        }

      

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            //string mySQLQuery = "select * from myTable";
            //SqlCommand myTableCommand = new SqlCommand(mySQLQuery, MyConnection);
            //DataTable dt = new DataTable();
            //SqlDataAdapter a = new SqlDataAdapter(myTableCommand);
            //a.Fill(dt);
            //myDataGrid.ItemsSource = dt.DefaultView;

        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            // Configure save file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Scripts"; // Default file name
            dialog.DefaultExt = ".sql"; // Default file extension
            dialog.Filter = "Scripts (.sql)|*.sql"; // Filter files by extension

            // Show save file dialog box
            bool? result = dialog.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                ScriptName = dialog.FileName;
            }
        }
    }
}
