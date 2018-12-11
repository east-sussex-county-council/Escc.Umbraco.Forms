using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            ExceptionlessClient.Default.Startup();
            XmlConfigurator.Configure();

            try
            {
                if (!CheckEnvironmentPrecondition())
                {
                    return;
                }

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

                var credentials = new NetworkCredential(apiUser, apiPassword);

                // Make lots of separate requests because even one form with lots of entries can be too much work
                // to complete within the timeout of one web request.
                //
                // Run synchronously so that we don't hit the web server with requests to process every form at once.

                // Get the list of forms
                var forms = MakeRequest<IEnumerable<Guid>>(new Uri($"{baseUrl.TrimEnd('/')}/umbraco/api/UmbracoFormsRetentionApi/ListForms"), credentials, true);
                _log.Info($"{forms.Count()} forms found");

                foreach (var formId in forms)
                {
                    // Get the list of entries
                    var entries = MakeRequest<IEnumerable<Guid>>(new Uri($"{baseUrl.TrimEnd('/')}/umbraco/api/UmbracoFormsRetentionApi/ListEntries?formId={formId}"), credentials, false);
                    if (entries == null) continue;
                    _log.Info($"{entries.Count()} entries found for form '{formId}'");

                    foreach (var entryId in entries)
                    {
                        // Get the retention date for the entry
                        var retentionDate = MakeRequest<DateTime?>(new Uri($"{baseUrl.TrimEnd('/')}/umbraco/api/UmbracoFormsRetentionApi/RetentionDate?entryId={entryId}"), credentials, false);

                        // then compare it to the current date and delete the record if the retention date has passed
                        if (retentionDate.HasValue && retentionDate < DateTime.Today)
                        {
                            try
                            {
                                var deleteRequest = WebRequest.Create($"{baseUrl.TrimEnd('/')}/umbraco/api/UmbracoFormsRetentionApi/DeleteEntry?entryId={entryId}");
                                deleteRequest.Method = "DELETE";
                                deleteRequest.Credentials = credentials;
                                _log.Info($"Deleting entry '{entryId}' for form '{formId}' as its retention date '{retentionDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}' has passed.");
                                using (var deleteResponse = deleteRequest.GetResponse())
                                {
                                }
                            }
                            catch (WebException ex)
                            {
                                ex.ToExceptionless().Submit();
                                _log.Error(ex.Message);
                                continue;
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                _log.Error(ex.Message);
                throw;
            }
        }

        private static T MakeRequest<T>(Uri url, NetworkCredential credential, bool throwErrors)
        {
            try
            {
                var request = WebRequest.Create(url);
                request.Credentials = credential;
                _log.Info($"Requesting {request.RequestUri}");
                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (throwErrors)
                {
                    throw;
                }
                else
                {
                    ex.ToExceptionless().Submit();
                    _log.Error(ex.Message);
                    return default(T);
                }
            }
        }

        private static bool CheckEnvironmentPrecondition()
        {
            var precondition = ConfigurationManager.AppSettings["Precondition"];
            if (!string.IsNullOrEmpty(precondition))
            {
                var split = ConfigurationManager.AppSettings["Precondition"].Split('=');
                if (split.Length == 2)
                {
                    var result = (Environment.GetEnvironmentVariable(split[0]).Equals(split[1], StringComparison.OrdinalIgnoreCase));
                    _log.Info("Precondition " + precondition + (result ? " OK." : " failed."));
                    return result;
                }
            }
            return true;
        }
    }
}
