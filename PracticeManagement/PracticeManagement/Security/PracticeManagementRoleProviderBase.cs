using System;
using System.ServiceModel;
using System.Web.Security;
using PraticeManagement.RoleService;

namespace PraticeManagement.Security
{
    /// <summary>
    /// Provides a Roles management functionality working through the WCF Services.
    /// </summary>
    public class PracticeManagementRoleProviderBase : RoleProvider
    {
        #region Service call delegates

        private static T CallService<T>(Func<RoleServiceClient, T> func)
        {
            using (var serviceClient = new RoleServiceClient())
            {
                try
                {
                    return func(serviceClient);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        private static void CallServiceVoid(Action<RoleServiceClient> func)
        {
            using (var serviceClient = new RoleServiceClient())
            {
                try
                {
                    func(serviceClient);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        #endregion

        #region Role provider methods

        /// <summary>
        /// Adds the specified user names to the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be added to the specified roles.</param>
        /// <param name="roleNames">A string array of the role names to add the specified user names to.</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            CallServiceVoid(c => c.AddUsersToRoles(usernames, roleNames)); 
        }

        /// <summary>
        /// Gets or sets the name of the application to store and retrieve role information for.
        /// </summary>
        public override sealed string ApplicationName
        {
            get { return CallService(c => c.GetApplicationName()); }
            set { CallServiceVoid(c => c.SetApplicationName(value)); }
        }

        /// <summary>
        /// Adds a new role to the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        public override void CreateRole(string roleName)
        {
            CallServiceVoid(c => c.CreateRole(roleName));
        }

        /// <summary>
        /// Removes a role from the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to delete.</param>
        /// <param name="throwOnPopulatedRole">
        /// If true, throw an exception if roleName has one or more members and do not delete roleName.
        /// </param>
        /// <returns>true if the role was successfully deleted; otherwise, false.</returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            return CallService(c => c.DeleteRole(roleName, throwOnPopulatedRole));
        }

        /// <summary>
        /// Gets an array of user names in a role where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="roleName">The role to search in.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <returns>
        /// A string array containing the names of all the users where the user name matches usernameToMatch
        /// and the user is a member of the specified role.
        /// </returns>
        public override sealed string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            return CallService(c => c.FindUsersInRole(roleName, usernameToMatch));
        }

        /// <summary>
        /// Gets a list of all the roles for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the roles stored in the data source for the configured
        /// applicationName.
        /// </returns>
        public override sealed string[] GetAllRoles()
        {
            return CallService(c => c.GetAllRoles());
        }

        /// <summary>
        /// Gets a list of the roles that a specified user is in for the configured applicationName.
        /// </summary>
        /// <param name="username">The user to return a list of roles for.</param>
        /// <returns>
        /// A string array containing the names of all the roles that the specified user is in for the configured
        /// applicationName.
        /// </returns>
        public override string[] GetRolesForUser(string username)
        {
            return CallService(c => c.GetRolesForUser(username));
        }

        /// <summary>
        /// Gets a list of users in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to get the list of users for.</param>
        /// <returns>
        /// A string array containing the names of all the users who are members of the specified role for the
        /// configured applicationName.
        /// </returns>
        public override sealed string[] GetUsersInRole(string roleName)
        {
            return CallService(c => c.GetUsersInRole(roleName));
        }

        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role for the configured
        /// applicationName.
        /// </summary>
        /// <param name="username">The user name to search for.</param>
        /// <param name="roleName">The role to search in.</param>
        /// <returns>
        /// true if the specified user is in the specified role for the configured applicationName;
        /// otherwise, false
        /// </returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            return CallService(c => c.IsUserInRole(username, roleName));
        }

        /// <summary>
        /// Removes the specified user names from the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be removed from the specified roles.</param>
        /// <param name="roleNames">A string array of role names to remove the specified user names from.</param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            CallServiceVoid(c => c.RemoveUsersFromRoles(usernames, roleNames));
        }

        /// <summary>
        /// Gets a value indicating whether the specified role name already exists in the role data source for
        /// the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to search for in the data source.</param>
        /// <returns>
        /// true if the role name already exists in the data source for the configured applicationName;
        /// otherwise, false.
        /// </returns>
        public override sealed bool RoleExists(string roleName)
        {
            return CallService(c => c.RoleExists(roleName));
        }

        #endregion
    }
}
