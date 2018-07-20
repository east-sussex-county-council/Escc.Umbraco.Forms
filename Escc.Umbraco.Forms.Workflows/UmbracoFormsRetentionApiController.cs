using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core.Logging;
using Umbraco.Forms.Data.Storage;
using Umbraco.Web.WebApi;

namespace Escc.Umbraco.Forms.Workflows
{
    /// <summary>
    /// API controller to work with retention schedules for Umbraco forms
    /// </summary>
    /// <seealso cref="Umbraco.Web.WebApi.UmbracoApiController" />
    [Authorize]
    public class UmbracoFormsRetentionApiController : UmbracoApiController
    {
        /// <summary>
        /// Applies the 'after a set date' retention schedule defined in <see cref="Workflows.RetentionAfterSetDateWorkflow"/>.
        /// </summary>
        [HttpDelete]
        public void ApplySetDateRetentionSchedule()
        {
            try
            {
                LogHelper.Info<UmbracoFormsRetentionApiController>($"ApplySetDateRetentionSchedule started at {DateTime.UtcNow} (UTC).");

                // Loop through all fields on all records on all forms looking for the retention date,
                // then compare it to the current date and delete the record if the retention date has passed
                using (var formStorage = new FormStorage())
                {
                    var forms = formStorage.GetAllForms();
                    foreach (var form in forms)
                    {
                        using (var recordStorage = new RecordStorage())
                        {
                            var records = recordStorage.GetAllRecords(form);
                            foreach (var record in records)
                            {
                                foreach (var field in record.RecordFields)
                                {
                                    if (field.Value.Alias == "deleteAfter" && field.Value.HasValue() && !String.IsNullOrEmpty(field.Value.ValuesAsString()))
                                    {
                                        DateTime retentionDate;
                                        if (DateTime.TryParse(field.Value.ValuesAsString(), out retentionDate))
                                        {
                                            if (retentionDate < DateTime.Today)
                                            {
                                                LogHelper.Info<UmbracoFormsRetentionApiController>($"Deleting record '{record.UniqueId}' for form '{form.Name}' with Id '{form.Id}' as its retention date '{field.Value.ValuesAsString()}' has passed.");
                                                recordStorage.DeleteRecord(record, form);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                LogHelper.Info<UmbracoFormsRetentionApiController>($"ApplyRetentionSchedules completed at {DateTime.UtcNow} (UTC).");
            }
            catch (Exception exception)
            {
                LogHelper.Error<UmbracoFormsRetentionApiController>(exception.Message, exception);
                throw;
            }
        }
    }
}