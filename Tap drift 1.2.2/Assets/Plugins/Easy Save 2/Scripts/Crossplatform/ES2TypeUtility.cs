using System.Reflection;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class ES2TypeUtility
{
	public static bool IsEnum(System.Type type)
	{
#if NETFX_CORE
        return type.GetTypeInfo().IsEnum;
#else
		return type.IsEnum;
#endif
    }
}

