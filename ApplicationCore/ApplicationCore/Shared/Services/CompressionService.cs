﻿using System.IO.Compression;

namespace ApplicationCore.Shared.Services;

internal class CompressionService {

    private static void CopyTo(Stream src, Stream dest) {
        byte[] bytes = new byte[4096];

        int cnt;

        while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0) {
            dest.Write(bytes, 0, cnt);
        }
    }

    public static byte[] Compress(byte[] bytes) {

        /*
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using var gs = new GZipStream(mso, CompressionMode.Compress);
        
        CopyTo(msi, gs);
    
        return mso.ToArray();
        */

        using (var msi = new MemoryStream(bytes))
        using (var mso = new MemoryStream()) {
            using (var gs = new GZipStream(mso, CompressionMode.Compress)) {
                CopyTo(msi, gs);
            }

            return mso.ToArray();
        }

    }

    public static byte[] Uncompress(byte[] bytes) {

        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using var gs = new GZipStream(msi, CompressionMode.Decompress);

        CopyTo(gs, mso);

        return mso.ToArray();

    }

}
