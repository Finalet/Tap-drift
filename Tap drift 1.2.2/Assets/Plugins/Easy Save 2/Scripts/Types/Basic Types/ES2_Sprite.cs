using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Sprite : ES2Type
{
	public ES2_Sprite() : base(typeof(Sprite))
	{
		key = (byte)31;
	}
	
	public override void Write(object data, ES2Writer writer)
	{
		Sprite sprite = (Sprite)data;
		writer.Write(sprite.texture);
		writer.Write(sprite.rect);
		
		// Calculate and write pivot point
		float pivotX = - sprite.bounds.center.x / sprite.bounds.extents.x / 2 + 0.5f;
		float pivotY = - sprite.bounds.center.y / sprite.bounds.extents.y / 2 + 0.5f;
		writer.Write(new Vector2(pivotX, pivotY));
		
		// Calculate pixelsToUnits
		writer.Write(sprite.textureRect.width / sprite.bounds.size.x);
		writer.Write(sprite.name);
	}
	
	public override object Read(ES2Reader reader)
	{
		Sprite sprite = Sprite.Create(reader.Read<Texture2D>(), reader.Read<Rect>(), reader.Read<Vector2>(), reader.Read<float>());
		sprite.name = reader.Read<string>();
		return sprite;
	}
}