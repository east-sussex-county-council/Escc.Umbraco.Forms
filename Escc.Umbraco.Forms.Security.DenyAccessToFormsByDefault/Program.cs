using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Exceptionless;
using Exceptionless.Json;
using log4net;
using log4net.Config;

namespace Escc.Umbraco.Forms.Security.DenyAccessToFormsByDefault
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            ExceptionlessClient.Default.Startup();
            XmlConfigurator.Configure();

            try
            {
                var baseUrl = ConfigurationManager.AppSettings["UmbracoBaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new ConfigurationErrorsException("appSettings > UmbracoBaseUrl was not set in app.config");
                }

                var apiUser = ConfigurationManager.AppSettings["ApiUser"];
                if (string.IsNullOrEmpty(apiUser))
                {
                    throw new ConfigurationErrorsException("appSettings > ApiUser was not set in app.config");
                }

                var apiPassword = ConfigurationManager.AppSettings["ApiPassword"];
                if (string.IsNullOrEmpty(apiPassword))
                {
                    throw new ConfigurationErrorsException("appSettings > ApiPassword was not set in app.config");
                }

                var request = WebRequest.Create($"{baseUrl.TrimEnd('/')}/umbraco/api/UmbracoFormsSecurityApi/ListUserIds");
                request.Credentials = new NetworkCredential(apiUser, apiPassword);
                log.Info($"Requesting {request.RequestUri}");
                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            // Get the list of users
                            var userIds = JsonConvert.DeserializeObject<IEnumerable<int>>(reader.ReadToEnd());
                            log.Info($"{userIds.Count()} users found");

                            // Make a separate DELETE request for each user. This is more scaleable that doing everything in one web request,
                            // since each web request is limited to the work necessary to process forms for one user.

                            // Run synchronously so that we don't hit the web server with requests to process every user at once.
                            foreach (var userId in userIds)
                            {
                                var postRequest = WebRequest.Create($"{baseUrl.TrimEnd('/')}/umbraco/api/UmbracoFormsSecurityApi/DenyAccessToFormsByDefault?userId={userId}");
                                postRequest.Method = "POST";
                                postRequest.Credentials = new NetworkCredential(apiUser, apiPassword);
                                postRequest.ContentLength = 0;
                                log.Info($"Requesting {postRequest.RequestUri}");
                                using (var postResponse = postRequest.GetResponse())
                                {
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                log.Error(ex.Message);
                throw;
            }
        }
    }
}
