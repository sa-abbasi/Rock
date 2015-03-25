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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Model;

namespace Rock.MergeTemplates
{
    /// <summary>
    /// 
    /// </summary>
    [System.ComponentModel.Description( "An HTML Document merge template" )]
    [Export( typeof( MergeTemplateType ) )]
    [ExportMetadata( "ComponentName", "HTML Document" )]
    public class HtmlMergeTemplateType : MergeTemplateType
    {
        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="mergeTemplate">The merge template.</param>
        /// <param name="mergeObjectsList">The merge objects list.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override BinaryFile CreateDocument( MergeTemplate mergeTemplate, List<Dictionary<string, object>> mergeObjectsList )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the exceptions.
        /// </summary>
        /// <value>
        /// The exceptions.
        /// </value>
        public override List<Exception> Exceptions {get; set;}
    }
}