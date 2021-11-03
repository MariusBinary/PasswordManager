using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Logica di interazione per item_key.xaml
    /// </summary>
    public partial class item_key : UserControl
    {
        int position = 0;
        public item_key(int position, string name)
        {
            InitializeComponent();
            this.name.Text = name;
            this.position = position;
        }

        private void btn_watch_Click(object sender, RoutedEventArgs e)
        {
            PasswordManagerExplorer.context.itemWatch(position);
        }


        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            PasswordManagerExplorer.context.itemDelete(position);
        }
    }
}
