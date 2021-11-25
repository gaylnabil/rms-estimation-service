 using System.Collections.Generic;
 using System.Configuration;
 using System.Data.SqlClient;
 using System.Globalization;
 using System.Linq;
 using System.Reflection;
 using System.Security.Cryptography;
 using System.Security.Principal;
 using System.Text;
 using System.Threading.Tasks;
 using System.Windows.Media.Animation;
 using System.Windows.Threading;
 using System.Xml;
 using SimajeEmeakomLib.Messages;
 using TableDependency.SqlClient;
 using TableDependency.SqlClient.Base.Enums;

 namespace RMS_Estimation_Service
 {
    using Models;
    using ServiceSettings;
    using System;
    using System.Data;
    using System.IO;
    using System.Threading;
    using System.Windows;
    using TableDependency.SqlClient.Base.EventArgs;

    /// <summary>
    /// Defines the <see cref="RmsService" />.
    /// </summary>
    public static class RmsService
     {
         //'Public con As New SqlConnection("Data Source=192.168.0.253,1433; Network Library=DBMSSOCN; Initial Catalog=SimajeCasaSupport; User ID=Nabil_Gayl; Password=SIMSQLSERVER@2017;")
         //'("server=.;Initial Catalog=CasaSupport;Integrated Security=True")


         public static readonly CultureInfo FrenchCulture = CultureInfo.CreateSpecificCulture("fr-FR");

         /// <summary>
         /// Defines the RmsMain.
         /// </summary>
         internal static MainRmsEstimationService RmsMain;

         /// <summary>
         /// Defines the DsService.
         /// </summary>
         internal static DataSet DsService = new DataSet("RMS");

         /// <summary>
         /// Defines the Adapter.
         /// </summary>
         internal static SqlDataAdapter Adapter;

         /// <summary>
         /// Defines the TableNames.
         /// </summary>
         internal static List<string> TableNames = new List<string>(
             new string[] {"ServiceCategory", "Services", "ServiceRelationship", "Quotes"} //
         );

        //internal static List<string> TableNames = new List<string>(
        //     new string[] { "ServiceCategory", "Services", "ServiceRelationship", "Quotes", "DataAsync" } //
        // );

        /// <summary>
        /// Defines the ViewCategories, ViewServices, ViewRelationship.
        /// </summary>
        internal static DataView ViewCategories, ViewServices, ViewRelationship;

         /// <summary>
         /// Defines the wSettings.
         /// </summary>
         internal static WindowSettings wSettings;

         internal static CategorieSettings CategorySettings;

        private static int _deletedId = -1;

         internal static SqlTableDependency<Service> SyncService { get; set; }
         internal static SqlTableDependency<ServiceCategory> SyncCategory { get; set; }

        /// <summary>
        /// Defines the StringConnection.
        /// </summary>
        private static readonly string StringConnection = GetConnectionString();

         /// <summary>
         /// Defines the connection.
         /// </summary>
         private static SqlConnection connection = new SqlConnection(StringConnection);

         public static string GetConnectionString()
         {
             string str;
             try
             {
                 //var dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                 //var filename = Path.Combine(dir, $@"RMS Estimation Service\Config\data.xml");

                 //var doc = new XmlDocument();

                 //doc.Load(filename);

                 var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                 var filename = Path.Combine(dir ?? string.Empty, $@"data.nga");

                 var doc = new XmlDocument();

                 doc.LoadXml(DecryptFile(filename));
                 //doc.Load(filename);

                 var root = doc.ChildNodes[0];

                 var serverName = root.ChildNodes[0].InnerText;
                 var port = root.ChildNodes[1].InnerText;
                 var databaseName = root.ChildNodes[2].InnerText;
                 var userName = root.ChildNodes[3].InnerText;
                 var password = root.ChildNodes[4].InnerText;
                //MessageBox.Show($"serverName: {serverName}\n"+
                //                $"databaseName: {databaseName}\n"+
                //                $"userName: {userName}\n" +
                //                $"password: {password}\n" 
                //                );

                //var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                /*****************************************************************/
                // Configuration for Remote SQL Server Connection
                /*****************************************************************/
                //if (config.ConnectionStrings.ConnectionStrings["NGAConnectionString"] == null)
                // {
                //     var connectionSetting = new ConnectionStringSettings
                //     {
                //         Name = $@"NGAConnectionString",
                //         ConnectionString =
                //             $@"Data Source={serverName},{port}; Network Library=DBMSSOCN; Initial Catalog={databaseName}; User ID={userName}; Password={password};",
                //         ProviderName = $@"System.Data.SqlClient"
                //     };

                //     config.ConnectionStrings.ConnectionStrings.Add(connectionSetting);
                //     config.Save(ConfigurationSaveMode.Modified);
                // }

                // return System.Configuration.ConfigurationManager.ConnectionStrings["RmsServicesNgaSqlServer"].ConnectionString;
                // str = config.ConnectionStrings.ConnectionStrings["NGAConnectionString"].ConnectionString;
                //return System.Configuration.ConfigurationManager.ConnectionStrings["RmsServicesRemoteSqlServer"].ConnectionString;
                // return System.Configuration.ConfigurationManager.ConnectionStrings["RmsServicesNgaSqlServer"].ConnectionString;
                // str = config.ConnectionStrings.ConnectionStrings["NGAConnectionString"].ConnectionString;
                //return System.Configuration.ConfigurationManager.ConnectionStrings["RmsServicesRemoteSqlServer"].ConnectionString;
                /*****************************************************************/
                /*****************************************************************/


                /*****************************************************************/
                // Configuration for LocalDB SQL Server Connection
                /*****************************************************************/
                str = System.Configuration.ConfigurationManager.ConnectionStrings["RmsServicesLocalDB"].ConnectionString;
                //str =  config.ConnectionStrings.ConnectionStrings["RmsServicesLocalDB"].ConnectionString;
                /*****************************************************************/
                /*****************************************************************/

               
            }
            catch (Exception ex)
             {
                 str = string.Empty;
                 SimajeMessageBox.Show(ex.Message, @"Error", MessageBoxButton.OK,
                     SimajeMessageBox.MessageBoxImage.Error);
             }

             return str;
         }

         internal static void InitializeTable(string tableName)
        {
            Adapter = new SqlDataAdapter($"SELECT * FROM {tableName} ORDER BY ID", connection);
            var cmdb = new SqlCommandBuilder { DataAdapter = Adapter };
            Adapter.FillLoadOption = LoadOption.OverwriteChanges;
            Adapter.Fill(DsService, tableName);
            DsService.Tables[tableName].PrimaryKey = new[] { DsService.Tables[tableName].Columns[0] };

        }

        //internal static int GetDataAsyncCount()
        //{
        //    var cmd = new SqlCommand($"SELECT COUNT(ID) FROM {TableNames[4]}", connection);
        //    connection.Open();
        //    var count = (int) cmd.ExecuteScalar();
        //    connection.Close();

        //    return count;

        //}

        //private static DataTable CheckedNewUpdate()
        //{
        //    var oldLines = DsService.Tables[TableNames[4]].Rows.Count;
        //    var newLines = GetDataAsyncCount();

        //    var table = new DataTable("DataAsync");

        //    if (oldLines == newLines)
        //        return table;

        //    var resultLines = newLines - oldLines;
        //    RefreshDataTable(TableNames[4]); //DataAsync
        //    //MessageBox.Show($@"oldLines : {oldLines}; newLines :{newLines}; result :{resultLines}");
        //    Adapter = new SqlDataAdapter($"SELECT * FROM {TableNames[4]} ORDER BY ID DESC OFFSET 0 ROWS FETCH NEXT {resultLines} ROWS ONLY", connection);
        //    var cmdb = new SqlCommandBuilder { DataAdapter = Adapter };
        //    Adapter.FillLoadOption = LoadOption.OverwriteChanges;


        //    Adapter.Fill(table);

        //    return table;
        //}


        //internal static void AsynchronousDataUpdate()
        //{
        //    var table = CheckedNewUpdate();

        //    if (table.Rows.Count == 0) return;

        //    foreach (DataRow row in table.Rows)
        //    {
        //        RefreshDataTable(row["TableName"].ToString(), int.Parse(row["Id_table"].ToString()), row["Operation"].ToString());
        //    }
        //    RmsMain.RefreshDataCategories();
        //    RmsMain.RefreshDataServices();
        //}


        internal static void FillServiceRelationship(string tableName, int id, ChangeType operation)
        {
            var condition = tableName.Equals("services", StringComparison.OrdinalIgnoreCase) ?
                            $"s.ID = {id}" :
                            $"s.Id_ServiceCategory = {id}";

            condition = $"s.ID = {id}";
            //if (tableName.Equals("services", StringComparison.OrdinalIgnoreCase))
            //{
            var query = $"SELECT s.ID, s.Service_Type, sc.Name, s.Estimation, s.Description, s.Id_ServiceCategory " +
                $"FROM ServiceCategory sc " +
                $"INNER JOIN Services s ON sc.ID = s.Id_ServiceCategory AND {condition} ";

            //MessageBox.Show("FillServiceRelationship : "+condition);

            if (operation == ChangeType.Delete)
            {
                var drRelation = DsService.Tables[tableName].Rows.Find(id);
                drRelation.Delete();
            }

            using (Adapter = new SqlDataAdapter(query, connection))
            {
                Adapter.ResetFillLoadOption();
                Adapter.FillLoadOption = LoadOption.OverwriteChanges;
                var cmdb = new SqlCommandBuilder { DataAdapter = Adapter };

                Adapter.Fill(DsService, tableName);

                DsService.Tables[tableName].AcceptChanges();
            }
        }


        internal static void RefreshDataTable(string tableName, int id, ChangeType operation)
        {
            if (operation == ChangeType.Delete)
            {
                if (tableName.Equals("services", StringComparison.OrdinalIgnoreCase))
                {
                    var dr = DsService.Tables[tableName].Rows.Find(id);
                    dr.Delete();
                }
                if (tableName.Equals("ServiceCategory", StringComparison.OrdinalIgnoreCase))
                {
                    var dr = DsService.Tables[tableName].Rows.Find(id);
                    dr.Delete();
                }
            }

            using (Adapter = new SqlDataAdapter($"SELECT * FROM {tableName} WHERE id = {id}", connection))
            {
                Adapter.ResetFillLoadOption();
                Adapter.FillLoadOption = LoadOption.OverwriteChanges;
                var cmdb = new SqlCommandBuilder { DataAdapter = Adapter };

                Adapter.Fill(DsService, tableName);

                DsService.Tables[tableName].AcceptChanges();
            }

            if (tableName.Equals("services", StringComparison.OrdinalIgnoreCase))
            {
                FillServiceRelationship(TableNames[2], id, operation);
            }
        }


        /// <summary>
        /// The FillByNumberLine.
        /// </summary>
        /// <param name="tableName">The tableName<see cref="string"/>.</param>
        /// <param name="lineStep">The lineStep<see cref="int"/>.</param>
        /// <param name="lines">The lines<see cref="int"/>.</param>
        internal static void FillTableByNumberLine(string tableName, int lineStep, int lines)
         {
             var query = $"SELECT * FROM {tableName} ORDER BY ID " +
                         $"OFFSET {lines} ROWS " +
                         $"FETCH NEXT {lineStep} ROWS ONLY";

             if (tableName.Equals("ServiceRelationship"))
             {
                 query =
                     $"SELECT s.ID, s.Service_Type, sc.Name, s.Estimation, s.Description, s.Id_ServiceCategory FROM ServiceCategory sc " +
                     $"INNER JOIN Services s ON sc.ID = s.Id_ServiceCategory ORDER BY s.ID " +
                     $"OFFSET {lines} ROWS " +
                     $"FETCH NEXT {lineStep} ROWS ONLY";
             }

            //s.ID, s.Service_Type, sc.Name, s.Estimation

            using (Adapter = new SqlDataAdapter(query, connection))
            {
                var cmdb = new SqlCommandBuilder { DataAdapter = Adapter };
                Adapter.Fill(DsService, tableName);
                DsService.Tables[tableName].PrimaryKey = new[] { DsService.Tables[tableName].Columns["ID"] };

                DsService.Tables[tableName].AcceptChanges();
            }
         }
         /// <summary>
         /// The GetServiceCategories.
         /// </summary>
         /// <returns>The <see cref="DataTable"/>.</returns>
         internal static DataTable GetServiceCategories()
         {
             return DsService.Tables["ServiceCategory"];
         }

         /// <summary>
         /// The GetServicesTypesByIdServiceCategory.
         /// </summary>
         /// <param name="id">The id<see cref="int"/>.</param>
         /// <returns>The <see cref="DataTable"/>.</returns>
         internal static DataTable GetServicesTypesByIdServiceCategory(int id)
         {
             ViewServices.RowFilter = $"Id_ServiceCategory =  {id}";
             ViewServices.Sort = "ID ASC";
             return ViewServices.ToTable();
         }

        /// <summary>
        /// The AddServiceType.
        /// </summary>
        /// <param name="service">The service<see cref="Service"/>.</param>
        internal static  void AddServiceTypeAsync(Service service)
        {
            try
            {
                var dr = DsService.Tables[TableNames[1]].NewRow();
                dr["ID"] = service.ID;
                dr["Service_Type"] = service.Service_Type;
                dr["Estimation"] = service.Estimation;
                dr["Description"] = service.Description;
                dr["Id_ServiceCategory"] = service.Id_ServiceCategory;

                DsService.Tables[TableNames[1]].Rows.Add(dr);

                var drRelation = DsService.Tables[TableNames[2]].NewRow();

                drRelation["ID"] = -1;
                drRelation["Service_Type"] = service.Service_Type;
                drRelation["Estimation"] = service.Estimation;
                drRelation["Description"] = service.Description;
                drRelation["Id_ServiceCategory"] = service.Id_ServiceCategory;
                drRelation["Name"] = DsService.Tables[TableNames[0]].Rows.Find(service.Id_ServiceCategory)["Name"];

                DsService.Tables[TableNames[2]].Rows.Add(drRelation);

                 UpdateData(TableNames[1], service.ID);
            }
            catch (Exception ex)
            {
                SimajeMessageBox.Show(ex.Message, @"Error : AddServiceType", MessageBoxButton.OK,
                    SimajeMessageBox.MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// The UpdateServiceType.
        /// </summary>
        /// <param name="service">The service<see cref="Service"/>.</param>
        internal static  void UpdateServiceTypeAsync(Service service)
         {
             try
             {
                 var dr = DsService.Tables[TableNames[1]].Rows.Find(service.ID);
                 var drRelation = DsService.Tables[TableNames[2]].Rows.Find(service.ID);

                 dr["Service_Type"] = service.Service_Type;
                 dr["Estimation"] = service.Estimation;
                 dr["Description"] = service.Description;
                 dr["Id_ServiceCategory"] = service.Id_ServiceCategory;


                 drRelation["Name"] = DsService.Tables[TableNames[0]].Rows.Find(service.Id_ServiceCategory)["Name"];
                 drRelation["Service_Type"] = service.Service_Type;
                 drRelation["Estimation"] = service.Estimation;
                 drRelation["Description"] = service.Description;
                 drRelation["Id_ServiceCategory"] = service.Id_ServiceCategory;

                 UpdateData(TableNames[1], service.ID);
             }
             catch (Exception ex)
             {
                 SimajeMessageBox.Show(ex.Message, @"Error : UpdateServiceType", MessageBoxButton.OK,
                     SimajeMessageBox.MessageBoxImage.Error);
             }

         }

         internal static  void AddDataAsync(DataAsync data)
         {
            try
            {
                var dr = DsService.Tables[TableNames[4]].NewRow();
                dr["ID"] = -1;
                dr["Username"] = data.Username;
                dr["TableName"] = data.TableName;
                dr["Id_table"] = data.Id_table;
                dr["Operation"] = data.Operation;
                dr["Date_operation"] = data.Date_operation;

                DsService.Tables[TableNames[4]].Rows.Add(dr);
                UpdateData(TableNames[4], data.ID);


            }
            catch (Exception ex)
            {
                SimajeMessageBox.Show(ex.Message, @"Error : AddDataAsync", MessageBoxButton.OK,
                    SimajeMessageBox.MessageBoxImage.Error);
            }

        }



        /// <summary>
        /// The DeleteServiceType.
        /// </summary>
        /// <param name="service">The service<see cref="Service"/>.</param>
        internal static  void DeleteServiceTypeAsync(Service service)
         {
            try
            {
                var id = service.ID;
                var dr = DsService.Tables[TableNames[1]].Rows.Find(id);

                var drRelation = DsService.Tables[TableNames[2]].Rows.Find(id);

                dr.Delete();
                drRelation.Delete();
                UpdateData(TableNames[1], id);
            }
            catch (Exception ex)
            {
                SimajeMessageBox.Show(ex.Message, @"Error : DeleteServiceType", MessageBoxButton.OK,
                    SimajeMessageBox.MessageBoxImage.Error);
            }
        }


        internal static  void AddServiceCategoryAsync(ServiceCategory category)
         {
            try
            {
                var dr = DsService.Tables[TableNames[0]].NewRow();
                dr["ID"] = -1;
                dr["Name"] = category.Name;
                dr["Description"] = category.Description;
                dr["IconData"] = category.IconData;
                dr["IconName"] = category.IconName;

                DsService.Tables[TableNames[0]].Rows.Add(dr);

                 UpdateData(TableNames[0], -1);
            }
            catch (Exception ex)
            {
                SimajeMessageBox.Show(ex.Message, @"Error : AddServiceCategory", MessageBoxButton.OK,
                    SimajeMessageBox.MessageBoxImage.Error);
            }
        }

        internal static  void UpdateServiceCategoryAsync(ServiceCategory category)
         {
             try
             {
                 var dr = DsService.Tables[TableNames[0]].Rows.Find(category.ID);

                 dr["Name"] = category.Name;
                 dr["Description"] = category.Description;
                 dr["IconData"] = category.IconData;
                 dr["IconName"] = category.IconName;

                  UpdateData(TableNames[0], category.ID);


             }
             catch (Exception ex)
             {
                 SimajeMessageBox.Show(ex.Message, @"Error : UpdateServiceCategory", MessageBoxButton.OK,
                     SimajeMessageBox.MessageBoxImage.Error);
             }
         }

         internal static  void DeleteServiceCategoryAsync(ServiceCategory category)
         {
             try
             {
                 var dr = DsService.Tables[TableNames[0]].Rows.Find(category.ID);
                 dr.Delete();
                  UpdateData(TableNames[0], category.ID);
             }
             catch (Exception ex)
             {
                 SimajeMessageBox.Show(ex.Message, @"Error : DeleteServiceCategory", MessageBoxButton.OK,
                     SimajeMessageBox.MessageBoxImage.Error);

             }
         }

         /// <summary>
         /// The UpdateData.
         /// </summary>
         /// <param name="tableName">The tableName<see cref="string"/>.</param>
         /// <param name="status">The status<see cref="OperationStatus"/>.</param>
         private static void UpdateData(string tableName, int id)
         {
             _deletedId = id;

             using (Adapter = new SqlDataAdapter($"SELECT * FROM {tableName} WHERE id = {id}", connection))
             {
                 Adapter.FillLoadOption = LoadOption.OverwriteChanges;
                 Adapter.RowUpdated += Adapter_OnRowUpdated;
                 var cmdb = new SqlCommandBuilder {DataAdapter = Adapter};

                 Adapter.Update(DsService, tableName);
             }
         }

         /// <summary>
         /// The AdapterOnRowUpdated.
         /// </summary>
         /// <param name="sender">The sender<see cref="object"/>.</param>
         /// <param name="e">The e<see cref="SqlRowUpdatedEventArgs"/>.</param>
         private static void Adapter_OnRowUpdated(object sender, SqlRowUpdatedEventArgs e)
         {
             DataAsync data;
             switch (e.StatementType)
             {
                 case StatementType.Insert:
                     var cmd = new SqlCommand($"SELECT IDENT_CURRENT ('{e.TableMapping.SourceTable}')", connection);

                    var newId = Convert.ToInt32(cmd.ExecuteScalar());

                    e.Row["ID"] = newId;

                     if (e.TableMapping.SourceTable.Equals("Services"))
                     {
                         var row = DsService.Tables[TableNames[2]].Rows.Find(-1);
                         row["ID"] = newId;
                     }


                     if (e.TableMapping.SourceTable.Equals("DataAsync")) return;

                    data = new DataAsync
                     {
                         Username = WindowsIdentity.GetCurrent().Name,
                         TableName = e.TableMapping.SourceTable,
                         Id_table = newId,
                         Operation = "Add",
                         Date_operation = DateTime.Now
                     };

                     AddDataAsync(data);

                    break;

                 case StatementType.Update:

                     if (e.TableMapping.SourceTable.Equals("DataAsync")) return;


                    data = new DataAsync
                     {
                         Username = WindowsIdentity.GetCurrent().Name,
                         TableName = "Services",
                         Id_table = int.Parse(e.Row["ID"].ToString()),
                         Operation = "Update",
                         Date_operation = DateTime.Now
                     };

                     AddDataAsync(data);
                     break;
                case StatementType.Delete:
                    if (e.TableMapping.SourceTable.Equals("ServiceCategory"))
                    {

                        var idCategory = _deletedId;

                        var relationship = DsService.Tables[TableNames[2]];

                        foreach (var dataRow in relationship.Select($"Id_ServiceCategory = {idCategory}"))
                        {
                            dataRow.Delete();
                        }
                    }

                    data = new DataAsync
                    {
                        Username = WindowsIdentity.GetCurrent().Name,
                        TableName = e.TableMapping.SourceTable,
                        Id_table = _deletedId,
                        Operation = "Delete",
                        Date_operation = DateTime.Now
                    };

                    AddDataAsync(data);
                    break;
                default:
                     break;
             }
         }


         // 1 - Sql Server : Right Click in Database "RmsServices" => properties => Options => Service Broker =>
         // Make Broker Enabled = true
         // Right Click in Database "RmsServices" => properties => Files => Owner => Change it to 'sa'

        internal static void BeginDbSynchronization()
        {
            SyncService = new SqlTableDependency<Service>(GetConnectionString(), "Services");

            SyncService.OnChanged += SynchronizeService_OnChanged;
            //SyncService.OnStatusChanged +=SyncService_OnStatusChanged;
            SyncService.Start();

            SyncCategory = new SqlTableDependency<ServiceCategory>(GetConnectionString(), "ServiceCategory");
            SyncCategory.OnChanged += SynchronizeServiceCategory_OnChangedAsync;
            SyncCategory.Start();
        }



        internal static void StopDbSynchronization()
        {
            if (SyncService == null) return;
            SyncService.Stop();

            SyncService = null;

            if (SyncCategory == null) return;
            SyncCategory.Stop();

            SyncCategory = null;
        }

        private static  void SynchronizeServiceCategory_OnChangedAsync(object sender, RecordChangedEventArgs<ServiceCategory> e)
        {

            switch (e.ChangeType)
            {
                case ChangeType.None:
                    break;
                case ChangeType.Delete:

                    Thread.Sleep(1500);
                    RefreshDataTable("ServiceCategory", e.Entity.ID, ChangeType.Delete);
                    
                    break;
                case ChangeType.Insert:
                    RefreshDataTable("ServiceCategory", e.Entity.ID, ChangeType.Insert);
                    break;
                case ChangeType.Update:
                    RefreshDataTable("ServiceCategory", e.Entity.ID, ChangeType.Update);

                    break;
                default:
                    break;
            }

            if (e.ChangeType != ChangeType.None)
            {
                RmsMain.Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(() => { RmsMain.RefreshDataCategories(); }));
                if (CategorySettings != null && CategorySettings.Visible)
                    CategorySettings.RefreshData();
            }

        }

        private static void SynchronizeService_OnChanged(object sender, RecordChangedEventArgs<Service> e)
        {
            switch (e.ChangeType)
            {
                case ChangeType.None:
                    break;
                case ChangeType.Delete:
                    RefreshDataTable("Services", e.Entity.ID, ChangeType.Delete);

                    break;
                case ChangeType.Insert:
                    RefreshDataTable("Services", e.Entity.ID, ChangeType.Insert);
                    break;
                case ChangeType.Update:
                    RefreshDataTable("Services", e.Entity.ID, ChangeType.Update);

                    break;
                default:
                    break;
            }

            if (e.ChangeType != ChangeType.None)
            {

                RmsMain.Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        RmsMain.RefreshDataServices();
                    }));
                if (wSettings != null && wSettings.Visible)
                    wSettings.RefreshData();
            }
        }

        /// <summary>
        /// The TimeSpanToDouble.
        /// </summary>
        /// <param name="timeSpan">The timeSpan<see cref="TimeSpan"/>.</param>
        /// <param name="unit">The unit<see cref="char"/>.</param>
        /// <returns>The <see cref="double"/>.</returns>
        internal static double TimeSpanToDouble(TimeSpan timeSpan, char unit)
         {
             if (unit.Equals('s'))
                 return timeSpan.TotalSeconds;
             if (unit.Equals('m'))
                 return timeSpan.TotalMinutes;
             if (unit.Equals('h'))
                 return timeSpan.TotalHours;
             if (unit.Equals('d'))
                 return timeSpan.TotalDays;
             return 0;
         }

         /// <summary>
         /// The DoubleToTimeSpan.
         /// </summary>
         /// <param name="value">The value<see cref="double"/>.</param>
         /// <param name="unit">The unit<see cref="char"/>.</param>
         /// <returns>The <see cref="TimeSpan"/>.</returns>
         internal static TimeSpan DoubleToTimeSpan(double value, char unit)
         {
             if (unit.Equals('s'))
                 return TimeSpan.FromSeconds(value);
             if (unit.Equals('m'))
                 return TimeSpan.FromMinutes(value);
             if (unit.Equals('h'))
                 return TimeSpan.FromHours(value);
             if (unit.Equals('d'))
                 return TimeSpan.FromDays(value);
             return TimeSpan.Zero;
         }

         /* Splash Screen ****************************************************/

         public static int GetAllLinesCount()
         {
             var count = 0;

             foreach (var name in TableNames)
             {
                 count += (int) GetTableLinesCount(name);
             }

             return count;
         }

         public static int GetTableLinesCount(string tableName)
         {
             var query = $"SELECT COUNT(*) FROM {tableName}";

             if (tableName.Equals("ServiceRelationship"))
             {
                 query = "SELECT COUNT(*) FROM ServiceCategory sc " +
                         "INNER JOIN Services s ON sc.ID = s.Id_ServiceCategory";

             }

             var cmd = new SqlCommand(query, connection);
             if (connection.State == ConnectionState.Closed)
             {
                 connection.Open();
             }

             var count = (int) cmd.ExecuteScalar();

             if (connection.State == ConnectionState.Open)
             {
                 connection.Close();
             }

             return count;
         }

         public static void DoAnimation(FrameworkElement element, double from, double to, TimeSpan time,
             DependencyProperty property)
         {
             var storyboard = new Storyboard();
             var animate = new DoubleAnimation
             {
                 From = from,
                 To = to,
                 Duration = new Duration(time)
             };

             var ease = new ExponentialEase() {EasingMode = EasingMode.EaseOut};
             animate.EasingFunction = ease;
             Storyboard.SetTarget(animate, element);
             Storyboard.SetTargetProperty(animate, new PropertyPath(FrameworkElement.OpacityProperty));
             storyboard.Children.Add(animate);

             storyboard.Begin(element);
         }

         public static void DoAnimation(FrameworkElement element, double from, double to, TimeSpan time,
             DependencyProperty property, EventHandler storyboardEventHandler)
         {
             var storyboard = new Storyboard();
             storyboard.Completed += storyboardEventHandler;
             var animate = new DoubleAnimation
             {
                 From = from,
                 To = to,
                 Duration = new Duration(time)
             };

             var ease = new ExponentialEase() {EasingMode = EasingMode.EaseOut};
             animate.EasingFunction = ease;
             Storyboard.SetTargetName(animate, element.Name);
             Storyboard.SetTargetProperty(animate, new PropertyPath(property));
             storyboard.Children.Add(animate);

             storyboard.Begin(element);
         }

         internal static string GetQuote()
         {
             var random = new Random();
             var tableQuotes = DsService.Tables[TableNames[3]];
             var count = tableQuotes.Rows.Count - 1;
             var i = random.Next(0, count);
             var row = tableQuotes.Rows[i];
             return row["QuoteName"].ToString();
         }


        internal static string DecryptFile(string fileName)
        {
            string decryptedData = string.Empty;
            var myKey = new Guid("8BEA4F4C-9C63-499b-ADBA-B3755004DCB9");

            if (!File.Exists(fileName)) return decryptedData;

            string content;
            using (StreamReader streamReader = new StreamReader(fileName, true))
            {
                content = streamReader.ReadToEnd();
                streamReader.Close();
            }

            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(content);
            var tdes = new TripleDESCryptoServiceProvider();
            var hashmd5 = new MD5CryptoServiceProvider();

            try
            {
                keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(myKey.ToString()));

                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateDecryptor();
                var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                decryptedData = Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception ex)
            {
                SimajeMessageBox.Show("DecryptFile : " + ex.Message, "Error", MessageBoxButton.OK, SimajeMessageBox.MessageBoxImage.Error);
            }
            finally
            {
                tdes.Clear();
                hashmd5.Clear();
            }

            tdes.Padding = PaddingMode.None;

            return decryptedData;
        }

        /*******************************************************************/
    }


 }
