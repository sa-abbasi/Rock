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
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;

using Rock;
using Rock.Model;


namespace Rock.Migrations
{
    /// <summary>
    /// Custom Migration methods
    /// </summary>
    public abstract class RockMigration4 : DbMigration
    {
        #region Entity Type Methods

        /// <summary>
        /// Updates the EntityType by name (if it exists); otherwise it inserts a new record.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="isEntity">if set to <c>true</c> [is entity].</param>
        /// <param name="isSecured">if set to <c>true</c> [is secured].</param>
        public void UpdateEntityType( string name, string guid, bool isEntity, bool isSecured )
        {
            Sql( string.Format( @"
                IF EXISTS ( SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}' )
                BEGIN
                    UPDATE [EntityType] SET 
                        [IsEntity] = {1},
                        [IsSecured] = {2},
                        [Guid] = '{3}'
                    WHERE [Name] = '{0}'
                END
                ELSE
                BEGIN
                    INSERT INTO [EntityType] ([Name], [IsEntity], [IsSecured], [IsCommon], [Guid])
                    VALUES ('{0}', {1}, {2}, 0, '{3}')
                END
",
                name,
                isEntity ? "1" : "0",
                isSecured ? "1" : "0",
                guid ) );
        }

        /// <summary>
        /// Updates the EntityType by name (if it exists); otherwise it inserts a new record.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="friendlyName">Name of the friendly.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="isEntity">if set to <c>true</c> [is entity].</param>
        /// <param name="isSecured">if set to <c>true</c> [is secured].</param>
        /// <param name="guid">The GUID.</param>
        public void UpdateEntityType( string name, string friendlyName, string assemblyName, bool isEntity, bool isSecured, string guid )
        {
            Sql( string.Format( @"

                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [EntityType] (
                        [Name],[FriendlyName],[AssemblyName],[IsEntity],[IsSecured],[IsCommon],[Guid])
                    VALUES(
                        '{0}','{1}','{2}',{3},{4},0,'{5}')
                END
                ELSE
                BEGIN
                    UPDATE [EntityType] SET 
                        [FriendlyName] = '{1}',
                        [AssemblyName] = '{2}',
                        [IsEntity] = {3},
                        [IsSecured] = {4},
                        [Guid] = '{5}'
                    WHERE [Name] = '{0}'
                END
",
                    name.Replace( "'", "''" ),
                    friendlyName.Replace( "'", "''" ),
                    assemblyName.Replace( "'", "''" ),
                    isEntity ? "1" : "0",
                    isSecured ? "1" : "0",
                    guid ) );
        }

