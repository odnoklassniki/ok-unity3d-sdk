using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Odnoklassniki.Util
{
	public static class iOSURLSchemePostProcess
	{

		//Domains that aren't using proper TLS 1.2
		//static readonly string[] TLSDomains = { "mycdn.me" };

		//Domains that are using HTTP
		static readonly string[] HTTPDomains = { "odnoklassniki.ru", "mycdn.me" };

		//Schemas that this application is able to handle
		static readonly string[] InternalSchemas = { "ok" + OKSettings.AppId };

		//Schemas that this application will try to open
		static readonly string[] ExternalSchemas = { "okauth" };

		[PostProcessBuild]
		public static void Process(BuildTarget target, string path)
		{
			Debug.Log("iOS URL scheme post process");
#if UNITY_5
			if (target != BuildTarget.iOS)
#else
			if (target != BuildTarget.iPhone)
#endif
			{
				Debug.Log("Bad target: " + target);
				return;
			}

			UpdatePlist(path);
		}

		private static void UpdatePlist(string path)
		{
			const string fileName = "Info.plist";
			string fullPath = Path.Combine(path, fileName);

			if (string.IsNullOrEmpty(OKSettings.AppId))
			{
				Debug.LogError("Could not parse URL schema: ProjectConfig.bundleId not specified");
				return;
			}

			var parser = new PListParser(fullPath);
			foreach (var schema in InternalSchemas)
			{
				parser.AddSchema(schema, true);
			}

			foreach (var schema in ExternalSchemas)
			{
				parser.AddSchema(schema, false);
			}

			//parser.AddExceptionDomains(TLSDomains, subDomains: true, skipForwardSecrecy: true, allowHttp: false);
			parser.AddExceptionDomains(HTTPDomains, subDomains: true, skipForwardSecrecy: false, allowHttp: true);
			parser.WriteToFile();
		}
	}
}