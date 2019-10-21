/*
 * Because Windows Store does not support stream.Close(), and .NET
 * does not support stream.Dispose() as it's a protected method,
 * we use this to manage cross-compatibility when disposing streams.
 * ie. Windows Store uses stream.Dispose(), .NET uses stream.Close().
 */
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class ES2Dispose
{
    public static void Dispose(System.IO.BinaryReader reader)
    {
#if NETFX_CORE
        reader.Dispose();
#else
        reader.Close();
#endif
    }

    public static void Dispose(System.IO.BinaryWriter writer)
    {
#if NETFX_CORE
        writer.Dispose();
#else
        writer.Close();
#endif
    }
}
