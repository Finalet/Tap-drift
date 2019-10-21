using UnityEngine;

public class ES3AutoSave : MonoBehaviour
{
	public bool saveChildren = false;
	private bool isQuitting = false;

	public void Awake()
	{
		ES3AutoSaveMgr.AddAutoSave (this);
	}

	public void OnApplicationQuit()
	{
		isQuitting = true;
	}

	public void OnDestroy()
	{
		// If this is being destroyed, but not because the application is quitting,
		// remove the AutoSave from the manager.
		if(!isQuitting)
			ES3AutoSaveMgr.RemoveAutoSave (this);
	}
}