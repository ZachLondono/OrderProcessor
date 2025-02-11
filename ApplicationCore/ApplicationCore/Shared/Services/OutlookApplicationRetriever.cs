using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace ApplicationCore.Shared.Services;

public static class OutlookApplicationRetriever {

    internal const string OLEAUT32 = "oleaut32.dll";
    internal const string OLE32 = "ole32.dll";

    public static Outlook.Application? GetApplication() {

        var obj = GetActiveObject("Outlook.Application");

        return obj as Outlook.Application;

    }

    [SecurityCritical]  // auto-generated_required
    private static object GetActiveObject(string progID) {
        object obj = null;
        Guid clsid;

        // Call CLSIDFromProgIDEx first then fall back on CLSIDFromProgID if
        // CLSIDFromProgIDEx doesn't exist.
        try {
            CLSIDFromProgIDEx(progID, out clsid);
        }
        //            catch
        catch (Exception) {
            CLSIDFromProgID(progID, out clsid);
        }

        GetActiveObject(ref clsid, nint.Zero, out obj);
        return obj;
    }

    //[DllImport(Microsoft.Win32.Win32Native.OLE32, PreserveSig = false)]
    [DllImport(OLE32, PreserveSig = false)]
    [ResourceExposure(ResourceScope.None)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]  // auto-generated
    private static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

    //[DllImport(Microsoft.Win32.Win32Native.OLE32, PreserveSig = false)]
    [DllImport(OLE32, PreserveSig = false)]
    [ResourceExposure(ResourceScope.None)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]  // auto-generated
    private static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

    //[DllImport(Microsoft.Win32.Win32Native.OLEAUT32, PreserveSig = false)]
    [DllImport(OLEAUT32, PreserveSig = false)]
    [ResourceExposure(ResourceScope.None)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]  // auto-generated
    private static extern void GetActiveObject(ref Guid rclsid, nint reserved, [MarshalAs(UnmanagedType.Interface)] out object ppunk);

}
