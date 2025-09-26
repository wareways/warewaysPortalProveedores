using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace Wareways.PortalProv.Infraestructura.SAP
{
	public class DiServer
	{
		private const string sRem = "|Empty Node|";

		public static System.Xml.XmlDocument GetEmpySchema(string SessionID, string objeto)
		{
			SBODI_Server.Node n = null;
			string s = null, strXML = null;
			System.Xml.XmlDocument d = null;

			d = new System.Xml.XmlDocument();
			n = new SBODI_Server.Node();

			strXML = @"<?xml version=""1.0"" encoding=""UTF-16""?>" + @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">" + "<env:Header>" + "<SessionID>" + SessionID + "</SessionID>" + @"</env:Header><env:Body><dis:GetBusinessObjectTemplate xmlns:dis=""http://www.sap.com/SBO/DIS"">" + "<Object>" + objeto + "</Object>" + "</dis:GetBusinessObjectTemplate></env:Body></env:Envelope>";

			s = n.Interact(strXML);
			d.LoadXml(s);
			return (RemoveEnv(d));
		}

		public static XmlDocument RemoveEnv(System.Xml.XmlDocument xmlD)
		{
			System.Xml.XmlDocument d = null;
			string s = null;

			d = new System.Xml.XmlDocument();
			if (Strings.InStr(xmlD.InnerXml, "<env:Fault>", (Microsoft.VisualBasic.CompareMethod)(0)) != 0)
			{
				return xmlD;
			}
			else
			{
				try {
					s = xmlD.FirstChild.NextSibling.FirstChild.FirstChild.InnerXml;
					d.LoadXml(s);
				}
				catch {
					s = xmlD.FirstChild.NextSibling.FirstChild.InnerXml;
					d.LoadXml(s);
				}
				
			}

			return d;

		}

		public static string Login(string DataBaseServer, string DataBaseName, string DataBaseType, string DataBaseUserName, string DataBasePassword, string CompanyUserName, string CompanyPassword, string Language, string LicenseServer)
		{
			SBODI_Server.Node DISnode = null;
			string sSOAPans = null, sCmd = null;

			DISnode = new SBODI_Server.Node();

			sCmd = @"<?xml version=""1.0"" encoding=""UTF-16""?>";
			sCmd += @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">";
			sCmd += @"<env:Body><dis:Login xmlns:dis=""http://www.sap.com/SBO/DIS"">";
			sCmd += "<DatabaseServer>" + DataBaseServer + "</DatabaseServer>";
			sCmd += "<DatabaseName>" + DataBaseName + "</DatabaseName>";
			sCmd += "<DatabaseType>" + DataBaseType + "</DatabaseType>";
			sCmd += "<DatabaseUsername>" + DataBaseUserName + "</DatabaseUsername>";
			sCmd += "<DatabasePassword>" + DataBasePassword + "</DatabasePassword>";
			sCmd += "<CompanyUsername>" + CompanyUserName + "</CompanyUsername>";
			sCmd += "<CompanyPassword>" + CompanyPassword + "</CompanyPassword>";
			sCmd += "<Language>" + Language + "</Language>";
			sCmd += "<LicenseServer>" + LicenseServer + "</LicenseServer>"; // ILTLVH25
			sCmd += "</dis:Login></env:Body></env:Envelope>";

			sSOAPans = DISnode.Interact(sCmd);

			//  Parse the SOAP answer
			System.Xml.XmlValidatingReader xmlValid = null;
			string sRet = null;
			xmlValid = new System.Xml.XmlValidatingReader(sSOAPans, System.Xml.XmlNodeType.Document, null);
			while (xmlValid.Read())
			{
				if (xmlValid.NodeType == System.Xml.XmlNodeType.Text)
				{
					if (sRet == null)
					{
						sRet += xmlValid.Value;
					}
					else
					{
						if (sRet.StartsWith("Error"))
						{
							sRet += " " + xmlValid.Value;
						}
						else
						{
							sRet = "Error " + sRet + " " + xmlValid.Value;
						}
					}
				}
			}
			if (Strings.InStr(sSOAPans, "<env:Fault>", Microsoft.VisualBasic.CompareMethod.Text) != 0)
			{
				sRet = "Error: " + sRet;
			}
			return sRet;
		}

		public static string Logout(string LoginToken)
		{
			SBODI_Server.Node DISnode = null;
			string sSOAPans = null, sCmd = null;

			DISnode = new SBODI_Server.Node();

			sCmd = @"<?xml version=""1.0"" encoding=""UTF-16""?>";
			sCmd += @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">";
			sCmd += @"	<env:Header>";
			sCmd += @"		<SessionID>" + LoginToken + "</SessionID>";
			sCmd += @"	</env:Header>";
			sCmd += @"	<env:Body>";
			sCmd += @"		<dis:Logout xmlns:dis=""http://www.sap.com/SBO/DIS"">";
			sCmd += @"		</dis:Logout>";
			sCmd += @"	</env:Body>";
			sCmd += "</env:Envelope>";

			sSOAPans = DISnode.Interact(sCmd);

			//  Parse the SOAP answer
			System.Xml.XmlValidatingReader xmlValid = null;
			string sRet = null;
			xmlValid = new System.Xml.XmlValidatingReader(sSOAPans, System.Xml.XmlNodeType.Document, null);
			while (xmlValid.Read())
			{
				if (xmlValid.NodeType == System.Xml.XmlNodeType.Text)
				{
					if (sRet == null)
					{
						sRet += xmlValid.Value;
					}
					else
					{
						if (sRet.StartsWith("Error"))
						{
							sRet += " " + xmlValid.Value;
						}
						else
						{
							sRet = "Error " + sRet + " " + xmlValid.Value;
						}
					}
				}
			}
			if (Strings.InStr(sSOAPans, "<env:Fault>", Microsoft.VisualBasic.CompareMethod.Text) != 0)
			{
				sRet = "Error: " + sRet;
			}
			return sRet;
		}

		public static System.Xml.XmlNode RemoveEmptyNodes(System.Xml.XmlNode pXmlReceive)
		{

			System.Xml.XmlNode pXmlAnswer = null;
			System.Xml.XmlNodeList pXmlNodeList = null;
			string sSelect = null;
			System.Xml.XmlNode pTempXml = null;

			pXmlAnswer = MarkEmptyNodes(pXmlReceive);

			// build the marked string
			sSelect = @"//*[text()=""";
			sSelect += sRem;
			sSelect += @"""]";

			// get node list
			pXmlNodeList = pXmlAnswer.SelectNodes(sSelect);

			// remove the marked nodes
			foreach (System.Xml.XmlNode transTemp0 in pXmlNodeList)
			{
				pTempXml = transTemp0; /* TRANSWARNING: check temp variable in foreach */
				pTempXml.ParentNode.RemoveChild(pTempXml);
			}

			// return answer
			return pXmlAnswer;

		}
		private static System.Xml.XmlNode MarkEmptyNodes(System.Xml.XmlNode pXmlReceive)
		{

			System.Xml.XmlNode pMainNode = null;
			System.Xml.XmlNode pXmlTemp = null;
			int i = 0, Removed = 0;

			pMainNode = pXmlReceive;

			i = 0;

			Removed = 0;

			// mark empty nodes
			for (i = 0; i <= pMainNode.ChildNodes.Count - 1 - Removed; i++)
			{
				pXmlTemp = pMainNode.ChildNodes[i];
				if (pXmlTemp.InnerText == "")
				{
					pXmlTemp.InnerText = sRem;
				}
				else if (pXmlTemp.HasChildNodes)
				{
					pXmlTemp = MarkEmptyNodes(pXmlTemp);
				}
			}

			// return answer
			return pMainNode;

		}

		public static System.Xml.XmlDocument AddInvoice(string SessionID, string sXmlQuotationObject)
		{
			sXmlQuotationObject = sXmlQuotationObject.Replace("&", "");


			SBODI_Server.Node pDISnode = null;
			System.Xml.XmlDocument pXmlReturn = null, pXML = null;
			string sAddCmd = null;
			System.Xml.XmlNode pNetoXML = null;
			string sResult = null;


			pXmlReturn = new System.Xml.XmlDocument();

			// get server node
			pDISnode = new SBODI_Server.Node();

			pXML = new System.Xml.XmlDocument();

			// load the string into xml document
			pXML.LoadXml(sXmlQuotationObject);

			// remove the empty nodes
			pNetoXML = (RemoveEmptyNodes(pXML));


			// build the soap string ,adding the session, the command-AddObject
			// and the XmlQuotation string
			sAddCmd = @"<?xml version=""1.0"" encoding=""UTF-16""?>" + @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">" + "<env:Header>" + "<SessionID>" + SessionID + "</SessionID>" + @"</env:Header><env:Body><dis:AddObject xmlns:dis=""http://www.sap.com/SBO/DIS"">" + pNetoXML.InnerXml + "</dis:AddObject></env:Body></env:Envelope>";



			// execute interact and return the result
			sResult = pDISnode.Interact(sAddCmd);

			// load string to xml document
			pXmlReturn.LoadXml(sResult);

			// remove the envelope string & return the result as XmlDocument
			return (RemoveEnv(pXmlReturn));

		}

		public static System.Xml.XmlDocument UpdateInvoice(string SessionID, string sXmlQuotationObject)
		{

			SBODI_Server.Node pDISnode = null;
			System.Xml.XmlDocument pXmlReturn = null, pXML = null;
			string sAddCmd = null;
			System.Xml.XmlNode pNetoXML = null;
			string sResult = null;


			pXmlReturn = new System.Xml.XmlDocument();

			// get server node
			pDISnode = new SBODI_Server.Node();

			pXML = new System.Xml.XmlDocument();

			// load the string into xml document
			pXML.LoadXml(sXmlQuotationObject);

			// remove the empty nodes
			pNetoXML = (RemoveEmptyNodes(pXML));


			// build the soap string ,adding the session, the command-AddObject
			// and the XmlQuotation string
			sAddCmd = @"<?xml version=""1.0"" encoding=""UTF-16""?>" + @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">" + "<env:Header>" + "<SessionID>" + SessionID + "</SessionID>" + @"</env:Header><env:Body><dis:UpdateObject xmlns:dis=""http://www.sap.com/SBO/DIS"">" + pNetoXML.InnerXml + "</dis:UpdateObject></env:Body></env:Envelope>";

			//sAddCmd = sAddCmd.Replace("<Version>2</Version></AdmInfo>", "</AdmInfo><QueryParams><CardCode>C20000</CardCode></QueryParams>");

			// execute interact and return the result
			sResult = pDISnode.Interact(sAddCmd);

			// load string to xml document
			pXmlReturn.LoadXml(sResult);

			// remove the envelope string & return the result as XmlDocument
			return (RemoveEnv(pXmlReturn));

		}

		public static System.Xml.XmlDocument CloseInvoice(string SessionID, string sXmlQuotationObject)
		{

			SBODI_Server.Node pDISnode = null;
			System.Xml.XmlDocument pXmlReturn = null, pXML = null;
			string sAddCmd = null;
			System.Xml.XmlNode pNetoXML = null;
			string sResult = null;


			pXmlReturn = new System.Xml.XmlDocument();

			// get server node
			pDISnode = new SBODI_Server.Node();

			pXML = new System.Xml.XmlDocument();

			// load the string into xml document
			pXML.LoadXml(sXmlQuotationObject);

			// remove the empty nodes
			pNetoXML = (RemoveEmptyNodes(pXML));


			// build the soap string ,adding the session, the command-AddObject
			// and the XmlQuotation string
			sAddCmd = @"<?xml version=""1.0"" encoding=""UTF-16""?>" + @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">" + "<env:Header>" + "<SessionID>" + SessionID + "</SessionID>" + @"</env:Header><env:Body><dis:CloseObject xmlns:dis=""http://www.sap.com/SBO/DIS"">" + pNetoXML.InnerXml + "</dis:CloseObject></env:Body></env:Envelope>";

			//sAddCmd = sAddCmd.Replace("<Version>2</Version></AdmInfo>", "</AdmInfo><QueryParams><CardCode>C20000</CardCode></QueryParams>");

			// execute interact and return the result
			sResult = pDISnode.Interact(sAddCmd);

			// load string to xml document
			pXmlReturn.LoadXml(sResult);

			// remove the envelope string & return the result as XmlDocument
			return (RemoveEnv(pXmlReturn));

		}

		public static System.Xml.XmlDocument GetUSerFields(string SessionID, string sXmlQuotationObject)
		{

			SBODI_Server.Node pDISnode = null;
			System.Xml.XmlDocument pXmlReturn = null, pXML = null;
			string sAddCmd = null;
			System.Xml.XmlNode pNetoXML = null;
			string sResult = null;


			pXmlReturn = new System.Xml.XmlDocument();

			// get server node
			pDISnode = new SBODI_Server.Node();

			pXML = new System.Xml.XmlDocument();

			// load the string into xml document
			pXML.LoadXml(sXmlQuotationObject);

			// remove the empty nodes
			pNetoXML = (RemoveEmptyNodes(pXML));


			// build the soap string ,adding the session, the command-AddObject
			// and the XmlQuotation string
			sAddCmd = @"<?xml version=""1.0"" encoding=""UTF-16""?>" + @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">" + "<env:Header>" + "<SessionID>" + SessionID + "</SessionID>" + @"</env:Header><env:Body><dis:GetByKey xmlns:dis=""http://www.sap.com/SBO/DIS"">" + pNetoXML.InnerXml + "</dis:GetByKey></env:Body></env:Envelope>";



			// execute interact and return the result
			sResult = pDISnode.Interact(sAddCmd);

			// load string to xml document
			pXmlReturn.LoadXml(sResult);

			// remove the envelope string & return the result as XmlDocument
			return (RemoveEnv(pXmlReturn));

		}

		public static System.Xml.XmlDocument GetByKey(string SessionID, string sXmlQuotationObject)
		{

			SBODI_Server.Node pDISnode = null;
			System.Xml.XmlDocument pXmlReturn = null, pXML = null;
			string sAddCmd = null;
			System.Xml.XmlNode pNetoXML = null;
			string sResult = null;


			pXmlReturn = new System.Xml.XmlDocument();

			// get server node
			pDISnode = new SBODI_Server.Node();

			pXML = new System.Xml.XmlDocument();

			// load the string into xml document
			//pXML.LoadXml(sXmlQuotationObject);

			// remove the empty nodes
			//pNetoXML = (RemoveEmptyNodes(pXML));


			// build the soap string ,adding the session, the command-AddObject
			// and the XmlQuotation string
			sAddCmd = @"<?xml version=""1.0"" encoding=""UTF-16""?>" + @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">" + "<env:Header>" + "<SessionID>" + SessionID + "</SessionID>" + @"</env:Header><env:Body><dis:GetByKey xmlns:dis=""http://www.sap.com/SBO/DIS"">" + sXmlQuotationObject + "</dis:GetByKey></env:Body></env:Envelope>";



			// execute interact and return the result
			sResult = pDISnode.Interact(sAddCmd);

			// load string to xml document
			pXmlReturn.LoadXml(sResult);

			// remove the envelope string & return the result as XmlDocument
			return (RemoveEnv(pXmlReturn));

		}
	}
}