using UnityEngine;
using System.Collections;

public class ES2Auto : MonoBehaviour 
{
	public enum SaveEvent{OnDisable, OnInterval};
	public enum LoadEvent{OnAwake, OnStart, OnEnable};
	
	// When we want to save our data.
	[SerializeField]
	public bool saveOnDisable = true;
	[SerializeField]
	public bool saveOnInterval = true;
	[SerializeField]
	public float saveInterval = 20f;
	// When we want to load our data
	[SerializeField]
	public bool loadOnEnable = true;
	[SerializeField]
	public bool loadOnAwake = false;
	[SerializeField]
	public bool loadOnStart = false;
	[SerializeField]
	public bool isPlayMaker = false;
	
	// A tag to uniquely identify this Game Object.
	// Uses the object's name by default.
	[SerializeField]
	public string uniqueTag = "";
	
	[SerializeField]
	public bool savePosition = false;
	[SerializeField]
	public bool saveRotation = false;
	[SerializeField]
	public bool saveScale = false;
	[SerializeField]
	public bool saveMesh = false;
	[SerializeField]
	public bool saveSphereCollider = false;
	[SerializeField]
	public bool saveBoxCollider = false;
	[SerializeField]
	public bool saveCapsuleCollider = false;
	[SerializeField]
	public bool saveMeshCollider = false;
	

	[SerializeField]
	public string saveFile = "defaultFile.txt";
	[SerializeField]
	public ES2Settings.SaveLocation saveLocation = ES2Settings.SaveLocation.File;
	[SerializeField]
	public bool encrypt = false;
	[SerializeField]
	public string encryptionPassword = ES2GlobalSettings.defaultEncryptionPassword;
	[SerializeField]
	public string webUsername = ES2GlobalSettings.defaultWebUsername;
	[SerializeField]
	public string webPassword = ES2GlobalSettings.defaultWebPassword;
	
	private bool isQuitting = false;
	public ES2Settings settings;
	
	/*
		SAVE & LOAD METHODS
	*/
	private void SavePosition()
	{ 
		ES2.Save(transform.position, GetFullFilename("position"), settings);
	}
	
	private void LoadPosition()
	{
		if(!ES2.Exists(GetFullFilename("position"), settings))
			return;
		transform.position = ES2.Load<Vector3>(GetFullFilename("position"), settings);
	}
	
	private void SaveRotation()
	{
		ES2.Save(transform.rotation, GetFullFilename("rotation"), settings);
	}
	
	private void LoadRotation()
	{
		if(!ES2.Exists(GetFullFilename("rotation"), settings))
			return;
		transform.rotation = ES2.Load<Quaternion>(GetFullFilename("rotation"), settings);
	}
	
	private void SaveScale()
	{
		ES2.Save(transform.localScale, GetFullFilename("scale"), settings);
	}
	
	private void LoadScale()
	{
		if(!ES2.Exists(GetFullFilename("scale"), settings))
			return;
		transform.localScale = ES2.Load<Vector3>(GetFullFilename("scale"), settings);
	}
	
	private void SaveMesh()
	{
		MeshFilter filter = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
		if(filter == null || filter.mesh == null)
			return;
		ES2.Save(filter.mesh, GetFullFilename("mesh"), settings);
	}
	
	private void LoadMesh()
	{
		if(!ES2.Exists(GetFullFilename("mesh"), settings))
			return;
		MeshFilter filter = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
		if(filter == null)
			filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		if(filter.mesh != null)
			Destroy(filter.mesh);
		filter.mesh = ES2.Load<Mesh>(GetFullFilename("mesh"), settings);
	}
	
	private void SaveSphereCollider()
	{
		SphereCollider s = gameObject.GetComponent(typeof(SphereCollider)) as SphereCollider;
		if(s == null)
			return;
		ES2.Save(s, GetFullFilename("spherecollider"), settings);
	}
	
	private void LoadSphereCollider()
	{
		if(!ES2.Exists(GetFullFilename("spherecollider"), settings))
			return;
		SphereCollider s = gameObject.GetComponent(typeof(SphereCollider)) as SphereCollider;
		if(s == null)
			s = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
		 ES2.Load<SphereCollider>(GetFullFilename("spherecollider"), s, settings);
	}
	
	private void SaveCapsuleCollider()
	{
		CapsuleCollider s = gameObject.GetComponent(typeof(CapsuleCollider)) as CapsuleCollider;
		if(s == null)
			return;
		ES2.Save(s, GetFullFilename("capsulecollider"), settings);
	}
	
