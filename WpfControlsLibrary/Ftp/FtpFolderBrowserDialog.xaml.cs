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
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Security;

namespace WpfControlsLibrary.Ftp
{
	/// <summary>
	/// Interaction logic for FtpFolderBrowserDialog.xaml
	/// </summary>
	public partial class FtpFolderBrowserDialog : Window
	{
		/// <summary>
		/// Gets a value indicating whether this instance has the root directory loaded.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has the root loaded; otherwise, <c>false</c>.
		/// </value>
		public bool IsRootLoaded { get; private set; }

		/// <summary>
		/// Gets or sets the descriptive text displayed above the tree view control in the dialog box.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
		public string Description
		{
			get { return (string)GetValue(DescriptionProperty); }
			set { SetValue(DescriptionProperty, value); }
		}
		public static readonly DependencyProperty DescriptionProperty =
			DependencyProperty.Register("Description", typeof(string), typeof(FtpFolderBrowserDialog));

		/// <summary>
		/// Gets or sets the root folder where the browsing starts from.
		/// </summary>
		/// <value>
		/// The root folder.
		/// </value>
		public string RootFolder
		{
			get { return (string)GetValue(RootFolderProperty); }
			set { SetValue(RootFolderProperty, value); }
		}
		public static readonly DependencyProperty RootFolderProperty =
			DependencyProperty.Register("RootFolder", typeof(string), typeof(FtpFolderBrowserDialog));

		/// <summary>
		/// Gets or sets the path selected by the user.
		/// </summary>
		/// <value>
		/// The selected path.
		/// </value>
		public string SelectedPath
		{
			get { return (string)GetValue(SelectedPathProperty); }
			set { SetValue(SelectedPathProperty, value); }
		}
		public static readonly DependencyProperty SelectedPathProperty =
			DependencyProperty.Register("SelectedPath", typeof(string), typeof(FtpFolderBrowserDialog));

		/// <summary>
		/// Gets or sets a value indicating whether the <b>New Folder</b> button appears in the folder browser dialog box.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [show new folder button]; otherwise, <c>false</c>.
		/// </value>
		public bool ShowNewFolderButton
		{
			get { return (bool)GetValue(ShowNewFolderButtonProperty); }
			set { SetValue(ShowNewFolderButtonProperty, value); }
		}
		public static readonly DependencyProperty ShowNewFolderButtonProperty =
			DependencyProperty.Register("ShowNewFolderButton", typeof(bool), typeof(FtpFolderBrowserDialog));

		/// <summary>
		/// Gets or sets the server. If the string does not start with 'ftp:' it will be attached.
		/// </summary>
		/// <value>
		/// The server.
		/// </value>
		public string Server
		{
			get { return (string)GetValue(ServerProperty); }
			set { SetValue(ServerProperty, value); }
		}
		public static readonly DependencyProperty ServerProperty =
			DependencyProperty.Register("Server", typeof(string), typeof(FtpFolderBrowserDialog));

		/// <summary>
		/// Gets or sets the username.
		/// </summary>
		/// <value>
		/// The username.
		/// </value>
		public string Username
		{
			get { return (string)GetValue(UsernameProperty); }
			set { SetValue(UsernameProperty, value); }
		}
		public static readonly DependencyProperty UsernameProperty =
			DependencyProperty.Register("Username", typeof(string), typeof(FtpFolderBrowserDialog));

		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		/// <value>
		/// The password.
		/// </value>
		public SecureString Password
		{
			get { return (SecureString)GetValue(PasswordProperty); }
			set { SetValue(PasswordProperty, value); }
		}
		public static readonly DependencyProperty PasswordProperty =
			DependencyProperty.Register("Password", typeof(SecureString), typeof(FtpFolderBrowserDialog));
		
		/// <summary>
		/// Initializes a new instance of the <see cref="FtpFolderBrowserDialog"/> class.
		/// </summary>
		public FtpFolderBrowserDialog()
		{
			InitializeComponent();
			this.SourceInitialized += (x, y) =>
			{
				this.HideMinimizeAndMaximizeButtons();
				this.HideIcon();
			};
		}

		/// <summary>
		/// Opens a window and returns without waiting for the newly opened window to close.
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException">
		///   <see cref="M:System.Windows.Window.Show"/> is called on a window that is closing (<see cref="E:System.Windows.Window.Closing"/>) or has been closed (<see cref="E:System.Windows.Window.Closed"/>).</exception>
		new public void Show() { PrepareRoot(); base.Show(); }
		/// <summary>
		/// Opens a window and returns only when the newly opened window is closed.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Nullable`1"/> value of type <see cref="T:System.Boolean"/> that signifies how a window was closed by the user.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///   <see cref="M:System.Windows.Window.ShowDialog"/> is called on a <see cref="T:System.Windows.Window"/> that is visible-or-<see cref="M:System.Windows.Window.ShowDialog"/> is called on a visible <see cref="T:System.Windows.Window"/> that was opened by calling <see cref="M:System.Windows.Window.ShowDialog"/>.</exception>
		///   
		/// <exception cref="T:System.InvalidOperationException">
		///   <see cref="M:System.Windows.Window.ShowDialog"/> is called on a window that is closing (<see cref="E:System.Windows.Window.Closing"/>) or has been closed (<see cref="E:System.Windows.Window.Closed"/>).</exception>
		new public bool? ShowDialog() { PrepareRoot(); return base.ShowDialog(); }

		/// <summary>
		/// Prepares the root directory. This also estabilshes the connection to the FTP server.
		/// </summary>
		public void PrepareRoot()
		{
			if (IsRootLoaded)
				return;

			string server = (Server.StartsWith("ftp://") ? String.Empty : "ftp://") + Server;
			FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(server);
			ftp.Method = WebRequestMethods.Ftp.ListDirectory;
			if (Username != null && Password != null)
				ftp.Credentials = new NetworkCredential(Username, Password.ToInsecureString());
			FtpWebResponse response = (FtpWebResponse)ftp.GetResponse();

			Stream responseStream = response.GetResponseStream();
			StreamReader reader = new StreamReader(responseStream);

			string[] folders = reader.ReadToEnd().Split('\n');
			response.Close();

			List<TreeViewItem> items = new List<TreeViewItem>();
			foreach (string folder in folders)
			{
				TreeViewItem item = new TreeViewItem();
				StackPanel panel = new StackPanel();
				Image image = new Image();
				TextBlock text = new TextBlock();

				image.Source = new BitmapImage(new Uri(@"Icons\folder.png", UriKind.Relative));
				panel.Children.Add(image);
				text.Text = folder;
				text.Height = 18;
				panel.Children.Add(text);
				panel.Orientation = Orientation.Horizontal;
				item.Header = panel;

				items.Add(item);
			}
			treeViewFolders.ItemsSource = items;
		}
	}
}
