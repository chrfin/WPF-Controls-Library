using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfControlsLibrary.Ftp;
using WpfControlsLibrary.Demo.Properties;

namespace WpfControlsLibrary.Demo
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MainWindow"/> class.
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Handles the Loaded event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			passwordBoxFTP.Password = Settings.Default.FTPPassword.DecryptString().ToInsecureString();
		}

		/// <summary>
		/// Handles the Closing event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Settings.Default.FTPPassword = passwordBoxFTP.Password.ToSecureString().EncryptString();
			Settings.Default.Save();
		}

		/// <summary>
		/// Handles the Click event of the buttonFtpFolderBrowse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void buttonFtpFolderBrowse_Click(object sender, RoutedEventArgs e)
		{
			FtpFolderBrowserDialog fbd = new FtpFolderBrowserDialog();
			fbd.Owner = this;
			fbd.Description = "THIS IS THE DESCRIPTION";
			fbd.ShowNewFolderButton = true;

			fbd.Server = Settings.Default.FTPServer;
			fbd.Username = Settings.Default.FTPUsername;
			fbd.Password = passwordBoxFTP.Password.ToSecureString();

			if (fbd.ShowDialog().Value)
			{
				MessageBox.Show("SelectedPath: " + fbd.SelectedPath);
			}
		}
	}
}
