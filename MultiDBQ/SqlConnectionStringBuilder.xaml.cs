using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MultiDBQ
{
    /// <summary>
    /// Interaction logic for SqlConnectionStringBuilder.xaml
    /// </summary>
    [ContentProperty("Footer")]
    public partial class SqlConnectionStringBuilder : INotifyPropertyChanged
    {
        private static readonly SqlConnectionString DefaultValue = new SqlConnectionString { IntegratedSecurity = false, Pooling = false };

        public static readonly DependencyProperty DatabaseItemsProperty =
        DependencyProperty.Register("DatabaseItems",
        typeof(DataView),
        typeof(SqlConnectionStringBuilder));

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

        public static readonly DependencyProperty DataViewProperty =
       DependencyProperty.Register("DataView",
       typeof(DataView),
       typeof(SqlConnectionStringBuilder),
                                       new FrameworkPropertyMetadata(
                                           new DataView(),
                                           FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                           DataViewChanged));

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

        public static readonly DependencyProperty ConnectionStringProperty =
            DependencyProperty.Register("ConnectionString", typeof(SqlConnectionString),
                                        typeof(SqlConnectionStringBuilder),
                                        new FrameworkPropertyMetadata(
                                            DefaultValue,
                                            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                            ConnectionStringChanged));

        private static void ConnectionStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var builder = (SqlConnectionStringBuilder)d;
            if (e.NewValue == null)
                builder.Dispatcher.BeginInvoke((Action)(() => d.SetValue(ConnectionStringProperty, DefaultValue)));
            else
                builder.RegisterNewConnectionString((SqlConnectionString)e.NewValue);
        }

        private static void DataViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var builder = (SqlConnectionStringBuilder)d;
            builder.DataView = e.NewValue as DataView;
        }

        public SqlConnectionStringBuilder()
            : this(new SmoTasks())
        {
            InitializeComponent();
        }

        public SqlConnectionString ConnectionString
        {
            get { return (SqlConnectionString)GetValue(ConnectionStringProperty); }
            set { SetValue(ConnectionStringProperty, value); }
        }

        private readonly ISmoTasks _smoTasks;
        private static ObservableCollection<string> _servers;
        private static readonly object ServersLock = new object();

        public event PropertyChangedEventHandler PropertyChanged;
        private readonly BackgroundWorker _dbLoader = new BackgroundWorker();
        private string _header = "Sql Configuration";

        public SqlConnectionStringBuilder(ISmoTasks smoTasks)
        {
            _smoTasks = smoTasks;

            _dbLoader.DoWork += DbLoaderDoWork;
            _dbLoader.RunWorkerCompleted += DbLoaderRunWorkerCompleted;
            DataTable dt = new DataTable();
            DatabaseItems = dt.DefaultView;
            DataView = dt.DefaultView;
        }

        public static void SetConnectionString(DependencyObject dp, SqlConnectionString value)
        {
            dp.SetValue(ConnectionStringProperty, value);
        }

        public static SqlConnectionString GetConnectionString(DependencyObject dp)
        {
            return (SqlConnectionString)dp.GetValue(ConnectionStringProperty);
        }

        private void RegisterNewConnectionString(SqlConnectionString newValue)
        {
            if (newValue != null)
                newValue.PropertyChanged += ConnectionStringPropertyChanged;
        }

        void ConnectionStringPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Server has changed, reload
            if (e.PropertyName == "Server" && !_dbLoader.IsBusy)
            {
                OnPropertyChanged("DatabasesLoading");
            }

            GetBindingExpression(ConnectionStringProperty).UpdateSource();
        }

        public bool DatabasesLoading
        {
            get
            {
                return _dbLoader.IsBusy;
            }
        }

        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                OnPropertyChanged(nameof(Header));
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

        void DbLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            var connString = e.Argument as SqlConnectionString;

            if (string.IsNullOrEmpty(connString.Server)) return;

            e.Result = _smoTasks.GetDatabases(connString);
        }

        void DbLoaderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                var databases = e.Result as List<string>;
                if (databases == null) return;
                DataTable dt = new DataTable();

                foreach (var database in databases.OrderBy(d => d))
                {
                    DataRow dr = dt.NewRow();

                    string colDatabaseName = "Database";
                    if (!dt.Columns.Contains(colDatabaseName))
                    {
                        dt.Columns.Add(colDatabaseName);
                    }

                    dr[colDatabaseName] = database;
                    dt.Rows.Add(dr);
                }

                DatabaseItems = dt.DefaultView;
                DataView = dt.DefaultView;
                OnPropertyChanged(nameof(DatabasesLoading));
            }
            else
            {
                OnPropertyChanged(nameof(DatabasesLoading));
                MessageBox.Show(e.Error.Message);
            }

        }

        void PasswordChangedHandler(Object sender, RoutedEventArgs args)
        {
            ConnectionString.Password = ((Wpf.Ui.Controls.PasswordBox)sender).Password;
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            if (!ConnectionString.IsValid())
            {
                MessageBox.Show("There is no a valid sql server connection.");
                return;
            }
            if (!_dbLoader.IsBusy)
            {
                _dbLoader.RunWorkerAsync(ConnectionString);
                OnPropertyChanged(nameof(DatabasesLoading));
            }

        }
    }
}
