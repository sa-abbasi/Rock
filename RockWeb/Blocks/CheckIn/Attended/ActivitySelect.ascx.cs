﻿//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Attended
{
    [Description( "Attended Check-In Activity Select Block" )]
    public partial class ActivitySelect : CheckInBlock
    {
        /// <summary>
        /// Check-In information class used to bind the selected grid.
        /// </summary>
        protected class CheckIn
        {
            public string Location { get; set; }
            public string Schedule { get; set; }
            public DateTime? StartTime { get; set; }
            public int LocationId { get; set; }
            public int ScheduleId { get; set; }

            public CheckIn()
            {
                Location = string.Empty;
                Schedule = string.Empty;
                StartTime = new DateTime?();
                LocationId = 0;
                ScheduleId = 0;
            }

        }
        
        /// <summary>
        /// Gets the error when a page's parameter string is invalid.
        /// </summary>
        /// <value>
        /// The invalid parameter error.
        /// </value>
        protected string InvalidParameterError
        {
            get
            {
                return "The selected person's check-in information could not be loaded.";
            }
        }

        #region Control Methods 

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    //mpeAddNote.OnCancelScript = string.Format( "$('#{0}').val('');", hfAllergyAttributeId.ClientID );

                    var personId = Request.QueryString["personId"].AsType<int?>();
                    if ( personId != null )
                    {
                        CheckInPerson person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                            .People.Where( p => p.Person.Id == personId ).FirstOrDefault();
                    
                        if ( person != null )
                        {
                            lblPersonName.Text = person.Person.FullName;

                            //hfLocationId.Value = Request.QueryString["locationId"];
                            //hfScheduleId.Value = Request.QueryString["scheduleId"];

                            BindGroupTypes( person );
                            BindLocations( person );
                            BindSchedules( person );
                            BindSelectedGrid();
                        }
                    }
                    else
                    {
                        maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
                        GoBack();
                    }
                }
                else
                {
                    string allergyAttributeId = Request.Form[hfAllergyAttributeId.UniqueID];
                    if ( !string.IsNullOrEmpty( allergyAttributeId ) )
                    {
                        ShowNoteModal( int.Parse( allergyAttributeId ), new Person().TypeId );
                    }
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the ItemCommand event of the rGroupType control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rGroupType_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            { 
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                    .People.Where( p => p.Person.Id == personId ).FirstOrDefault();

                foreach ( RepeaterItem item in rGroupType.Items )
                {
                    if ( item.ID != e.Item.ID )
                    { 
                        ( (LinkButton)item.FindControl( "lbGroupType" ) ).RemoveCssClass( "active" );
                    }
                    else
                    {
                        ( (LinkButton)e.Item.FindControl( "lbGroupType" ) ).AddCssClass( "active" );
                    }
                }

                hfGroupTypeId.Value = e.CommandArgument.ToString();
                pnlGroupTypes.Update();
                BindSchedules( person );
                BindLocations( person );
            }
            else 
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvLocation_ItemCommand( object sender, ListViewCommandEventArgs e )
        {
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            { 
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                    .People.Where( p => p.Person.Id == personId ).FirstOrDefault();

                foreach ( ListViewDataItem item in lvLocation.Items )
                {
                    if ( item.ID != e.Item.ID )
                    {
                        ( (LinkButton)item.FindControl( "lbLocation" ) ).RemoveCssClass( "active" );
                    }
                    else
                    {
                        ( (LinkButton)e.Item.FindControl( "lbLocation" ) ).AddCssClass( "active" );
                    }                    
                }

                hfLocationId.Value = e.CommandArgument.ToString();
                pnlLocations.Update();                
                BindSchedules( person );
            }
            else 
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rSchedule control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rSchedule_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            { 
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                    .People.Where( p => p.Person.Id == personId ).FirstOrDefault();

                ( (LinkButton)e.Item.FindControl( "lbSchedule" ) ).AddCssClass( "active" );

                int groupTypeId = hfGroupTypeId.ValueAsInt();
                int locationId = hfLocationId.ValueAsInt();
                int scheduleId = Int32.Parse( e.CommandArgument.ToString() );
                
                // clear any other locations and schedules selected for this time
                var groupTypes = person.GroupTypes.ToList();
                var groups = groupTypes.SelectMany( gt => gt.Groups ).ToList();
                var locations = groups.SelectMany( g => g.Locations ).ToList();
                var schedules = locations.SelectMany( l => l.Schedules )
                    .Where( s => s.Schedule.Id == scheduleId ).ToList();

                // clear out any schedules that are currently selected for the chosen schedule. 
                schedules.Where( s => s.Schedule.Id == scheduleId && s.Selected ).ToList()
                    .ForEach( s => s.Selected = false );

                // clear out any locations where all the schedules are not selected
                locations.Where( l => l.Schedules.All( s => s.Selected == false ) ).ToList()
                    .ForEach( l => l.Selected = false );

                // clear out any groups where all the locations are not selected.
                groups.Where( g => g.Locations.All( l => l.Selected == false ) ).ToList()
                    .ForEach( g => g.Selected = false );

                groupTypes.Where( gt => gt.GroupType.Id == groupTypeId ).Select( gt => gt.Selected = true );
                groups.Where( g => g.Locations.Any( l => l.Location.Id == locationId ) ).Select( g => g.Selected = true );
                locations.Where( l => l.Location.Id == locationId ).Select( l => l.Selected = true );
                
                hfScheduleId.Value = e.CommandArgument.ToString();
                pnlSchedules.Update();
                BindSelectedGrid();
            }
            else 
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rGroupTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rGroupType_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var groupType = (CheckInGroupType)e.Item.DataItem;
                var lbGroupType = (LinkButton)e.Item.FindControl( "lbGroupType" );
                lbGroupType.CommandArgument = groupType.GroupType.Id.ToString();
                lbGroupType.Text = groupType.GroupType.Name;
                if ( groupType.Selected )
                {
                    lbGroupType.AddCssClass( "active" );
                    hfGroupTypeId.Value = groupType.GroupType.Id.ToString();
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvLocation_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            if ( e.Item.ItemType == ListViewItemType.DataItem )
            {
                var location = (CheckInLocation)e.Item.DataItem;
                var lbLocation = (LinkButton)e.Item.FindControl( "lbLocation" );
                lbLocation.CommandArgument = location.Location.Id.ToString();
                lbLocation.Text = location.Location.Name;
                if ( location.Selected )
                {
                    lbLocation.AddCssClass( "active" );
                    hfLocationId.Value = location.Location.Id.ToString();
                }               
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rSchedule_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var schedule = (CheckInSchedule)e.Item.DataItem;
                var lbSchedule = (LinkButton)e.Item.FindControl( "lbSchedule" );
                lbSchedule.CommandArgument = schedule.Schedule.Id.ToString();
                lbSchedule.Text = schedule.Schedule.Name;
                if ( schedule.Selected )
                {
                    lbSchedule.AddCssClass( "active" );
                    hfScheduleId.Value = schedule.Schedule.Id.ToString();
                }
            }
        }

        /// <summary>
        /// Handles the PagePropertiesChanging event of the lvLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PagePropertiesChangingEventArgs"/> instance containing the event data.</param>
        protected void lvLocation_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            Pager.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );
            lvLocation.DataSource = Session["locations"];
            lvLocation.DataBind();
        }

        /// <summary>
        /// Handles the Delete event of the gCheckInList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSelectedList_Delete( object sender, RowEventArgs e )
        {
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            {
                // Delete an item. Remove the selected attribute from the group, location and schedule
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                    .People.Where( p => p.Person.Id == personId ).FirstOrDefault();

                int index = e.RowIndex;
                var row = gSelectedList.Rows[index];
                var dataKeyValues = gSelectedList.DataKeys[index].Values;
                var locationId = int.Parse( dataKeyValues["LocationId"].ToString() );
                var scheduleId = int.Parse( dataKeyValues["ScheduleId"].ToString() );

                var selectedGroupType = person.GroupTypes.Where( gt => gt.Selected ).FirstOrDefault();
                GroupLocationService groupLocationService = new GroupLocationService();
                var groupLocationGroupId = groupLocationService.GetByLocation( locationId )
                    .Select( l => l.GroupId ).FirstOrDefault();
                var selectedGroup = selectedGroupType.Groups.Where( g => g.Selected && g.Group.Id == groupLocationGroupId ).FirstOrDefault();
                var selectedLocation = selectedGroup.Locations.Where( l => l.Selected && l.Location.Id == locationId ).FirstOrDefault();
                var selectedSchedule = selectedLocation.Schedules.Where( s => s.Selected && s.Schedule.Id == scheduleId ).FirstOrDefault();

                selectedSchedule.Selected = false;

                var clearLocation = selectedLocation.Schedules.All( s => s.Selected == false );
                if ( clearLocation )
                {
                    selectedLocation.Selected = false;
                }

                var clearGroup = selectedGroup.Locations.All( l => l.Selected == false );
                if ( clearGroup )
                {
                    selectedGroup.Selected = false;
                }

                BindLocations( person );
                BindSchedules( person );
                BindSelectedGrid();
            }
            else 
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }            
        }

        /// <summary>
        /// Handles the Click event of the lbAddNote control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddNote_Click( object sender, EventArgs e )
        {
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            {
                var allergyAttributeId = new AttributeService().GetByEntityTypeId( new Person().TypeId )
                    .Where( a => a.Name.ToUpper() == "ALLERGY" ).FirstOrDefault().Id;
                ShowNoteModal( allergyAttributeId, (int)personId );
            }
            else
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddNoteCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        //protected void lbAddNoteCancel_Click( object sender, EventArgs e )
        //{
        //    hfAllergyAttributeId.Value = string.Empty;
        //}

        /// <summary>
        /// Handles the Click event of the lbAddNoteSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddNoteSave_Click( object sender, EventArgs e )
        {
            var personId = Request.QueryString["personId"].AsType<int?>();
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Person.Id == personId ).FirstOrDefault();

            // Need to load a full person because the person.Person is only a clone.
            // Person Person = new PersonService().Get( person.Person.Id );
            person.Person.LoadAttributes();

            var allergyAttributeId = new AttributeService().GetByEntityTypeId( new Person().TypeId )
                .Where( a => a.Name.ToUpper() == "ALLERGY" ).FirstOrDefault().Id;
            var allergyAttribute = Rock.Web.Cache.AttributeCache.Read( allergyAttributeId );

            Control allergyAttributeControl = fsNotes.FindControl( string.Format( "attribute_field_{0}", allergyAttributeId ) );
            if ( allergyAttributeControl != null )
            {
                person.Person.SetAttributeValue( "Allergy", allergyAttribute.FieldType.Field
                    .GetEditValue( allergyAttributeControl, allergyAttribute.QualifierValues ) );
            }

            Rock.Attribute.Helper.SaveAttributeValues( person.Person, CurrentPersonId );
            hfAllergyAttributeId.Value = string.Empty;
            mpeAddNote.Hide();
        }

        
        /// <summary>
        /// Handles the Click event of the lbBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }

        /// <summary>
        /// Handles the Click event of the lbNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbNext_Click( object sender, EventArgs e )
        {
            GoNext();   
        }

        #endregion

        #region Internal Methods 

        /// <summary>
        /// Binds the group types.
        /// </summary>
        /// <param name="person">The person.</param>
        protected void BindGroupTypes( CheckInPerson person )
        {
            rGroupType.DataSource = person.GroupTypes;
            rGroupType.DataBind();
            pnlGroupTypes.Update();
        }

        /// <summary>
        /// Binds the locations.
        /// </summary>
        /// <param name="person">The person.</param>
        protected void BindLocations( CheckInPerson person )
        {
            CheckInGroupType groupType = null;
            if ( person.GroupTypes.Any( gt => gt.Selected ) )
            {
                groupType = person.GroupTypes.Where( gt => gt.Selected ).FirstOrDefault();
            }
            else
            {
                groupType = person.GroupTypes.FirstOrDefault();
            }

            CheckInGroup group = null;
            if ( groupType.Groups.Any( g => g.Selected ) )
            {
                group = groupType.Groups.Where( g => g.Selected ).FirstOrDefault();
            }
            else
            {
                group = groupType.Groups.FirstOrDefault();
            }

            CheckInLocation location = null;
            if ( group.Locations.Any( l => l.Selected ) )
            {
                location = group.Locations.Where( l => l.Selected ).FirstOrDefault();
            }
            else 
            {
                location = group.Locations.FirstOrDefault();
            }
            
            if ( location != null )
            {
                var selectedLocationPlaceInList = group.Locations.IndexOf( location ) + 1;
                var pageSize = this.Pager.PageSize;
                var pageToGoTo = selectedLocationPlaceInList / pageSize;
                if ( selectedLocationPlaceInList % pageSize != 0 )
                {
                    pageToGoTo++;
                }

                this.Pager.SetPageProperties( ( pageToGoTo - 1 ) * this.Pager.PageSize, this.Pager.MaximumRows, false );
            }

            Session["locations"] = group.Locations;
            lvLocation.DataSource = group.Locations;
            lvLocation.DataBind();
            pnlLocations.Update();
        }

        /// <summary>
        /// Binds the schedules.
        /// </summary>
        /// <param name="person">The person.</param>
        protected void BindSchedules( CheckInPerson person )
        {
            CheckInGroupType groupType = null;
            if ( person.GroupTypes.Any( gt => gt.Selected ) )
            {
                groupType = person.GroupTypes.Where( gt => gt.Selected ).FirstOrDefault();
            }
            else
            {
                groupType = person.GroupTypes.FirstOrDefault();
            }

            CheckInGroup group = null;
            if ( groupType.Groups.Any( g => g.Selected ) )
            {
                group = groupType.Groups.Where( g => g.Selected ).FirstOrDefault();
            }
            else
            {
                group = groupType.Groups.FirstOrDefault();
            }

            CheckInLocation location = null;
            if ( group.Locations.Any( l => l.Selected ) )
            {
                location = group.Locations.Where( l => l.Selected ).FirstOrDefault();
            }
            else
            {
                location = group.Locations.FirstOrDefault();
            }

            rSchedule.DataSource = location.Schedules.ToList();
            rSchedule.DataBind();
            pnlSchedules.Update();
        }

        /// <summary>
        /// Binds the selected items to the grid.
        /// </summary>
        protected void BindSelectedGrid()
        {
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            {
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                    .People.Where( p => p.Person.Id == (int)personId ).FirstOrDefault();

                var selectedGroupTypes = person.GroupTypes.Where( gt => gt.Selected ).ToList();
                var selectedGroups = selectedGroupTypes.SelectMany( gt => gt.Groups.Where( g => g.Selected ) ).ToList();
                var selectedLocations = selectedGroups.SelectMany( g => g.Locations.Where( l => l.Selected ) ).ToList();
                var selectedSchedules = selectedLocations.SelectMany( l => l.Schedules.Where( s => s.Selected ) ).ToList();

                var checkInList = new List<CheckIn>();
                foreach ( var location in selectedLocations )
                {
                    foreach ( var schedule in location.Schedules.Where( s => s.Selected ) )
                    {
                        var checkIn = new CheckIn();
                        checkIn.Location = location.Location.Name;
                        checkIn.Schedule = schedule.Schedule.Name;
                        checkIn.StartTime = Convert.ToDateTime( schedule.StartTime );
                        checkIn.LocationId = location.Location.Id;
                        checkIn.ScheduleId = schedule.Schedule.Id;
                        checkInList.Add( checkIn );
                    }
                }
                gSelectedList.DataSource = checkInList.OrderBy( c => c.StartTime ).ToList();
                gSelectedList.DataBind();
                pnlSelected.Update();
            }
        }

        /// <summary>
        /// Goes back to the confirmation page with no changes.
        /// </summary>
        private new void GoBack()
        {
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            {
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Person.Id == personId ).FirstOrDefault();

                var groupTypes = person.GroupTypes.ToList();
                groupTypes.ForEach( gt => gt.Selected = gt.PreSelected );

                var groups = groupTypes.SelectMany( gt => gt.Groups ).ToList();
                groups.ForEach( g => g.Selected = g.PreSelected );

                var locations = groups.SelectMany( g => g.Locations ).ToList();
                locations.ForEach( l => l.Selected = l.PreSelected );

                var schedules = locations.SelectMany( l => l.Schedules ).ToList();
                schedules.ForEach( s => s.Selected = s.PreSelected );             
            }
            else 
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }

            NavigateToPreviousPage();
        }

        /// <summary>
        /// Goes to the confirmation page with changes.
        /// </summary>
        private void GoNext()
        {
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            {
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                    .People.Where( p => p.Person.Id == personId ).FirstOrDefault();

                var groupTypes = person.GroupTypes.ToList();
                groupTypes.ForEach( gt => gt.PreSelected = gt.Selected );

                var groups = groupTypes.SelectMany( gt => gt.Groups ).ToList();
                groups.ForEach( g => g.PreSelected = g.Selected );

                var locations = groups.SelectMany( g => g.Locations ).ToList();
                locations.ForEach( l => l.PreSelected = l.Selected );

                var schedules = locations.SelectMany( l => l.Schedules ).ToList();
                schedules.ForEach( s => s.PreSelected = s.Selected );
            }
            else
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }

            SaveState();
            NavigateToNextPage();
        }


        /// <summary>
        /// Shows the note modal.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        /// <param name="entityId">The entity id.</param>
        protected void ShowNoteModal( int allergyAttributeId, int personId )
        {
            var attribute = AttributeCache.Read( allergyAttributeId );
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Person.Id == personId ).FirstOrDefault();
            // load a Rock person because the CheckInPerson doesn't have attributes
            //var person = new PersonService().Get( personId );
            fsNotes.Controls.Clear();

            person.Person.LoadAttributes();
            var attributeValue = person.Person.GetAttributeValue( attribute.Key );
            attribute.AddControl( fsNotes.Controls, attributeValue, "", true, true );
            hfAllergyAttributeId.Value = attribute.Id.ToString();

            mpeAddNote.Show();            
        }
        
        #endregion        
    }
}