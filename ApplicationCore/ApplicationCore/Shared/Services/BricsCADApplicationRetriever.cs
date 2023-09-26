using BricscadApp;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
namespace ApplicationCore.Shared.Services;

public class BricsCADApplicationRetriever {

    internal const string strProgId = "BricscadApp.AcadApplication.20.0";
    internal const string OLEAUT32 = "oleaut32.dll";
    internal const string OLE32 = "ole32.dll";

    public static AcadApplication? GetAcadApplication(bool createIfNotOpen = false) {

        try {
            return GetActiveObject(strProgId) as AcadApplication;
        } catch {

            if (!createIfNotOpen) {
                throw;
            } 

            if (Type.GetTypeFromProgID(strProgId) is not Type type) {
                throw;
            }

            var instance = Activator.CreateInstance(type) as AcadApplication;
            
            if (instance is null) {
                throw;
            }

            instance.Visible = true;
            return instance;

        }
    }


    [System.Security.SecurityCritical]  // auto-generated_required
    private static object? GetActiveObject(string progID) {
        object? obj = null;
        Guid clsid;

        // Call CLSIDFromProgIDEx first then fall back on CLSIDFromProgID if
        // CLSIDFromProgIDEx doesn't exist.
        try {
            CLSIDFromProgIDEx(progID, out clsid);
        } catch (Exception) {
            CLSIDFromProgID(progID, out clsid);
        }

        GetActiveObject(ref clsid, IntPtr.Zero, out obj);
        return obj;
    }

    //[DllImport(Microsoft.Win32.Win32Native.OLE32, PreserveSig = false)]
    [DllImport(OLE32, PreserveSig = false)]
    [ResourceExposure(ResourceScope.None)]
    [SuppressUnmanagedCodeSecurity]
    [System.Security.SecurityCritical]  // auto-generated
    private static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] String progId, out Guid clsid);

    //[DllImport(Microsoft.Win32.Win32Native.OLE32, PreserveSig = false)]
    [DllImport(OLE32, PreserveSig = false)]
    [ResourceExposure(ResourceScope.None)]
    [SuppressUnmanagedCodeSecurity]
    [System.Security.SecurityCritical]  // auto-generated
    private static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] String progId, out Guid clsid);

    //[DllImport(Microsoft.Win32.Win32Native.OLEAUT32, PreserveSig = false)]
    [DllImport(OLEAUT32, PreserveSig = false)]
    [ResourceExposure(ResourceScope.None)]
    [SuppressUnmanagedCodeSecurity]
    [System.Security.SecurityCritical]  // auto-generated
    private static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out Object ppunk);



}

