using System.Drawing.Printing;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ApplicationCore.Features.CNC.GCode.Domain;

public class Printer
{

    private readonly Image _image;

    public Printer(Image image)
    {
        _image = image;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "<Pending>")]
    public bool Print(string printer)
    {

        if (string.IsNullOrEmpty(printer))
            return false;

        try
        {
            // Configure print settings. 
            PrintDocument pd = new PrintDocument();
            PrinterSettings printerSettings = new PrinterSettings();
            printerSettings.PrinterName = printer;
            pd.PrinterSettings = printerSettings;

            pd.DefaultPageSettings.Margins.Bottom = 0;
            pd.DefaultPageSettings.Margins.Top = 0;
            pd.DefaultPageSettings.Margins.Left = 0;
            pd.DefaultPageSettings.Margins.Right = 0;

            pd.PrinterSettings.DefaultPageSettings.Margins.Bottom = 0;
            pd.PrinterSettings.DefaultPageSettings.Margins.Top = 0;
            pd.PrinterSettings.DefaultPageSettings.Margins.Left = 0;
            pd.PrinterSettings.DefaultPageSettings.Margins.Right = 0;

            // The name that will be displayed in Windows Printer Spool. 
            pd.DocumentName = "A sample print job";

            pd.PrintPage += new PrintPageEventHandler(Pd_PrintPage);
            pd.Print();


            pd.Dispose();
            return true;
        }
        catch (Exception exp)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Exception in StartPrint: " + exp.Message);
            Console.ResetColor();
            return false;
        }

    }

    private void Pd_PrintPage(object sender, PrintPageEventArgs ev)
    {
        // Change this to true if image scaling is required. 
        bool ScaleToFit = true;

        if (ev.Graphics is null) return;

        try
        {
            // Various print settings can be specified here. Can use default values. 
            ev.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            ev.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            ev.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            ev.Graphics.PageUnit = GraphicsUnit.Point;

            if (_image != null)
            {
                if (!ScaleToFit)
                    ev.Graphics.DrawImage(_image, 0, 0, _image.Width, _image.Height);
                else
                {
                    float s1 = (float)ev.Graphics.VisibleClipBounds.Width / _image.Width;
                    float s2 = (float)ev.Graphics.VisibleClipBounds.Height / _image.Height;
                    if (s1 > s2) s1 = s2;
                    int w = (int)(_image.Width * s1);
                    int h = (int)(_image.Height * s1);
                    int l = Convert.ToInt32(ev.Graphics.VisibleClipBounds.Left + (ev.Graphics.VisibleClipBounds.Width - w) / 2);
                    int t = Convert.ToInt32(ev.Graphics.VisibleClipBounds.Top + (ev.Graphics.VisibleClipBounds.Height - h) / 2);
                    ev.Graphics.DrawImage(_image, new Rectangle(l, t, w, h), new Rectangle(0, 0, _image.Width, _image.Height), GraphicsUnit.Pixel);
                }
            }

            ev.HasMorePages = false;

        }
        catch (Exception exp)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("An error occurred whiling printing: " + exp.Message);
            Console.ResetColor();
        }
    }

    /*#region Managed
	// Returns the list of printer drivers that are installed. 
	private static List<string> GetPrinterListManaged() {
		List<string> printers = new List<string>();

		// Getting the list of printers is easy with Standard .Net Framework.
		System.Drawing.Printing.PrinterSettings.StringCollection printers = System.Drawing.Printing.PrinterSettings.InstalledPrinters;
		foreach (string printer in printers) {
			// Extra checks can be placed here. For example: 
			// if(printer.Contains("CX-7000") == false)
			//		continue;
			printers.Add(printer);
		}

		return printers;
	}

	#endregion


	#region Unmanaged
	[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool EnumPrinters(int Flags, [MarshalAs(UnmanagedType.LPWStr)] string Name, uint Level, byte[] pPrinterEnum, uint cbBuf, ref uint pcbNeeded, ref uint pcReturned);

	// Gets the list of installed printer drivers. 
	public static string[] GetPrinterUnmanaged(string name) {
		List<string> printers = EnumPrinters();
		string[] printers = (from b in printers where b.ToLower().Contains(name) select b).ToArray();

		return printers;
	}

	private static List<string> EnumPrinters() {
		uint cbNeeded = 0;
		uint cReturned = 0;

		// The following call gets required buffer size. 
		if (EnumPrinters(2, null, 4, null, 0, ref cbNeeded, ref cReturned)) {
			return null;
		}
		List<string> printers = new List<string>();
		byte[] byteArray = new byte[cbNeeded];
		// IntPtr pAddr = Marshal.AllocHGlobal((int)cbNeeded);
		if (!EnumPrinters(2, null, 4, byteArray, cbNeeded, ref cbNeeded, ref cReturned)) {
			//throw new Win32Exception(lastWin32Error);
			return null;
		}

		GCHandle pinnedArray = GCHandle.Alloc(byteArray, GCHandleType.Pinned);
		IntPtr pointer = pinnedArray.AddrOfPinnedObject();
		int offset = pointer.ToInt32();
		Type type = typeof(PrinterInfo4);
		int increment = Marshal.SizeOf(type);
		for (int i = 0; i < cReturned; i++) {
			PrinterInfo4 printerInfo4 = new PrinterInfo4();
			Marshal.PtrToStructure(new IntPtr(offset), printerInfo4);
			printers.Add(printerInfo4.Name);
			offset += increment;
		}
		pinnedArray.Free();
		//Marshal.FreeHGlobal(pAddr);
		return printers;
	}
	#endregion*/
}