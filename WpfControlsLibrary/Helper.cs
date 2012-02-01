using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Security;

namespace WpfControlsLibrary
{
	/// <summary>
	/// Container for several helper methods
	/// </summary>
	public static class Helper
	{
		/// <summary>
		/// Got from http://weblogs.asp.net/jgalloway/archive/2008/04/13/encrypting-passwords-in-a-net-app-config-file.aspx
		/// </summary>
		static byte[] entropy = System.Text.Encoding.Unicode.GetBytes("wpfControlsLibrary");

		/// <summary>
		/// Encrypts the string.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		public static string EncryptString(this SecureString input)
		{
			byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
				System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
				entropy,
				System.Security.Cryptography.DataProtectionScope.CurrentUser);
			return Convert.ToBase64String(encryptedData);
		}

		/// <summary>
		/// Decrypts the string.
		/// </summary>
		/// <param name="encryptedData">The encrypted data.</param>
		/// <returns></returns>
		public static SecureString DecryptString(this string encryptedData)
		{
			try
			{
				byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
					Convert.FromBase64String(encryptedData),
					entropy,
					System.Security.Cryptography.DataProtectionScope.CurrentUser);
				return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
			}
			catch
			{
				return new SecureString();
			}
		}

		/// <summary>
		/// Toes the secure string.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		public static SecureString ToSecureString(this string input)
		{
			SecureString secure = new SecureString();
			foreach (char c in input)
			{
				secure.AppendChar(c);
			}
			secure.MakeReadOnly();
			return secure;
		}

		/// <summary>
		/// Toes the insecure string.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		public static string ToInsecureString(this SecureString input)
		{
			string returnValue = string.Empty;
			IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
			try
			{
				returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
			}
			finally
			{
				System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
			}
			return returnValue;
		}
	}

	/// <summary>
	/// Helper for the Window class.
	/// </summary>
	/// <remarks>Source: http://stackoverflow.com/questions/339620/how-do-i-remove-minimize-and-maximize-from-a-resizable-window-in-wpf</remarks>
	internal static class WindowExtensions
	{
		[DllImport("user32.dll")]
		internal extern static int GetWindowLong(IntPtr hwnd, int index);
		[DllImport("user32.dll")]
		internal extern static int SetWindowLong(IntPtr hwnd, int index, int value);
		[DllImport("user32.dll")]
		static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

		/// <summary>
		/// Hides the minimize and maximize buttons.
		/// </summary>
		/// <param name="window">The window.</param>
		internal static void HideMinimizeAndMaximizeButtons(this Window window)
		{
			const int GWL_STYLE = -16;

			IntPtr hwnd = new WindowInteropHelper(window).Handle;
			long value = GetWindowLong(hwnd, GWL_STYLE);

			SetWindowLong(hwnd, GWL_STYLE, (int)(value & -131073 & -65537));
		}

		/// <summary>
		/// Hides the icon.
		/// </summary>
		/// <param name="window">The window.</param>
		/// <remarks>Source: http://www.danrigsby.com/blog/index.php/2008/05/26/remove-icon-from-wpf-window/</remarks>
		internal static void HideIcon(this Window window)
		{
			const int GWL_EXSTYLE = -20;
			const int WS_EX_DLGMODALFRAME = 0x0001;
			const int SWP_NOSIZE = 0x0001;
			const int SWP_NOMOVE = 0x0002;
			const int SWP_NOZORDER = 0x0004;
			const int SWP_FRAMECHANGED = 0x0020;

			IntPtr hwnd = new WindowInteropHelper(window).Handle;

			// Change the extended window style to not show a window icon
			int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
			SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);

			// Update the window's non-client area to reflect the changes
			SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
		}
	}
}
