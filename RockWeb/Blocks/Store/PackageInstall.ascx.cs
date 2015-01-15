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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Store;
using System.Text;
using Rock.Utility;
using System.Net;
using System.IO.Compression;
using Microsoft.Web.XmlTransform;


namespace RockWeb.Blocks.Store
{
    /// <summary>
    /// Lists packages that have been purchased in the Rock Store.
    /// </summary>
    [DisplayName( "Package Install" )]
    [Category( "Store" )]
    [Description( "Installs a package." )]
    [LinkedPage( "Link Organization Page", "Page to allow the user to link an organization to the store.", false, "", "")]
    public partial class PackageInstall : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        private string _installPurchaseMessage = "Login below with your Rock Store account. Your credit card on file will be charged ${0}.";
        private string _installFreeMessage = "Login below with your Rock Store account to install this free package.";
        private string _updateMessage = "Login below with your Rock Store account to upgrade this package.";
        private string _installPreviousPurchase = "Login below with your Rock Store account to install this previously purchased package.";

        const string _xdtExtension = ".rock.xdt";
        #endregion

        #region Properties

        // used for public / protected properties

        int packageId = -1;

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // get package id
            if ( !string.IsNullOrWhiteSpace( PageParameter( "PackageId" ) ) )
            {
                packageId = Convert.ToInt32( PageParameter( "PackageId" ) );
            }

