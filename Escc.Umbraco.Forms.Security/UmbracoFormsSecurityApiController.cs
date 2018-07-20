using System;
using Umbraco.Web.WebApi;
using System.Web.Http;
using Umbraco.Core.Logging;

namespace Escc.Umbraco.Forms.Security
{
    /// <summary>
    /// A web API to update the security settings in Umbraco Forms
    /// </summary>
    /// <seealso cref="Umbraco.Web.WebApi.UmbracoApiController" />
    [Authorize]
    public class UmbracoFormsSecurityApiController : UmbracoApiController
    {
        /// <summary>
        /// Removes all Umbraco Forms security settings, which leaves all users with default access to every form and Manage Forms permission.
        /// </summary>
        /// <remarks>Typically used only for testing.</remarks>
        [HttpPost]
        public void ResetFormsSecurity()
        {
            try
            {
                var formsSecurity = new UmbracoFormsSecurity();
                formsSecurity.ResetFormsSecurity(Services.UserService);
            }
            catch (Exception exception)
            {
                LogHelper.Error<UmbracoFormsSecurityApiController>(exception.Message, exception);
                throw;
            }
        }


        /// <summary>
        /// Locks down Umbraco Forms security by creating a 'deny' record for every user and every form.
        /// </summary>
        /// <remarks>Typically used only for testing, or as a clean start when installing Umbraco Forms.</remarks>
        [HttpPost]
        public void LockdownFormsSecurity()
        {
            try
            {
                var formsSecurity = new UmbracoFormsSecurity();
                formsSecurity.LockdownFormsSecurity(Services.UserService);
            }
            catch (Exception exception)
            {
                LogHelper.Error<UmbracoFormsSecurityApiController>(exception.Message, exception);
                throw;
            }
        }

        /// <summary>
        /// Where a user or a form has no security settings in Umbraco Forms, add a 'deny all' record.
        /// </summary>
        /// <remarks>
        /// Where a user or a form has no security settings in Umbraco Forms, they are allowed access by default.
        /// This happens for new users and new forms. Since there is no event to watch for new users or new forms,
        /// run this method frequently to check for missing permissions and create a 'deny all' record. Where 
        /// permissions have already been set they are left unchanged.
        /// </remarks>
        [HttpPost]
        public void DenyAccessToFormsByDefault()
        {
            try
            {
                var formsSecurity = new UmbracoFormsSecurity();
                formsSecurity.DenyAccessToFormsByDefault(Services.UserService);
            }
            catch (Exception exception)
            {
                LogHelper.Error<UmbracoFormsSecurityApiController>(exception.Message, exception);
                throw;
            }
        }
    }

}
