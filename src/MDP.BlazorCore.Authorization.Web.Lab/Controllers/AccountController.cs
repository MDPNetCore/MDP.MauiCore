﻿using MDP.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authorization.Web.Lab
{
    public partial class AccountController : Controller
    {
        // Methods
        [AllowAnonymous]
        public ActionResult Login()
        {
            // Return
            return this.View();
        }

        [AllowAnonymous]
        public Task<ActionResult> Logout(string returnUrl = null)
        {
            // Return
            return this.LogoutAsync(returnUrl);
        }

        [AllowAnonymous]
        public Task<ActionResult> Refresh(string returnUrl = null)
        {
            // Return
            return this.RefreshAsync(returnUrl);
        }

        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            // Return
            return this.View();
        }
    }

    public partial class AccountController : Controller
    {
        // Methods

        [AllowAnonymous]
        public async Task<ActionResult> LoginByPassword(string username, string password = null, string returnUrl = null)
        {
            #region Contracts

            if (string.IsNullOrEmpty(username) == true) throw new ArgumentException($"{nameof(username)}=null");

            #endregion

            // ClaimsIdentity
            ClaimsIdentity claimsIdentity = null;

            // Clark(Admin) 
            if (claimsIdentity == null && username == "Clark")
            {
                claimsIdentity = new ClaimsIdentity(authenticationType: "Password", claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, $"{username}"),
                    new Claim(ClaimTypes.Email, $"{username}@example.com"),
                    new Claim(ClaimTypes.Role, "Admin")
                });
            }

            // Other(User) 
            if (claimsIdentity == null)
            {
                claimsIdentity = new ClaimsIdentity(authenticationType: "Password", claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, $"{username}"),
                    new Claim(ClaimTypes.Email, $"{username}@example.com"),
                    new Claim(ClaimTypes.Role, "User")
                });
            }

            // Return
            return await this.SignInAsync(claimsIdentity, returnUrl);
        }
    }
}
