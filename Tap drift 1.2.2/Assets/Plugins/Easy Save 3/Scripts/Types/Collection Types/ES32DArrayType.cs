using System;
using System.Collections;
using System.Collections.Generic;
using ES3Internal;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	public class ES32DArrayType : ES3CollectionType
	{
		public ES32DArrayType(Type type) : base(type){}

		public override void Write(object obj, ES3Writer writer, ES3.ReferenceMode unityObjectType)
		{
			var array = (System.Array)obj;

			if(elementType == null)
				throw new ArgumentNullException("ES3Type argument cannot be null.");

			writer.StartWriteCollection(array.Length);

			for(int i=0; i < array.GetLength(0); i++)
			{
				writer.StartWriteCollectionItem(i);
				writer.StartWriteCollection(array.Length);
				for(int j=0; j < array.GetLength(1); j++)
				{
					writer.StartWriteCollectionItem(j);
					writer.Write(array.GetValue(i,j), elementType, unityObjectType);
					writer.EndWriteCollectionItem(j);
				}
				writer.EndWriteCollection();
				writer.EndWriteCollectionItem(i);
			}

			writer.EndWriteCollection();
		}

		public override object Read<T>(ES3Reader reader)
		{
			if(reader.StartReadCollection())
				return null;

			// Create a List to store the items as a 1D array, which we can work out the positions of by calculating the lengths of the two dimensions.
			var items = new List<T>();
			int length1 = 0;

			// Iterate through each character until we reach the end of the array.
			while(true)
			{
				if(!reader.StartReadCollectionItem())
					break;

				ReadICollection<T>(reader, items, elementType);
				length1++;

				if(reader.EndReadCollectionItem())
					break;
			}

			int length2 = items.Count / length1;

			var array = new T[length1,length2];

			for(int i=0; i<length1; i++)
				for(int j=0; j<length2; j++)
					array[i,j] = items[ (i * length2) + j ];

			return array;
		}

		public override object Read(ES3Reader reader)
		{
			if(reader.StartReadCollection())
				return null;

			// Create a List to store the items as a 1D array, which we can work out the positions of by calculating the lengths of the two dimensions.
			var items = new List<object>();
			int length1 = 0;

			// Iterate through each character until we reach the end of the array.
			while(true)
			{
				if(!reader.StartReadCollectionItem())
					break;

				ReadICollection<object>(reader, items, elementType);
				length1++;

				if(reader.EndReadCollectionItem())
					break;
			}

			int length2 = items.Count / length1;

			var array = ES3Reflection.ArrayCreateInstance(elementType.type, new int[]{length1, length2});

			for(int i=0; i<length1; i++)
				for(int j=0; j<length2; j++)
					array.SetValue(items[ (i * length2) + j ], i, j);

			return array;
		}
	}
}