public struct ES2Header
{
	public ES2Keys.Key collectionType;
	public int keyType;
	public int valueType;
	public ES2Settings settings;

	public ES2Header(ES2Keys.Key collectionType, int keyType, int valueType, ES2Settings settings)
	{
		this.collectionType = collectionType;
		this.keyType = keyType;
		this.valueType = valueType;
		this.settings = settings;
	}
}

