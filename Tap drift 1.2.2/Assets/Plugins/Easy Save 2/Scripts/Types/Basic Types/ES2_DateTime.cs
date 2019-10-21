using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ES2_DateTime : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		writer.Write(((System.DateTime)obj).Ticks);
	}
	
	public override object Read(ES2Reader reader)
	{
		return new System.DateTime(reader.Read<long>());
	}
	
	/* ! Don't modify anything below this line ! */
	public ES2_DateTime():base(typeof(System.DateTime)){}
}