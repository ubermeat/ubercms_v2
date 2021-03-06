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
 *      Path:           /App_Code/CMS/Base/Data.cs
 * 
 *      Change-Log:
 *                      2013-09-23      Finished initial class.
 * 
 * *********************************************************************************************************************
 * Used for passing data between the controller and plugins; this may also be used for non-request exchange of
 * infomration.
 * *********************************************************************************************************************
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using CMS.Base;
using UberLib.Connector;

namespace CMS.Base
{
    /// <summary>
    /// Used for passing data between the controller and plugins; this may also be used for non-request exchange of
    /// infomration.
    /// </summary>
	public class Data
	{
		// Fields  *****************************************************************************************************
        private bool                            outputContent;      // Indicates if the content should be written to the client.
		private PathInfo 						pathInfo;			// Information about the request path.
		private HttpRequest 					request;			// ASP.NET request object.
		private HttpResponse 					response;			// ASP.NET response object.
		private Stopwatch						stopwatch;			// Used to measure the current request's process speed.
		private Dictionary<string, string>		variables;			// Used to store request variables for template rendering.
		private Connector						connector;			// The database connector.
		// Methods - Constructors **************************************************************************************
        /// <summary>
        /// Creates a new Data model. This will create a new connector if both the request and response parameters are
        /// not null, same for the underlying measurement of the request speed. Path data will not be parsed if also
        /// null.
        /// </summary>
        /// <param name="request">ASP.NET request object.</param>
        /// <param name="response">ASP.NET response object.</param>
		public Data(HttpRequest request, HttpResponse response)
		{
            this.pathInfo = request == null ? null : new PathInfo(request);
			this.request = request;
			this.response = response;
			this.variables = new Dictionary<string, string>();
            this.outputContent = true;
            if (request != null && response != null)
            {
                this.connector = Core.connectorCreate(false);
                this.stopwatch = new Stopwatch();
            }
		}
		// Methods *****************************************************************************************************
        /// <summary>
        /// Starts timing, for recording the total time taken to serve the request.
        /// </summary>
		public void timingStart()
		{
			stopwatch.Start();
		}
        /// <summary>
        /// Stops timing the time taken for the current request.
        /// </summary>
		public void timingEnd()
		{
			stopwatch.Stop();
            variables["BenchMark_ms"] = stopwatch.ElapsedMilliseconds.ToString();
            variables["BenchMark_s"] = ((float)stopwatch.ElapsedMilliseconds / 1000.0f).ToString();
		}
        /// <summary>
        /// Disposes the resources used by this class.
        /// </summary>
		public void dispose()
		{
			connector.disconnect();
		}
        // Methods - Mutators ******************************************************************************************
        /// <summary>
        /// Sets a variable's key to null; if the variable already exists, the value is unchanged. This method is useful
        /// for setting flags/boolean values for the templating system.
        /// </summary>
        /// <param name="key">The key to set.</param>
        public void setFlag(string key)
        {
            if(!variables.ContainsKey(key))
                variables[key] = null;
        }
		// Methods - Accessors *****************************************************************************************
		/// <summary>
		/// Indicates if a variable has been defined.
		/// </summary>
		/// <returns>True if defined, false if not defined.</returns>
		/// <param name="key">Key.</param>
		public bool isKeySet(string key)
		{
			return variables.ContainsKey(key);
		}
		// Methods - Properties ****************************************************************************************
        /// <summary>
        /// Set/get a variable (stored only whilst processing the current request).
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The data for the specified key.</returns>
		public string this[string key]
		{
			get
			{
                return variables.ContainsKey(key) ? variables[key] : "";
			}
			set
			{
                variables[key] = value;
			}
		}
        /// <summary>
        /// The collection of variables for the current request.
        /// </summary>
        public Dictionary<string, string> Variables
        {
            get
            {
                return variables;
            }
        }
        /// <summary>
        /// Set/get if to output content for the current request; true = write content to client, false = do not write any data to
        /// the client. Useful for plugins serving files.
        /// </summary>
        public bool OutputContent
        {
            get
            {
                return outputContent;
            }
            set
            {
                outputContent = value;
            }
        }
        /// <summary>
        /// The ASP.NET request object to represent the request data of the current request.
        /// </summary>
		public HttpRequest Request
		{
			get
			{
				return request;
			}
		}
        /// <summary>
        /// The ASP.NET response object to represent the response actions and data for the current request.
        /// </summary>
		public HttpResponse Response
		{
			get
			{
				return response;
			}
		}
        /// <summary>
        /// Information about the parsed path for the current request.
        /// </summary>
		public PathInfo PathInfo
		{
			get
			{
				return pathInfo;
			}
		}
        /// <summary>
        /// The database connector used for database interaction; this should be used by code to serve
        /// content for the current request only! Core and non-request methods of plugins should use the
        /// Core.Connector object (which is a persistent connection).
        /// </summary>
		public Connector Connector
		{
			get
			{
				return connector;
			}
            set
            {
                connector = value;
            }
		}
	}
}