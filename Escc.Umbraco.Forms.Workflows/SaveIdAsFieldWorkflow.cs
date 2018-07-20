using System;
using System.Collections.Generic;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Data.Storage;
using Umbraco.Core.Logging;

namespace Escc.Umbraco.Forms.Workflows
{
    /// <summary>
    /// Saves the unique ids of a form and a form entry as a field on the form, so that it can be used in locations like email where only the field data is available
    /// </summary>
    /// <seealso cref="Umbraco.Forms.Core.WorkflowType" />
    public class SaveIdAsFieldWorkflow : WorkflowType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveIdAsFieldWorkflow"/> class.
        /// </summary>
        public SaveIdAsFieldWorkflow()
        {
            this.Name = "Save form entry id";
            this.Id = new Guid("2d5a9ec0-0a1e-4ed0-b2a8-9c6bddd5fa01");
            this.Description = "Saves the unique ids of a form and a form entry as a field on the form, so that it can be used in locations like email where only the field data is available";
            this.Icon = "icon-key";
        }

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

                // Look for a field with the well-known alias "formEntryID" and insert the unique ids in it
                var updateRecord = false;
                foreach (var field in record.RecordFields)
                {
                    if (field.Value.Alias.ToUpperInvariant() == "FORMENTRYID")
                    {
                        field.Value.Values.Clear();
                        field.Value.Values.Add(e.Form.Id.ToString());
                        field.Value.Values.Add(record.UniqueId.ToString());
                        updateRecord = true;
                        break;
                    }
                }

                // Update the record with the id
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
                LogHelper.Error<SaveIdAsFieldWorkflow>(exception.Message, exception);
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
            return exceptions;
        }
    }
}