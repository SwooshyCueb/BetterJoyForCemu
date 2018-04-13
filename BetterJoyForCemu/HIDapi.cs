using System;
using System.Runtime.InteropServices;

namespace BetterJoyForCemu {
	public class HIDapi {
		const string dll = "hidapi.dll";

		[StructLayout(LayoutKind.Sequential)]
		public struct hid_device_info {
			[MarshalAs(UnmanagedType.LPStr)]
			public string path;
			public ushort vendor_id;
			public ushort product_id;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string serial_number;
			public ushort release_number;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string manufacturer_string;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string product_string;
			public ushort usage_page;
			public ushort usage;
			public int interface_number;
			public IntPtr next;
		};

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hid_init();

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hid_exit();

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr hid_enumerate(ushort vendor_id, ushort product_id);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hid_free_enumeration(IntPtr phid_device_info);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr hid_open(ushort vendor_id, ushort product_id, [MarshalAs(UnmanagedType.LPWStr)]string serial_number);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr hid_open_path([MarshalAs(UnmanagedType.LPStr)]string path);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hid_write(IntPtr device, byte[] data, UIntPtr length);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hid_read_timeout(IntPtr dev, byte[] data, UIntPtr length, int milliseconds);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hid_read(IntPtr device, byte[] data, UIntPtr length);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hid_set_nonblocking(IntPtr device, int nonblock);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hid_send_feature_report(IntPtr device, byte[] data, UIntPtr length);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hid_get_feature_report(IntPtr device, byte[] data, UIntPtr length);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hid_close(IntPtr device);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hid_get_manufacturer_string(IntPtr device, [MarshalAs(UnmanagedType.LPWStr)]string string_, UIntPtr maxlen);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hid_get_product_string(IntPtr device, [MarshalAs(UnmanagedType.LPWStr)]string string_, UIntPtr maxlen);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hid_get_serial_number_string(IntPtr device, [MarshalAs(UnmanagedType.LPWStr)]string string_, UIntPtr maxlen);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hid_get_indexed_string(IntPtr device, int string_index, [MarshalAs(UnmanagedType.LPWStr)]string string_, UIntPtr maxlen);

		[DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string hid_error(IntPtr device);

		static void PrintEnumeration(IntPtr phid_device_info) {
			if (!phid_device_info.Equals(IntPtr.Zero)) {
				hid_device_info hdev = (hid_device_info)Marshal.PtrToStructure(phid_device_info, typeof(hid_device_info));

				PrintDevInfo(ref hdev);

				PrintEnumeration(hdev.next);
			}
		}

		internal static void PrintDevInfo(ref hid_device_info devinfo) {
			Console.WriteLine($"path:                {devinfo.path}");
			Console.WriteLine($"vendor_id:           0x{devinfo.vendor_id:x4}");
			Console.WriteLine($"product_id:          0x{devinfo.product_id:x4}");
			Console.WriteLine($"serial_number:       {devinfo.serial_number}");
			Console.WriteLine($"release_number:      0x{devinfo.release_number:x4}");
			Console.WriteLine($"manufacturer_string: {devinfo.manufacturer_string}");
			Console.WriteLine($"product_string:      {devinfo.product_string}");
			Console.WriteLine($"usage_page:          0x{devinfo.usage_page:x4}");
			Console.WriteLine($"usage:               0x{devinfo.usage:x4}");
			Console.WriteLine($"interface_number:    {devinfo.interface_number}");

			Console.WriteLine("");
		}

		static string _getDevicePath(IntPtr phid_device_info, ushort usagePage, ushort usage) {
			if (!phid_device_info.Equals(IntPtr.Zero)) {
				hid_device_info hdev = (hid_device_info)Marshal.PtrToStructure(phid_device_info, typeof(hid_device_info));
				if (usagePage == hdev.usage_page && usage == hdev.usage)
					return hdev.path;
				else
					return _getDevicePath(hdev.next, usagePage, usage);
			}
			return null;
		}

		public static string GetDevicePath(ushort vendorId, ushort productId, ushort usagePage, ushort usage) {
			return _getDevicePath(hid_enumerate(vendorId, productId), usagePage, usage);
		}
	}
}
