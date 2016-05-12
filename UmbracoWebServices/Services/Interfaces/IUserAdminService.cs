﻿using System.Collections.Generic;
using UmbracoWebServices.Models;

namespace UmbracoWebServices.Services.Interfaces
{
    public interface IUserAdminService
    {
        void CreateUmbracoUser(UmbracoUserModel model);

        IList<UmbracoUserModel> LookupUserByEmail(string emailaddress);

        IList<UmbracoUserModel> LookupUserByUsername(string emailaddress);

        UmbracoUserModel LookupUserById(int id);

        void ResetUsersPassword(PasswordResetModel model);

        void DisableUser(UmbracoUserModel model);

        void EnableUser(UmbracoUserModel model);

        IList<ContentTreeModel> ContentRoot();

        IList<ContentTreeModel> ContentRoot(int uid);

        IList<ContentTreeModel> ContentChild(int root);

        IList<ContentTreeModel> ContentChild(int root, int uid);

        void SetUserPagePermissions(PermissionsModel model);

        void RemoveUserPagePermissions(PermissionsModel model);

        IList<PermissionsModel> CheckUserPermissions(int userId);

        IList<PermissionsModel> CheckPagePermissions(string url);

        IList<PermissionsModel> GetPagesWithoutAuthor();

        void ClonePermissions(PermissionsModel model);

        ContentTreeModel GetPage(int pageId);
    }
}