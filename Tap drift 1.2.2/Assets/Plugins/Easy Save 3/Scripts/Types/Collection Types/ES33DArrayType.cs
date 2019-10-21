using System;
using System.Collections;
using System.Collections.Generic;
using ES3Internal;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	public class ES33DArrayType : ES3CollectionType
	{
		public ES33DArrayType(Type type) : base(type){}

		public override void Write(object obj, ES3Writer writer, ES3.ReferenceMode memberReferenceMode)
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
					writer.StartWriteCollection(array.Length);

					for(int k=0; k < array.GetLength(2); k++)
					{
						writer.StartWriteCollectionItem(k);
						writer.Write(array.GetValue(i,j,k), elementType, memberReferenceMode);
						writer.EndWriteCollectionItem(k);
					}
					writer.EndWriteCollection();
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
			int length2 = 0;

			// Iterate through each sub-array
			while(true)
			{
				if(!reader.StartReadCollectionItem())
					break;
				reader.StartReadCollection();

				length1++;

				while(true)
				{
					if(!reader.StartReadCollectionItem())
						break;

					ReadICollection<T>(reader, items, elementType);
					length2++;

					if(reader.EndReadCollectionItem())
						break;
				}

				reader.EndReadCollection();
				if(reader.EndReadCollectionItem())
					break;
			}

			reader.EndReadCollection();

			length2 = length2/length1;
			int length3 = items.Count / length2 / length1;

			var array = new T[length1,length2,length3];

			for(int i=0; i<length1; i++)
				for(int j=0; j<length2; j++)
					for(int k=0; k<length3; k++)
						array[i,j,k] = items[i * (length2*length3) + (j * length3) + k];

			return array;
		}

		public override object Read(ES3Reader reader)
		{
			if(reader.StartReadCollection())
				return null;

			// Create a List to store the items as a 1D array, which we can work out the positions of by calculating the lengths of the two dimensions.
			var items = new List<object>();
			int length1 = 0;
			int length2 = 0;

			// Iterate through each sub-array
			while(true)
			{
				if(!reader.StartReadCollectionItem())
					break;
				reader.StartReadCollection();

				length1++;

				while(true)
				{
					if(!reader.StartReadCollectionItem())
						break;

					ReadICollection<object>(reader, items, elementType);
					length2++;

					if(reader.EndReadCollectionItem())
						break;
				}

				reader.EndReadCollection();
				if(reader.EndReadCollectionItem())
					break;
			}

			reader.EndReadCollection();

			length2 = length2/length1;
			int length3 = items.Count / length2 / length1;

			var array = ES3Reflection.ArrayCreateInstance(elementType.type, new int[]{length1,length2,length3});

			for(int i=0; i<length1; i++)
				for(int j=0; j<length2; j++)
					for(int k=0; k<length3; k++)
						array.SetValue(items[i * (length2*length3) + (j * length3) + k], i, j, k);

			return array;
		}
	}
}