﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authorization.Maui
{
    public interface IRoleAssignmentProvider
    {
        // Methods
        List<RoleAssignment> Create(ClaimsIdentity claimsIdentity);
    }
}