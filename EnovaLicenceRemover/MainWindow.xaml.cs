using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EnovaLicenceRemover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            InstanceCB.SelectedItem = Settings1.Default.DataSource;
            SQLUserTB.Text = Settings1.Default.User;
            SQLPasswordPB.Password = Settings1.Default.Password;
            WindowsAuthCB.IsChecked = Settings1.Default.WindowsAuth;
            SecurityCB.IsChecked = Settings1.Default.PersistSecurityInfo;
                        
            if (InstanceCB.SelectedItem != null)            
                DatabaseCB.ItemsSource = GetDataBaseList();            
        }
        /// <summary>
        /// Populate the DataSourceCB with allowed local server instances
        /// </summary>
        /// <param name="sender"></param>
        /// <param
        private void LoadBT_Click(object sender, RoutedEventArgs e) => InstanceCB.ItemsSource = GetDataSources();
        /// <summary>
        /// Controls the visibility of items and sets a value for @windowsAuth
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowsAuthCB_Checked(object sender, RoutedEventArgs e)
        {
           SQLUserTB.IsEnabled = SQLPasswordPB.IsEnabled = false;                
        }
        private void WindowsAuthCB_Unchecked(object sender, RoutedEventArgs e)
        {
            SQLUserTB.IsEnabled = SQLPasswordPB.IsEnabled = true;
        }
        /// <summary>
        /// Reconnects to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshBT_Click(object sender, RoutedEventArgs e)
        {
            if (InstanceCB.SelectedItem != null)
                if (!String.IsNullOrEmpty(InstanceCB.SelectedItem.ToString()))            
                    DatabaseCB.ItemsSource = GetDataBaseList();            
        }
        /// <summary>
        /// Primary event that does the job
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveBT_Click(object sender, RoutedEventArgs e) => Commit();
        /// <summary>
        /// Depending on the selected database, it downloads information about the license number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DatabaseCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = getDBConnection(DatabaseCB.SelectedValue.ToString()))
                {
                    connection.Open();
                    string licence = "none";
                    using (SqlCommand cmd = new SqlCommand("SELECT Value FROM dbo.SystemInfos WHERE Ident = 10", connection))
                    {
                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                                licence = dr[0].ToString();
                        }
                    }
                    LicenceTB.Text = licence;
                }

            }
            catch (Exception ex) { MessageBox.Show("Error occured\n" + ex.Message); }
        }

        
    }

    public partial class MainWindow : Window
    {
        /// <summary>
        /// Gathers a list of databases and fills them in the comboBox
        /// </summary>
        /// <returns></returns>
        public List<string> GetDataBaseList()
        {
            List<string> list = new List<string>();
            try
            {
                using (SqlConnection conn = getDBConnection("master"))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", conn))
                    {
                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                                list.Add(dr[0].ToString());
                        }
                    }
                }
                list.Sort(delegate (string x, string y)
                {
                    return x.CompareTo(y);
                });
            }
            catch (Exception ex)
            {
                DatabaseCB.ItemsSource= null;
                LicenceTB.Text = string.Empty;
                MessageBox.Show("Failed to login to the server\n" + ex.Message);
            }
            return list;
        }
        /// <summary>
        /// Creates connection with @dataSource and database named @DataBaseName
        /// Depending on @windowsAuth, it using Integrated Security or SQL Credentials 
        /// </summary>
        /// <param name="DataBaseName"></param>
        /// <returns></returns>
        public SqlConnection getDBConnection(string DataBaseName)
        {
            if ((bool)WindowsAuthCB.IsChecked)
            {

                try
                {
                    return new SqlConnection(@"Data Source=" + InstanceCB.Text + ";Initial Catalog=" + DataBaseName + ";Integrated Security=True;");
                }
                catch { return null; }
            }
            else
            {
                try
                {
                    return new SqlConnection(@"Data Source=" + InstanceCB.Text + ";Initial Catalog=" + DataBaseName + ";Persist Security Info=" + (bool)SecurityCB.IsChecked + ";User ID=" + SQLUserTB.Text + ";Password=" + SQLPasswordPB.Password+ "");
                }
                catch { return null; }
            }

        }
        /// <summary>
        /// Deletes the record with the license number from the selected database
        /// </summary>
        public void Commit()
        {
            try
            {
                using (SqlConnection connection = getDBConnection(DatabaseCB.SelectedValue.ToString()))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM dbo.SystemInfos WHERE Ident = 10", connection);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Done");
            }
            catch (Exception ex) { MessageBox.Show("Error occured\n" + ex); }
        }
        /// <summary>
        /// Depending on the version of the system, it retrieves information about SQL server instances from the registry
        /// </summary>
        /// <returns></returns>
        private List<string> GetDataSources()
        {
            List<string> list = new List<string>();
            RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                RegistryKey instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);
                if (instanceKey != null)
                    foreach (var instanceName in instanceKey.GetValueNames())
                        if (instanceName == "MSSQLSERVER")
                            list.Add(".\\");
                        else
                            list.Add(".\\" + instanceName);
            }
            return list;
        }
    }
}
