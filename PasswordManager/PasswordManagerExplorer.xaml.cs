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
using System.Windows.Shapes;

namespace PasswordManager
{
    /// <summary>
    /// Logica di interazione per PasswordManagerExplorer.xaml
    /// </summary>
    public partial class PasswordManagerExplorer : Window
    {
        string curr_path;

        JObject curr_object;
        List<Tuple<String, String, String>> curr_keys_list = new List<Tuple<string, string, string>>();

        public void itemWatch(int position)
        {
            watch_form.Visibility = Visibility.Visible;
            watch_form_account.Text = curr_keys_list[position].Item1;
            watch_form_descr_account.Text = "Watch saved data from " + curr_keys_list[position].Item1;
            watch_form_username.Text = curr_keys_list[position].Item2;
            watch_form_password.Text = curr_keys_list[position].Item3;

            edit_last_position = position;
        }

        public void itemDelete(int position)
        {
            curr_keys_list.RemoveAt(position);
            writeSecureFile("remove");
        }

        public static PasswordManagerExplorer context;
        public PasswordManagerExplorer(string path)
        {
            InitializeComponent();

            new_account_form.Visibility = Visibility.Collapsed;
            aler_no_keys.Visibility = Visibility.Collapsed;
            watch_form.Visibility = Visibility.Collapsed;
            alert_form.Visibility = Visibility.Collapsed;

            curr_path = path;
            refreshList();

            context = this;
        }