            if ( !Page.IsPostBack )
            {
                DisplayPackageInfo();
            }
        }

        protected void cbAgreeToTerms_CheckedChanged( object sender, EventArgs e )
        {
            CheckBox cbAgreeToTerms = sender as CheckBox;
            btnInstall.Enabled = cbAgreeToTerms.Checked;
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            DisplayPackageInfo();
        }

        protected void btnInstall_Click( object sender, EventArgs e )
        {
            StoreService storeService = new StoreService();

            var installResponse = storeService.Purchase( txtUsername.Text, txtPassword.Text, packageId );

            switch ( installResponse.PurchaseResult )
            {
                case PurchaseResult.AuthenicationFailed:
                    lMessages.Text = string.Format("<div class='alert alert-warning margin-t-md'><strong>Could Not Authenicate</strong> {0}</div>", installResponse.Message);
                    break;
                case PurchaseResult.Error:
                    lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>An Error Occurred</strong> {0}</div>", installResponse.Message );
                    break;
                case PurchaseResult.NoCardOnFile:
                    lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>No Card On File</strong> No credit card is on file for your organization. Please add a card from your <a href='{0}'>Account Page</a>.</div>", ResolveRockUrl("~/RockShop/Account") );
                    break;
                case PurchaseResult.NotAuthorized:
                    lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Unauthorized</strong> You are not currently authorized to make purchased for this organization. Please see your organization's primary contact to enable your account for purchases.</div>" );
                    break;
                case PurchaseResult.PaymentFailed:
                    lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Payment Error</strong> An error occurred will processing the credit card on file for your organization. The error was: {0}. Please update your card's information from your <a href='{1}'>Account Page</a>.</div>", installResponse.Message, ResolveRockUrl("~/RockShop/Account") );
                    break;
                case PurchaseResult.Success:
                    ProcessInstall(installResponse.PackageInstallSteps);
                    break;
            }
        }

        #endregion

        #region Methods

        private void ProcessInstall( List<PackageInstallStep> installSteps )
        {
            if ( installSteps != null )
            {

                foreach ( var installStep in installSteps )
                {
                    string appRoot = Server.MapPath( "~/" );
                    string rockShopWorkingDir = appRoot + "App_Data/RockShop";
                    string installWorkingDirectory = rockShopWorkingDir + "/packageversion-" + installStep.VersionId.ToString();
                    string sourceFile = installStep.InstallPackageUrl.Replace( "~", "http://www.rockrms.com" );  // todo remove before flight
                    string destinationFile = rockShopWorkingDir + string.Format( "/{0}.plugin", installStep.PackageId.ToString() );

                    // check that the RockShop directory exists
                    if ( !Directory.Exists( rockShopWorkingDir ) )
                    {
                        Directory.CreateDirectory( rockShopWorkingDir );
                    }

                    // create working directory
                    if ( !Directory.Exists( installWorkingDirectory ) )
                    {
                        Directory.CreateDirectory( installWorkingDirectory );
                    }

                    // download file
                    try
                    {
                        WebClient wc = new WebClient();
                        wc.DownloadFile( sourceFile, destinationFile );
                    }
                    catch ( Exception ex )
                    {
                        CleanUpPackage( installWorkingDirectory, destinationFile );
                        lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Downloading Package</strong> An error occurred while downloading package from the store. Please try again later. <br><em>Error: {0}</em></div>", ex.Message );
                        return;
                    }

                    // unzip the file
                    try
                    {
                        using ( ZipArchive packageZip = ZipFile.OpenRead( destinationFile ) )
                        {
                            foreach ( ZipArchiveEntry entry in packageZip.Entries )
                            {
                                if ( entry.FullName == "App_Data" && (entry.FullName.EndsWith( ".sql", StringComparison.OrdinalIgnoreCase ) || entry.FullName.EndsWith( ".del", StringComparison.OrdinalIgnoreCase )) )
                                {
                                    // move sql and .del files in the App_Data directory to working directory
                                    entry.ExtractToFile( installWorkingDirectory + "/" + entry.Name, true );
                                }
                                else if ( entry.FullName.EndsWith( _xdtExtension, StringComparison.OrdinalIgnoreCase ) )
                                {
                                    // process xtd
                                    string transformTargetFile = entry.FullName.Substring( 0, entry.FullName.LastIndexOf( _xdtExtension ) );
                                    string transformFile = installWorkingDirectory + "/" + entry.Name;
                                    
                                    // copy xtd file to working directory
                                    entry.ExtractToFile( transformFile, true );

                                    // process transform
                                    using ( XmlTransformableDocument document = new XmlTransformableDocument() )
                                    {
                                        document.PreserveWhitespace = true;
                                        document.Load( transformTargetFile );

                                        using ( XmlTransformation transform = new XmlTransformation( transformFile ) )
                                        {
                                            if ( transform.Apply( document ) )
                                            {
                                                document.Save( transformTargetFile );
                                                File.Delete( transformFile );
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // process all files that are not .sql, .del or .xtd
                                    string fullpath = Path.Combine( appRoot, entry.FullName );
                                    string directory = Path.GetDirectoryName( fullpath );

                                    if ( !Directory.Exists( directory ) )
                                    {
                                        Directory.CreateDirectory( directory );
                                    }

                                    entry.ExtractToFile( fullpath, true );
                                }
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        CleanUpPackage( installWorkingDirectory, destinationFile );
                        lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Extracting Package</strong> An error occurred while extracting the contents of the package. <br><em>Error: {0}</em></div>", ex.Message );
                        return;
                    }

                    // process and sql files
                    string[] files = Directory.GetFiles( installWorkingDirectory, "*.sql" );

                    try
                    {
                        using ( var context = new RockContext() )
                        {
                            Array.Sort( files );
                            
                            foreach ( var file in files)
                            {
                                context.Database.ExecuteSqlCommand( System.IO.File.ReadAllText( file ) );
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        CleanUpPackage( installWorkingDirectory, destinationFile );
                        lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Updating Database</strong> An error occurred while updating the database. <br><em>Error: {0}</em></div>", ex.Message );
                        return;
                    }

                    // process delete instructions
                    files = Directory.GetFiles( installWorkingDirectory, "*.del" );

                    try
                    {
                        Array.Sort( files );
                        
                        foreach ( var file in files )
                        {
                            string deleteItem = "";

                            System.IO.StreamReader srFile = new System.IO.StreamReader( file );
                            while ( (deleteItem = srFile.ReadLine()) != null )
                            {
                                if ( Directory.Exists( deleteItem ) )
                                {
                                    Directory.Delete( deleteItem );
                                }

                                if ( File.Exists( deleteItem ) )
                                {
                                    File.Delete( deleteItem );
                                }
                            }

                            srFile.Close();
                        }
                    }
                    catch ( Exception ex )
                    {
                        CleanUpPackage( installWorkingDirectory, destinationFile );
                        lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Modifing Files</strong> An error occurred while modifing files. <br><em>Error: {0}</em></div>", ex.Message );
                        return;
                    }

                    // cleanup package
                    CleanUpPackage( installWorkingDirectory, destinationFile );

                    // update package install json file
                    InstalledPackageService.SaveInstall( installStep.PackageId, installStep.PackageName, installStep.VersionId, installStep.VersionLabel, installStep.VendorId, installStep.VendorName );
                }
            }
            else
            {
                lMessages.Text = "<div class='alert alert-warning margin-t-md'><strong>Error</strong> Install package was not valid. Please try again later.";
            }
        }

        private void CleanUpPackage(string installWorkingDirectory, string packageFile)
        {
            try
            {
                if ( Directory.Exists( installWorkingDirectory ) )
                {
                    Directory.Delete( installWorkingDirectory, true );
                }

                if ( File.Exists( packageFile ) )
                {
                    File.Delete( packageFile );
                }

            } catch(Exception ex){
                lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Cleaning Up</strong> An error occurred while cleaning up after the install. <br><em>Error: {0}</em></div>", ex.Message );
                return;
            }
        }

        private void DisplayPackageInfo()
        {
            string errorResponse = string.Empty;

            // check that store is configured
            if ( StoreService.OrganizationIsConfigured() )
            {
                
                PackageService packageService = new PackageService();
                var package = packageService.GetPackage( packageId, out errorResponse );

                // check for errors
                ErrorCheck( errorResponse );

                lPackageName.Text = package.Name;
                lPackageDescription.Text = package.Description;

                lPackageImage.Text = String.Format( @"<div class=""margin-b-md"" style=""
                                background: url('{0}') no-repeat center;
                                width: 100%;
                                height: 140px;"">
                                </div>", package.PackageIconBinaryFile.ImageUrl );

                if ( package.IsFree )
                {
                    lCost.Text = "<div class='pricelabel free'><h4>Free</h4></div>";
                    lInstallMessage.Text = _installFreeMessage;
                }
                else
                {
                    lCost.Text = string.Format( "<div class='pricelabel cost'><h4>${0}</h4></div>", package.Price );
                    lInstallMessage.Text = string.Format( _installPurchaseMessage, package.Price.ToString() );
                }

                if ( package.IsPurchased )
                {
                    // check if it's installed
                    // determine the state of the install button (install, update, buy or installed)
                    InstalledPackage installedPackage = InstalledPackageService.InstalledPackageVersion( package.Id );

                    if ( installedPackage == null )
                    {
                        lCost.Visible = false;
                        lInstallMessage.Text = _installPreviousPurchase;
                    }
                    else
                    {
                        lCost.Visible = false;
                        lInstallMessage.Text = _updateMessage;
                        btnInstall.Text = "Update";
                    }

                }
            }
            else 
            {
                var queryParams = new Dictionary<string, string>();
                queryParams.Add( "ReturnUrl", Request.RawUrl );

                NavigateToLinkedPage( "LinkOrganizationPage", queryParams );
            }
        }

        private void ErrorCheck( string errorResponse )
        {
            if ( errorResponse != string.Empty )
            {
                pnlInstall.Visible = false;
                pnlError.Visible = true;
                lErrorMessage.Text = errorResponse;
            }
        }

        #endregion
    }
}