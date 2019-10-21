#if PLAYMAKER_1_8_OR_NEWER
using System;
using UnityEngine;
using System.Collections.Generic;

namespace HutongGames.PlayMaker.Actions
{
	/* BASE CLASSES */

	#region Settings_BaseClasses

	public class ES2SettingsAction : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The unique tag for this data. For example, the object's name if no other objects use the same name.")]
		public FsmString tag = "";
		[RequiredField]
		[Tooltip("The name or absolute path of the file where our data will be stored.")]
		public FsmString filename = "defaultFile.txt";
		[Tooltip("Whether to encrypt the data or not. If set to true, you must set an encryption password.")]
		public FsmBool encrypt = ES2GlobalSettings.defaultEncrypt;
		[Tooltip("The password to use for encryption if it is enabled.")]
		public FsmString encryptionPassword = ES2GlobalSettings.defaultEncryptionPassword;
		[Tooltip("This event is called if an error occurs.")]
		public FsmEvent ifError = null;

		public override void Reset() 
		{
			tag.Value = "";
			filename.Value = "defaultFile.txt";
			encrypt.Value = ES2GlobalSettings.defaultEncrypt;
			encryptionPassword.Value = ES2GlobalSettings.defaultEncryptionPassword;
		}

		public virtual ES2Settings GetSettings(ES2Settings settings)
		{
			settings.encrypt = encrypt.Value;
			settings.encryptionPassword = encryptionPassword.Value;
			settings.tag = tag.Value;

			return settings;
		}
	}

	public class ES2WebSettingsAction : ES2SettingsAction
	{
		[Tooltip("The username that you have specified in your ES2.php file.")]
		public FsmString webUsername = ES2GlobalSettings.defaultWebUsername;
		[RequiredField]
		[Tooltip("The password that you have specified in your ES2.php file.")]
		public FsmString webPassword = ES2GlobalSettings.defaultWebPassword;

		public override void Reset()
		{
			webUsername.Value = ES2GlobalSettings.defaultWebUsername;
			webPassword.Value = ES2GlobalSettings.defaultWebPassword;
			base.Reset();
		}

		public override ES2Settings GetSettings(ES2Settings settings)
		{
			settings.webUsername = webUsername.Value;
			settings.webPassword = webPassword.Value;
			return base.GetSettings(settings);
		}
	}
	#endregion

	#region Save_BaseClasses
	public class ES2SaveAction : ES2SettingsAction
	{	
		public override void OnEnter()
		{
			Log("Saved to "+filename.Value);
			Finish();
		}
	}

	public class ES2SaveObjectAction<T> : ES2SaveAction where T : UnityEngine.Object
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmObject saveValue;

		public override void Reset()
		{
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save((T)saveValue.Value, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	public class ES2SaveComponentAction<T> : ES2SaveAction where T : Component
	{
		[RequiredField]
		[Tooltip("The GameObject containing the Component we want to save.")]
		public FsmOwnerDefault saveValue;

		public override void OnEnter()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(saveValue);
			T component;

			if(go == null)
			{
				LogError("Could not save Component. No GameObject has been specified.");
				Finish ();
				return;
			}

			if((component = go.GetComponent<T>()) == null)
			{
				LogError("Could not save Component because this GameObject does not have this Component.");
				Finish ();
				return;
			}

			try
			{
				ES2.Save(component, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}


	#endregion

	#region Load_BaseClasses
	public class ES2LoadComponentAction<T> : ES2LoadAction where T : Component
	{
		[RequiredField]
		[Tooltip("The GameObject we want to assign our loaded Component to.")]
		[UIHint(UIHint.Variable)]
		public FsmOwnerDefault loadValue;

		public override void OnEnter()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(loadValue);
			T component;

			if(go == null)
			{
				LogError("Could not load Component. No GameObject has been specified.");
				Finish ();
				return;
			}

			if((component = go.GetComponent<T>()) == null)
				component = go.AddComponent<T>();
			try
			{
				ES2.Load<T>(filename.Value, component, GetSettings(new ES2Settings()));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	public class ES2LoadObjectAction<T> : ES2LoadAction where T : UnityEngine.Object
	{
		[RequiredField]
		[Tooltip("Our loaded data, or the variable we want to load our data into.")]
		[UIHint(UIHint.Variable)]
		public FsmObject loadValue;


		public override void Reset()
		{
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Value = ES2.Load<T>(filename.Value, GetSettings(new ES2Settings())) as UnityEngine.Object;
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	public class ES2LoadAction : ES2SettingsAction
	{	
		public override void OnEnter()
		{
			Log("Loaded from "+filename.Value);
			Finish();
		}
	}

	#endregion


	#region ES2Web_BaseClasses

	#if !DISABLE_WWW

	public class ES2WebAction : ES2WebSettingsAction
	{
		[Tooltip("The URL to our ES2.PHP file. See http://moodkie.com/easysave/documentation/guides/saving-and-loading-from-web/ for more information on setting up ES2Web")]
		public FsmString url = "http://www.mysite.com/ES2.php";
		[Tooltip("The Event to send if the ES2Web operation succeeded.")]
		public FsmEvent isDone;
		[Tooltip("Where any errors thrown will be stored. Set this to a variable, or leave it blank.")]
		public FsmString errorMessage = "";
		[Tooltip("Where any error codes thrown will be stored. Set this to a variable, or leave it blank.")]
		public FsmString errorCode = "";

		protected ES2Web web = null;

		public void CreateES2Web()
		{
			ES2Settings settings = new ES2Settings();
			settings.webFilename = filename.Value;
			web = new ES2Web(url.Value, GetSettings(settings));
		}

		public override void Reset ()
		{
			url = "http://www.mysite.com/ES2.php";
			errorMessage = "";
			errorCode = "";
			web = null;
		}

		public override void OnUpdate()
		{
			if(web.isError)
			{
				errorMessage.Value = web.error;
				errorCode.Value = web.errorCode;
				Log("Error occurred when trying to perform ES2Web operation to "+filename.Value);
				Fsm.Event(ifError);
			}
			else if(web.isDone)
			{
				IsDone();
			}
		}

		public virtual void IsDone()
		{
			Log("ES2Web operation to "+filename.Value+" done.");
			Fsm.Event(isDone);
		}
	}

	#endif

	#endregion

	#region Upload_BaseClasses

	#if !DISABLE_WWW

	public class ES2UploadAction : ES2WebAction
	{
		public override void OnEnter()
		{
			Log("Uploading to "+filename.Value);
		}
	}

	public class ES2UploadObjectAction<T> : ES2UploadAction where T : UnityEngine.Object
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmObject saveValue;

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Value));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			} // Ensure that base.OnEnter() is called when done.
		}
	}

	public class ES2UploadComponentAction<T> : ES2UploadAction where T : Component
	{
		[RequiredField]
		[Tooltip("The GameObject containing the Component we want to save.")]
		public FsmOwnerDefault saveValue;

		public override void OnEnter()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(saveValue);
			T component;

			if(go == null)
			{
				LogError("Could not save Component. No GameObject has been specified.");
				Finish ();
				return;
			}

			if((component = go.GetComponent<T>()) == null)
			{
				LogError("Could not save Component because this GameObject does not have this Component.");
				Finish ();
				return;
			}

			CreateES2Web();
			this.Fsm.Owner.StartCoroutine(web.Upload(component));
			base.OnEnter();
		}
	}

	#endif

	#endregion

	#region Download_BaseClasses

	#if !DISABLE_WWW

	public class ES2DownloadAction : ES2WebAction
	{
		[Tooltip("If uploading or downloading data, the upload/download progress will be stored here (a value from 0.0 to 1.0). This assumes that the server you're downloading from supports progress monitoring.")]
		public FsmFloat progress;

		public override void OnEnter()
		{
			CreateES2Web();
			this.Fsm.Owner.StartCoroutine(web.Download());

			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			Log("Downloading from "+filename.Value);
		}

		public override void OnUpdate()
		{
			if(this.web != null)
				progress.Value = this.web.progress;
			base.OnUpdate();
		}
	}

	public class ES2DownloadObjectAction<T> : ES2DownloadAction where T : UnityEngine.Object
	{
		[RequiredField]
		[Tooltip("The FsmObject we want to load our data into.")]
		[UIHint(UIHint.Variable)]
		public FsmObject loadValue;

		public override void IsDone()
		{
			loadValue.Value = web.Load<T>(tag.Value);
			base.IsDone();
		}
	}

	public class ES2DownloadComponentAction<T> : ES2DownloadAction where T : Component
	{
		[RequiredField]
		[Tooltip("The GameObject containing the Component we want to load our data into.")]
		[UIHint(UIHint.Variable)]
		public FsmOwnerDefault loadValue;


		public override void IsDone()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(loadValue);
			T component;

			if(go == null)
			{
				LogError("Could not load Component. No GameObject has been specified to load Component into.");
				Finish ();
				return;
			}

			if((component = go.GetComponent<T>()) == null)
				component = go.AddComponent<T>();

			web.Load<T>(tag.Value, component);
			base.IsDone();
		}
	}

	#endif

	#endregion

	#region GeneralES2Methods
	[ActionCategory("Easy Save 2")]
	[Tooltip("Deletes the data at the specified path.")]
	public class Delete : FsmStateAction
	{
		[Tooltip("The tag that we want to delete (Optional).")]
		public FsmString tag = "";
		[RequiredField]
		[Tooltip("The file we want to delete, or delete the tag from.")]
		public FsmString filename = "defaultFile.txt";
		[Tooltip("This event is called if an error occurs.")]
		public FsmEvent ifError = null;

		public override void Reset()
		{
			tag = "";
			filename = "defaultFile.txt";
		}

		public override void OnEnter()
		{
			ES2Settings settings = new ES2Settings();

			if(tag.Value != "")
				settings.tag = tag.Value;
			try
			{
				ES2.Delete(filename.Value, settings);
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}

			Log("Deleted "+filename);
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	[Tooltip("Renames the data at the specified path.")]
	public class RenameFile : FsmStateAction
	{
		[Tooltip("The path or filename of the original file.")]
		public FsmString filename = "defaultFile.txt";
		[RequiredField]
		[Tooltip("The path or filename we want to rename the file to.")]
		public FsmString newFilename = "newFile.txt";
		[Tooltip("This event is called if an error occurs.")]
		public FsmEvent ifError = null;

		public override void Reset()
		{
			newFilename = "newFile.txt";
			filename = "defaultFile.txt";
		}

		public override void OnEnter()
		{
			ES2Settings settings = new ES2Settings();

			try
			{
				ES2.Rename(filename.Value, newFilename.Value, settings);
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}

			Log("Renamed "+filename);
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	[Tooltip("Deletes the default save folder, or clears PlayerPrefs if using Web Player.")]
	public class DeleteDefaultFolder : FsmStateAction
	{	
		public override void OnEnter()
		{
			ES2.DeleteDefaultFolder();
			Log("Deleted default folder.");
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	[Tooltip("Checks if data exists at the specified path and loads the result into a bool.")]
	public class ExistsIntoBool : FsmStateAction
	{
		[Tooltip("Whether the file exists.")]
		public FsmBool exists = false;
		public FsmEvent ifError = null;
		[Tooltip("The tag that we want to check for (Optional).")]
		public FsmString tag = "";
		[RequiredField]
		[Tooltip("The file we want to check the existence of.")]
		public FsmString filename = "defaultFile.txt";

		public override void Reset()
		{
			exists = false;
			tag = "";
			filename = "defaultFile.txt";
		}

		public override void OnEnter()
		{
			ES2Settings settings = new ES2Settings();

			if(tag.Value != "")
				settings.tag = tag.Value;

			Log("Checked existence of "+filename);

			try
			{
				exists.Value = ES2.Exists(filename.Value, settings);
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	[Tooltip("Checks if data exists at the specified path.")]
	public class Exists : FsmStateAction
	{
		[Tooltip("The Event to send if this it does exist.")]
		public FsmEvent ifExists = null;
		[Tooltip("The event to sent if it doesn't exist.")]
		public FsmEvent ifDoesNotExist = null;
		[Tooltip("This event is called if an error occurs.")]
		public FsmEvent ifError = null;
		[Tooltip("The tag that we want to check for (Optional).")]
		public FsmString tag = "";
		[RequiredField]
		[Tooltip("The file we want to check the existence of.")]
		public FsmString filename = "defaultFile.txt";

		public override void Reset()
		{
			tag = "";
			filename = "defaultFile.txt";
		}

		public override void OnEnter()
		{
			ES2Settings settings = new ES2Settings();

			if(tag.Value != "")
				settings.tag = tag.Value;

			Log("Checked existence of "+filename);

			try
			{
				if(ES2.Exists(filename.Value, settings))
				{
					if(ifExists != null)
						Fsm.Event(ifExists);
				}
				else
				{
					if(ifDoesNotExist != null)
						Fsm.Event(ifDoesNotExist);
				}
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			Finish();
		}
	}

	#if PLAYMAKER_1_8_OR_NEWER

	[ActionCategory("Easy Save 2")]
	public class GetFiles : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The array we want to load our filenames into.")]
		[ArrayEditorAttribute(VariableType.String)]
		public FsmArray filenames;
		public FsmString path = "";
		public FsmString extension = "";

		public override void Reset()
		{
			filenames = null;
			path = "";
			extension = "";
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				if(!string.IsNullOrEmpty(extension.Value))
					filenames.Values = ES2.GetFiles(path.Value, extension.Value);
				else
					filenames.Values = ES2.GetFiles(path.Value);
				filenames.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
				Finish();
			}
			catch(System.Exception e)
			{
				LogError(e.Message);
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class GetFolders : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The array we want to load our folder names into.")]
		[ArrayEditorAttribute(VariableType.String)]
		public FsmArray folders;
		public FsmString path = "";

		public override void Reset()
		{
			folders = null;
			path = "";
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				folders.Values = ES2.GetFolders(path.Value);
				folders.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
				Finish();
			}
			catch(System.Exception e)
			{
				LogError(e.Message);
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class GetTags : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The array we want to load our filenames into.")]
		[ArrayEditorAttribute(VariableType.String)]
		public FsmArray filenames;
		[Tooltip("The path of the file we want to get the tags from.")]
		public FsmString path = "";
		[Tooltip("If specified, it will only include tags which match the regular expression.")]
		public FsmString regex = "";

		public override void Reset()
		{
			filenames = null;
			path = "";
			regex = "";
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				if(string.IsNullOrEmpty(regex.Value))
					filenames.Values = ES2.GetTags(path.Value);
				else
				{
					string[] tags = ES2.GetTags(path.Value);
					var regexTags = new List<string>();
					for(int i=0; i<tags.Length; i++)
						if(System.Text.RegularExpressions.Regex.IsMatch(tags[i], regex.Value))
							regexTags.Add(tags[i]);
					filenames.Values = regexTags.ToArray();
				}
				filenames.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
				Finish();
			}
			catch(System.Exception e)
			{
				LogError(e.Message);
			}
		}
	}
	#endif

	#endregion

	#region SavePlaymakerTypes
	[ActionCategory("Easy Save 2")]
	public class SavePosition : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The GameObject we want to save the position of.")]
		public FsmOwnerDefault saveValue;

		public override void OnEnter()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(saveValue);

			if(go == null)
			{
				LogError("Could not save position. No GameObject has been specified.");
				Finish ();
				return;
			}

			try
			{
				ES2.Save(go.transform.position, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveRotation : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The GameObject we want to save the rotation of.")]
		public FsmOwnerDefault saveValue;

		public override void OnEnter()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(saveValue);

			if(go == null)
			{
				LogError("Could not save rotation. No GameObject has been specified.");
				Finish ();
				return;
			}

			try
			{
				ES2.Save(go.transform.rotation, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveScale : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The GameObject we want to save the scale of.")]
		public FsmOwnerDefault saveValue;

		public override void OnEnter()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(saveValue);

			if(go == null)
			{
				LogError("Could not save scale. No GameObject has been specified.");
				Finish ();
				return;
			}

			try
			{
				ES2.Save(go.transform.localScale, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveAll : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The name or absolute path of the file where our data will be stored. If the file doesn't exist, it will be created.")]
		public FsmString filename = "defaultFile.txt";
		[Tooltip("Whether to encrypt the data or not. If set to true, you must set an encryption password.")]
		public FsmBool encrypt = ES2GlobalSettings.defaultEncrypt;
		[Tooltip("The password to use for encryption if it is enabled.")]
		public FsmString encryptionPassword = ES2GlobalSettings.defaultEncryptionPassword;
		[Tooltip("Save the local variables accessible in this FSM?")]
		public FsmBool saveFsmVariables = true;
		[Tooltip("Save the global variables accessible in all FSMs?")]
		public FsmBool saveGlobalVariables = true;
		[Tooltip("This event is called if an error occurs.")]
		public FsmEvent ifError = null;

		public override void Reset()
		{
			filename.Value = "defaultFile.txt";
			encrypt.Value = ES2GlobalSettings.defaultEncrypt;
			encryptionPassword.Value = ES2GlobalSettings.defaultEncryptionPassword;
		}

		public override void OnEnter()
		{
			try
			{
				ES2Settings settings = new ES2Settings();
				settings.encrypt = encrypt.Value;
				settings.encryptionPassword = encryptionPassword.Value;

				// Get FSMVariables objects required based on whether the user wants to save
				// local variables, global variables or both.
				FsmVariables[] fsmVariables;
				if(saveFsmVariables.Value && saveGlobalVariables.Value)
					fsmVariables = new FsmVariables[]{Fsm.Variables, FsmVariables.GlobalVariables};
				else if(saveFsmVariables.Value && !saveGlobalVariables.Value)
					fsmVariables = new FsmVariables[]{Fsm.Variables};
				else if(!saveFsmVariables.Value && saveGlobalVariables.Value)
					fsmVariables = new FsmVariables[]{FsmVariables.GlobalVariables};
				else
					fsmVariables = new FsmVariables[0];

				foreach(FsmVariables fsmVariable in fsmVariables)
				{
					// Variables are stored in seperate arrays based on their types.
					// Save each item in each array seperately.
					foreach(FsmBool fsmVar in fsmVariable.BoolVariables)
					{
						settings.tag = fsmVar.Name;
						ES2.Save(fsmVar.Value, filename.Value, settings);
					}

					foreach(FsmFloat fsmVar in fsmVariable.FloatVariables)
					{
						settings.tag = fsmVar.Name;
						ES2.Save(fsmVar.Value, filename.Value, settings);
					}

					foreach(FsmInt fsmVar in fsmVariable.IntVariables)
					{
						settings.tag = fsmVar.Name;
						ES2.Save(fsmVar.Value, filename.Value, settings);
					}

					foreach(FsmString fsmVar in fsmVariable.StringVariables)
					{
						if(fsmVar.Value == null)
							continue;
						settings.tag = fsmVar.Name;
						ES2.Save(fsmVar.Value, filename.Value, settings);
					}

					foreach(FsmVector2 fsmVar in fsmVariable.Vector2Variables)
					{
						settings.tag = fsmVar.Name;
						ES2.Save(fsmVar.Value, filename.Value, settings);
					}

					foreach(FsmVector3 fsmVar in fsmVariable.Vector3Variables)
					{
						settings.tag = fsmVar.Name;
						ES2.Save(fsmVar.Value, filename.Value, settings);
					}

					foreach(FsmRect fsmVar in fsmVariable.RectVariables)
					{
						settings.tag = fsmVar.Name;
						ES2.Save(fsmVar.Value, filename.Value, settings);
					}

					foreach(FsmQuaternion fsmVar in fsmVariable.QuaternionVariables)
					{
						settings.tag = fsmVar.Name;
						ES2.Save(fsmVar.Value, filename.Value, settings);
					}

					foreach(FsmColor fsmVar in fsmVariable.ColorVariables)
					{
						settings.tag = fsmVar.Name;
						ES2.Save(fsmVar.Value, filename.Value, settings);
					}

					foreach(FsmMaterial fsmVar in fsmVariable.MaterialVariables)
					{
						if(fsmVar.Value == null)
							continue;
						settings.tag = fsmVar.Name;
						ES2.Save(fsmVar.Value, filename.Value, settings);
					}

					foreach(FsmTexture fsmVar in fsmVariable.TextureVariables)
					{
						if(fsmVar.Value == null)
							continue;
						settings.tag = fsmVar.Name;
						ES2.Save(fsmVar.Value as Texture2D, filename.Value, settings);
					}

					foreach(FsmEnum fsmVar in fsmVariable.EnumVariables)
					{
						if(fsmVar.Value == null)
							continue;
						settings.tag = fsmVar.Name;
						ES2.Save(fsmVar.Value, filename.Value, settings);
					}

					foreach(FsmObject fsmVar in fsmVariable.ObjectVariables)
					{
						if(fsmVar.Value == null)
							continue;
						settings.tag = fsmVar.Name;
						ES2.Save(fsmVar.Value, filename.Value, settings);
					}

					#if PLAYMAKER_1_8_OR_NEWER
					foreach(FsmArray fsmVar in fsmVariable.ArrayVariables)
					{
						if(fsmVar.Values == null)
							continue;
						settings.tag = fsmVar.Name;
						ES2.Save(fsmVar.Values, filename.Value, settings);
					}
					#endif
				}

				Log("Loaded from "+filename.Value);
				Finish();
			}
			catch(Exception e)
			{
				if(ifError != null)
				{
					LogError (e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadAll : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The name or absolute path of the file where our data will be stored.")]
		public FsmString filename = "defaultFile.txt";
		[Tooltip("Whether to encrypt the data or not. If set to true, you must set an encryption password.")]
		public FsmBool encrypt = ES2GlobalSettings.defaultEncrypt;
		[Tooltip("The password to use for encryption if it is enabled.")]
		public FsmString encryptionPassword = ES2GlobalSettings.defaultEncryptionPassword;
		[Tooltip("Load the local variables accessible in this FSM?")]
		public FsmBool loadFsmVariables = true;
		[Tooltip("Load the global variables accessible in all FSMs?")]
		public FsmBool loadGlobalVariables = true;
		[Tooltip("This event is called if an error occurs.")]
		public FsmEvent ifError = null;

		public override void Reset()
		{
			filename.Value = "defaultFile.txt";
			encrypt.Value = ES2GlobalSettings.defaultEncrypt;
			encryptionPassword.Value = ES2GlobalSettings.defaultEncryptionPassword;
		}

		public override void OnEnter()
		{
			try
			{
				ES2Settings settings = new ES2Settings(filename.Value);
				settings.encrypt = encrypt.Value;
				settings.encryptionPassword = encryptionPassword.Value;

				ES2Data es2Data = ES2.LoadAll(filename.Value, settings);

				if(es2Data.loadedData.Count < 1)
				{
					Finish();
					return;
				}

				// Get FSMVariables objects required based on whether the user wants to save
				// local variables, global variables or both.
				FsmVariables[] fsmVariables;
				if(loadFsmVariables.Value && loadGlobalVariables.Value)
					fsmVariables = new FsmVariables[]{Fsm.Variables, FsmVariables.GlobalVariables};
				else if(loadFsmVariables.Value && !loadGlobalVariables.Value)
					fsmVariables = new FsmVariables[]{Fsm.Variables};
				else if(!loadFsmVariables.Value && loadGlobalVariables.Value)
					fsmVariables = new FsmVariables[]{FsmVariables.GlobalVariables};
				else
					fsmVariables = new FsmVariables[0];

				foreach(KeyValuePair<string, object> entry in es2Data.loadedData)
				{
					Type type = entry.Value.GetType();

					// Handle arrays seperately.
					#if PLAYMAKER_1_8_OR_NEWER
					if(type.IsArray)
					{
						foreach(FsmVariables variable in fsmVariables)
						{
							FsmArray thisVar = variable.FindFsmArray(entry.Key);
							if(thisVar != null)
							{
								thisVar.Values = (object[])entry.Value;
								thisVar.SaveChanges();
							}
						}
						continue;
					}
					#endif

					ES2Type es2Type = ES2TypeManager.GetES2Type(entry.Value.GetType());						
					if(es2Type == null)
						continue;
					type = es2Type.type;

					foreach(FsmVariables variable in fsmVariables)
					{
						if(type == typeof(bool))
						{
							FsmBool thisVar = variable.FindFsmBool(entry.Key);
							if(thisVar != null)
								thisVar.Value = es2Data.Load<bool>(entry.Key);
						}
						else if(type == typeof(float))
						{
							FsmFloat thisVar = variable.FindFsmFloat(entry.Key);
							if(thisVar != null)
								thisVar.Value = es2Data.Load<float>(entry.Key);
						}
						else if(type == typeof(int))
						{
							FsmInt thisVar = variable.FindFsmInt(entry.Key);
							if(thisVar != null)
							{
								thisVar.Value = es2Data.Load<int>(entry.Key);
								continue;
							}

							// Check incase it's an enum.
							FsmEnum thisEnumVar = variable.FindFsmEnum(entry.Key);
							if(thisEnumVar != null)
								thisEnumVar.Value = (System.Enum)Enum.ToObject(thisEnumVar.EnumType, (byte)es2Data.Load<int>(entry.Key));
						}
						else if(type == typeof(string))
						{
							FsmString thisVar = variable.FindFsmString(entry.Key);
							if(thisVar != null)
								thisVar.Value = es2Data.Load<string>(entry.Key);
						}
						else if(type == typeof(Vector2))
						{
							FsmVector2 thisVar = variable.FindFsmVector2(entry.Key);
							if(thisVar != null)
								thisVar.Value =es2Data.Load<Vector2>(entry.Key);
						}
						else if(type == typeof(Vector3))
						{
							FsmVector3 thisVar = variable.FindFsmVector3(entry.Key);
							if(thisVar != null)
								thisVar.Value =es2Data.Load<Vector3>(entry.Key);
						}
						else if(type == typeof(Rect))
						{
							FsmRect thisVar = variable.FindFsmRect(entry.Key);
							if(thisVar != null)
								thisVar.Value = es2Data.Load<Rect>(entry.Key);
						}
						else if(type == typeof(Quaternion))
						{
							FsmQuaternion thisVar = variable.FindFsmQuaternion(entry.Key);
							if(thisVar != null)
								thisVar.Value =es2Data.Load<Quaternion>(entry.Key);
						}
						else if(type == typeof(Color))
						{
							FsmColor thisVar = variable.FindFsmColor(entry.Key);
							if(thisVar != null)
								thisVar.Value = es2Data.Load<Color>(entry.Key);
						}
						else if(type == typeof(Material))
						{
							FsmMaterial thisVar = variable.FindFsmMaterial(entry.Key);
							if(thisVar != null)
								thisVar.Value = es2Data.Load<Material>(entry.Key);
						}
						else if(type == typeof(Texture2D))
						{
							FsmTexture thisVar = variable.FindFsmTexture(entry.Key);
							if(thisVar != null)
								thisVar.Value = es2Data.Load<Texture2D>(entry.Key);
						}
						else
						{
							FsmObject thisVar = variable.FindFsmObject(entry.Key);
							if(thisVar != null)
								thisVar.Value = entry.Value as UnityEngine.Object;
						}
					}
				}

				Log("Loaded from "+filename.Value);
				Finish();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.ToString());
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveFloat : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmFloat saveValue;


		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Value, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveInt : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmInt saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Value, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveBool : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmBool saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Value, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveString : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmString saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Value, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveVector2 : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmVector2 saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Value, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveVector3 : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmVector3 saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Value, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveEnum : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmEnum saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save<int>(System.Convert.ToInt32(saveValue.Value), filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveColor : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmColor saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Value, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveMaterial : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmMaterial saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Value, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveTexture : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmTexture saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			ES2.Save(saveValue.Value as Texture2D, filename.Value, GetSettings(new ES2Settings()));
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveQuaternion : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmQuaternion saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Value, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	#if PLAYMAKER_1_8_OR_NEWER
	[ActionCategory("Easy Save 2")]
	public class SaveObjectArray : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Object)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Values, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				Debug.Log(e.ToString());
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveFloatArray : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Float)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Values, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				Debug.Log(e.ToString());
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveIntArray : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Int)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Values, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				Debug.Log(e.ToString());
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveBoolArray : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Bool)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Values, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				Debug.Log(e.ToString());
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveStringArray : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.String)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Values, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				Debug.Log(e.ToString());
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveVector2Array : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Vector2)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Values, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				Debug.Log(e.ToString());
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveVector3Array : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Vector3)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Values, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				Debug.Log(e.ToString());
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveColorArray : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Color)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Values, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				Debug.Log(e.ToString());
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveRectArray : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Rect)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Values, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				Debug.Log(e.ToString());
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveMaterialArray : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Material)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Values, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				Debug.Log(e.ToString());
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveTextureArray : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Texture)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Values, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				Debug.Log(e.ToString());
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveQuaternionArray : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Quaternion)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Values, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				Debug.Log(e.ToString());
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveEnumArray : ES2SaveAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Enum)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.Save(saveValue.Values, filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				Debug.Log(e.ToString());
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
		}
	}

	#endif

	[ActionCategory("Easy Save 2")]
	public class SaveImage : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The Texture we want to save as a PNG.")]
		public FsmTexture saveValue;
		[RequiredField]
		[Tooltip("The PNG file we want to save the Texture as.")]
		public FsmString filename = "image.png";
		[Tooltip("This event is called if an error occurs.")]
		public FsmEvent ifError = null;

		public override void Reset()
		{
			saveValue = null;
			filename = "image.png";
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.SaveImage(saveValue.Value as Texture2D, filename.Value);
				Finish();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class CopyFile : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The file we want to copy.")]
		public FsmString filename = "defaultFile.txt";
		[Tooltip("The filename we want to use for the copy of the file.")]
		public FsmString newFilename = "newFile.txt";
		[Tooltip("This event is called if an error occurs.")]
		public FsmEvent ifError = null;

		public override void Reset()
		{
			filename = "defaultFile.txt";
			newFilename = "newFile.txt";
			ifError = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.SaveRaw(ES2.LoadRaw(filename.Value), newFilename.Value);
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
				Finish();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveRawUTF8 : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The raw string we want to save to a file.")]
		public FsmString saveValue;
		[Tooltip("The name or absolute path of the file where we want to store our raw string. If the file doesn't exist, it will be created.")]
		public FsmString filename = "defaultFile.txt";
		[Tooltip("This event is called if an error occurs.")]
		public FsmEvent ifError = null;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.SaveRaw(System.Text.Encoding.UTF8.GetBytes(saveValue.Value), filename.Value);
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
				Finish();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class SaveRawBase64 : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The raw string we want to save to a file.")]
		public FsmString saveValue;
		[Tooltip("The name or absolute path of the file where we want to store our raw string. If the file doesn't exist, it will be created.")]
		public FsmString filename = "defaultFile.txt";
		[Tooltip("This event is called if an error occurs.")]
		public FsmEvent ifError = null;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.SaveRaw(System.Convert.FromBase64String(saveValue.Value), filename.Value);
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
				Finish();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class AppendRaw : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The raw string we want to append to a file.")]
		public FsmString saveValue;
		[Tooltip("The name or absolute path of the file we want to append our raw string to. If the file doesn't exist, it will be created.")]
		public FsmString filename = "defaultFile.txt";
		[Tooltip("This event is called if an error occurs.")]
		public FsmEvent ifError = null;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				ES2.AppendRaw(saveValue.Value, filename.Value);
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
				Finish();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}
	#endregion

	#region SaveComponents
	[ActionCategory("Easy Save 2")]
	public class SaveSphereCollider : ES2SaveComponentAction<SphereCollider>{}
	[ActionCategory("Easy Save 2")]
	public class SaveBoxCollider : ES2SaveComponentAction<BoxCollider>{}
	[ActionCategory("Easy Save 2")]
	public class SaveCapsuleCollider : ES2SaveComponentAction<CapsuleCollider>{}
	[ActionCategory("Easy Save 2")]
	public class SaveMeshCollider : ES2SaveComponentAction<MeshCollider>{}
	[ActionCategory("Easy Save 2")]
	public class SaveTransform : ES2SaveComponentAction<Transform>{}
	#endregion
	#region SaveNonPlaymakerTypes
	[ActionCategory("Easy Save 2")]
	public class SaveMesh : ES2SaveObjectAction<Mesh>{}
	[ActionCategory("Easy Save 2")]
	public class SaveAudioClip : ES2SaveObjectAction<AudioClip>{}
	[ActionCategory("Easy Save 2")]
	public class SaveSprite : ES2SaveObjectAction<Sprite>{}
	#endregion

	#region LoadPlaymakerTypes

	[ActionCategory("Easy Save 2")]
	public class LoadPosition : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The GameObject we want to assign the position to.")]
		[UIHint(UIHint.Variable)]
		public FsmOwnerDefault loadValue;

		public override void OnEnter()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(loadValue);

			if(go == null)
			{
				LogError("Could not load position. No GameObject has been specified.");
				Finish ();
				return;
			}

			try
			{
				go.transform.position = ES2.Load<Vector3>(filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadRotation : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The GameObject we want to assign the rotation to.")]
		[UIHint(UIHint.Variable)]
		public FsmOwnerDefault loadValue;

		public override void OnEnter()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(loadValue);

			if(go == null)
			{
				LogError("Could not load rotation. No GameObject has been specified.");
				Finish ();
				return;
			}

			try
			{
				go.transform.rotation = ES2.Load<Quaternion>(filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadScale : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The GameObject we want to assign the scale to.")]
		[UIHint(UIHint.Variable)]
		public FsmOwnerDefault loadValue;

		public override void OnEnter()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(loadValue);

			if(go == null)
			{
				LogError("Could not load scale. No GameObject has been specified.");
				Finish ();
				return;
			}

			try
			{
				go.transform.localScale = ES2.Load<Vector3>(filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadFloat : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		public FsmFloat loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Value = ES2.Load<float>(filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadInt : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		public FsmInt loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Value = ES2.Load<int>(filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadBool : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		public FsmBool loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Value = ES2.Load<bool>(filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadString : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		public FsmString loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Value = ES2.Load<string>(filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadVector2 : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		public FsmVector2 loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Value = ES2.Load<Vector2>(filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadVector3 : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		public FsmVector3 loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Value = ES2.Load<Vector3>(filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadEnum : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		public FsmEnum loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Value = (System.Enum)Enum.ToObject(loadValue.EnumType, (byte)ES2.Load<int>(filename.Value, GetSettings(new ES2Settings())));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadColor : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		public FsmColor loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Value = ES2.Load<Color>(filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadMaterial : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		public FsmMaterial loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Value = ES2.Load<Material>(filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadTexture : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		public FsmTexture loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Value = ES2.Load<Texture2D>(filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadQuaternion : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		public FsmQuaternion loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Value = ES2.Load<Quaternion>(filename.Value, GetSettings(new ES2Settings()));
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}


	#if PLAYMAKER_1_8_OR_NEWER
	[ActionCategory("Easy Save 2")]
	public class LoadObjectArray : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		[ArrayEditorAttribute(VariableType.Object)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Values = ES2.LoadArray<object>(filename.Value, GetSettings(new ES2Settings()));
				loadValue.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadFloatArray : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		[ArrayEditorAttribute(VariableType.Float)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Values = ES2.LoadArray<object>(filename.Value, GetSettings(new ES2Settings()));
				loadValue.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadIntArray : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		[ArrayEditorAttribute(VariableType.Int)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Values = ES2.LoadArray<object>(filename.Value, GetSettings(new ES2Settings()));
				loadValue.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadBoolArray : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		[ArrayEditorAttribute(VariableType.Bool)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Values = ES2.LoadArray<object>(filename.Value, GetSettings(new ES2Settings()));
				loadValue.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadStringArray : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		[ArrayEditorAttribute(VariableType.String)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Values = ES2.LoadArray<object>(filename.Value, GetSettings(new ES2Settings()));
				loadValue.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadVector2Array : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		[ArrayEditorAttribute(VariableType.Vector2)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Values = ES2.LoadArray<object>(filename.Value, GetSettings(new ES2Settings()));
				loadValue.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadVector3Array : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		[ArrayEditorAttribute(VariableType.Vector3)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Values = ES2.LoadArray<object>(filename.Value, GetSettings(new ES2Settings()));
				loadValue.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadColorArray : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		[ArrayEditorAttribute(VariableType.Color)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Values = ES2.LoadArray<object>(filename.Value, GetSettings(new ES2Settings()));
				loadValue.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadRectArray : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		[ArrayEditorAttribute(VariableType.Rect)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Values = ES2.LoadArray<object>(filename.Value, GetSettings(new ES2Settings()));
				loadValue.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadMaterialArray : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		[ArrayEditorAttribute(VariableType.Material)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Values = ES2.LoadArray<object>(filename.Value, GetSettings(new ES2Settings()));
				loadValue.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadTextureArray : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		[ArrayEditorAttribute(VariableType.Texture)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Values = ES2.LoadArray<object>(filename.Value, GetSettings(new ES2Settings()));
				loadValue.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadQuaternionArray : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		[ArrayEditorAttribute(VariableType.Quaternion)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Values = ES2.LoadArray<object>(filename.Value, GetSettings(new ES2Settings()));
				loadValue.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadEnumArray : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		[ArrayEditorAttribute(VariableType.Enum)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Values = ES2.LoadArray<object>(filename.Value, GetSettings(new ES2Settings()));
				loadValue.SaveChanges();
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	#endif

	[ActionCategory("Easy Save 2")]
	public class LoadImage : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The texture we want to load the image into.")]
		public FsmTexture loadValue;
		[RequiredField]
		[Tooltip("The PNG or JPG file we want to load.")]
		public FsmString filename = "image.png";
		[Tooltip("This event is called if an error occurs.")]
		public FsmEvent ifError = null;

		public override void Reset()
		{
			loadValue = null;
			filename = "image.png";
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Value = ES2.LoadImage(filename.Value);
				Finish();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class LoadRaw : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The data we we want to load.")]
		public FsmString loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				byte[] bytes = ES2.LoadRaw(filename.Value, GetSettings(new ES2Settings()));
				loadValue.Value = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class GetTagType : ES2LoadAction
	{
		[RequiredField]
		[Tooltip("The string we want to store the type string in.")]
		public FsmString loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				loadValue.Value = ES2.LoadObject(filename.Value, GetSettings(new ES2Settings())).GetType().Name;
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	#endregion

	#region LoadComponents
	[ActionCategory("Easy Save 2")]
	public class LoadSphereCollider : ES2LoadComponentAction<SphereCollider>{}
	[ActionCategory("Easy Save 2")]
	public class LoadBoxCollider : ES2LoadComponentAction<BoxCollider>{}
	[ActionCategory("Easy Save 2")]
	public class LoadCapsuleCollider : ES2LoadComponentAction<CapsuleCollider>{}
	[ActionCategory("Easy Save 2")]
	public class LoadMeshCollider : ES2LoadComponentAction<MeshCollider>{}
	[ActionCategory("Easy Save 2")]
	public class LoadTransform : ES2LoadComponentAction<Transform>{}
	#endregion

	#region LoadNonPlaymakerTypes
	/* SAVE NON-PLAYMAKER TYPES */
	[ActionCategory("Easy Save 2")]
	public class LoadMesh : ES2LoadObjectAction<Mesh>{}
	[ActionCategory("Easy Save 2")]
	public class LoadAudioClip : ES2LoadObjectAction<AudioClip>{}
	[ActionCategory("Easy Save 2")]
	public class LoadSprite : ES2LoadObjectAction<Sprite>{}
	#endregion

	#region UploadPlaymakerTypes

	#if !DISABLE_WWW

	[ActionCategory("Easy Save 2")]
	public class UploadRaw : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmString saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.UploadRaw(saveValue.Value));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadFile : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The name or path of the local file we want to upload.")]
		public FsmString localFilename = "defaultFile.txt";

		public override void Reset()
		{
			localFilename = "defaultFile.txt";
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				Debug.Log (new ES2Settings().saveLocation);
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.UploadRaw(ES2.LoadRaw(localFilename.Value)));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadFloat : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmFloat saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Value));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadInt : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmInt saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Value));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadBool : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmBool saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Value));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadString : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmString saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Value));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadVector2 : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmVector2 saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Value));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadVector3 : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmVector3 saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Value));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadColor : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmColor saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Value));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadMaterial : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmMaterial saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Value));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadTexture : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmTexture saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Value));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadQuaternion : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		public FsmQuaternion saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Value));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	#if PLAYMAKER_1_8_OR_NEWER

	[ActionCategory("Easy Save 2")]
	public class UploadObjectArray : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Object)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Values));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadFloatArray : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Float)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Values));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadIntArray : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Int)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Values));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadBoolArray : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Bool)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Values));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadStringArray : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.String)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Values));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadVector2Array : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Vector2)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Values));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadVector3Array : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Vector3)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Values));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadColorArray : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Color)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Values));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadRectArray : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Rect)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Values));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadMaterialArray : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Material)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Values));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadTextureArray : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Texture)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Values));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadQuaternionArray : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Quaternion)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Values));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class UploadEnumArray : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The data we want to save.")]
		[ArrayEditorAttribute(VariableType.Enum)]
		public FsmArray saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				this.Fsm.Owner.StartCoroutine(web.Upload(saveValue.Values));
				base.OnEnter();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	#endif

	[ActionCategory("Easy Save 2")]
	public class UploadImage : ES2UploadAction
	{
		[RequiredField]
		[Tooltip("The Texture2D we want to save as a PNG.")]
		public FsmTexture saveValue;

		public override void Reset()
		{
			saveValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			CreateES2Web();
			this.Fsm.Owner.StartCoroutine(web.UploadImage(saveValue.Value as Texture2D));
			base.OnEnter();
		}
	}

	#endif

	#endregion

	#region UploadComponents

	#if !DISABLE_WWW

	[ActionCategory("Easy Save 2")]
	public class UploadSphereCollider : ES2UploadComponentAction<SphereCollider>{}
	[ActionCategory("Easy Save 2")]
	public class UploadBoxCollider : ES2UploadComponentAction<BoxCollider>{}
	[ActionCategory("Easy Save 2")]
	public class UploadCapsuleCollider : ES2UploadComponentAction<CapsuleCollider>{}
	[ActionCategory("Easy Save 2")]
	public class UploadMeshCollider : ES2UploadComponentAction<MeshCollider>{}
	[ActionCategory("Easy Save 2")]
	public class UploadTransform : ES2UploadComponentAction<Transform>{}

	#endif

	#endregion

	#region UploadNonPlaymakerTypes

	#if !DISABLE_WWW

	[ActionCategory("Easy Save 2")]
	public class UploadMesh : ES2UploadObjectAction<Mesh>{}
	[ActionCategory("Easy Save 2")]
	public class UploadAudioClip : ES2UploadObjectAction<AudioClip>{}
	[ActionCategory("Easy Save 2")]
	public class UploadSprite : ES2UploadObjectAction<Sprite>{}

	#endif

	#endregion

	#region DownloadPlaymakerTypes

	#if !DISABLE_WWW

	[ActionCategory("Easy Save 2")]
	public class DownloadFloat : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		public FsmFloat loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Value = web.Load<float>(tag.Value);
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadInt : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		public FsmInt loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Value = web.Load<int>(tag.Value);
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadBool : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		public FsmBool loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Value = web.Load<bool>(tag.Value);
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadString : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		public FsmString loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Value = web.Load<string>(tag.Value);
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadVector2 : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		public FsmVector2 loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Value = web.Load<Vector2>(tag.Value);
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadVector3 : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		public FsmVector3 loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Value = web.Load<Vector3>(tag.Value);
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadColor : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		public FsmColor loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Value = web.Load<Color>(tag.Value);
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadMaterial : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		public FsmMaterial loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Value = web.Load<Material>(tag.Value);
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadTexture : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		public FsmTexture loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Value = web.Load<Texture2D>(tag.Value);
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadQuaternion : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		public FsmQuaternion loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Value = web.Load<Quaternion>(tag.Value);
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	#if PLAYMAKER_1_8_OR_NEWER

	[ActionCategory("Easy Save 2")]
	public class DownloadObjectArray : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		[ArrayEditorAttribute(VariableType.Object)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Values = web.LoadArray<object>(tag.Value);
				loadValue.SaveChanges();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadFloatArray : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		[ArrayEditorAttribute(VariableType.Float)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Values = web.LoadArray<object>(tag.Value);
				loadValue.SaveChanges();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadIntArray : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		[ArrayEditorAttribute(VariableType.Int)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Values = web.LoadArray<object>(tag.Value);
				loadValue.SaveChanges();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadBoolArray : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		[ArrayEditorAttribute(VariableType.Bool)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Values = web.LoadArray<object>(tag.Value);
				loadValue.SaveChanges();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadStringArray : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		[ArrayEditorAttribute(VariableType.String)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Values = web.LoadArray<object>(tag.Value);
				loadValue.SaveChanges();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadFilenames : ES2WebAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		[ArrayEditorAttribute(VariableType.String)]
		public FsmArray loadValue;

		public override void OnEnter()
		{
			CreateES2Web();
			this.Fsm.Owner.StartCoroutine(web.DownloadFilenames());

			base.OnEnter(); // Ensure that base.OnEnter() is called when done.
			Log("Downloading from "+filename.Value);
		}
		
		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Values = web.GetFilenames();
				loadValue.SaveChanges();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadVector2Array : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		[ArrayEditorAttribute(VariableType.Vector2)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Values = web.LoadArray<object>(tag.Value);
				loadValue.SaveChanges();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadVector3Array : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		[ArrayEditorAttribute(VariableType.Vector3)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Values = web.LoadArray<object>(tag.Value);
				loadValue.SaveChanges();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadColorArray : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		[ArrayEditorAttribute(VariableType.Color)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Values = web.LoadArray<object>(tag.Value);
				loadValue.SaveChanges();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadRectArray : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		[ArrayEditorAttribute(VariableType.Rect)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Values = web.LoadArray<object>(tag.Value);
				loadValue.SaveChanges();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadMaterialArray : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		[ArrayEditorAttribute(VariableType.Material)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Values = web.LoadArray<object>(tag.Value);
				loadValue.SaveChanges();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadTextureArray : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		[ArrayEditorAttribute(VariableType.Texture)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Values = web.LoadArray<object>(tag.Value);
				loadValue.SaveChanges();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadQuaternionArray : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		[ArrayEditorAttribute(VariableType.Quaternion)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Values = web.LoadArray<object>(tag.Value);
				loadValue.SaveChanges();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadEnumArray : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The variable we want to load our data into.")]
		[ArrayEditorAttribute(VariableType.Enum)]
		public FsmArray loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Values = web.LoadArray<object>(tag.Value);
				loadValue.SaveChanges();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	#endif

	[ActionCategory("Easy Save 2")]
	public class DownloadImage : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The Texture2D we want to load our PNG or JPEG into.")]
		public FsmTexture loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Value = web.LoadImage();
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadFile : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The local filename we want to save our downloaded file to.")]
		public FsmString localFilename = "defaultFile.txt";

		public override void Reset()
		{
			localFilename = "defaultFile.txt";
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				web.SaveToFile(localFilename.Value);
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	[ActionCategory("Easy Save 2")]
	public class DownloadRaw : ES2DownloadAction
	{
		[RequiredField]
		[Tooltip("The local filename we want to save our downloaded file to.")]
		public FsmString loadValue;

		public override void Reset()
		{
			loadValue = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void IsDone()
		{
			try
			{
				loadValue.Value = web.text;
				base.IsDone();
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	#endif

	#endregion

	#region DownloadComponents

	#if !DISABLE_WWW

	[ActionCategory("Easy Save 2")]
	public class DownloadSphereCollider : ES2DownloadComponentAction<SphereCollider>{}
	[ActionCategory("Easy Save 2")]
	public class DownloadBoxCollider : ES2DownloadComponentAction<BoxCollider>{}
	[ActionCategory("Easy Save 2")]
	public class DownloadCapsuleCollider : ES2DownloadComponentAction<CapsuleCollider>{}
	[ActionCategory("Easy Save 2")]
	public class DownloadMeshCollider : ES2DownloadComponentAction<MeshCollider>{}
	[ActionCategory("Easy Save 2")]
	public class DownloadTransform : ES2DownloadComponentAction<Transform>{}

	#endif

	#endregion

	#region DownloadNonPlaymakerTypes

	#if !DISABLE_WWW

	[ActionCategory("Easy Save 2")]
	public class DownloadMesh : ES2DownloadObjectAction<Mesh>{}
	[ActionCategory("Easy Save 2")]
	public class DownloadAudioClip : ES2DownloadObjectAction<AudioClip>{}
	[ActionCategory("Easy Save 2")]
	public class DownloadSprite : ES2DownloadObjectAction<Sprite>{}

	#endif

	#endregion

	#region ES2WebMethods

	#if !DISABLE_WWW

	[ActionCategory("Easy Save 2")]
	public class DeleteFromWeb : ES2WebAction
	{
		public override void OnEnter()
		{
			try
			{
				CreateES2Web();
				if(string.IsNullOrEmpty(tag.Value))
					web.settings.filenameData.tag = "";
				this.Fsm.Owner.StartCoroutine(web.Delete());
				base.OnEnter(); // Ensure that base.OnEnter() is called when done.
				Log("Deleting from "+filename.Value);
			}
			catch(System.Exception e)
			{
				if(ifError != null)
				{
					LogError(e.Message);
					Fsm.Event(ifError);
				}
			}
		}
	}

	#endif

	#endregion

	#region ES2Spreadsheet

	[ActionCategory("Easy Save 2")]
	public class CreateES2Spreadsheet : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The variable we want to store our Spreadsheet in.")]
		public FsmObject spreadsheet;
		[Tooltip("Whether this spreadsheet appends data when saving.")]
		public FsmBool append;

		public override void Reset()
		{
			spreadsheet = null;
			append = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			FSM_ES2Spreadsheet fsmSpreadsheet = FSM_ES2Spreadsheet.Create();
			fsmSpreadsheet.Value.append = append.Value;
			spreadsheet.Value = fsmSpreadsheet;
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	public class ES2SpreadsheetSetCellString : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The spreadsheet we want to set a cell of.")]
		public FsmObject spreadsheet;
		public FsmInt col;
		public FsmInt row;
		public FsmString value;

		public override void Reset()
		{
			spreadsheet = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(col.Value, row.Value, value.Value);
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	public class ES2SpreadsheetSetCellInt : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The spreadsheet we want to set a cell of.")]
		public FsmObject spreadsheet;
		public FsmInt col;
		public FsmInt row;
		public FsmInt value;

		public override void Reset()
		{
			spreadsheet = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(col.Value, row.Value, value.Value);
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	public class ES2SpreadsheetSetCellFloat : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The spreadsheet we want to set a cell of.")]
		public FsmObject spreadsheet;
		public FsmInt col;
		public FsmInt row;
		public FsmFloat value;

		public override void Reset()
		{
			spreadsheet = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(col.Value, row.Value, value.Value);
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	public class ES2SpreadsheetSetCellObject : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The spreadsheet we want to set a cell of.")]
		public FsmObject spreadsheet;
		public FsmInt col;
		public FsmInt row;
		public FsmObject value;

		public override void Reset()
		{
			spreadsheet = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(col.Value, row.Value, value.Value);
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	public class ES2SpreadsheetSetCellVector2 : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The spreadsheet we want to set a cell of.")]
		public FsmObject spreadsheet;
		public FsmInt col;
		public FsmInt row;
		public FsmVector2 value;

		public override void Reset()
		{
			spreadsheet = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(col.Value, row.Value, value.Value);
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	public class ES2SpreadsheetSetCellVector3 : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The spreadsheet we want to set a cell of.")]
		public FsmObject spreadsheet;
		public FsmInt col;
		public FsmInt row;
		public FsmVector3 value;

		public override void Reset()
		{
			spreadsheet = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(col.Value, row.Value, value.Value);
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	public class ES2SpreadsheetSave : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The spreadsheet we want to save.")]
		public FsmObject spreadsheet;
		[RequiredField]
		[Tooltip("The filename or path we want to save the spreadsheet to.")]
		public FsmString path;

		public override void Reset()
		{
			spreadsheet = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			((FSM_ES2Spreadsheet)spreadsheet.Value).Value.Save(path.Value);
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	public class ES2SpreadsheetSaveAll : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The spreadsheet we want to insert our values into.")]
		public FsmObject spreadsheet;
		[Tooltip("Save the local variables accessible in this FSM?")]
		public FsmBool saveFsmVariables = true;
		[Tooltip("Save the global variables accessible in all FSMs?")]
		public FsmBool saveGlobalVariables = true;

		public override void Reset()
		{
			spreadsheet = null;
			saveFsmVariables = true;
			saveGlobalVariables = true;
			base.Reset();
		}

		public override void OnEnter()
		{
			// Get FSMVariables objects required based on whether the user wants to save
			// local variables, global variables or both.
			FsmVariables[] fsmVariables;
			if(saveFsmVariables.Value && saveGlobalVariables.Value)
				fsmVariables = new FsmVariables[]{Fsm.Variables, FsmVariables.GlobalVariables};
			else if(saveFsmVariables.Value && !saveGlobalVariables.Value)
				fsmVariables = new FsmVariables[]{Fsm.Variables};
			else if(!saveFsmVariables.Value && saveGlobalVariables.Value)
				fsmVariables = new FsmVariables[]{FsmVariables.GlobalVariables};
			else
				fsmVariables = new FsmVariables[0];

			int row = 0;

			foreach(FsmVariables fsmVariable in fsmVariables)
			{
				// Variables are stored in seperate arrays based on their types.
				// Save each item in each array seperately.
				foreach(FsmBool fsmVar in fsmVariable.BoolVariables)
				{
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(0, row, fsmVar.Name);
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(1, row, fsmVar.Value.ToString());
					row++;
				}

				foreach(FsmFloat fsmVar in fsmVariable.FloatVariables)
				{
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(0, row, fsmVar.Name);
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(1, row, fsmVar.Value.ToString());
					row++;
				}

				foreach(FsmInt fsmVar in fsmVariable.IntVariables)
				{
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(0, row, fsmVar.Name);
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(1, row, fsmVar.Value.ToString());
					row++;
				}

				foreach(FsmString fsmVar in fsmVariable.StringVariables)
				{
					if(fsmVar.Value == null)
						continue;
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(0, row, fsmVar.Name);
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(1, row, fsmVar.Value.ToString());
					row++;
				}

				foreach(FsmVector2 fsmVar in fsmVariable.Vector2Variables)
				{
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(0, row, fsmVar.Name);
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(1, row, fsmVar.Value.ToString());
					row++;
				}

				foreach(FsmVector3 fsmVar in fsmVariable.Vector3Variables)
				{
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(0, row, fsmVar.Name);
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(1, row, fsmVar.Value.ToString());
					row++;
				}

				foreach(FsmRect fsmVar in fsmVariable.RectVariables)
				{
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(0, row, fsmVar.Name);
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(1, row, fsmVar.Value.ToString());
					row++;
				}

				foreach(FsmQuaternion fsmVar in fsmVariable.QuaternionVariables)
				{
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(0, row, fsmVar.Name);
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(1, row, fsmVar.Value.ToString());
					row++;
				}

				foreach(FsmColor fsmVar in fsmVariable.ColorVariables)
				{
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(0, row, fsmVar.Name);
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(1, row, fsmVar.Value.ToString());
					row++;
				}

				foreach(FsmMaterial fsmVar in fsmVariable.MaterialVariables)
				{
					if(fsmVar.Value == null)
						continue;
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(0, row, fsmVar.Name);
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(1, row, fsmVar.Value.ToString());
					row++;
				}

				foreach(FsmTexture fsmVar in fsmVariable.TextureVariables)
				{
					if(fsmVar.Value == null)
						continue;
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(0, row, fsmVar.Name);
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(1, row, fsmVar.Value.ToString());
					row++;
				}

				foreach(FsmObject fsmVar in fsmVariable.ObjectVariables)
				{
					if(fsmVar.Value == null)
						continue;
					if(fsmVar.Value.GetType() == typeof(FSM_ES2Spreadsheet))
						continue;
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(0, row, fsmVar.Name);
					((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(1, row, fsmVar.Value.ToString());
					row++;
				}

				#if PLAYMAKER_1_8_OR_NEWER
				foreach(FsmArray fsmVar in fsmVariable.ArrayVariables)
				{
					if(fsmVar.Values == null)
					continue;

					int i=0;
					foreach(var obj in fsmVar.Values)
					{
						((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(0, row, fsmVar.Name + "["+i+"]" );
						((FSM_ES2Spreadsheet)spreadsheet.Value).Value.SetCell(1, row, obj.ToString());
						row++;
						i++;
					}
				}
				#endif
			}

			Finish();
		}
	}


	/* 
		A proxy of ES2Spreadsheet so we can use it as an FSMObject variable, which only accepts
		a UnityEngine.Object, not System.Object.
	*/
	public class FSM_ES2Spreadsheet : ScriptableObject
	{
		public ES2Spreadsheet Value = null;

		public static FSM_ES2Spreadsheet Create()
		{
			FSM_ES2Spreadsheet spreadsheet = ScriptableObject.CreateInstance<FSM_ES2Spreadsheet>();
			spreadsheet.Value = new ES2Spreadsheet();
			return spreadsheet;
		}
	}

	#endregion

	#region Miscellaneous Actions

	[ActionCategory("Easy Save 2")]
	public class MoveFileFromResources : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The filename or path of the file in Resources we want to move.")]
		public FsmString resourcesFilename;
		[Tooltip("The filename or path we want to move the file to, relative to the default save location.")]
		public FsmString destinationFilename;

		public override void Reset()
		{
			resourcesFilename = null;
			destinationFilename = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			ES2Settings resourcesSettings = new ES2Settings();
			resourcesSettings.saveLocation = ES2Settings.SaveLocation.Resources;

			byte[] bytes = ES2.LoadRaw(resourcesFilename.Value, resourcesSettings);
			ES2.SaveRaw(bytes, destinationFilename.Value);

			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	public class Texture2DToPNGString : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The Texture2D we want to convert to a PNG byte array.")]
		public FsmTexture texture2D;
		[Tooltip("The Texture2D converted to a PNG,  as a string.")]
		public FsmString pngString;

		public override void Reset()
		{
			texture2D = null;
			pngString = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			pngString.Value = System.Convert.ToBase64String(((Texture2D)texture2D.Value).EncodeToPNG());
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	public class GetAssetsFolderPath : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The variable we want to put our path in.")]
		public FsmString path;
		[Tooltip("A string we want to append to the path (for example, a filename). Leave blank if you do not want to append anything.")]
		public FsmString appendString;

		public override void Reset()
		{
			path = null;
			appendString = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			string fullPath = Application.dataPath;
			if(appendString != null)
				fullPath += appendString.Value;
			path.Value = fullPath;
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	public class GetPersistentDataPath : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The variable we want to put our path in.")]
		public FsmString path;
		[Tooltip("A string we want to append to the path (for example, a filename). Leave blank if you do not want to append anything.")]
		public FsmString appendString;

		public override void Reset()
		{
			path = null;
			appendString = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			string fullPath = Application.persistentDataPath;
			if(appendString != null)
				fullPath += appendString.Value;
			path.Value = fullPath;
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	public class GetDataPath : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The variable we want to put our path in.")]
		public FsmString path;
		[Tooltip("A string we want to append to the path (for example, a filename). Leave blank if you do not want to append anything.")]
		public FsmString appendString;

		public override void Reset()
		{
			path = null;
			appendString = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			string fullPath = Application.dataPath;
			if(appendString != null)
				fullPath += appendString.Value;
			path.Value = fullPath;
			Finish();
		}
	}

	[ActionCategory("Easy Save 2")]
	public class GetStreamingAssetsPath : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The variable we want to put our path in.")]
		public FsmString path;
		[Tooltip("A string we want to append to the path (for example, a filename). Leave blank if you do not want to append anything.")]
		public FsmString appendString;

		public override void Reset()
		{
			path = null;
			appendString = null;
			base.Reset (); // Ensure that base.Reset() is called when done.
		}

		public override void OnEnter()
		{
			string fullPath = Application.streamingAssetsPath;
			if(appendString != null)
				fullPath += appendString.Value;
			path.Value = fullPath;
			Finish();
		}
	}

	#endregion

	#if UNITY_5 || UNITY_5_3_OR_NEWER
	[ActionCategory("Easy Save 2")]
	public class SaveAutoSave : FsmStateAction
	{
		public override void OnEnter()
		{
			GameObject go = GameObject.Find("ES2 Auto Save Manager");
			ES2AutoSaveManager mgr;
			if(go == null || (mgr = go.GetComponent<ES2AutoSaveManager>()) == null)
				LogError("Could not run Save Auto Save action: Auto Save is not enabled for this scene.");
			else
				mgr.Save();
			Finish();
		}
	}
	
	[ActionCategory("Easy Save 2")]
	public class LoadAutoSave : FsmStateAction
	{
		public override void OnEnter()
		{
			GameObject go = GameObject.Find("ES2 Auto Save Manager");
			ES2AutoSaveManager mgr;
			if(go == null || (mgr = go.GetComponent<ES2AutoSaveManager>()) == null)
				LogError("Could not run Load Auto Save action: Auto Save is not enabled for this scene.");
			else
				mgr.Load();
			Finish();
		}
	}
	#endif
}
#endif