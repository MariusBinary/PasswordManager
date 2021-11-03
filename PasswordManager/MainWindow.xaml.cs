using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace PasswordManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            access_form.Visibility = Visibility.Collapsed;
            create_form.Visibility = Visibility.Collapsed;
            alert_form.Visibility = Visibility.Collapsed;

        }

        private void btn_new_package_Click(object sender, RoutedEventArgs e)
        {
            create_form.Visibility = Visibility.Visible;
        }

        string load_path = null;
        JObject load_json_data = null;
        private void btn_load_package_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".pmsk";
            dlg.Filter = "PasswordManager (*.pmsk)|*.pmsk";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                load_path = filename;

                string text = "";
                var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    text = SecureKey.Decrypt(streamReader.ReadToEnd(), "Random");
                }

                if (text == "ERR:CORRUPTED-FILE")
                {
                    alert_form.Visibility = Visibility.Visible;
                }
                else
                {
                    load_json_data = JObject.Parse(text);
                    access_form.Visibility = Visibility.Visible;
                    access_form_password.Focus();
                }
            }
        }

        private void access_form_btn_decrypt_Click(object sender, RoutedEventArgs e)
        {
            access_form_decrypt();
        }

        private void access_form_decrypt()
        {
            JObject file_info = load_json_data;
            Console.WriteLine(file_info);

            string password = (string)file_info["password"];
            if (password == PasswordValidator.encryptPassword(access_form_password.Password))
            {
                PasswordManagerExplorer p = new PasswordManagerExplorer(load_path);
                p.Show();
                this.Close();
            }
        }

        private void create_form_btn_add_Click(object sender, RoutedEventArgs e)
        {
            string json = null;
            DateTime dateTime = DateTime.UtcNow.Date;

            string name = create_form_name.Text;
            string password = PasswordValidator.encryptPassword(create_form_password.Password);
            string creation_data = dateTime.ToString("dd/MM/yyyy");
            int count = 0;

            JObject file_info = new JObject();
            file_info.Add("name", name);
            file_info.Add("password", password);
            file_info.Add("creation_data", creation_data);
            file_info.Add("count", count);
            //file_info.Add("saved_keys", new JArray("Real", "OnSale"));

            Console.WriteLine(file_info);
            json = file_info.ToString();

            SaveFileDialog savefile = new SaveFileDialog();
            // set a default file name
            savefile.FileName = "keys_" + name + ".pmsk";
            // set filters - this can be done in properties as well
            savefile.Filter = "PasswordManager (*.pmsk)|*.pmsk";

            Nullable<bool> result = savefile.ShowDialog();
            if (result == true)
            {
                using (StreamWriter sw = new StreamWriter(savefile.FileName))
                    sw.WriteLine(SecureKey.Encrypt(json, "Random"));

                PasswordManagerExplorer p = new PasswordManagerExplorer(savefile.FileName);
                p.Show();
                this.Close();
            }
        }

        private void access_form_back_Click(object sender, RoutedEventArgs e)
        {
            access_form.Visibility = Visibility.Collapsed;
        }

        private void create_form_back_Click(object sender, RoutedEventArgs e)
        {
            create_form.Visibility = Visibility.Collapsed;
        }

        private void alert_form_close_Click(object sender, RoutedEventArgs e)
        {
            alert_form.Visibility = Visibility.Collapsed;
        }

        private void access_form_password_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                access_form_decrypt();
            }
        }
    }
}
