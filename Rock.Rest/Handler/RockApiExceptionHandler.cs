﻿// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;
using System.Web.Http.ExceptionHandling;
using Rock.Data;
using Rock.Model;

namespace Rock.Rest
{
    /// <summary>
    /// 
    /// </summary>
    public class RockApiExceptionHandler : ExceptionHandler 
    {
        /// <summary>
        /// When overridden in a derived class, handles the exception synchronously.
        /// </summary>
        /// <param name="context">The exception handler context.</param>
        public override void Handle( ExceptionHandlerContext context )
        {
             // check to see if the user is an admin, if so allow them to view the error details
            var userLogin = Rock.Model.UserLoginService.GetCurrentUser();
            GroupService service = new GroupService( new RockContext() );
            Group adminGroup = service.GetByGuid( new Guid( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS ) );
            context.RequestContext.IncludeErrorDetail = userLogin != null && adminGroup.Members.Where( m => m.PersonId == userLogin.PersonId ).Count() > 0;
            
            base.Handle( context );
        }
    }
}