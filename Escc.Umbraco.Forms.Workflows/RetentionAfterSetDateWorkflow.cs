using System;
using System.Collections.Generic;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Data.Storage;
using Umbraco.Forms.Core.Attributes;
using System.Linq;
using Umbraco.Core.Logging;
using System.Globalization;

namespace Escc.Umbraco.Forms.Workflows
{
    /// <summary>
    /// An Umbraco Forms workflow which sets a date in a "deleteAfter" field, indicating to a retention schedule process when the record can be deleted
    /// </summary>
    /// <seealso cref="Umbraco.Forms.Core.WorkflowType" />
    public class RetentionAfterSetDateWorkflow : WorkflowType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetentionAfterSetDateWorkflow"/> class.
        /// </summary>
        public RetentionAfterSetDateWorkflow()
        {
            this.Name = "Retention schedule: after a set date";
            this.Id = new Guid("82311c7d-351b-4a50-8924-0a919dc806fd");
            this.Description = "Records will be deleted after a set date interval specified in days, weeks, months and years";
            this.Icon = "icon-timer";
            this.Group = "Retention";
        }

        [Setting("Days", description = "Delete the record after this many days (added to weeks, months and years)", view = "TextField")]
        public string Days { get; set; }

        [Setting("Weeks", description = "Delete the record after this many weeks (added to days, months and years)", view = "TextField")]
        public string Weeks { get; set; }

        [Setting("Months", description = "Delete the record after this many months (added to days, weeks and years)", view = "TextField")]
        public string Months { get; set; }

        [Setting("Years", description = "Delete the record after this many years (added to days, weeks and months)", view = "TextField")]
        public string Years { get; set; }

        /// <summary>
        /// Called bu Umbraco Forms to execute the workflow.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="e">The <see cref="RecordEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        public override WorkflowExecutionStatus Execute(Record record, RecordEventArgs e)
        {
            try
            {
                if (record == null)
                {
                    return WorkflowExecutionStatus.Failed;
                }

                if (e == null)
                {
                    return WorkflowExecutionStatus.Failed;
                }

                List<Exception> settingsErrors = this.ValidateSettings();
                if (settingsErrors != null && Enumerable.Any<Exception>(settingsErrors))
                {
                    foreach (Exception exception in settingsErrors)
                    {
                        LogHelper.Error<RetentionAfterSetDateWorkflow>(exception.Message, exception);
                    }
                    return WorkflowExecutionStatus.Failed;
                }

                var days = 0;
                int.TryParse(Days, out days);

                var weeks = 0;
                int.TryParse(Weeks, out weeks);

                var months = 0;
                int.TryParse(Months, out months);

                var years = 0;
                int.TryParse(Years, out years);

                // Look for a field with the well-known alias "deleteAfter" and insert a retention date in it
                var updateRecord = false;
                foreach (var field in record.RecordFields)
                {
                    if (field.Value.Alias.ToUpperInvariant() == "DELETEAFTER")
                    {
                        field.Value.Values.Clear();
                        field.Value.Values.Add(DateTime.Today.AddDays(days).AddDays(weeks * 7).AddMonths(months).AddYears(years).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                        updateRecord = true;
                        break;
                    }

                }

                // Update the record with the retention date
               if (updateRecord)
                {
                    using (RecordStorage recordStorage = new RecordStorage())
                    {
                        // (note recordStorage.UpdateRecord() doesn't work for the first workflow - see http://issues.umbraco.org/issue/CON-1482)
                        record.RecordData = record.GenerateRecordDataAsJson();
                        if (record.Id > 0)
                        {
                            record = recordStorage.UpdateRecord(record, e.Form);
                        }
                        else
                        {
                            record = recordStorage.InsertRecord(record, e.Form);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LogHelper.Error<RetentionAfterSetDateWorkflow>(exception.Message, exception);
                return WorkflowExecutionStatus.Failed;
            }
            return WorkflowExecutionStatus.Completed;
        }

        /// <summary>
        /// Called by Umbraco Forms to validate the user-specified settings.
        /// </summary>
        /// <returns></returns>
        public override List<Exception> ValidateSettings()
        {
            var exceptions = new List<Exception>();

            var days = 0;
            if (!String.IsNullOrEmpty(Days) && (!int.TryParse(Days, out days) || days < 0))
            {
                exceptions.Add(new FormatException($"{Days} is not a valid value for days"));
            }

            var weeks = 0;
            if (!String.IsNullOrEmpty(Weeks) && (!int.TryParse(Weeks, out weeks) || weeks < 0))
            {
                exceptions.Add(new FormatException($"{Weeks} is not a valid value for weeks"));
            }

            var months = 0;
            if (!String.IsNullOrEmpty(Months) && (!int.TryParse(Months, out months) || months < 0))
            {
                exceptions.Add(new FormatException($"{Months} is not a valid value for months"));
            }

            var years = 0;
            if (!String.IsNullOrEmpty(Years) && (!int.TryParse(Years, out years) || years < 0))
            {
                exceptions.Add(new FormatException($"{Years} is not a valid value for years"));
            }

            return exceptions;
        }
    }
}