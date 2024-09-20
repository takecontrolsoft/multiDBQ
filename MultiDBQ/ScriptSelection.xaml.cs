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

        public bool DatabasesLoading
        {
            get
            {
                return _dbLoader.IsBusy;
            }
        }
        private readonly BackgroundWorker _dbLoader = new BackgroundWorker();

        public ScriptSelection()
        {
            InitializeComponent();
            _dbLoader.DoWork += DbLoaderDoWork;
            _dbLoader.RunWorkerCompleted += DbLoaderRunWorkerCompleted;

        }

        void DbLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            var loadedTable = (e.Argument as object[])[0] as DataTable;
            var connectionString = (e.Argument as object[])[1] as SqlConnectionString;
            var query = (e.Argument as object[])[2] as string;
            DataTable dt = new DataTable();
            string colDatabaseName = "Database";
            foreach (DataRow database in loadedTable.Rows)
            {

                try
                {
                    using (var conn = new SqlConnection(connectionString.WithDatabase(database[colDatabaseName].ToString())))
                    {
                        conn.Open();
                        SqlCommand myTableCommand = new SqlCommand(query, conn);
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
                    if (ex is SqlException && ex.Message.StartsWith("Incorrect syntax"))
                    {
                        throw;
                    }

                    AddNewDatabase(dt, colDatabaseName, database);
                }
            }
            e.Result = dt;
        }

        void DbLoaderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                var dt = e.Result as DataTable;
                if (dt == null) return;


                DataView = dt.DefaultView;
                OnPropertyChanged(nameof(DatabasesLoading));
            }
            else
            {
                OnPropertyChanged(nameof(DatabasesLoading));
                MessageBox.Show(e.Error.Message);
            }
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
            _dbLoader.RunWorkerAsync(new object[] { DatabaseItems.Table, ConnectionString, Query });
            OnPropertyChanged(nameof(DatabasesLoading));
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
