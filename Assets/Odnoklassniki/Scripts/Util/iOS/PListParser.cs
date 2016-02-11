using System;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PListParser
{
	private const string KEY_TYPES = "CFBundleURLTypes";
	private const string KEY_SCHEMES = "CFBundleURLSchemes";

	private const string KEY_QUERIES_SCHEMES = "LSApplicationQueriesSchemes";

	private const string KEY_SECURITY = "NSAppTransportSecurity";
	private const string KEY_EXCEPTIONS = "NSExceptionDomains";

	private const string KEY_SUBDOMAINS = "NSIncludesSubdomains";
	private const string KEY_FORWARD_SECRECY = "NSExceptionRequiresForwardSecrecy";
	private const string KEY_ALLOW_HTTP = "NSExceptionAllowsInsecureHTTPLoads";

	public PListWrapper xmlDict;
	private readonly string filePath;

	public PListParser(string fullPath)
	{
		filePath = fullPath;
		XmlReaderSettings settings = new XmlReaderSettings();
		settings.ProhibitDtd = false;
		XmlReader plistReader = XmlReader.Create(filePath, settings);

		XDocument doc = XDocument.Load(plistReader);
		XElement plist = doc.Element("plist");
		XElement dict = plist.Element("dict");
		xmlDict = new PListWrapper(dict);
		plistReader.Close();
	}

	public void AddExceptionDomains(string[] domains, bool subDomains = false, bool skipForwardSecrecy = false, bool allowHttp = false)
	{
		PListWrapper security;
		if (xmlDict.ContainsKey(KEY_SECURITY)) {
			security = (PListWrapper) xmlDict[KEY_SECURITY];
		} else {
			Debug.Log("Added new NSAppTransportSecurity entry");
			security = new PListWrapper();
			xmlDict.Add(KEY_SECURITY, security);
		}

		PListWrapper exceptionDomains;
		if (security.ContainsKey(KEY_EXCEPTIONS))
		{
			exceptionDomains = (PListWrapper)security[KEY_EXCEPTIONS];
		}
		else
		{
			Debug.Log("Added new NSExceptionDomains entry");
			exceptionDomains = new PListWrapper();
			security.Add(KEY_EXCEPTIONS, exceptionDomains);
		}

		foreach (string domain in domains)
		{
			if (!exceptionDomains.ContainsKey(domain))
			{
				exceptionDomains.Add(domain, GenerateExceptionEntry(subDomains, skipForwardSecrecy, allowHttp));
				Debug.Log("Added exception entry for: " + domain);
			}
		}
	}

	private PListWrapper GenerateExceptionEntry(bool subDomains, bool skipForwardSecrecy, bool allowHttp)
	{
		var entry = new PListWrapper();
		if (subDomains)
		{
			entry.Add(KEY_SUBDOMAINS, true);
		}

		if (skipForwardSecrecy)
		{
			entry.Add(KEY_FORWARD_SECRECY, false);
		}

		if (allowHttp)
		{
			entry.Add(KEY_ALLOW_HTTP, true);
		}

		return entry;
	}

	private void AddBundleUrlTypesSchemes(string urlScheme)
	{
		Debug.Log("Adding schema: " + urlScheme);
		if (xmlDict.ContainsKey(KEY_TYPES))
		{
			var currentSchemas = (List<object>)xmlDict[KEY_TYPES];
			foreach (object schema in currentSchemas)
			{
				// if it's not a dictionary, go to next index
				if (schema.GetType() == typeof(PListWrapper))
				{
					var bundleTypeNode = (PListWrapper)schema;
					if (bundleTypeNode.ContainsKey(KEY_SCHEMES) && bundleTypeNode[KEY_SCHEMES].GetType() == typeof(List<object>))
					{
						var appIdsFromPListDict = (List<object>)bundleTypeNode[KEY_SCHEMES];
						Debug.Log("Added schema to existing scheme");
						appIdsFromPListDict.Add(urlScheme);
						return;
					}
				}
			}
			Debug.Log("Added new schema");
			// Didn't find schema, add new schema to the list of schemas already present
			currentSchemas.Add(GenerateSchema(urlScheme));
		}
		else
		{
			// Didn't find any CFBundleURLTypes, let's create one
			Debug.Log("Added new types and schema");
			var currentSchemas = new List<object>();
			currentSchemas.Add(GenerateSchema(urlScheme));
			xmlDict.Add(KEY_TYPES, currentSchemas);
		}
	}

	private void AddQueriesSchemes(string urlScheme)
	{
		//For iOS 9+
		Debug.Log("Adding query schema (for iOS 9)");
		List<object> queriesSchemes;
		if (xmlDict.ContainsKey(KEY_QUERIES_SCHEMES))
		{
			queriesSchemes = (List<object>)xmlDict[KEY_QUERIES_SCHEMES];
		}
		else
		{
			Debug.Log("Creating new LSApplicationQueriesSchemes entry");
			queriesSchemes = new List<object>();
			xmlDict.Add(KEY_QUERIES_SCHEMES, queriesSchemes);
		}

		if (!queriesSchemes.Contains(urlScheme))
		{
			queriesSchemes.Add(urlScheme);
		}
	}

	public void AddSchema(string urlScheme, bool register)
	{
		if (register)
		{
			AddBundleUrlTypesSchemes(urlScheme);
		}

		AddQueriesSchemes(urlScheme);
	}

	private PListWrapper GenerateSchema(string urlScheme)
	{
		var schemeList = new List<object> {urlScheme};
		var schemaEntry = new PListWrapper {{KEY_SCHEMES, schemeList}};
		return schemaEntry;
	}

	public void WriteToFile()
	{
		// Corrected header of the plist
		const string publicId = "-//Apple//DTD PLIST 1.0//EN";
		const string stringId = "http://www.apple.com/DTDs/PropertyList-1.0.dtd";
		var declaration = new XDeclaration("1.0", "UTF-8", null);
		var docType = new XDocumentType("plist", publicId, stringId, null);

		xmlDict.Save(filePath, declaration, docType);
	}
}