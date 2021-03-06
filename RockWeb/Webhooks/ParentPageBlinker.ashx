﻿<%@ WebHandler Language="C#" Class="ParentPageBlinker" %>

using System;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Communication;
using System.Diagnostics;
using System.Globalization;

public class ParentPageBlinker : IHttpHandler
{

    public void ProcessRequest( HttpContext context )
    {

        var request = context.Request;
        var response = context.Response;
        response.ContentType = "text/plain";

        if ( request.HttpMethod != "GET" )
        {
            response.Write( "Invalid request type." );
            return;
        }

        var rockContext = new RockContext();
        var defValServ = new DefinedValueService( rockContext );

        Guid campusGuid = Guid.Empty;
        if ( !String.IsNullOrWhiteSpace( request.Params["Campus"] ) )
        {
            var campus = CampusCache.All( false ).Where( c => c.ShortCode.Equals( request.Params["Campus"], StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();
            campusGuid = campus != null ? campus.Guid : campusGuid;
        }

        var statusToShow = new WorkflowService( rockContext ).Queryable().Where( w => w.CompletedDateTime == null && w.WorkflowTypeId == 193 ).ToList().Select( p =>
        {
            p.LoadAttributes();
            return p;
        } ).Where( p => campusGuid.IsEmpty() || p.GetAttributeValue("Campus").AsGuid() == campusGuid).Select( p =>
        {
            return defValServ.Get( p.GetAttributeValue( "PagerStatus" ).AsGuid() );
        } ).Where( dv => dv != null ).OrderBy( dv => dv.Order ).FirstOrDefault();

        if ( statusToShow == null )
        {
            response.Write( "#005500" );
            return;
        }

        statusToShow.LoadAttributes();

        if ( request.Params["Production"] == "true" )
        {
            response.Write( statusToShow.GetAttributeValue( "ProductionTeamBlinkCode" ) );
        }
        else
        {
            response.Write( statusToShow.GetAttributeValue( "KidsTeamBlinkCode" ) );
        }

    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}