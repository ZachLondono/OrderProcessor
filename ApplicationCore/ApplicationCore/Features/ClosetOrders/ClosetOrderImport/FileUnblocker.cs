using System.Runtime.InteropServices;

namespace ApplicationCore.Features.ClosetOrders.ClosetOrderImport;

public static class FileUnblocker {

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteFile(string name);

    public static bool Unblock(string fileName) {
        return DeleteFile(fileName + ":Zone.Identifier");
    }

}
