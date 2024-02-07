using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Domain.Services;

public class UserDataProtection {

    // TODO: Maybe use a different source of entropy
    private static readonly byte[] _entropy = Encoding.ASCII.GetBytes("ApplicationCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" /*Assembly.GetExecutingAssembly().FullName*/);

    public static string Protect(string str) {

        if (string.IsNullOrEmpty(str)) return str;

        byte[] data = Encoding.ASCII.GetBytes(str);

        return Convert.ToBase64String(ProtectedData.Protect(data, _entropy, DataProtectionScope.CurrentUser));

    }

    public static string Unprotect(string str) {

        if (string.IsNullOrEmpty(str)) return str;

        byte[] protectedData = Convert.FromBase64String(str);

        return Encoding.ASCII.GetString(ProtectedData.Unprotect(protectedData, _entropy, DataProtectionScope.CurrentUser));

    }

}
