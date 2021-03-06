﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Search;

namespace org.newpointe.PageSearch
{
    /// <summary>
    /// Searches for people with matching names
    /// </summary>
    [Description( "Page Title Search" )]
    [Export( typeof( SearchComponent ) )]
    [ExportMetadata( "ComponentName", "Page Title" )]
    [LinkedPage( "Root Page", "The page to start searching from.", false, "", "", 0 )]
    public class PageTitle : SearchComponent
    {

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "SearchLabel", "Page Title" );
                return defaults;
            }
        }

        /// <summary>
        /// The url to redirect user to after they've entered search criteria
        /// </summary>
        public override string ResultUrl
        {
            get
            {
                bool allowFirstNameSearch = GetAttributeValue( "FirstNameSearch" ).AsBooleanOrNull() ?? false;
                if ( allowFirstNameSearch )
                {
                    string url = base.ResultUrl;
                    return url + ( url.Contains( "?" ) ? "&" : "?" ) + "allowFirstNameOnly=true";
                }

                return base.ResultUrl;
            }
        }

        /// <summary>
        /// Returns a list of matching people
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchterm )
        {
            var rootPageGuid = GetAttributeValue( "RootPage" ).AsGuid();
            var terms = searchterm.Split( ' ' );

            var pageServ = new PageService( new RockContext() );
            IEnumerable<Page> pages;
            var rootPage = pageServ.Get( rootPageGuid );
            if ( rootPage != null )
            {
                pages = pageServ.GetAllDescendents( rootPage.Id );
            }
            else
            {
                pages = pageServ.Queryable();
            }

            return pages.ToList().Where( p => Regex.IsMatch( p.PageTitle, String.Join( "\\w* ", terms.Select( t => Regex.Escape( t ) ) ), RegexOptions.IgnoreCase ) ).Select( p => p.PageTitle ).AsQueryable();

        }
    }
}