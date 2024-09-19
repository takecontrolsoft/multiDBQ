﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        typeof(ObservableCollection<string>),
        typeof(SqlConnectionStringBuilder));
        public ObservableCollection<string> DatabaseItems
        {
            get
            {
                return (ObservableCollection<string>)GetValue(DatabaseItemsProperty);
            }
            set
            {
                SetValue(DatabaseItemsProperty, value);
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

        private readonly ObservableCollection<string> _databases = new ObservableCollection<string>();
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly BackgroundWorker _dbLoader = new BackgroundWorker();
        private string _lastServer;
        private string _header = "Sql Configuration";
        private bool _serversLoading;

        public SqlConnectionStringBuilder(ISmoTasks smoTasks)
        {
            _smoTasks = smoTasks;

            _dbLoader.DoWork += DbLoaderDoWork;
            _dbLoader.RunWorkerCompleted += DbLoaderRunWorkerCompleted;
            DatabaseItems = _databases;
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

        public ObservableCollection<string> Servers
        {
            get
            {
                lock (ServersLock)
                {
                    if (_servers == null)
                    {
                        _servers = new ObservableCollection<string>();
                        ServersLoading = true;
                        LoadServersAsync();
                    }
                }

                return _servers;
            }
        }

        public ObservableCollection<string> Databases
        {
            get { return _databases; }
        }

        public bool ServersLoading
        {
            get
            {
                return _serversLoading;
            }
            private set
            {
                _serversLoading = value;
                OnPropertyChanged("ServersLoading");
            }
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
                OnPropertyChanged("Header");
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

            //No need to refesh databases if last server is the same as current server
            if (connString == null || _lastServer == connString.Server)
                return;

            _lastServer = connString.Server;

            if (string.IsNullOrEmpty(connString.Server)) return;

            e.Result = _smoTasks.GetDatabases(connString);
        }

        void DbLoaderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                var databases = e.Result as List<string>;
                if (databases == null) return;

                _lastServer = null;
                foreach (var database in databases.OrderBy(d => d))
                {
                    _databases.Add(database);
                }
            }

            OnPropertyChanged("DatabasesLoading");
        }

        private void LoadServersAsync()
        {
            var serverLoader = new BackgroundWorker();
            serverLoader.DoWork += ((sender, e) => e.Result = _smoTasks.SqlServers.OrderBy(r => r).ToArray());

            serverLoader.RunWorkerCompleted += ((sender, e) =>
            {
                foreach (var server in (string[])e.Result)
                {
                    _servers.Add(server);
                }
                ServersLoading = false;
            });

            serverLoader.RunWorkerAsync();
        }

        void PasswordChangedHandler(Object sender, RoutedEventArgs args)
        {
            // Increment a counter each time the event fires.
            ConnectionString.Password = ((PasswordBox)sender).Password;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ConnectionString.IsValid())
            {
                _dbLoader.RunWorkerAsync(ConnectionString);
                return;
            }
        }
    }
}