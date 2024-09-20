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
using System.IO;

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

        public static readonly DependencyProperty DatabaseItemsProperty =
            DependencyProperty.Register("DatabaseItems",
            typeof(DataView),
            typeof(ScriptSelection));

        public static readonly DependencyProperty DataViewProperty =
            DependencyProperty.Register("DataView",
            typeof(DataView),
            typeof(ScriptSelection));

        public static readonly DependencyProperty ConnectionStringProperty =
            DependencyProperty.Register("ConnectionString",
            typeof(SqlConnectionString),
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

        public DataView DatabaseItems
        {
            get
            {
                return (DataView)GetValue(DatabaseItemsProperty);
            }
            set
            {
                SetValue(DatabaseItemsProperty, value);

                OnPropertyChanged(nameof(DatabaseItems));
            }
        }

        public DataView DataView
        {
            get
            {
                return (DataView)GetValue(DataViewProperty);
            }
            set
            {
                SetValue(DataViewProperty, value);

                OnPropertyChanged(nameof(DataView));
            }
        }

        public SqlConnectionString ConnectionString
        {
            get
            {
                return (SqlConnectionString)GetValue(ConnectionStringProperty);
            }
            set
            {
                SetValue(ConnectionStringProperty, value);
            }
        }

        public ScriptSelection()
        {
            InitializeComponent();
        }


        private void OnPropertyChanged(params string[] propertyNames)
        {
            if (PropertyChanged == null) return;

            foreach (var propertyName in propertyNames)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            if (!ConnectionString.IsValid())
            {
                MessageBox.Show("Please set sql server name and credentials");
                return;
            }
            if (Query == null || Query.Trim().Length == 0)
            {
                MessageBox.Show("No query selected");
                return;
            }

            DataTable dt = new DataTable();
            string colDatabaseName = "Database";

            foreach (DataRow database in DatabaseItems.Table.Rows)
            {

                try
                {
                    using (var conn = new SqlConnection(ConnectionString.WithDatabase(database[colDatabaseName].ToString())))
                    {
                        conn.Open();
                        SqlCommand myTableCommand = new SqlCommand(Query, conn);
                        SqlDataAdapter a = new SqlDataAdapter(myTableCommand);
                        DataTable currentDt = new DataTable();
                        a.Fill(currentDt);
                        foreach (DataColumn column in currentDt.Columns)
                        {
                            if (!dt.Columns.Contains(column.ColumnName))
                            {
                                dt.Columns.Add(column.ColumnName);
                            }
                        }
                        if (currentDt.Rows.Count > 0)
                        {
                            foreach (DataRow row in currentDt.Rows)
                            {
                                dt.ImportRow(row);
                            }
                        }
                        else
                        {
                            AddNewDatabase(dt, colDatabaseName, database);
                        }

                    }
                }
                catch (Exception ex)
                {
                    AddNewDatabase(dt, colDatabaseName, database);
                }
            }
            DataView = dt.DefaultView;
        }

        private static void AddNewDatabase(DataTable dt, string colDatabaseName, DataRow database)
        {
            DataRow dr = dt.NewRow();

            if (!dt.Columns.Contains(colDatabaseName))
            {
                dt.Columns.Add(colDatabaseName);
            }

            if (database.Table.Columns.Contains(colDatabaseName))
            {
                dr[colDatabaseName] = database[colDatabaseName].ToString();
            }

            dt.Rows.Add(dr);
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
                Query = File.ReadAllText(ScriptName);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(ScriptName, Query);
        }
    }
}
