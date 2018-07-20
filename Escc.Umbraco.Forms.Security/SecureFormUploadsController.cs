using Escc.Umbraco.Forms.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;

namespace Escc.Umbraco.Forms.Security
{
    /// <summary>
    /// Controller for displaying and downloading in the Umbraco back office files uploaded to Umbraco Forms 
    /// </summary>
    /// <seealso cref="Umbraco.Web.Mvc.UmbracoAuthorizedController" />
    public class SecureFormUploadsController : UmbracoAuthorizedController
    {
        /// <summary>
        /// Returns a file for display in the browser, checking first for access to the associated form
        /// </summary>
        /// <remarks>For images in the Umbraco Forms entries viewer. This has no Content-Disposition header.</remarks>
        /// <param name="formId">The form identifier.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult View(string formId, string fileId, string filename)
        {
            try
            {
                var security = new UmbracoFormsSecurity();
                if (!security.UserHasAccessToForm(UmbracoContext.Security.CurrentUser.Id, new Guid(formId)))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                }

                var fileSystem = FileSystemProviderManager.Current.GetUnderlyingFileSystemProvider("media");
                if (fileSystem == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }

                return ViewFile(formId, fileId, filename, fileSystem);
            }
            catch (Exception exception)
            {
                LogHelper.Error<SecureFormUploadsController>(exception.Message, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns a file for display in the browser.
        /// </summary>
        /// <param name="formId">The form identifier.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <returns></returns>
        protected virtual ActionResult ViewFile(string formId, string fileId, string filename, IFileSystem fileSystem)
        {
            return File(fileSystem.OpenFile($"forms\\upload\\form_{formId}\\{fileId}\\{filename}"), MimeMapping.GetMimeMapping(filename));
        }

        /// <summary>
        /// Returns a file for download, checking first for access to the associated form
        /// </summary>
        /// <remarks>For downloads from the Umbraco Forms entries viewer. Returns a Content-Disposition header.</remarks>
        /// <param name="formId">The form identifier.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Download(string formId, string fileId, string filename)
        {
            try
            {
                var security = new UmbracoFormsSecurity();
                if (!security.UserHasAccessToForm(UmbracoContext.Security.CurrentUser.Id, new Guid(formId)))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                }

                var fileSystem = FileSystemProviderManager.Current.GetUnderlyingFileSystemProvider("media");
                if (fileSystem == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }

                return DownloadFile(formId, fileId, filename, fileSystem);
            }
            catch (Exception exception)
            {
                LogHelper.Error<SecureFormUploadsController>(exception.Message, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns a file for download
        /// </summary>
        /// <param name="formId">The form identifier.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <returns></returns>
        protected virtual ActionResult DownloadFile(string formId, string fileId, string filename, IFileSystem fileSystem)
        {
            return File(fileSystem.OpenFile($"forms\\upload\\form_{formId}\\{fileId}\\{filename}"), MimeMapping.GetMimeMapping(filename), filename);
        }
    }
}