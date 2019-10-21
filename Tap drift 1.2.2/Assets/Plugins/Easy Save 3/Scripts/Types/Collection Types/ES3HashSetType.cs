using System;
using System.Collections;
using System.Collections.Generic;
using ES3Internal;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	public class ES3HashSetType : ES3CollectionType
	{
		public ES3HashSetType(Type type) : base(type){}

		public override void Write(object obj, ES3Writer writer, ES3.ReferenceMode memberReferenceMode)
		{
			if(obj == null){ writer.WriteNull(); return; };

			var list = (IEnumerable)obj;

			if(elementType == null)
				throw new ArgumentNullException("ES3Type argument cannot be null.");

			int count = 0;
			foreach (var item in list)
				count++;

			writer.StartWriteCollection(count);

			int i = 0;
			foreach(object item in list)
			{
				writer.StartWriteCollectionItem(i);
				writer.Write(item, elementType, memberReferenceMode);
				writer.EndWriteCollectionItem(i);
				i++;
			}

			writer.EndWriteCollection();
		}

		public override object Read<T>(ES3Reader reader)
		{
			var list = new HashSet<T>();
			if(!ReadICollection<T>(reader, list, elementType))
				return null;
			return list;
		}

		public override void ReadInto<T>(ES3Reader reader, object obj)
		{
			ReadICollectionInto(reader, (HashSet<T>)obj, elementType);
		}
			
	}
}