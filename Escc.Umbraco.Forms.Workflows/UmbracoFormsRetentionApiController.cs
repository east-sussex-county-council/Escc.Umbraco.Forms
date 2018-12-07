using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core.Logging;
using Umbraco.Forms.Core;
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
        /// Gets a list of all forms
        /// </summary>
        [HttpGet]
        public IEnumerable<Guid> ListForms()
        {
            try
            {
                using (var formStorage = new FormStorage())
                {
                    return formStorage.GetAllForms().Select(form => form.Id);
                }
            }
            catch (Exception exception)
            {
                LogHelper.Error<UmbracoFormsRetentionApiController>(exception.Message, exception);
                throw;
            }
        }

        /// <summary>
        /// Gets a list of all entries for a form
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Guid> ListEntries(Guid formId)
        {
            try
            {
                using (var formStorage = new FormStorage())
                {
                    var form = formStorage.GetForm(formId);
                    if (form == null) return new Guid[0];
                    using (var recordStorage = new RecordStorage())
                    {
                        return recordStorage.GetAllRecords(form, false).Select(record => record.UniqueId);
                    }
                }
            }
            catch (Exception exception)
            {
                LogHelper.Error<UmbracoFormsRetentionApiController>(exception.Message, exception);
                throw;
            }
        }

        /// <summary>
        /// Gets the retention date, if any, for a given form entry
        /// </summary>
        /// <param name="entryId"></param>
        /// <returns></returns>
        [HttpGet]
        public DateTime? RetentionDate(Guid entryId)
        {
            try
            {
                using (var recordStorage = new RecordStorage())
                {
                    var record = recordStorage.GetRecordByUniqueId(entryId);
                    if (record == null) return null;

                    foreach (var field in record.RecordFields)
                    {
                        if (field.Value.Alias == "deleteAfter")
                        {
                            if (field.Value.HasValue() && !String.IsNullOrEmpty(field.Value.ValuesAsString()))
                            {
                                DateTime retentionDate;
                                if (DateTime.TryParse(field.Value.ValuesAsString(), out retentionDate))
                                {
                                    return retentionDate;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LogHelper.Error<UmbracoFormsRetentionApiController>(exception.Message, exception);
                throw;
            }
            return null;
        }

        /// <summary>
        /// Deletes a form entry
        /// </summary>
        [HttpDelete]
        public void DeleteEntry(Guid entryId)
        {
            try
            {
                using (var recordStorage = new RecordStorage())
                {
                    var record = recordStorage.GetRecordByUniqueId(entryId);
                    if (record == null) return;

                    var form = record.GetForm();
                    if (form == null) return;

                    LogHelper.Info<UmbracoFormsRetentionApiController>($"Deleting record '{record.UniqueId}' for form '{form.Name}' with id '{form.Id}'.");
                    recordStorage.DeleteRecord(record, form);
                }
            }
            catch (Exception exception)
            {
                LogHelper.Error<UmbracoFormsRetentionApiController>(exception.Message, exception);
                throw;
            }
        }
    }
}