        private void refreshList()
        {
            stackPanel.Children.Clear();

            string text = "";
            var fileStream = new FileStream(curr_path, FileMode.Open, FileAccess.Read);
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
                curr_object = JObject.Parse(text);

                JObject file_info = curr_object;
                Console.WriteLine(file_info);

                string name = (string)file_info["name"];
                string creation_data = (string)file_info["creation_data"];
                string keys_count = (string)file_info["count"];

                menu_file.Text = null;
                menu_data.Text = null;
                menu_keys_count.Text = null;

                menu_file.Inlines.Add(new Run("File : ") { Foreground = System.Windows.Media.Brushes.Gray });
                menu_file.Inlines.Add(new Run(name) { Foreground = System.Windows.Media.Brushes.Gray, FontWeight = FontWeights.Bold });

                menu_data.Inlines.Add(new Run("Data : ") { Foreground = System.Windows.Media.Brushes.Gray });
                menu_data.Inlines.Add(new Run(creation_data) { Foreground = System.Windows.Media.Brushes.Gray, FontWeight = FontWeights.Bold });

                menu_keys_count.Inlines.Add(new Run("Keys : ") { Foreground = System.Windows.Media.Brushes.Gray });
                menu_keys_count.Inlines.Add(new Run(keys_count) { Foreground = System.Windows.Media.Brushes.Gray, FontWeight = FontWeights.Bold });

                if (Convert.ToInt32(keys_count) == 0)
                {
                    aler_no_keys.Visibility = Visibility.Visible;
                }

                curr_keys_list = new List<Tuple<string, string, string>>();
                for (int i = 0; i < Convert.ToInt32(keys_count); i++)
                {
                    curr_keys_list.Add(Tuple.Create((string)file_info["saved_keys"][i]["name"], (string)file_info["saved_keys"][i]["username"], (string)file_info["saved_keys"][i]["password"]));
                    item_key key = new item_key(i, (string)file_info["saved_keys"][i]["name"]);
                    key.Width = stackPanel.Width;
                    key.Height = 70;
                    Grid grd = new Grid();
                    grd.Width = 2;
                    grd.Height = 2;
                    stackPanel.Children.Add(grd);
                    stackPanel.Children.Add(key);
                }
            }
        }

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            new_account_form.Visibility = Visibility.Visible;
        }

        private void new_account_form_btn_save_Click(object sender, RoutedEventArgs e)
        {
            new_account_form.Visibility = Visibility.Collapsed;

            if (new_account_form_account.Text != String.Empty || new_account_form_username.Text != String.Empty || new_account_form_password.Password != String.Empty)
            {
                curr_keys_list.Add(Tuple.Create(new_account_form_account.Text, new_account_form_username.Text, new_account_form_password.Password));
                writeSecureFile("add");
                new_account_form_account.Text = string.Empty;
                new_account_form_username.Text = string.Empty;
                new_account_form_password.Password = string.Empty;
            }
            else
            {
                MessageBox.Show("Empty all spaces !");
            }
        }

        private void writeSecureFile(string action)
        {
            string json = null;

            string name = (string)curr_object["name"];
            string password = (string)curr_object["password"];
            string creation_data = (string)curr_object["creation_data"];

            int count = 0;
            if (action.Equals("add"))
                count = (int)curr_object["count"] + 1;
            else if (action.Equals("remove"))
                count = (int)curr_object["count"] - 1;
            else if (action.Equals("edit"))
                count = (int)curr_object["count"];

            JObject file_info = new JObject();
            file_info.Add("name", name);
            file_info.Add("password", password);
            file_info.Add("creation_data", creation_data);
            file_info.Add("count", count);

            List<JObject> list_arrays = new List<JObject>();
            for (int i = 0; i < count; i++)
            {
                JObject array_info = new JObject();
                array_info.Add("name", curr_keys_list[i].Item1);
                array_info.Add("username", curr_keys_list[i].Item2);
                array_info.Add("password", curr_keys_list[i].Item3);
                list_arrays.Add(array_info);
            }

            file_info.Add("saved_keys", new JArray(list_arrays));

            Console.WriteLine(file_info);
            json = file_info.ToString();

            using (StreamWriter sw = new StreamWriter(curr_path))
                sw.WriteLine(SecureKey.Encrypt(json, "Random"));

            refreshList();
        }

        private void watch_form_btn_close_Click(object sender, RoutedEventArgs e)
        {
            watch_form.Visibility = Visibility.Collapsed;

            watch_form_username.IsReadOnly = true;
            watch_form_password.IsReadOnly = true;
            watch_form_btn_edit.Content = "Edit";
            watch_form.Visibility = Visibility.Collapsed;
            isWatchEditMode = false;
        }

        int edit_last_position;
 
        bool isWatchEditMode = false;
        private void watch_form_btn_edit_Click(object sender, RoutedEventArgs e)
        {
            if (isWatchEditMode)
            {
                watch_form_descr_account.Text = "Watch saved data from " + curr_keys_list[edit_last_position].Item1;
                watch_form_username.IsReadOnly = true;
                watch_form_password.IsReadOnly = true;
                watch_form_btn_edit.Content = "Edit";
                watch_form.Visibility = Visibility.Collapsed;
                isWatchEditMode = false;

                if (watch_form_username.Text != curr_keys_list[edit_last_position].Item2 || watch_form_password.Text != curr_keys_list[edit_last_position].Item3)
                {
                    Tuple<string,string,string> edit_tuple = Tuple.Create(curr_keys_list[edit_last_position].Item1, watch_form_username.Text, watch_form_password.Text);
                    curr_keys_list[edit_last_position] = edit_tuple;
                    writeSecureFile("edit");
                }
            }
            else
            {
                watch_form_descr_account.Text = "Edit saved data from " + curr_keys_list[edit_last_position].Item1;
                watch_form_username.IsReadOnly = false;
                watch_form_password.IsReadOnly = false;
                watch_form_btn_edit.Content = "Save";
                isWatchEditMode = true;
            }
        }

        private void alert_form_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void new_account_form_back_Click(object sender, RoutedEventArgs e)
        {
            new_account_form_account.Text = string.Empty;
            new_account_form_username.Text = string.Empty;
            new_account_form_password.Password = string.Empty;
            new_account_form.Visibility = Visibility.Collapsed;
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();
            mw.Show();
            this.Close();
        }
    }
}
