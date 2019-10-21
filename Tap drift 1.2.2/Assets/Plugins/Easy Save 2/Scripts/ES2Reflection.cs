#if NETFX_CORE

using System.Reflection;
using System;

public static class ES2Reflection
{
public static bool SetValue(object obj, string name, object value, bool isProperty=false)
{
Type type = obj.GetType();

/*var accessor = TypeAccessor.Create(type);
if(accessor == null)
return false;
accessor[obj, name] = value;*/

if(isProperty)
{
PropertyInfo property = GetPropertyPortable(name, type);
if(property == null)
return false;
property.SetValue(obj, value, null);
}
else
{
FieldInfo field = GetFieldPortable(name, type);
if(field == null)
return false;
field.SetValue(obj, value);
}
return true;
}

public static object GetValue(object obj, string name, bool isProperty=false)
{
Type type = obj.GetType();

/*var accessor = ObjectAccessor.Create(obj);
if(accessor == null)
return null;
return accessor[name];*/

if(isProperty)
{
PropertyInfo property = GetPropertyPortable(name, type);
if(property == null)
return null;
return GetGetMethodPortable(property).Invoke(obj, null);
}
else
{
FieldInfo field = GetFieldPortable(name, type);
if(field == null)
return null;
return field.GetValue(obj);
}
}

public static bool IsAssignableFrom(Type a, Type b)
{
return a.GetTypeInfo().IsAssignableFrom(b.GetTypeInfo());
}

public static bool IsGenericType(Type type)
{
return type.GetTypeInfo().IsGenericType;
}

public static Type[] GetGenericArguments(Type type)
{
return type.GenericTypeArguments;
}

/*
* 	Creates an instance of a generic class.
* 	Type must be an empty generic type (i.e. typeof(List<>) ).
*/
public static object CreateGenericInstance(Type type, Type genericType)
{
return System.Activator.CreateInstance(type.MakeGenericType(genericType));
}

public static object CreateGenericInstance(Type type, Type genericType, Type genericType2)
{
return System.Activator.CreateInstance(type.MakeGenericType(new Type[] { genericType, genericType2 }));
}

/*
*   Reflection methods specific to portable class libraries.
*/

private static PropertyInfo GetPropertyPortable(string name, Type type)
{
return type.GetTypeInfo().GetDeclaredProperty(name);
}

private static FieldInfo GetFieldPortable(string name, Type type)
{
return type.GetTypeInfo().GetDeclaredField(name);
}

private static MethodInfo GetGetMethodPortable(PropertyInfo info)
{
return info.GetMethod;
}
}
#else
using System.Reflection;
using System;
using System.Collections;

public static class ES2Reflection
{
	public static bool SetValue(object obj, string name, object value, bool isProperty=false)
	{
		Type type = obj.GetType();

		/*var accessor = TypeAccessor.Create(type);
		if(accessor == null)
			return false;
		accessor[obj, name] = value;*/

		if(isProperty)
		{
			PropertyInfo property = type.GetProperty(name);
			if(property == null)
				return false;
			property.SetValue(obj, value, null);
		}
		else
		{
			FieldInfo field = type.GetField(name);
			if(field == null)
				return false;
			field.SetValue(obj, value);
		}
		return true;
	}

	public static object GetValue(object obj, string name, bool isProperty=false)
	{
		Type type = obj.GetType();

		/*var accessor = ObjectAccessor.Create(obj);
		if(accessor == null)
			return null;
		return accessor[name];*/

		if(isProperty)
		{
			PropertyInfo property = type.GetProperty(name);
			if(property == null)
				return null;
			return property.GetGetMethod().Invoke(obj, null);
		}
		else
		{
			FieldInfo field = type.GetField(name);
			if(field == null)
				return null;
			return field.GetValue(obj);
		}
	}

	public static bool IsAssignableFrom(Type a, Type b)
	{
		return a.IsAssignableFrom(b);
	}

    	public static bool IsGenericType(Type type)
    	{
#if NETFX_CORE
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
    	}

    	public static Type[] GetGenericArguments(Type type)
    	{
        	return type.GetGenericArguments();
    	}

	/*
	 * 	Creates an instance of a generic class.
	 * 	Type must be an empty generic type (i.e. typeof(List<>) ).
	 */
	public static object CreateGenericInstance(Type type, Type genericType)
	{
		return System.Activator.CreateInstance( type.MakeGenericType(genericType) );
	}

	public static object CreateGenericInstance(Type type, Type genericType, Type genericType2)
	{
		return System.Activator.CreateInstance( type.MakeGenericType(new Type[]{genericType, genericType2}) );
	}

	/* Determines if this is a collection type */
	public static bool IsCollectionType(Type type)
	{
		if(!typeof(IEnumerable).IsAssignableFrom(type))
			return false;

		if(type.IsArray)
			return true;

		// If it's not an array and doesn't have generic arguments, we don't consider it to be a collection.
		if(type.GetGenericArguments().Length == 0)
			return false;

		return true;
	}
}

#endif