	private void LoadCapsuleCollider()
	{
		if(!ES2.Exists(GetFullFilename("capsulecollider"), settings))
			return;
		CapsuleCollider s = gameObject.GetComponent(typeof(CapsuleCollider)) as CapsuleCollider;
		if(s == null)
			s = gameObject.AddComponent(typeof(CapsuleCollider)) as CapsuleCollider;
		 ES2.Load<CapsuleCollider>(GetFullFilename("capsulecollider"), s, settings);
	}
	
	private void SaveBoxCollider()
	{
		BoxCollider s = gameObject.GetComponent(typeof(BoxCollider)) as BoxCollider;
		if(s == null)
			return;
		ES2.Save(s, GetFullFilename("boxcollider"), settings);
	}
	
	private void LoadBoxCollider()
	{
		if(!ES2.Exists(GetFullFilename("boxcollider"), settings))
			return;
		BoxCollider s = gameObject.GetComponent(typeof(BoxCollider)) as BoxCollider;
		if(s == null)
			s = gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;
		 ES2.Load<BoxCollider>(GetFullFilename("boxcollider"), s, settings);
	}
	
	private void SaveMeshCollider()
	{
		MeshCollider s = gameObject.GetComponent(typeof(MeshCollider)) as MeshCollider;
		if(s == null)
			return;
		ES2.Save(s, GetFullFilename("meshcollider"), settings);
	}
	
	private void LoadMeshCollider()
	{
		if(!ES2.Exists(GetFullFilename("meshcollider"), settings))
			return;
		MeshCollider s = gameObject.GetComponent(typeof(MeshCollider)) as MeshCollider;
		if(s == null)
			s = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
		 ES2.Load<MeshCollider>(GetFullFilename("meshcollider"), s, settings);
	}
		
	/*
		SAVE EVENTS
	*/
	
	public void Save()
	{
		if(savePosition)
			SavePosition();
		if(saveRotation)
			SaveRotation();
		if(saveScale)
			SaveScale();
		if(saveMesh)
			SaveMesh();
		if(saveSphereCollider)
			SaveSphereCollider();
		if(saveBoxCollider)
			SaveBoxCollider();
		if(saveCapsuleCollider)
			SaveCapsuleCollider();
		if(saveMeshCollider)
			SaveMeshCollider();
	}
	
	public void Load()
	{
		if(savePosition)
			LoadPosition();
		if(saveRotation)
			LoadRotation();
		if(saveScale)
			LoadScale();
		if(saveMesh)
			LoadMesh();
		if(saveSphereCollider)
			LoadSphereCollider();
		if(saveBoxCollider)
			LoadBoxCollider();
		if(saveCapsuleCollider)
			LoadCapsuleCollider();
		if(saveMeshCollider)
			LoadMeshCollider();
	}
	
	/*
		SAVE EVENTS
	*/
	
	private IEnumerator SaveRoutine()
	{
		while(!isQuitting)
		{
			yield return new WaitForSeconds(saveInterval);
			Save();
		}
	}
	
	private void OnApplicationQuit()
	{
		if(isPlayMaker)
			return;
		isQuitting = true;
		Save();
	}
		
	private void OnDisable()
	{
		if(isPlayMaker)
			return;
		if(!saveOnDisable || isQuitting)
			return;
		Save();
	}
	
	/*
		LOAD EVENTS
	*/
	
	private void Awake()
	{
		InitializeSettings();
		if(isPlayMaker)
			return;
		if(saveOnInterval)
			StartCoroutine(SaveRoutine());
		if(!loadOnAwake)
			return;
		
		Load();
	}
	
	private void Start()
	{
		if(isPlayMaker)
			return;
		if(!loadOnStart)
			return;
		Load();
	}
	
	private void OnEnable()
	{
		if(isPlayMaker)
			return;
		if(!loadOnEnable)
			return;
		Load();
	}
	
	/*
		MISCELLANEOUS METHODS
	*/
	private string GetFullFilename(string tagExtension)
	{
		if(uniqueTag == "")
			uniqueTag = gameObject.name;
		return saveFile+"?tag="+uniqueTag+tagExtension;
	}
	
	private void InitializeSettings()
	{
		settings = new ES2Settings(saveFile);

		// We can't initialize a field using a function of getter/setter method on
		// some platforms so we must initialize in Awake or Start.
		if(saveLocation != ES2Settings.SaveLocation.File)
			settings.saveLocation = saveLocation;
		else
			settings.saveLocation = ES2GlobalSettings.defaultSaveLocation;

		settings.encrypt = encrypt;
		settings.encryptionPassword = encryptionPassword;
		settings.webUsername = webUsername;
		settings.webPassword = webPassword;
	}
}