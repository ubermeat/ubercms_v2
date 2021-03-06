/*                       ____               ____________
 *                      |    |             |            |
 *                      |    |             |    ________|
 *                      |    |             |   |
 *                      |    |             |   |    
 *                      |    |             |   |    ____
 *                      |    |             |   |   |    |
 *                      |    |_______      |   |___|    |
 *                      |            |  _  |            |
 *                      |____________| |_| |____________|
 *                        
 *      Author(s):      limpygnome (Marcus Craske)              limpygnome@gmail.com
 * 
 *      License:        Creative Commons Attribution-ShareAlike 3.0 Unported
 *                      http://creativecommons.org/licenses/by-sa/3.0/
 * 
 *      Path:           /App_Code/CMS/Base/PathInfo.cs
 * 
 *      Change-Log:
 *                      2013-09-23      Finished initial class.
 * 
 * *********************************************************************************************************************
 * Used to parse url-rewriting/request data. Plugins are invoked based on either the first directory in the URL, the
 * module-handler variable, being matched or the full-path. Thus a plugin A could own /exmaple and plugin B could own
 * /exmaple/test by having a higher priority.
 * *********************************************************************************************************************
 */
using System;
using System.Text;
using System.Web;

namespace CMS.Base
{
    /// <summary>
    /// Used to parse url-rewriting/request data. Plugins are invoked based on either the first directory in the URL,
    /// the module-handler variable, being matched or the full-path. Thus a plugin A could own /exmaple and plugin B
    /// could own /exmaple/test by having a higher priority.
    /// </summary>
	public class PathInfo
	{
		// Fields ******************************************************************************************************
		private string 		moduleHandler;          // The top-directory of the request.
		private string[] 	subDirs;                // Subsequent directories of the request, in order from 0 to n.
		private string 		fullPath;               // The full rebuilt path of the request (excludes query-string data).
		// Methods - Constructors **************************************************************************************
		public PathInfo(HttpRequest request)
		{
			parse(request, request.QueryString["path"]);
		}
        public PathInfo(HttpRequest request, string pathData)
        {
            parse(request, pathData);
        }
		// Methods *****************************************************************************************************
        /// <summary>
        /// Parses the current request for its request path.
        /// 
        /// Note: if the current path is not suitable/empty, it will be changed to the default url (refer to CMS
        /// configuration).
        /// </summary>
        /// <param name="pathData">The data for the current request path.</param>
		public void parse(HttpRequest request, string pathData)
		{
            parse(request, pathData, false);
		}
        private void parse(HttpRequest request, string pathData, bool usingDefaultUrl)
        {
            // Check against null reference
            if (pathData == null)
            {
                pathData = Core.DefaultURL;
                usingDefaultUrl = true;
            }
            // Remove starting /
            if (pathData.Length > 0 && pathData[0] == '/')
                pathData = pathData.Substring(1);
            // Check against query-string (apache/linux)
            {
                int qsi = pathData.IndexOf('?');
                if (qsi != -1)
                {
                    if (qsi > 1)
                    {
                        //string[] qsd = pathData.Substring(qsi).Split('&');
                        pathData = pathData.Substring(0, qsi);
                        //// Parse query-string data
                        //int ei;
                        //string k;
                        //foreach (string s in qsd)
                        //{
                        //    ei = s.IndexOf('=');
                        //    if (ei > 1 && ei != s.Length - 1 && request.QueryString[(k = s.Substring(0, ei))] == null)
                        //        request.QueryString[k] = s.Substring(ei + 1);
                        //}
                    }
                    else
                    {
                        pathData = Core.DefaultURL;
                        usingDefaultUrl = true;
                    }
                }
            }
            // Process tokens
            string[] exp = pathData.Split('/');
            if (exp.Length > 0)
            {
                int totaltokens = exp.Length == 0 ? 0 : exp[exp.Length - 1].Length > 0 ? exp.Length : exp.Length - 1; // Tailing slash empty token protection
                moduleHandler = totaltokens == 0 ? string.Empty : exp[0];
                subDirs = new string[totaltokens > 0 ? totaltokens - 1 : 0];
                for (int i = 1; i < totaltokens; i++)
                    subDirs[i - 1] = exp[i];
                // Check against empty paths
                if (moduleHandler.Length == 0)
                {
                    if (usingDefaultUrl)
                        throw new Exception("Invalid default URL '" + pathData + "' specified for CMS!");
                    else
                        parse(request, Core.DefaultURL, true);
                }
            }
            else
                parse(request, Core.DefaultURL, true);
            // Build full-path
            StringBuilder sb = new StringBuilder();
            sb.Append(moduleHandler).Append("/");
            foreach (string s in subDirs)
                sb.Append(s).Append("/");
            sb.Remove(sb.Length - 1, 1);
            fullPath = sb.ToString();
        }
		// Methods - Accessors *****************************************************************************************
        /// <summary>
        /// Returns debug information about the current path.
        /// </summary>
        /// <returns>Debug information about the current path.</returns>
		public string getPathInfo()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Handler: '" + moduleHandler + "' : ");
			for(int i = 0; i < subDirs.Length; i++)
			{
				sb.Append(" sub-dir [" + i + "] '" + subDirs[i] + "'");
				if(i < subDirs.Length -1)
					sb.Append(" : ");
			}
			sb.Append(" : sub-dir count: '" + subDirs.Length + "'");
			return sb.ToString();
		}
		// Methods - Properties ****************************************************************************************
        /// <summary>
        /// Gets the directory at the specified index; returns null if the directory cannot be found.
        /// 0 = module handler.
        /// 1...n = sub-dir.
        /// </summary>
        /// <param name="index">The directory index of the request.</param>
        /// <returns>The directory's name/alias.</returns>
        public string this[int index]
        {
            get
            {
                if (index > this.subDirs.Length)
                    return null;
                else if (index == 0)
                    return moduleHandler;
                else
                    return this.subDirs[index-1];
            }
        }
        /// <summary>
        /// The top-directory of the request. This is called the module-handler because some plugins can handle
        /// all requests for a top-directory, regardless of sub-directories.
        /// </summary>
		public string ModuleHandler
		{
			get
			{
				return this.moduleHandler;
			}
		}
        /// <summary>
        /// The sub-directories of the request.
        /// </summary>
		public string[] SubDirectories
		{
			get
			{
				return this.subDirs;
			}
		}
        /// <summary>
        /// The full-path of the request, excludes query-string data. This is rebuilt from the parsed data for
        /// safety.
        /// </summary>
		public string FullPath
		{
			get
			{
				return this.fullPath;
			}
		}
	}
}
