// <copyright file="Auth0AuthenticationProvider.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Auth0
{
    public class Auth0AuthenticationProvider : IAuth0AuthenticationProvider
    {
        public Auth0AuthenticationProvider()
        {
            OnAuthenticated = async _ => { };
            OnReturnEndpoint = async _ => { };
        }

        public Func<Auth0AuthenticatedContext, Task> OnAuthenticated { get; set; }

        public Func<Auth0ReturnEndpointContext, Task> OnReturnEndpoint { get; set; }

        public virtual Task Authenticated(Auth0AuthenticatedContext context)
        {
            return OnAuthenticated(context);
        }

        public virtual Task ReturnEndpoint(Auth0ReturnEndpointContext context)
        {
            return OnReturnEndpoint(context);
        }
    }
}