        /// <summary>
        /// Deletes the EntityType.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteEntityType( string guid )
        {
            Sql( string.Format( @"
                DELETE [EntityType] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        /// <summary>
        /// Updates the EntityType SingleValueFieldType
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        public void UpdateEntityTypeSingleValueFieldType( string entityTypeName, string fieldTypeGuid)
        {
            EnsureEntityTypeExists( entityTypeName );

            Sql( string.Format( @"
                 
                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                UPDATE [EntityType] SET [SingleValueFieldTypeId] = @FieldTypeId WHERE [Id] = @EntityTypeId
                ", entityTypeName, fieldTypeGuid)
            );
        }

        /// <summary>
        /// Updates the EntityType MultiValueFieldType
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        public void UpdateEntityTypeMultiValueFieldType( string entityTypeName, string fieldTypeGuid )
        {
            EnsureEntityTypeExists( entityTypeName );

            Sql( string.Format( @"
                 
                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                UPDATE [EntityType] SET [MultiValueFieldTypeId] = @FieldTypeId WHERE [Id] = @EntityTypeId
                ", entityTypeName, fieldTypeGuid )
            );
        }

        #endregion

        #region Field Type Methods

        /// <summary>
        /// Updates the FieldType by assembly and className (if it exists); otherwise it inserts a new record.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="IsSystem">if set to <c>true</c> [is system].</param>
        public void UpdateFieldType( string name, string description, string assembly, string className, string guid, bool IsSystem = true )
        {
            Sql( string.Format( @"

                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [FieldType] WHERE [Assembly] = '{2}' AND [Class] = '{3}')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [FieldType] (
                        [Name],[Description],[Assembly],[Class],[Guid],[IsSystem])
                    VALUES(
                        '{0}','{1}','{2}','{3}','{4}',{5})
                END
                ELSE
                BEGIN
                    UPDATE [FieldType] SET 
                        [Name] = '{0}', 
                        [Description] = '{1}',
                        [Guid] = '{4}',
                        [IsSystem] = {5}
                    WHERE [Assembly] = '{2}'
                    AND [Class] = '{3}'
                END
",
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    assembly,
                    className,
                    guid,
                    IsSystem ? "1" : "0" ) );
        }

        /// <summary>
        /// Deletes the FieldType.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteFieldType( string guid )
        {
            Sql( string.Format( @"
                DELETE [FieldType] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        #endregion

        #region Block Type Methods

        /// <summary>
        /// Updates the BlockType by path (if it exists);
        /// otherwise it inserts a new record. In either case it will be marked IsSystem.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="path">The path.</param>
        /// <param name="category">The category.</param>
        /// <param name="guid">The GUID.</param>
        public void UpdateBlockType( string name, string description, string path, string category, string guid )
        {
            Sql( string.Format( @"
                
                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [BlockType] WHERE [Path] = '{0}')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [BlockType] (
                        [IsSystem],[Path],[Category],[Name],[Description],
                        [Guid])
                    VALUES(
                        1,'{0}','{1}','{2}','{3}',
                        '{4}')
                END
                ELSE
                BEGIN
                    UPDATE [BlockType] SET 
                        [IsSystem] = 1,
                        [Category] = '{1}',
                        [Name] = '{2}',
                        [Description] = '{3}',
                        [Guid] = '{4}'
                    WHERE [Path] = '{0}'
                END
",
                    path,
                    category,
                    name,
                    description.Replace( "'", "''" ),
                    guid
                    ) );
        }
        
        /// <summary>
        /// Adds a new BlockType.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="path"></param>
        /// <param name="category"></param>
        /// <param name="guid"></param>
        public void AddBlockType( string name, string description, string path, string category, string guid )
        {
            Sql( string.Format( @"
                
                INSERT INTO [BlockType] (
                    [IsSystem],[Path],[Category],[Name],[Description],
                    [Guid])
                VALUES(
                    1,'{0}','{1}','{2}','{3}',
                    '{4}')
",
                    path,
                    category,
                    name,
                    description.Replace( "'", "''" ),
                    guid
                    ) );
        }

        /// <summary>
        /// Deletes the BlockType.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteBlockType( string guid )
        {
            Sql( string.Format( @"
                DELETE [BlockType] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        #endregion

        #region Layout Methods

        /// <summary>
        /// Adds a new Layout to the given site.
        /// </summary>
        /// <param name="siteGuid">The site GUID.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        public void AddLayout( string siteGuid, string fileName, string name, string description, string guid )
        {
            Sql( string.Format( @"

                DECLARE @SiteId int
                SET @SiteId = (SELECT [Id] FROM [Site] WHERE [Guid] = '{0}')
                        
                INSERT INTO [Layout] (
                    [IsSystem],[SiteId],[FileName],[Name],[Description],[Guid])
                VALUES(
                    1,@SiteId,'{1}','{2}','{3}','{4}')
",
                    siteGuid,
                    fileName,
                    name,
                    description.Replace( "'", "''" ),
                    guid
                    ) );
        }

        /// <summary>
        /// Deletes the Layout.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteLayout( string guid )
        {
            Sql( string.Format( @"
                DELETE [Layout] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        #endregion

        #region Page Methods

        /// <summary>
        /// Adds a new Page to the given parent page.
        /// The new page will be ordered as last child of the parent page.
        /// </summary>
        /// <param name="parentPageGuid">The parent page GUID.</param>
        /// <param name="layoutGuid">The layout GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="iconCssClass">The icon CSS class.</param>
        public void AddPage( string parentPageGuid, string layoutGuid, string name, string description, string guid, string iconCssClass = "" )
        {
            Sql( string.Format( @"

                DECLARE @ParentPageId int
                SET @ParentPageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                DECLARE @LayoutId int
                SET @LayoutId = (SELECT [Id] FROM [Layout] WHERE [Guid] = '{1}')
                        
                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [Page] WHERE [ParentPageId] = @ParentPageId;

                INSERT INTO [Page] (
                    [InternalName],[PageTitle],[BrowserTitle],[IsSystem],[ParentPageId],[LayoutId],
                    [RequiresEncryption],[EnableViewState],
                    [PageDisplayTitle],[PageDisplayBreadCrumb],[PageDisplayIcon],[PageDisplayDescription],
                    [MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],
                    [BreadCrumbDisplayName],[BreadCrumbDisplayIcon],
                    [Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],
                    [IconCssClass],[Guid])
                VALUES(
                    '{2}','{2}','{2}',1,@ParentPageId,@LayoutId,
                    0,1,
                    1,1,1,1,
                    0,0,1,0,
                    1,0,
                    @Order,0,'{3}',1,
                    '{5}','{4}')
",
                    parentPageGuid,
                    layoutGuid,
                    name,
                    description.Replace( "'", "''" ),
                    guid,
                    iconCssClass
                    ) );
        }

        /// <summary>
        /// Moves the Page to the new given parent page.
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="parentPageGuid">The parent page GUID.</param>
        public void MovePage( string pageGuid, string parentPageGuid )
        {
            Sql( string.Format( @"

                DECLARE @parentPageId int
                SET @parentPageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                UPDATE [Page] SET [ParentPageId]=@parentPageId WHERE [Guid] = '{1}'
                ", parentPageGuid, pageGuid ) );
        }

        /// <summary>
        /// Deletes the Page and any PageViews that use the page.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeletePage( string guid )
        {
            Sql( string.Format( @"

                DELETE PV
                FROM [PageView] PV
                INNER JOIN [Page] P ON P.[Id] = PV.[PageId] AND P.[Guid] = '{0}'

                DELETE [Page] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        /// <summary>
        /// Adds a new PageRoute to the given page but only if the given route name does not exist.
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="route">The route.</param>
        public void AddPageRoute( string pageGuid, string route )
        {
            Sql( string.Format( @"

                DECLARE @PageId int
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                IF NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = '{1}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, '{1}', newid())
", pageGuid, route ) );

        }

        /// <summary>
        /// Adds a new PageContext to the given page.
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="idParameter">The id parameter.</param>
        public void AddPageContext( string pageGuid, string entity, string idParameter )
        {
            Sql( string.Format( @"

                DECLARE @PageId int
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                INSERT INTO [PageContext] (
                    [IsSystem],[PageId],[Entity],[IdParameter],[Guid])
                VALUES(
                    1, @PageId, '{1}', '{2}', newid())
", pageGuid, entity, idParameter ) );

        }

        #endregion

        #region Block Methods

        /// <summary>
        /// Adds a new Block of the given block type to the given page (optional) and layout (optional),
        /// setting its values with the given parameter values. If only the layout is given,
        /// edit/configuration authorization will also be inserted into the Auth table
        /// for the admin role (GroupId 2).
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="layoutGuid">The layout GUID.</param>
        /// <param name="blockTypeGuid">The block type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="preHtml">The pre HTML.</param>
        /// <param name="postHtml">The post HTML.</param>
        public void AddBlock( string pageGuid, string layoutGuid, string blockTypeGuid, string name, string zone, string preHtml, string postHtml, int order, string guid )
        {
            var sb = new StringBuilder();
            sb.Append( @"
                DECLARE @PageId int
                SET @PageId = null

                DECLARE @LayoutId int
                SET @LayoutId = null
" );

            if ( !string.IsNullOrWhiteSpace( pageGuid ) )
            {
                sb.AppendFormat( @"
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')
", pageGuid );
            }

            if ( !string.IsNullOrWhiteSpace( layoutGuid ) )
            {
                sb.AppendFormat( @"
                SET @LayoutId = (SELECT [Id] FROM [Layout] WHERE [Guid] = '{0}')
", layoutGuid );
            }

            sb.AppendFormat( @"
                
                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{0}')
                DECLARE @EntityTypeId int                
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

                DECLARE @BlockId int
                INSERT INTO [Block] (
                    [IsSystem],[PageId],[LayoutId],[BlockTypeId],[Zone],
                    [Order],[Name],[PreHtml],[PostHtml],[OutputCacheDuration],
                    [Guid])
                VALUES(
                    1,@PageId,@LayoutId,@BlockTypeId,'{1}',
                    {2},'{3}','{4}','{5}',0,
                    '{6}')
                SET @BlockId = SCOPE_IDENTITY()
",
                    blockTypeGuid,
                    zone,
                    order,
                    name,
                    preHtml.Replace( "'", "''" ),
                    postHtml.Replace( "'", "''" ),
                    guid );

            // If adding a layout block, give edit/configuration authorization to admin role
            if ( string.IsNullOrWhiteSpace( pageGuid ) )
                sb.Append( @"
                INSERT INTO [Auth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Edit','A',0,NULL,2,NEWID())
                INSERT INTO [Auth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Configure','A',0,NULL,2,NEWID())
" );
            Sql( sb.ToString() );
        }

        /// <summary>
        /// Deletes the block and any authorization records that belonged to it.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteBlock( string guid )
        {
            Sql( string.Format( @"
                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')
                DECLARE @EntityTypeId int                
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')
                DELETE [Auth] WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @BlockId
                DELETE [Block] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        #endregion

        #region Category Methods

        /// <summary>
        /// Updates the category.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="iconCssClass">The icon CSS class.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The unique identifier.</param>
        public void UpdateCategory( string entityTypeGuid, string name, string iconCssClass, string description, string guid )
        {
            Sql( string.Format( @"
                
                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{0}')

                IF EXISTS (
                    SELECT [Id] 
                    FROM [Category] 
                    WHERE [Guid] = '{4}' )
                BEGIN
                    UPDATE [Category] SET
                        [EntityTypeId] = @EntityTypeId,
                        [Name] = '{1}',
                        [IconCssClass] = '{2}',
                        [Description] = '{3}'
                    WHERE [Guid] = '{4}'
                END
                ELSE
                BEGIN
                    INSERT INTO [Category] ( [IsSystem],[EntityTypeId],[Name],[IconCssClass],[Description],[Order],[Guid] )
                    VALUES( 1,@EntityTypeId,'{1}','{2}','{3}',0,'{4}' )  
                END
",
                    entityTypeGuid,
                    name,
                    iconCssClass,
                    description.Replace( "'", "''" ),
                    guid )
            );
        }

        /// <summary>
        /// Deletes the category.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public void DeleteCategory( string guid )
        {
            Sql( string.Format( @"
                
                DELETE [Category] 
                WHERE [Guid] = '{0}'
",
                    guid )
            );
        }

        #endregion

        #region Attribute Methods

        /// <summary>
        /// Updates the BlockType Attribute for the given blocktype and key (if it exists);
        /// otherwise it inserts a new record.
        /// </summary>
        /// <param name="blockTypeGuid"></param>
        /// <param name="fieldTypeGuid"></param>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <param name="category"></param>
        /// <param name="description"></param>
        /// <param name="order"></param>
        /// <param name="defaultValue"></param>
        /// <param name="guid"></param>
        public void UpdateBlockTypeAttribute( string blockTypeGuid, string fieldTypeGuid, string name, string key, string category, string description, int order, string defaultValue, string guid )
        {
            if ( !string.IsNullOrWhiteSpace( category ) )
            {
                throw new Exception( "Attribute Category no longer supported by this helper function. You'll have to write special migration code yourself. Sorry!" );
            }

            Sql( string.Format( @"
                
                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                DECLARE @EntityTypeId int                
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

                IF EXISTS (
                    SELECT [Id] 
                    FROM [Attribute] 
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = 'BlockTypeId'
                    AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar)
                    AND [Key] = '{2}' )
                BEGIN
                    UPDATE [Attribute] SET
                        [Name] = '{3}',
                        [Description] = '{4}',
                        [Order] = {5},
                        [DefaultValue] = '{6}',
                        [Guid] = '{7}'
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = 'BlockTypeId'
                    AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar)
                    AND [Key] = '{2}'
                END
                ELSE
                BEGIN
                    INSERT INTO [Attribute] (
                        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                        [Key],[Name],[Description],
                        [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                        [Guid])
                    VALUES(
                        1,@FieldTypeId, @EntityTypeId,'BlockTypeId',CAST(@BlockTypeId as varchar),
                        '{2}','{3}','{4}',
                        {5},0,'{6}',0,0,
                        '{7}')  
                END
",
                    blockTypeGuid,
                    fieldTypeGuid,
                    key ?? name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue.Replace( "'", "''" ),
                    guid )
            );
        }

        /// <summary>
        /// Adds a new BlockType Attribute for the given blocktype and key.
        /// </summary>
        /// <param name="blockTypeGuid">The block GUID.</param>
        /// <param name="fieldTypeGuid">The field type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        /// <param name="category">The category.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The GUID.</param>
        public void AddBlockTypeAttribute( string blockTypeGuid, string fieldTypeGuid, string name, string key, string category, string description, int order, string defaultValue, string guid )
        {
            if ( !string.IsNullOrWhiteSpace( category ) )
            {
                throw new Exception( "Attribute Category no longer supported by this helper function. You'll have to write special migration code yourself. Sorry!" );
            }

            Sql( string.Format( @"
                
                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                DECLARE @EntityTypeId int                
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute] 
                WHERE [EntityTypeId] = @EntityTypeId
                AND [EntityTypeQualifierColumn] = 'BlockTypeId'
                AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar)
                AND [Key] = '{2}'

                INSERT INTO [Attribute] (
                    [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                    [Key],[Name],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [Guid])
                VALUES(
                    1,@FieldTypeId, @EntityTypeId,'BlockTypeId',CAST(@BlockTypeId as varchar),
                    '{2}','{3}','{4}',
                    {5},0,'{6}',0,0,
                    '{7}')
",
                    blockTypeGuid,
                    fieldTypeGuid,
                    key ?? name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue.Replace( "'", "''" ),
                    guid )
            );
        }

        /// <summary>
        /// Deletes the block Attribute.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteBlockAttribute( string guid )
        {
            DeleteAttribute( guid );
        }

        /// <summary>
        /// Adds a new EntityType Attribute for the given EntityType, FieldType, and name (key).
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="fieldTypeGuid">The field type GUID.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="name">The name.</param>
        /// <param name="category">The category.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The GUID.</param>
        public void AddEntityAttribute( string entityTypeName, string fieldTypeGuid, string entityTypeQualifierColumn, string entityTypeQualifierValue, string name, string category, string description, int order, string defaultValue, string guid )
        {
            if ( !string.IsNullOrWhiteSpace( category ) )
            {
                throw new Exception( "Attribute Category no longer supported by this helper function. You'll have to write special migration code yourself. Sorry!" );
            }

            EnsureEntityTypeExists( entityTypeName );

            Sql( string.Format( @"
                 
                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute] 
                WHERE [EntityTypeId] = @EntityTypeId
                AND [Key] = '{2}'
                AND [EntityTypeQualifierColumn] = '{8}'
                AND [EntityTypeQualifierValue] = '{9}'

                INSERT INTO [Attribute] (
                    [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                    [Key],[Name],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,@EntityTypeid,'{8}','{9}',
                    '{2}','{3}','{4}',
                    {5},0,'{6}',0,0,
                    '{7}')
",
                    entityTypeName,
                    fieldTypeGuid,
                    name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue,
                    guid,
                    entityTypeQualifierColumn,
                    entityTypeQualifierValue )
            );
        }

        /// <summary>
        /// Adds a global Attribute for the given FieldType, entityTypeQualifierColumn, entityTypeQualifierValue and name (key).
        /// Note: This method delets the Attribute first if it had already existed.
        /// </summary>
        /// <param name="fieldTypeGuid">The field type GUID.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The GUID.</param>
        public void AddGlobalAttribute( string fieldTypeGuid, string entityTypeQualifierColumn, string entityTypeQualifierValue, string name, string description, int order, string defaultValue, string guid )
        {
            Sql( string.Format( @"
                 
                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute] 
                WHERE [EntityTypeId] IS NULL
                AND [Key] = '{2}'
                AND [EntityTypeQualifierColumn] = '{8}'
                AND [EntityTypeQualifierValue] = '{9}'

                INSERT INTO [Attribute] (
                    [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                    [Key],[Name],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,NULL,'{8}','{9}',
                    '{2}','{3}','{4}',
                    {5},0,'{6}',0,0,
                    '{7}')
",
                    "", // no entity; keeps {#} the same as AddEntityAttribute()
                    fieldTypeGuid,
                    name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue,
                    guid,
                    entityTypeQualifierColumn,
                    entityTypeQualifierValue )
            );
        }


        /// <summary>
        /// Ensures the entity type exists by adding it by name if it did not already exist.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        private void EnsureEntityTypeExists( string entityTypeName )
        {
            Sql( string.Format( @"
                if not exists (
                select id from EntityType where name = '{0}')
                begin
                INSERT INTO [EntityType]
                           ([Name]
                           ,[FriendlyName]
                           ,[Guid])
                     VALUES
                           ('{0}'
                           ,null
                           ,newid()
                           )
                end"
                , entityTypeName )
            );
        }

        /// <summary>
        /// Adds a new attribute value for the given attributeGuid if it does not already exist.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="value">The value.</param>
        /// <param name="guid">The GUID.</param>
        public void AddAttributeValue( string attributeGuid, int entityId, string value, string guid )
        {
            Sql( string.Format( @"
                
                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{0}')

                IF NOT EXISTS(Select * FROM [AttributeValue] WHERE [Guid] = '{3}')
                    INSERT INTO [AttributeValue] (
                        [IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
                    VALUES(
                        1,@AttributeId,{1},0,'{2}','{3}')
",
                    attributeGuid,
                    entityId,
                    value,
                    guid )
            );
        }

        /// <summary>
        /// Deletes the attribute.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteAttribute( string guid )
        {
            Sql( string.Format( @"
                DELETE [Attribute] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        #endregion

        #region Block Attribute Value Methods

        /// <summary>
        /// Adds a new block attribute value for the given block guid and attribute guid,
        /// deleting any previously existing attribute value first.
        /// </summary>
        /// <param name="blockGuid">The block GUID.</param>
        /// <param name="attributeGuid">The attribute GUID.</param>
        /// <param name="value">The value.</param>
        public void AddBlockAttributeValue( string blockGuid, string attributeGuid, string value )
        {
            Sql( string.Format( @"
                
                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                -- Delete existing attribute value first (might have been created by Rock system)
                DELETE [AttributeValue]
                WHERE [AttributeId] = @AttributeId
                AND [EntityId] = @BlockId

                INSERT INTO [AttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],
                    [Order],[Value],
                    [Guid])
                VALUES(
                    1,@AttributeId,@BlockId,
                    0,'{2}',
                    NEWID())
",
                    blockGuid,
                    attributeGuid,
                    value.Replace( "'", "''" )
                )
            );
        }

        /// <summary>
        /// Deletes the block attribute value.
        /// </summary>
        /// <param name="blockGuid">The block GUID.</param>
        /// <param name="attributeGuid">The attribute GUID.</param>
        public void DeleteBlockAttributeValue( string blockGuid, string attributeGuid )
        {
            Sql( string.Format( @"

                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                DELETE [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId
",
                    blockGuid,
                    attributeGuid )
            );
        }

        #endregion

        #region DefinedType Methods

        /// <summary>
        /// Adds a new DefinedType.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        public void AddDefinedType( string category, string name, string description, string guid )
        {
            Sql( string.Format( @"
                
                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [DefinedType];

                INSERT INTO [DefinedType] (
                    [IsSystem],[FieldTypeId],[Order],
                    [Category],[Name],[Description],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,@Order,
                    '{0}','{1}','{2}',
                    '{3}')
",
                    category,
                    name,
                    description.Replace( "'", "''" ),
                    guid
                    ) );
        }

        /// <summary>
        /// Deletes the DefinedType.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteDefinedType( string guid )
        {
            Sql( string.Format( @"
                DELETE [DefinedType] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        #endregion

        #region DefinedValue Methods

        /// <summary>
        /// Adds a new DefinedValue for the given DefinedType.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        public void AddDefinedValue( string definedTypeGuid, string name, string description, string guid, bool isSystem = true )
        {
            Sql( string.Format( @"
                
                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId

                INSERT INTO [DefinedValue] (
                    [IsSystem],[DefinedTypeId],[Order],
                    [Name],[Description],
                    [Guid])
                VALUES(
                    {4},@DefinedTypeId,@Order,
                    '{1}','{2}',
                    '{3}')
",
                    definedTypeGuid,
                    name,
                    description.Replace( "'", "''" ),
                    guid,
                    isSystem.Bit().ToString()
                    ) );
        }

        /// <summary>
        /// Updates (or Adds) the defined value for the given DefinedType.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        public void UpdateDefinedValue( string definedTypeGuid, string name, string description, string guid, bool isSystem = true )
        {
            Sql( string.Format( @"

                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}')

                IF EXISTS ( SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{3}' )
                BEGIN
                    UPDATE [DefinedValue]
                    SET 
                        [IsSystem] = {4}
                        ,[DefinedTypeId] = @DefinedTypeId
                        ,[Name] = '{1}'
                        ,[Description] = '{2}'
                    WHERE
                        [Guid] = '{3}'
                END
                ELSE
                BEGIN
                    DECLARE @Order int
                    SELECT @Order = ISNULL(MAX([order])+1,0) FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId

                    INSERT INTO [DefinedValue]
                        ([IsSystem]
                        ,[DefinedTypeId]
                        ,[Order]
                        ,[Name]
                        ,[Description]
                        ,[Guid])
                    VALUES
                        ({4}
                        ,@DefinedTypeId
                        ,@Order
                        ,'{1}'
                        ,'{2}'
                        ,'{3}')
                END
",
                    definedTypeGuid,
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    guid,
                    isSystem.Bit().ToString()
                    ) );
        }

        /// <summary>
        /// Deletes the DefinedValue.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteDefinedValue( string guid )
        {
            Sql( string.Format( @"
                DELETE [DefinedValue] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        #endregion

        #region Security/Auth

        /// <summary>
        /// Adds the security role group.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        public void AddSecurityRoleGroup( string name, string description, string guid )
        {
            string sql = @"

DECLARE @groupTypeId int
SET @groupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Guid] = 'AECE949F-704C-483E-A4FB-93D5E4720C4C')

INSERT INTO [dbo].[Group]
           ([IsSystem]
           ,[ParentGroupId]
           ,[GroupTypeId]
           ,[CampusId]
           ,[Name]
           ,[Description]
           ,[IsSecurityRole]
           ,[IsActive]
           ,[Guid])
     VALUES
           (1
           ,null
           ,@groupTypeId
           ,null
           ,'{0}'
           ,'{1}'
           ,1
           ,1
           ,'{2}')
";
            Sql( string.Format( sql, name, description, guid ) );
        }

        /// <summary>
        /// Deletes the security role group.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteSecurityRoleGroup( string guid )
        {
            Sql( string.Format( "DELETE FROM [dbo].[Group] where [Guid] = '{0}'", guid ) );
        }

        /// <summary>
        /// Adds the security auth record for the given entity type and group.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="action">The action.</param>
        /// <param name="groupGuid">The group GUID.</param>
        /// <param name="authGuid">The auth GUID.</param>
        public void AddSecurityAuth( string entityTypeName, string action, string groupGuid, string authGuid )
        {
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
DECLARE @groupId int
SET @groupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{2}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = '{0}')

INSERT INTO [dbo].[Auth]
           ([EntityTypeId]
           ,[EntityId]
           ,[Order]
           ,[Action]
           ,[AllowOrDeny]
           ,[SpecialRole]
           ,[PersonId]
           ,[GroupId]
           ,[Guid])
     VALUES
           (@entityTypeId
           ,0
           ,0
           ,'{1}'
           ,'A'
           ,0
           ,null
           ,@groupId
           ,'{3}')
";
            Sql( string.Format( sql, entityTypeName, action, groupGuid, authGuid ) );
        }

        /// <summary>
        /// Deletes the security authentication for page.
        /// </summary>
        /// <param name="pageGuid">The page unique identifier.</param>
        public void DeleteSecurityAuthForPage( string pageGuid )
        {
            string sql = @"
DECLARE @pageId int
SET @pageId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = 'Rock.Model.Page')

DELETE [dbo].[Auth] 
WHERE [EntityTypeId] = @EntityTypeId
    AND [EntityId] = @pageId
";
            Sql( string.Format( sql, pageGuid ) );

        }

        /// <summary>
        /// Adds the page security authentication. Set GroupGuid to null when setting to a special role
        /// </summary>
        /// <param name="pageGuid">The page unique identifier.</param>
        /// <param name="action">The action.</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="authGuid">The authentication unique identifier.</param>
        public void AddSecurityAuthForPage( string pageGuid, int order, string action, bool allow, string groupGuid, Rock.Model.SpecialRole specialRole, string authGuid )
        {
            string entityTypeName = "Rock.Model.Page";
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
DECLARE @groupId int
SET @groupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = '{1}')

DECLARE @pageId int
SET @pageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{2}')

INSERT INTO [dbo].[Auth]
           ([EntityTypeId]
           ,[EntityId]
           ,[Order]
           ,[Action]
           ,[AllowOrDeny]
           ,[SpecialRole]
           ,[PersonId]
           ,[GroupId]
           ,[Guid])
     VALUES
           (@entityTypeId
           ,@pageId
           ,{6}
           ,'{3}'
           ,'{7}'
           ,{4}
           ,null
           ,@groupId
           ,'{5}')
";
            Sql( string.Format( sql, groupGuid ?? Guid.Empty.ToString(), entityTypeName, pageGuid, action, specialRole.ConvertToInt(), authGuid, order,
                ( allow ? "A" : "D" ) ) );
        }


        /// <summary>
        /// Deletes the security authentication for block.
        /// </summary>
        /// <param name="blockGuid">The block unique identifier.</param>
        public void DeleteSecurityAuthForBlock( string blockGuid )
        {
            string sql = @"
DECLARE @blockId int
SET @blockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = 'Rock.Model.Block')

DELETE [dbo].[Auth] 
WHERE [EntityTypeId] = @EntityTypeId
    AND [EntityId] = @blockId
";
            Sql( string.Format( sql, blockGuid ) );

        }
        /// <summary>
        /// Adds the page security authentication. Set GroupGuid to null when setting to a special role
        /// </summary>
        /// <param name="pageGuid">The page unique identifier.</param>
        /// <param name="action">The action.</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="authGuid">The authentication unique identifier.</param>
        public void AddSecurityAuthForBlock( string blockGuid, int order, string action, bool allow, string groupGuid, Rock.Model.SpecialRole specialRole, string authGuid )
        {
            string entityTypeName = "Rock.Model.Block";
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
DECLARE @groupId int
SET @groupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = '{1}')

DECLARE @blockId int
SET @blockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{2}')

INSERT INTO [dbo].[Auth]
           ([EntityTypeId]
           ,[EntityId]
           ,[Order]
           ,[Action]
           ,[AllowOrDeny]
           ,[SpecialRole]
           ,[PersonId]
           ,[GroupId]
           ,[Guid])
     VALUES
           (@entityTypeId
           ,@blockId
           ,{6}
           ,'{3}'
           ,'{7}'
           ,{4}
           ,null
           ,@groupId
           ,'{5}')
";
            Sql( string.Format( sql, groupGuid ?? Guid.Empty.ToString(), entityTypeName, blockGuid, action, specialRole.ConvertToInt(), authGuid, order,
                ( allow ? "A" : "D" ) ) );
        }

        /// <summary>
        /// Adds the binaryfiletype security authentication. Set GroupGuid to null when setting to a special role
        /// </summary>
        /// <param name="binaryFileTypeGuid">The binary file type unique identifier.</param>
        /// <param name="order">The order.</param>
        /// <param name="action">The action.</param>
        /// <param name="allow">if set to <c>true</c> [allow].</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="authGuid">The authentication unique identifier.</param>
        public void AddSecurityAuthForBinaryFileType( string binaryFileTypeGuid, int order, string action, bool allow, string groupGuid, Rock.Model.SpecialRole specialRole, string authGuid )
        {
            string entityTypeName = "Rock.Model.BinaryFileType";
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
DECLARE @groupId int
SET @groupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = '{1}')

DECLARE @binaryFileTypeId int
SET @binaryFileTypeId = (SELECT [Id] FROM [BinaryFileType] WHERE [Guid] = '{2}')

INSERT INTO [dbo].[Auth]
           ([EntityTypeId]
           ,[EntityId]
           ,[Order]
           ,[Action]
           ,[AllowOrDeny]
           ,[SpecialRole]
           ,[PersonId]
           ,[GroupId]
           ,[Guid])
     VALUES
           (@entityTypeId
           ,@binaryFileTypeId
           ,{6}
           ,'{3}'
           ,'{7}'
           ,{4}
           ,null
           ,@groupId
           ,'{5}')
";
            Sql( string.Format( sql, groupGuid ?? Guid.Empty.ToString(), entityTypeName, binaryFileTypeGuid, action, specialRole.ConvertToInt(), authGuid, order,
                ( allow ? "A" : "D" ) ) );
        }

        /// <summary>
        /// Deletes the security authentication for groupType.
        /// </summary>
        /// <param name="groupTypeGuid">The groupType unique identifier.</param>
        public void DeleteSecurityAuthForGroupType( string groupTypeGuid )
        {
            string sql = @"
DECLARE @groupTypeId int
SET @groupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = 'Rock.Model.GroupType')

DELETE [dbo].[Auth] 
WHERE [EntityTypeId] = @EntityTypeId
    AND [EntityId] = @groupTypeId
";
            Sql( string.Format( sql, groupTypeGuid ) );

        }
        /// <summary>
        /// Adds the page security authentication. Set GroupGuid to null when setting to a special role
        /// </summary>
        /// <param name="pageGuid">The page unique identifier.</param>
        /// <param name="action">The action.</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="authGuid">The authentication unique identifier.</param>
        public void AddSecurityAuthForGroupType( string groupTypeGuid, int order, string action, bool allow, string groupGuid, Rock.Model.SpecialRole specialRole, string authGuid )
        {
            string entityTypeName = "Rock.Model.GroupType";
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
DECLARE @groupId int
SET @groupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = '{1}')

DECLARE @groupTypeId int
SET @groupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '{2}')

INSERT INTO [dbo].[Auth]
           ([EntityTypeId]
           ,[EntityId]
           ,[Order]
           ,[Action]
           ,[AllowOrDeny]
           ,[SpecialRole]
           ,[PersonId]
           ,[GroupId]
           ,[Guid])
     VALUES
           (@entityTypeId
           ,@groupTypeId
           ,{6}
           ,'{3}'
           ,'{7}'
           ,{4}
           ,null
           ,@groupId
           ,'{5}')
";
            Sql( string.Format( sql, groupGuid ?? Guid.Empty.ToString(), entityTypeName, groupTypeGuid, action, specialRole.ConvertToInt(), authGuid, order,
                ( allow ? "A" : "D" ) ) );
        }


        /// <summary>
        /// Deletes the security auth record.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteSecurityAuth( string guid )
        {
            Sql( string.Format( "DELETE FROM [dbo].[Auth] where [Guid] = '{0}'", guid ) );
        }

        #endregion

        #region Group Type

        /// <summary>
        /// Adds a new GroupType "Group Attribute" for the given GroupType using the given values. 
        /// </summary>
        /// <param name="groupTypeGuid"></param>
        /// <param name="fieldTypeGuid"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="order"></param>
        /// <param name="defaultValue">a string, empty string, or NULL</param>
        /// <param name="guid"></param>
        public void AddGroupTypeGroupAttribute( string groupTypeGuid, string fieldTypeGuid, string name, string description, int order, string defaultValue, string guid )
        {
            Sql( string.Format( @"

                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Group')
                 
                DECLARE @GroupTypeId int
                SET @GroupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute] 
                WHERE
                    [EntityTypeId] = @EntityTypeId
                    AND [Key] = '{2}'
                    AND [EntityTypeQualifierColumn] = 'GroupTypeId'
                    AND [EntityTypeQualifierValue] = @GroupTypeId

                INSERT INTO [Attribute]
                    ([IsSystem]
                    ,[FieldTypeId]
                    ,[EntityTypeId]
                    ,[EntityTypeQualifierColumn]
                    ,[EntityTypeQualifierValue]
                    ,[Key]
                    ,[Name]
                    ,[Description]
                    ,[Order]
                    ,[IsGridColumn]
                    ,[DefaultValue]
                    ,[IsMultiValue]
                    ,[IsRequired]
                    ,[Guid])
                VALUES
                    (1
                    ,@FieldTypeId
                    ,@EntityTypeId
                    ,'GroupTypeId'
                    ,@GroupTypeId
                    ,'{2}'
                    ,'{3}'
                    ,'{4}'
                    ,{5}
                    ,0
                    ,{6}
                    ,0
                    ,0
                    ,'{7}')
",
                    groupTypeGuid,
                    fieldTypeGuid,
                    name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    (defaultValue == null) ? "NULL" : "'" + defaultValue +"'",
                    guid)
            );
        }
        #endregion


        #region PersonAttribute

        /// <summary>
        /// Updates the BlockType Attribute for the given blocktype and key (if it exists);
        /// otherwise it inserts a new record.
        /// </summary>
        /// <param name="blockTypeGuid"></param>
        /// <param name="fieldTypeGuid"></param>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <param name="category"></param>
        /// <param name="description"></param>
        /// <param name="order"></param>
        /// <param name="defaultValue"></param>
        /// <param name="guid"></param>
        public void UpdatePersonAttribute( string fieldTypeGuid, string categoryGuid, string name, string key, string iconCssClass, string description, int order, string defaultValue, string guid )
        {

            Sql( string.Format( @"
                
                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{0}')

                DECLARE @EntityTypeId int                
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person')

                IF EXISTS (
                    SELECT [Id] 
                    FROM [Attribute] 
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = ''
                    AND [EntityTypeQualifierValue] = ''
                    AND [Key] = '{1}' )
                BEGIN
                    UPDATE [Attribute] SET
                        [Name] = '{2}',
                        [IconCssClass] = '{3}',
                        [Description] = '{4}',
                        [Order] = {5},
                        [DefaultValue] = '{6}',
                        [Guid] = '{7}'
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = ''
                    AND [EntityTypeQualifierValue] = ''
                    AND [Key] = '{1}'
                END
                ELSE
                BEGIN
                    INSERT INTO [Attribute] (
                        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                        [Key],[Name],[IconCssClass],[Description],
                        [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                        [Guid])
                    VALUES(
                        1,@FieldTypeId, @EntityTypeId,'','',
                        '{1}','{2}','{3}','{4}',
                        {5},0,'{6}',0,0,
                        '{7}')  
                END
",
                    fieldTypeGuid,
                    key ?? name.Replace( " ", string.Empty ),
                    name,
                    iconCssClass,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue.Replace( "'", "''" ),
                    guid )
            );


            Sql( string.Format( @"
                
                DECLARE @AttributeId int                
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{0}')

                DECLARE @CategoryId int
                SET @CategoryId = (SELECT [Id] FROM [Category] WHERE [Guid] = '{1}')

                IF NOT EXISTS (
                    SELECT *
                    FROM [AttributeCategory] 
                    WHERE [AttributeId] = @AttributeId
                    AND [CategoryId] = CategoryId )
                BEGIN
                    INSERT INTO [AttributeCategory] ( [AttributeId], [CategoryId] )
                    VALUES( @AttributeId, @CategoryId )  
                END
",
                    guid,
                    categoryGuid )
            );

        }

        #endregion

        #region PersonBadge

        /// <summary>
        /// Updates the PersonBadge by Guid (if it exists); otherwise it inserts a new record.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="order">The order.</param>
        /// <param name="guid">The unique identifier.</param>
        public void UpdatePersonBadge( string name, string description, string entityTypeName, int order, string guid )
        {
            Sql( string.Format( @"
                    DECLARE @EntityTypeId int = (SELECT [ID] FROM [EntityType] WHERE [Name] = '{2}')
	                
                    IF EXISTS ( SELECT * FROM [PersonBadge] where [Guid] = '{4}')
                    BEGIN
                        UPDATE [PersonBadge] set 
                            [Name] = '{0}',
                            [Description] = '{1}',
                            [EntityTypeId] = @EntityTypeId,
                            [Order] = {3}
                        WHERE [Guid] = '{4}'
                        
                    END
                    ELSE
                    BEGIN
                        INSERT INTO [PersonBadge] ([Name],[Description],[EntityTypeId],[Order],[Guid])
                            VALUES ('{0}', '{1}', @EntityTypeId, {3}, '{4}')
                    END

",
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    entityTypeName,
                    order,
                    guid )
            );
        }
        
        /// <summary>
        /// Adds (or Deletes and Adds) the person badge attribute.
        /// </summary>
        /// <param name="personBadgeGuid">The person badge unique identifier.</param>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The unique identifier.</param>
        public void AddPersonBadgeAttribute( string personBadgeGuid, string fieldTypeGuid, string name, string key, string description, int order, string defaultValue, string guid )
        {
            Sql( string.Format( @"
                
                DECLARE @PersonBadgeId int
                SET @PersonBadgeId = (SELECT [Id] FROM [PersonBadge] WHERE [Guid] = '{0}')

                DECLARE @PersonBadgeEntityTypeId int
                SET @PersonBadgeEntityTypeId = (SELECT [EntityTypeId] FROM [PersonBadge] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                -- get the EntityTypeId for 'Rock.Model.PersonBadge'
                DECLARE @EntityTypeId int                
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PersonBadge')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute] 
                WHERE [EntityTypeId] = @EntityTypeId
                AND [EntityTypeQualifierColumn] = 'EntityTypeId'
                AND [EntityTypeQualifierValue] = CAST(@PersonBadgeEntityTypeId as varchar)
                AND [Key] = '{2}'

                INSERT INTO [Attribute] (
                    [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                    [Key],[Name],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [Guid])
                VALUES(
                    1,@FieldTypeId, @EntityTypeId,'EntityTypeId',CAST(@PersonBadgeEntityTypeId as varchar),
                    '{2}','{3}','{4}',
                    {5},0,'{6}',0,0,
                    '{7}')
",
                    personBadgeGuid,
                    fieldTypeGuid,
                    key ?? name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue.Replace( "'", "''" ),
                    guid )
            );
        }

        /// <summary>
        /// Adds/Updates the person badge attribute value.
        /// </summary>
        /// <param name="personBadgeGuid">The person badge unique identifier.</param>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        /// <param name="value">The value.</param>
        public void AddPersonBadgeAttributeValue( string personBadgeGuid, string attributeGuid, string value )
        {
            Sql( string.Format( @"
                
                DECLARE @PersonBadgeId int
                SET @PersonBadgeId = (SELECT [Id] FROM [PersonBadge] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                -- Delete existing attribute value first (might have been created by Rock system)
                DELETE [AttributeValue]
                WHERE [AttributeId] = @AttributeId
                AND [EntityId] = @PersonBadgeId

                INSERT INTO [AttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],
                    [Order],[Value],
                    [Guid])
                VALUES(
                    1,@AttributeId,@PersonBadgeId,
                    0,'{2}',
                    NEWID())
",
                    personBadgeGuid,
                    attributeGuid,
                    value.Replace( "'", "''" )
                )
            );
        }

        #endregion
    }
}