using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Exceptionless;
using log4net;
using log4net.Config;
using Newtonsoft.Json;

namespace Escc.Umbraco.Forms.Workflows.ApplyRetentionSchedule
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

                var request = WebRequest.Create($"{baseUrl.TrimEnd('/')}/umbraco/api/UmbracoFormsRetentionApi/ListForms");
                request.Credentials = new NetworkCredential(apiUser, apiPassword);
                log.Info($"Requesting {request.RequestUri}");
                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            // Get the list of forms
                            var forms = JsonConvert.DeserializeObject<IEnumerable<Guid>>(reader.ReadToEnd());
                            log.Info($"{forms.Count()} forms found");

                            // Make a separate DELETE request for each form. This is more scaleable that doing everything in one web request,
                            // since each web request is limited to the work necessary to process entries for one form.
                            
                            // Run synchronously so that we don't hit the web server with requests to process every form at once.
                            foreach (var formId in forms)
                            {
                                var deleteRequest = WebRequest.Create($"{baseUrl.TrimEnd('/')}/umbraco/api/UmbracoFormsRetentionApi/ApplySetDateRetentionSchedule?formId={formId}");
                                deleteRequest.Method = "DELETE";
                                deleteRequest.Credentials = new NetworkCredential(apiUser, apiPassword);
                                log.Info($"Requesting {deleteRequest.RequestUri}");
                                using (var deleteResponse = deleteRequest.GetResponse())
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
