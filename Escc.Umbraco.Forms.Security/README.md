# Escc.Umbraco.Forms.Security

## Secure forms by default

Unfortunately Umbraco Forms grants access to all forms for new users, and to all users for new forms. The default permission set should be 'deny' in both cases, and `UmbracoFormsSecurityApiController` exists to create a 'deny' record wherever one is missing. Run a scheduled task frequently to call `https://hostname/umbraco/api/UmbracoFormsSecurityApi/DenyAccessToFormsByDefault` using the HTTP `POST` method. It will pick up and fix new users and new forms as they are created.

This issue is logged with Umbraco as [CON-1022](http://issues.umbraco.org/issue/CON-1022).

When you create a new form you will need to grant access to the form to anyone who needs to view the data for processing (assuming the default workflow is used which stores the data in Umbraco). To grant access: 

1. Visit the Users section in Umbraco. 
2. Expand the 'Users' tree, find the user who needs access, and ensure that they have access to the Forms section. If the user does not edit content on the website, this should be the only section selected. 
3. Next expand 'Forms Security', find the user who needs access again, and tick 'Has Access' next to the form they need to view. 

## Find out who has access to an existing form

Umbraco Forms shows you what forms a user has access to, but not which users have access to a form. `FormPermissionsEventHandler` adds a 'Permissions' menu item to each form which displays a list of the users who have access, and links each user's name to the edit view for their Umbraco Forms permissions.

This dialog is loaded using a route configured in `FormPermissionsEventHandler` to go to `FormPermissionsController`, which loads the `~\Views\Partials\Forms\Permissions.cshtml` view. 

## Securing uploads

Umbraco Forms uploads files to the same `IFileSystem` as items in the media gallery, which means they are publicly available and insecure even though their contents may be sensitive.

Re-routing forms uploads to a secure location without affecting ordinary media files requires a customised `IFileSystem`. `PhysicalFileSystemWithSecureFormsUploads` is a copy of Umbraco's default `PhysicalFileSystem` with this re-routing added. Umbraco Forms locations are sent to a different folder, which can be locked down to a specific account with `Modify` permissions that is configured in `FileSystemProviders.config`. 

The only way to access the secured file for anyone without those account details is to sign in to the Umbraco back office and view the form entries. `SecureFormUploadsEventHandler` sets up the route to `SecureFormUploadsController`, and a customised version of `~\App_Plugins\UmbracoForms\Backoffice\Common\RenderTypes\file.html` ensures the back office entries viewer links to uploads using that route. `SecureFormUploadsController` checks first that the current Umbraco User has access to the relevant form in their Umbraco Forms permissions before returning a file. (It's not possible to re-route the standard URL because it must start with `/umbraco/backoffice/` to be authenticated, and the default URL starts with `/media/`.) 

`FileSystemProviders.config` is updated to enable support for secure uploads. You may need to update it with credentials to access your secured folder. 

	<FileSystemProviders>
	  <Provider alias="media" type="Escc.Umbraco.Forms.Security.PhysicalFileSystemWithSecureFormsUploads, Escc.Umbraco.Forms.Security">
	    <Parameters>
	      <add key="virtualRoot" value="~/media/" />
	      <add key="formUploadsVirtualRoot" value="~/App_Data/UmbracoForms/Uploads/" />

		  <!-- Optional: Credentials to access the secured folder for Umbraco Forms uploads -->
	      <add key="formUploadsDomain" value="" />
	      <add key="formUploadsUser" value="" />
	      <add key="formUploadsPassword" value="" />
	    </Parameters>
	  </Provider> 
	</FileSystemProviders>


This issue is logged with Umbraco as [CON-1454](http://issues.umbraco.org/issue/CON-1454).