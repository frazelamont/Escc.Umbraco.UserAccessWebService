﻿using System;
using System.Collections.Generic;
using System.Linq;
using ESCC.Umbraco.UserAccessWebService.Models;
using ESCC.Umbraco.UserAccessWebService.Services.Interfaces;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace ESCC.Umbraco.UserAccessWebService.Services
{
    public class ExpiringPagesService : IExpiringPagesService
    {
        private readonly IContentService _contentService;
        private readonly IUserService _userService;

        public ExpiringPagesService(IUserService userService, IContentService contentService)
        {
            _userService = userService;
            _contentService = contentService;
        }

        /// <summary>
        /// Get a list of expiring pages, collated by User
        /// </summary>
        /// <param name="noOfDaysFrom">
        /// How many days to look forward
        /// </param>
        /// <returns>
        /// List Users with expiring pages they are responsible for
        /// </returns>
        public IList<UserPagesModel> GetExpiringNodesByUser(int noOfDaysFrom)
        {
            GetConfigSettings();

            // Get pages that expire within the declared period, order by ?
            var home = _contentService.GetRootContent().First();

            //TODO: Do we need to check that unpublished pages are not selected?
            var expiringNodes = home.Descendants().Where(nn => nn.ExpireDate > DateTime.Now && nn.ExpireDate < DateTime.Now.AddDays(noOfDaysFrom)).OrderBy(nn => nn.ExpireDate);

            // For each page:
            IList<UserPagesModel> userPages = new List<UserPagesModel>();

            // Create a WebStaff record. Use -1 as an Id as there won't be a valid Umbraco user with that value.
            var webStaff = new UserPagesModel();
            var webstaffUser = new UmbracoUserModel
            {
                UserId = -1,
                UserName = "webstaff",
                FullName = "Web Staff",
                EmailAddress = "webstaff@eastsussex.gov.uk"
            };
            webStaff.User = webstaffUser;
            userPages.Add(webStaff);

            var helper = new UmbracoHelper();

            foreach (IContent expiringNode in expiringNodes)
            {
                if (expiringNode.ExpireDate == null) continue;

                //   Get Web Authors with permission
                var perms = _contentService.GetPermissionsForEntity(expiringNode);

                var nodeAuthors = perms as IList<EntityPermission> ?? perms.ToList();

                // if no Web Authors, add this page to the WebStaff list
                if (!nodeAuthors.Any())
                {
                    var userPage = new UserPageModel
                    {
                        PageId = expiringNode.Id,
                        PageName = expiringNode.Name,
                        PagePath = expiringNode.Path,
                        PageUrl = helper.Url(expiringNode.Id),
                        ExpiryDate = (DateTime)expiringNode.ExpireDate
                    };

                    userPages.Where(p => p.User.UserId == -1).ForEach(u => u.Pages.Add(userPage));
                    continue;
                }

                // Add the current page to each user
                foreach (var author in nodeAuthors)
                {
                    var userPage = new UserPageModel
                    {
                        PageId = expiringNode.Id,
                        PageName = expiringNode.Name,
                        PagePath = expiringNode.Path,
                        PageUrl = helper.Url(expiringNode.Id),
                        ExpiryDate = (DateTime)expiringNode.ExpireDate
                    };

                    var user = userPages.FirstOrDefault(f => f.User.UserId == author.UserId);
                    if (user == null)
                    {
                        var pUser = _userService.GetUserById(author.UserId);
                        var p = new UmbracoUserModel
                        {
                            UserId = author.UserId,
                            UserName = pUser.Username,
                            FullName = pUser.Name,
                            EmailAddress = pUser.Email
                        };

                        user = new UserPagesModel {User = p};
                        userPages.Add(user);
                    }


                    userPages.Where(p => p.User.UserId == user.User.UserId).ForEach(u => u.Pages.Add(userPage));
                }
            }

            // Return a list of users to email, along with the page details
            return userPages;
        }

        private void GetConfigSettings()
        {
        }
    }
}