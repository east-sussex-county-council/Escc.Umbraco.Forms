using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Forms.Core;
using Umbraco.Forms.Data.Storage;

namespace Escc.Umbraco.Forms.Security
{
    /// <summary>
    /// Query and update security settings in Umbraco Forms
    /// </summary>
    /// <remarks>Part of workarounds for http://issues.umbraco.org/issue/CON-1022 and http://issues.umbraco.org/issue/CON-1454 </remarks>
    public class UmbracoFormsSecurity
    {
        /// <summary>
        /// Checks whether a back office User has access to a particular form.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="formId">The form identifier.</param>
        /// <returns></returns>
        public bool UserHasAccessToForm(int userId, Guid formId)
        {
            using (FormStorage formStorage = new FormStorage())
            {
                using (UserFormSecurityStorage formSecurityStorage = new UserFormSecurityStorage())
                {
                    var form = formStorage.GetForm(formId);
                    var formSecurityForUser = formSecurityStorage.GetUserFormSecurity(userId, formId).FirstOrDefault();
                    if (formSecurityForUser == null || !formSecurityForUser.HasAccess) { return false; }
                }
            }
            return true;
        }

        /// <summary>
        /// Resets forms security, which removes all permissions and gives everyone access to every form
        /// </summary>
        /// <param name="userService">The user service.</param>
        /// <exception cref="ArgumentNullException">userService</exception>
        public void ResetFormsSecurity(IUserService userService)
        {
            if (userService == null)
            {
                throw new ArgumentNullException(nameof(userService));
            }

            // For every Umbraco User including disabled accounts, remove their Umbraco Forms permissions (both deny and allow).
            // This actually grants everyone Manage Forms permission because the default is to allow everyone.
            var page = 0;
            var total = 0;
            var users = userService.GetAll(page, 10, out total);
            while (users.Any())
            {
                foreach (var user in users)
                {
                    using (UserSecurityStorage userSecurityStorage = new UserSecurityStorage())
                    {
                        var userFormSecurityList = userSecurityStorage.GetUserSecurity(user.Id.ToString());
                        foreach (var userSecurity in userFormSecurityList) userSecurityStorage.DeleteUserSecurity(userSecurity);
                    }
                }

                page++;
                users = userService.GetAll(page, 10, out total);
            }

            // For every form in Umbraco Forms, remove all the user permissions (both deny and allow).
            // This actually grants everyone access to every form because the default is to allow everyone.
            using (FormStorage formStorage = new FormStorage())
            {
                using (UserFormSecurityStorage formSecurityStorage = new UserFormSecurityStorage())
                {
                    IEnumerable<Form> allForms = formStorage.GetAllForms();
                    foreach (Form form in allForms)
                    {
                        formSecurityStorage.DeleteAllUserFormSecurityForForm(form.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of users granted access to an Umbraco Form.
        /// </summary>
        /// <param name="userService">The user service.</param>
        /// <param name="formId">The form identifier.</param>
        /// <returns></returns>
        public FormSecurity PermissionsToForm(IUserService userService, Guid formId)
        {
            if (userService == null)
            {
                throw new ArgumentNullException(nameof(userService));
            }

            var formSecurity = new FormSecurity();
            using (FormStorage formStorage = new FormStorage())
            {
                var form = formStorage.GetForm(formId);
                if (form != null) formSecurity.FormName = form.Name;
            }

            using (UserFormSecurityStorage formSecurityStorage = new UserFormSecurityStorage())
            {
                var permissions = formSecurityStorage.GetUserFormSecurityForAllUsers(formId).Where<UserFormSecurity>(permission => permission.HasAccess == true);
                foreach (var permission in permissions)
                {
                    var userId = Int32.Parse(permission.User, CultureInfo.InvariantCulture);
                    formSecurity.Users.Add(new FormUser()
                    {
                        UserId = userId,
                        UserDisplayName = userService.GetUserById(userId).Name
                    });
                }
            }

            return formSecurity;
        }

        /// <summary>
        /// Locks down Umbraco Forms security by creating a 'deny' record for every user and every form.
        /// </summary>
        /// <param name="userService">The user service.</param>
        /// <exception cref="ArgumentNullException">userService</exception>
        public void LockdownFormsSecurity(IUserService userService)
        {
            if (userService == null)
            {
                throw new ArgumentNullException(nameof(userService));
            }

            var page = 0;
            var total = 0;
            var users = userService.GetAll(page, 10, out total);
            while (users.Any())
            {
                foreach (var user in users)
                {
                    RemoveManageFormsPermissions(user.Id, true);
                    RemoveDefaultAccessToForms(user.Id, true);
                }

                page++;
                users = userService.GetAll(page, 10, out total);
            }
        }

        /// <summary>
        /// Where a user or a form has no security settings in Umbraco Forms, add a 'deny all' record.
        /// </summary>
        /// <param name="userService">The user service.</param>
        /// <exception cref="ArgumentNullException">userService</exception>
        public IEnumerable<int> GetAllUserIds(IUserService userService)
        {
            if (userService == null)
            {
                throw new ArgumentNullException(nameof(userService));
            }

            var userIds = new List<int>();

            var page = 0;
            var total = 0;
            var users = userService.GetAll(page, 10, out total);
            while (users.Any())
            {
                userIds.AddRange(users.Select(user => user.Id));

                page++;
                users = userService.GetAll(page, 10, out total);
            }

            return userIds;
        }

        /// <summary>
        /// Each Umbraco User should have an Umbraco Forms permissions record which holds their overall permissions for Umbraco Forms.
        /// This preserves existing permissions and adds a 'deny all' permission if there is no record.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="forEveryone">if set to <c>true</c> overwrite all existing permissions with 'deny all'.</param>
        public void RemoveManageFormsPermissions(int userId, bool forEveryone)
        {
            using (UserSecurityStorage userSecurityStorage = new UserSecurityStorage())
            {
                var userSecurity = userSecurityStorage.GetUserSecurity(userId.ToString()).FirstOrDefault();
                var hasSecurityAlready = (userSecurity != null);
                if (!hasSecurityAlready)
                {
                    userSecurity = UserSecurity.Create();
                    userSecurity.User = userId.ToString();
                }
                userSecurity.ManageForms = false;
                userSecurity.ManageDataSources = false;
                userSecurity.ManagePreValueSources = false;
                userSecurity.ManageWorkflows = false;

                if (!hasSecurityAlready)
                {
                    userSecurityStorage.InsertUserSecurity(userSecurity);
                }
                else if (forEveryone)
                {
                    userSecurityStorage.UpdateUserSecurity(userSecurity);
                }
            }
        }

        /// <summary>
        /// Each Umbraco User should have an Umbraco Forms permissions record for each form.
        /// This preserves existing permissions and adds a 'deny' permission if there is no record.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="forEveryone">if set to <c>true</c> overwrite all existing permissions with 'deny'.</param>
        public void RemoveDefaultAccessToForms(int userId, bool forEveryone)
        {
            using (FormStorage formStorage = new FormStorage())
            {
                using (UserFormSecurityStorage formSecurityStorage = new UserFormSecurityStorage())
                {
                    IEnumerable<Form> allForms = formStorage.GetAllForms();
                    foreach (Form form in allForms)
                    {
                        var formSecurityForUser = formSecurityStorage.GetUserFormSecurity(userId, form.Id).FirstOrDefault();
                        var hasSecurityAlready = (formSecurityForUser != null);
                        if (!hasSecurityAlready)
                        {
                            formSecurityForUser = UserFormSecurity.Create();
                            formSecurityForUser.User = userId.ToString();
                            formSecurityForUser.Form = form.Id;
                        }
                        formSecurityForUser.HasAccess = false;

                        if (!hasSecurityAlready)
                        {
                            formSecurityStorage.InsertUserFormSecurity(formSecurityForUser);
                        }
                        else if (forEveryone)
                        {
                            formSecurityStorage.UpdateUserFormSecurity(formSecurityForUser);
                        }
                    }
                }
            }
        }
    }
}