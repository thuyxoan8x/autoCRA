using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AlloyTemplates.Automation.Models;
using AlloyTemplates.Models.Pages;
using EPiServer;
using EPiServer.Approvals;
using EPiServer.Approvals.ContentApprovals;
using EPiServer.ContentApi.Cms;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Search;
using EPiServer.ContentDefinitionsApi;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.Security.Internal;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Security;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Mvc;

namespace AlloyTemplates.Automation.Controllers
{
    public class SetupController : Controller
    {
        private readonly IContentRepository _contentRepository;
        private readonly ISiteDefinitionRepository _siteDefinitionRepository;
        private readonly ILanguageBranchRepository _languageBranchRepository;
        private readonly IAvailableSettingsRepository _availableSettingsRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IContentSecurityRepository _contentSecurityRepository;
        private readonly UIRoleProvider _roleProvider;
        private readonly IApprovalDefinitionRepository _approvalDefinitionRepository;
        private readonly ContentTypeAvailabilityService _contentTypeAvailabilityService;
        private readonly ContentApiOptions _contentApiOptions;
        private readonly ContentDeliveryApiOptions _contentDeliveryApiOptions;
        private readonly ContentDefinitionsApiOptions _contentDefinitionsApiOptions;
        private readonly ContentSearchApiOptions _contentSearchApiOptions;
        private readonly RoutingOptions _routingOption;
        private readonly ContentLanguageSettingRepository _contentLanguageSettingRepository;
        private readonly ISynchronizedUsersRepository _synchronizedUsersRepository;

        private const string ContainerPageTypeGuid = "D178950C-D20E-4A46-90BD-5338B2424745";
        private const string CmsAdminUsername = "cmsadmin";
        private const string WebAdminsGroupName = "WebAdmins";
        private const string WebEditorsGroupName = "WebEditors";

        public SetupController(
            IContentRepository contentRepository,
            ISiteDefinitionRepository siteDefinitionRepository,
            ILanguageBranchRepository languageBranchRepository,
            IContentTypeRepository contentTypeRepository,
            IAvailableSettingsRepository availableSettingsRepository,
            IContentSecurityRepository contentSecurityRepository,
            ContentTypeAvailabilityService contentTypeAvailabilityService,
            ContentApiOptions contentApiOptions,
            ContentDeliveryApiOptions contentDeliveryApiOptions,
            ContentDefinitionsApiOptions contentDefinitionsApiOptions,
            ContentSearchApiOptions contentSearchApiOptions,
            RoutingOptions routingOption,
            UIRoleProvider roleProvider,
            IApprovalDefinitionRepository approvalDefinitionRepository,
            ContentLanguageSettingRepository contentLanguageSettingRepository,
            ISynchronizedUsersRepository synchronizedUsersRepository)
        {
            _contentRepository = contentRepository;
            _siteDefinitionRepository = siteDefinitionRepository;
            _languageBranchRepository = languageBranchRepository;
            _contentTypeRepository = contentTypeRepository;
            _availableSettingsRepository = availableSettingsRepository;
            _contentSecurityRepository = contentSecurityRepository;
            _contentTypeAvailabilityService = contentTypeAvailabilityService;
            _roleProvider = roleProvider;
            _approvalDefinitionRepository = approvalDefinitionRepository;
            _contentApiOptions = contentApiOptions;
            _contentDeliveryApiOptions = contentDeliveryApiOptions;
            _contentSearchApiOptions = contentSearchApiOptions;
            _contentDefinitionsApiOptions = contentDefinitionsApiOptions;
            _routingOption = routingOption;
            _contentLanguageSettingRepository = contentLanguageSettingRepository;
            _synchronizedUsersRepository = synchronizedUsersRepository;
        }

        #region New Setup
        [Route("/Automation/Setup")]
        public async Task<Dictionary<string,int>> Setup(string siteDomain)
        {
            AdjustSettings();
            UpdateSiteDomain(siteDomain);

            var env = await SetupPagesAndFolders();

            return env;
        }

        private void UpdateSiteDomain(string siteDomain)
        {
            if (string.IsNullOrWhiteSpace(siteDomain))
            {
                return;
            }

            var currentSite = SiteDefinition.Current.CreateWritableClone();

            currentSite.SiteUrl = new Uri($"https://{siteDomain}");
            _siteDefinitionRepository.Save(currentSite);
        }

        private void AdjustSettings()
        {
            EnsureContentFolderIsAllowedUnderStartPage(SiteDefinition.Current.StartPage);

            // ensure site using site-specific asset
            if (SiteDefinition.Current.SiteAssetsRoot == SiteDefinition.Current.GlobalAssetsRoot)
            {
                var siteFolder = _contentRepository.GetBySegment(ContentReference.StartPage, SiteDefinition.SiteAssetsName, CultureInfo.InvariantCulture);
                if (siteFolder == null)
                {
                    siteFolder = _contentRepository.Get<ContentFolder>(CreateSystemFolder(SiteDefinition.Current.StartPage, SiteDefinition.SiteAssetsName).ToReferenceWithoutVersion());
                }

                var clone = SiteDefinition.Current.CreateWritableClone();

                clone.SiteAssetsRoot = siteFolder.ContentLink;
                _siteDefinitionRepository.Save(clone);
            }

            // enable es language
            var branch = _languageBranchRepository.Load("es");
            if (branch is not null)
            {
                branch = branch.CreateWritableClone();
                branch.Enabled = true;

                _languageBranchRepository.Save(branch);
            }

            // set Available Page Types = All for Container Page
            var contentType = _contentTypeRepository.Load(Guid.Parse(ContainerPageTypeGuid));
            var allPageTypes = _contentTypeRepository.List().Where(t => t.Base == ContentTypeBase.Page);
            var settings = new AvailableSetting();
            foreach (var pageType in allPageTypes)
            {
                settings.AllowedContentTypeNames.Add(pageType.Name);
            }

            settings.Availability = Availability.All;
            _availableSettingsRepository.RegisterSetting(contentType, settings);
        }

        private ContentReference CreateSystemFolder(ContentReference parent, string name)
        {
            var siteFolder = _contentRepository.GetDefault<ContentFolder>(parent, CultureInfo.InvariantCulture);
            siteFolder.Name = name;
            siteFolder.RouteSegment = name;
            return _contentRepository.Save(siteFolder, SaveAction.Publish, AccessLevel.NoAccess);
        }

        private void EnsureContentFolderIsAllowedUnderStartPage(ContentReference startPage)
        {
            var contentType =
                _contentTypeRepository.Load(_contentRepository.Get<IContent>(startPage, CultureInfo.InvariantCulture).ContentTypeID);
            var contentFolderType = _contentTypeRepository.Load(typeof(ContentFolder));

            if (!_contentTypeAvailabilityService.IsAllowed(contentType.Name, contentFolderType.Name))
            {
                var currentSettings = _contentTypeAvailabilityService.GetSetting(contentType.Name);
                currentSettings.Availability = Availability.Specific;
                currentSettings.AllowedContentTypeNames.Add(contentFolderType.Name);
                _availableSettingsRepository.RegisterSetting(contentType, currentSettings);
            }
        }

        private async Task<Dictionary<string, int>> SetupPagesAndFolders()
        {
            await AddRole(EnvironmentBase.ContentApiRead, AccessLevel.Read);
            await AddRole(EnvironmentBase.ContentApiWrite, AccessLevel.Read);
            await AddApplication(EnvironmentBase.ApplicationName, AccessLevel.FullAccess);

            RevokeAccessOfEveryoneGroup();

            var info = await GenerateEnvironmentVariableItemsAsync();
            return info;
        }

        private async Task AddRole(string roleName, AccessLevel accessLevel)
        {
            if (await _roleProvider.RoleExistsAsync(roleName))
            {
                await _roleProvider.DeleteRoleAsync(roleName, false);
            }

            await _roleProvider.CreateRoleAsync(roleName);

            var permissions = (IContentSecurityDescriptor)_contentSecurityRepository.Get(ContentReference.RootPage).CreateWritableClone();
            permissions.AddEntry(new AccessControlEntry(roleName, accessLevel));

            _contentSecurityRepository.Save(ContentReference.RootPage, permissions, SecuritySaveType.Replace);
            _contentSecurityRepository.Save(ContentReference.WasteBasket, permissions, SecuritySaveType.Replace);
        }

        private async Task AddApplication(string applicationName, AccessLevel accessLevel)
        {
            var permissions = (IContentSecurityDescriptor)_contentSecurityRepository.Get(ContentReference.RootPage).CreateWritableClone();
            permissions.AddEntry(new AccessControlEntry(applicationName, accessLevel, SecurityEntityType.Application));

            _contentSecurityRepository.Save(ContentReference.RootPage, permissions, SecuritySaveType.Replace);
            _contentSecurityRepository.Save(ContentReference.WasteBasket, permissions, SecuritySaveType.Replace);
        }

        private void RevokeAccessOfEveryoneGroup()
        {
            var securityDescriptor =
                _contentSecurityRepository.Get(ContentReference.RootPage).CreateWritableClone() as IContentSecurityDescriptor;
            var entries = securityDescriptor!.Entries.Where(e => e.Name != EveryoneRole.RoleName).ToList();

            securityDescriptor.Clear();

            foreach (var entry in entries)
            {
                securityDescriptor.AddEntry(entry);
            }

            _contentSecurityRepository.Save(ContentReference.RootPage, securityDescriptor, SecuritySaveType.Replace);
        }

        private async Task<Dictionary<string, int>> GenerateEnvironmentVariableItemsAsync()
        {
            Dictionary<string, int> variables = new Dictionary<string, int>();
            variables.Add("forThisSiteId", SiteDefinition.Current.SiteAssetsRoot.ID);
            variables.Add("testContainerFolderId", CreateEmptyTestFolderUnderGlobalAssetRoot().ID);
            var testPageReference = CreateContainerPageUnderStartPage();
            variables.Add("testContainerPageId", testPageReference.ID);
            variables.Add("deniedPageId", CreatePublishedPageWithNoAccessForCmsadmin(testPageReference).ID);
            variables.Add("draftPageId", CreateDraft(testPageReference).ID);
            variables.Add("approvalSequenceParentId", (await CreatePublishedPageWithApprovalSequenceEnabled(testPageReference)).ID);
            variables.Add("noPublishedRightParentId", CreatePublishedPageWithoutPublishAccessRight(testPageReference).ID);
            variables.Add("noAPIAccessParentId", CreatePublishedPageWithoutContentApiWriteAccessRight(testPageReference).ID);

            return variables;
        }

        /// <summary>
        /// Creates a published container page under Start, that contains all other pages created for automation
        /// testing purpose.
        /// </summary>
        /// <returns>The reference to the created container page</returns>
        private ContentReference CreateContainerPageUnderStartPage()
        {
            const string testPageName = "Automation Test";

            var children = _contentRepository.GetChildren<IContent>(ContentReference.StartPage);
            var pageA = children.FirstOrDefault(c => c.Name == testPageName);

            if (pageA is not null)
            {
                _contentRepository.Delete(pageA.ContentLink, true, AccessLevel.NoAccess);
            }

            pageA = _contentRepository.GetDefault<ContainerPage>(ContentReference.StartPage);
            pageA.Name = testPageName;

            return _contentRepository.Save(pageA, SaveAction.Publish, AccessLevel.NoAccess);
        }

        private ContentReference CreateEmptyTestFolderUnderGlobalAssetRoot()
        {
            const string testFolderName = "Automation Test";

            var folders = _contentRepository.GetChildren<ContentFolder>(SiteDefinition.Current.GlobalAssetsRoot);
            var testFolder = folders.FirstOrDefault(f => f.Name == testFolderName);

            if (testFolder is not null)
            {
                _contentRepository.Delete(testFolder.ContentLink, true, AccessLevel.NoAccess);
            }

            testFolder = _contentRepository.GetDefault<ContentFolder>(SiteDefinition.Current.GlobalAssetsRoot);
            testFolder.Name = testFolderName;

            return _contentRepository.Save(testFolder, SaveAction.Publish, AccessLevel.NoAccess);
        }

        private ContentReference CreatePublishedPageWithNoAccessForCmsadmin(ContentReference parentLink)
        {
            var noAccessForCmsadminUserPage = _contentRepository.GetDefault<ContainerPage>(parentLink);
            noAccessForCmsadminUserPage.Name = "No access for cmsadmin";
            var pageLink = _contentRepository.Save(noAccessForCmsadminUserPage, SaveAction.Publish, AccessLevel.NoAccess);

            var entries = _contentSecurityRepository.Get(pageLink).Entries
                .Where(e => e.Name != WebAdminsGroupName && e.Name != WebEditorsGroupName && e.Name != EnvironmentBase.ApplicationName)
                .ToList();
            var securityDescriptor = new ContentAccessControlList(pageLink).CreateWritableClone();

            foreach (var entry in entries)
            {
                securityDescriptor.AddEntry(entry);
            }

            _contentSecurityRepository.Save(pageLink, securityDescriptor, SecuritySaveType.Replace);

            return pageLink;
        }

        private ContentReference CreateDraft(ContentReference parentLink)
        {
            var draft = _contentRepository.GetDefault<ContainerPage>(parentLink);
            draft.Name = "Draft";

            return _contentRepository.Save(draft, SaveAction.Default, AccessLevel.NoAccess);
        }

        private async Task<ContentReference> CreatePublishedPageWithApprovalSequenceEnabled(ContentReference parentLink)
        {
            var approvalSequenceEnabledPage = _contentRepository.GetDefault<ContainerPage>(parentLink);
            approvalSequenceEnabledPage.Name = "Approval sequence enabled";
            var pageLink = _contentRepository.Save(approvalSequenceEnabledPage, SaveAction.Publish, AccessLevel.NoAccess);

            var approvalDefinition = new ContentApprovalDefinition
            {
                ContentLink = pageLink,
                Steps = new List<ApprovalDefinitionStep>
                {
                    new()
                    {
                        Name = "Single step",
                        Reviewers = new List<ApprovalDefinitionReviewer>
                        {
                            new()
                            {
                                Name = CmsAdminUsername,
                                ReviewerType = ApprovalDefinitionReviewerType.User,
                                Languages = _languageBranchRepository.ListEnabled().Select(x => x.Culture).ToList()
                            }
                        }
                    }
                },
                SavedBy = CmsAdminUsername,
            };
            await _approvalDefinitionRepository.SaveAsync(approvalDefinition);

            return pageLink;
        }

        private ContentReference CreatePublishedPageWithoutPublishAccessRight(ContentReference parentLink)
        {
            var noPublishAccessRightPage = _contentRepository.GetDefault<ContainerPage>(parentLink);
            noPublishAccessRightPage.Name = "No Publish access right";
            var pageLink = _contentRepository.Save(noPublishAccessRightPage, SaveAction.Publish, AccessLevel.NoAccess);

            var entriesWithoutPublishAccessRight = _contentSecurityRepository.Get(pageLink).Entries
                .Select(e => new AccessControlEntry(e.Name, e.Access & ~AccessLevel.Publish, e.EntityType))
                .ToList();
            var securityDescriptor = new ContentAccessControlList(pageLink).CreateWritableClone();

            foreach (var entry in entriesWithoutPublishAccessRight)
            {
                securityDescriptor.AddEntry(entry);
            }

            _contentSecurityRepository.Save(pageLink, securityDescriptor, SecuritySaveType.Replace);

            return pageLink;
        }

        private ContentReference CreatePublishedPageWithoutContentApiWriteAccessRight(ContentReference parentLink)
        {
            var noContentApiWritePage = _contentRepository.GetDefault<ContainerPage>(parentLink);
            noContentApiWritePage.Name = $"No {EnvironmentBase.ContentApiWrite} access right";
            var pageLink = _contentRepository.Save(noContentApiWritePage, SaveAction.Publish, AccessLevel.NoAccess);

            var entriesWithoutContentApiWrite = _contentSecurityRepository.Get(pageLink).Entries
                .Where(e => e.Name != EnvironmentBase.ContentApiWrite)
                .ToList();
            var securityDescriptor = new ContentAccessControlList(pageLink);

            foreach (var entry in entriesWithoutContentApiWrite)
            {
                securityDescriptor.AddEntry(entry);
            }

            _contentSecurityRepository.Save(pageLink, securityDescriptor, SecuritySaveType.Replace);

            return pageLink;
        }
        #endregion

        [Route("/Automation/GrantAccessRightsToContent")]
        public IActionResult GrantAccessRightsToContent()
        {
            string level = GetRequestQueryString("accessLevel");
            AccessLevel accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), level);
            string contentId = GetRequestQueryString("id");
            try
            {
                var pageLink = _contentRepository.Get<PageData>(new ContentReference(contentId)).CreateWritableClone().ContentLink;

                var securityDescriptor = new ContentAccessControlList(pageLink).CreateWritableClone();
                securityDescriptor.AddEntry(new AccessControlEntry(WebAdminsGroupName, accessLevel));
                securityDescriptor.AddEntry(new AccessControlEntry(EnvironmentBase.ApplicationName, accessLevel, SecurityEntityType.Application));
                securityDescriptor.AddEntry(new AccessControlEntry("Administrator", AccessLevel.FullAccess));
                securityDescriptor.AddEntry(new AccessControlEntry(EnvironmentBase.ContentApiWrite, AccessLevel.Read));
                securityDescriptor.AddEntry(new AccessControlEntry(EnvironmentBase.ContentApiRead, AccessLevel.Read));

                _contentSecurityRepository.Save(pageLink, securityDescriptor, SecuritySaveType.Replace);
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = "Error", messge = ex.Message });
            }
            return Ok(new { message = $"Grant access rights for {contentId} as {accessLevel} done" });
        }

        [Produces("application/json")]
        [HttpPost("/Automation/SetContentApiOption")]
        public IActionResult SetContentApiOption([FromBody] SetApiOptions options)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            try
            {
                if (!string.IsNullOrEmpty(options.FlattenPropertyModel))
                {
                    _contentApiOptions.SetFlattenPropertyModel(bool.Parse(options.FlattenPropertyModel));
                    messages.Add("FlattenPropertyModel", options.FlattenPropertyModel);
                }
                if (!string.IsNullOrEmpty(options.ForceAbsolute))
                {
                    _contentApiOptions.SetForceAbsolute(bool.Parse(options.ForceAbsolute));
                    messages.Add("ForceAbsolute", options.ForceAbsolute);
                }
                if (!string.IsNullOrEmpty(options.IncludeInternalContentRoots))
                {
                    _contentApiOptions.SetIncludeInternalContentRoots(bool.Parse(options.IncludeInternalContentRoots));
                    messages.Add("IncludeInternalContentRoots", options.IncludeInternalContentRoots);
                }
                if (!string.IsNullOrEmpty(options.ExpandedBehavior))
                {
                    ExpandedLanguageBehavior expOpt = (options.ExpandedBehavior == "RequestedLanguage") ? ExpandedLanguageBehavior.RequestedLanguage : ExpandedLanguageBehavior.ContentLanguage;
                    _contentApiOptions.SetExpandedBehavior(expOpt);
                    messages.Add("ExpandedBehavior", options.ExpandedBehavior);
                }
                if (!string.IsNullOrEmpty(options.IncludeEmptyContentProperties))
                {
                    _contentApiOptions.IncludeEmptyContentProperties = bool.Parse(options.IncludeEmptyContentProperties);
                    messages.Add("IncludeEmptyContentProperties", options.IncludeEmptyContentProperties);
                }
                if (!string.IsNullOrEmpty(options.IncludeMasterLanguage))
                {
                    _contentApiOptions.SetIncludeMasterLanguage(bool.Parse(options.IncludeMasterLanguage));
                    messages.Add("IncludeMasterLanguage", options.IncludeMasterLanguage);
                }
                if (!string.IsNullOrEmpty(options.IncludeNumericContentIdentifier))
                {
                    _contentApiOptions.SetIncludeNumericContentIdentifier(bool.Parse(options.IncludeNumericContentIdentifier));
                    messages.Add("IncludeNumericContentIdentifier", options.IncludeNumericContentIdentifier);
                }
                if (!string.IsNullOrEmpty(options.IncludeSiteHosts))
                {
                    _contentApiOptions.SetIncludeSiteHosts(bool.Parse(options.IncludeSiteHosts));
                    messages.Add("IncludeSiteHosts", options.IncludeSiteHosts);
                }
                if (!string.IsNullOrEmpty(options.MultiSiteFilteringEnabled))
                {
                    _contentApiOptions.SetMultiSiteFilteringEnabled(bool.Parse(options.MultiSiteFilteringEnabled));
                    messages.Add("MultiSiteFilteringEnabled", options.MultiSiteFilteringEnabled);
                }
                if (!string.IsNullOrEmpty(options.ValidateTemplateForContentUrl))
                {
                    _contentApiOptions.SetValidateTemplateForContentUrl(bool.Parse(options.ValidateTemplateForContentUrl));
                    messages.Add("ValidateTemplateForContentUrl", options.ValidateTemplateForContentUrl);
                }
                if (!string.IsNullOrEmpty(options.IncludeMetadataPropertiesPreview))
                {
                    _contentApiOptions.IncludeMetadataPropertiesPreview = bool.Parse(options.IncludeMetadataPropertiesPreview);
                    messages.Add("IncludeMetadataPropertiesPreview", options.IncludeMetadataPropertiesPreview);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = "Error", messge = ex.Message });
            }
            var response = new { message = "Set ContentApiOption done", options = messages };
            return Ok(response);
        }

        [Produces("application/json")]
        [HttpPost("/Automation/SetContentDeliveryApiOption")]
        public IActionResult SetContentDeliveryApiOptions([FromBody] SetApiOptions options)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            try
            {
                if (!string.IsNullOrEmpty(options.SiteDefinitionApiEnabled))
                {
                    _contentDeliveryApiOptions.SiteDefinitionApiEnabled = bool.Parse(options.SiteDefinitionApiEnabled);
                    messages.Add("SiteDefinitionApiEnabled", options.SiteDefinitionApiEnabled);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = "Error", messge = ex.Message });
            }
            var response = new { message = "Set ContentDeliveryApiOptions done", options = messages };
            return Ok(response);
        }

        [Produces("application/json")]
        [HttpPost("/Automation/SetContentDefinitionsApiOption")]
        public IActionResult SetContentDefinitionsApiOption([FromBody] SetApiOptions options)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            try
            {
                if (!string.IsNullOrEmpty(options.IncludeRequiredPreview))
                {
                    _contentDefinitionsApiOptions.IncludeRequiredPreview = bool.Parse(options.IncludeRequiredPreview);
                    messages.Add("IncludeRequiredPreview", options.IncludeRequiredPreview);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = "Error", messge = ex.Message });
            }
            var response = new { message = "Set ContentDefinitionsApiOption done", options = messages };
            return Ok(response);
        }

        [Produces("application/json")]
        [HttpPost("/Automation/SetContentSearchApiOption")]
        public IActionResult SetContentSearchApiOptions([FromBody] SetApiOptions options)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            try
            {
                if (!string.IsNullOrEmpty(options.MaximumSearchResults))
                {
                    _contentSearchApiOptions.MaximumSearchResults = int.Parse(options.MaximumSearchResults);
                    messages.Add("MaximumSearchResults", options.MaximumSearchResults);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = "Error", messge = ex.Message });
            }
            var response = new { message = "Set ContentSearchApiOption done", options = messages };
            return Ok(response);
        }

        public class SetApiOptions
        {
            public string FlattenPropertyModel { get; set; }
            public string ForceAbsolute { get; set; }
            public string IncludeInternalContentRoots { get; set; }
            public string ExpandedBehavior { get; set; }
            public string IncludeEmptyContentProperties { get; set; }
            public string IncludeMasterLanguage { get; set; }
            public string IncludeNumericContentIdentifier { get; set; }
            public string IncludeSiteHosts { get; set; }
            public string MultiSiteFilteringEnabled { get; set; }
            public string ValidateTemplateForContentUrl { get; set; }
            public string SiteDefinitionApiEnabled { get; set; }
            public string BaseRoute { get; set; }
            public string IncludeRequiredPreview { get; set; }
            public string IncludeMetadataPropertiesPreview { get; set; }
            public string MaximumSearchResults { get; set; }
        }

        [Route("/Automation/SetAssetFriendlyUrl")]
        public IActionResult SetAssetFriendlyUrl()
        {
            bool isEnable = bool.Parse(GetRequestQueryString("enable"));
            _routingOption.ContentAssetsBasePath = isEnable ? ContentAssetsBasePath.ContentOwner : ContentAssetsBasePath.Root;
            return Ok(new { message = $"Set SetAssetFriendlyUrl = {isEnable}" });
        }

        [Route("/Automation/SetRoutingOptions")]
        public IActionResult SetRoutingOption([FromBody] SetRoutingOptions options)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            try
            {
                if (!string.IsNullOrEmpty(options.ContentAssetsBasePath))
                {
                    _routingOption.ContentAssetsBasePath = (ContentAssetsBasePath)Enum.Parse(typeof(ContentAssetsBasePath), options.ContentAssetsBasePath);
                    messages.Add("ContentAssetsBasePath", options.ContentAssetsBasePath);
                }
                if (!string.IsNullOrEmpty(options.PreferredUrlFormat))
                {
                    _routingOption.PreferredUrlFormat = (PreferredUrlFormat)Enum.Parse(typeof(PreferredUrlFormat), options.PreferredUrlFormat);
                    messages.Add("PreferredUrlFormat", options.PreferredUrlFormat);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = "Error", messge = ex.Message });
            }
            var response = new { message = "Set RoutingOption done", options = messages };
            return Ok(response);
        }
        public class SetRoutingOptions
        {
            public string ContentAssetsBasePath { get; set; }
            public string PreferredUrlFormat { get; set; }
        }

        #region Replacement/ Fallback
        [Route("/Automation/SetReplacementLanguage")]
        public IActionResult SetReplacementLanguage()
        {
            string contentId = GetRequestQueryString("id");
            string from = GetRequestQueryString("from");
            string to = GetRequestQueryString("to");
            try
            {
                var pageRef = new PageReference(contentId);
                var languageSetting = new ContentLanguageSetting(pageRef, from, to, Array.Empty<string>());
                _contentLanguageSettingRepository.Save(languageSetting);
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = "Error", messge = ex.Message });
            }
            return Ok(new { message = $@"Set Replacement Language done for content {contentId} from language {from} to language {to}" });
        }

        [Route("/Automation/SetActiveLanguage")]
        public IActionResult SetActiveLanguage()
        {
            string language = GetRequestQueryString("language");
            string isActive = GetRequestQueryString("isActive");

            var pageRef = new PageReference(SiteDefinition.Current.StartPage);
            var languageSetting = new ContentLanguageSetting(pageRef, language);
            languageSetting.IsActive = isActive.Equals("true") ? true : false;
            _contentLanguageSettingRepository.Save(languageSetting);

            return Ok(new { message = $"set ok" });
        }

        [Route("/Automation/SetFallBackLanguage")]
        public IActionResult SetFallBackLanguage()
        {
            string contentId = GetRequestQueryString("id");
            string from = GetRequestQueryString("from");
            string to = GetRequestQueryString("to");
            try
            {
                var pageRef = new PageReference(contentId);
                var setting = new ContentLanguageSetting();
                setting.DefinedOnContent = pageRef;
                setting.LanguageBranch = from;
                setting.LanguageBranchFallback = new[] { to };
                _contentLanguageSettingRepository.Save(setting);
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = "Error", messge = ex.Message });
            }
            return Ok(new { message = $@"Set Fallback Language done for content {contentId} from language {from} to language {to}" });
        }
        #endregion

        #region Host
        [Route("/Automation/CreateHost")]
        public IActionResult CreateHost()
        {
            string hostName = GetRequestQueryString("name");
            string type = GetRequestQueryString("type");
            string language = GetRequestQueryString("language");
            try
            {
                var clone = SiteDefinition.Current.CreateWritableClone();
                HostDefinition newHost = new HostDefinition();
                newHost.Name = hostName;
                if (!string.IsNullOrEmpty(type))
                    newHost.Type = (HostDefinitionType)Enum.Parse(typeof(HostDefinitionType), type);
                if (!string.IsNullOrEmpty(language))
                    newHost.Language = new CultureInfo(language);

                clone.Hosts.Add(newHost);
                _siteDefinitionRepository.Save(clone);
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = "Error", messge = ex.Message });
            }
            return Ok(new { message = $"Create host {hostName} done" });
        }

        [Route("/Automation/DeleteEditHost")]
        public IActionResult DeleteHost()
        {
            string hostName = GetRequestQueryString("name");
            try
            {
                var clone = SiteDefinition.Current.CreateWritableClone();
                clone.Hosts.Remove(clone.GetHost(hostName, false));
                _siteDefinitionRepository.Save(clone);
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = "Error", messge = ex.Message });
            }
            return Ok(new { message = $"Delete host {hostName} done" });
        }

        [Route("/Automation/CreateSite")]
        public IActionResult CreateSite()
        {
            string siteName = GetRequestQueryString("siteName");
            string startPageName = GetRequestQueryString("startPageName");
            int startPageId = -1;
            try
            {
                var startPageLink = CreateSite(siteName: siteName, startPageName: startPageName);
                startPageId = startPageLink.ID;
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = "Error", messge = ex.Message });
            }
            return Ok(new { message = $"Create site {siteName} with start page {startPageName}({startPageId}) done" });
        }

        [Route("/Automation/DeleteSite")]
        public IActionResult RemoveSite()
        {
            string siteName = GetRequestQueryString("siteName");
            string startPage = GetRequestQueryString("startPage");
            try
            {
                DeleteSite(new PageReference(startPage), siteName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = "Error", messge = ex.Message });
            }
            return Ok(new { message = $"Delete site {siteName} done" });
        }

        private ContentReference CreateSite(string siteUrl = "http://epvnlptming:8010/", string siteName = "TestSite", string startPageName = "StartPage", IEnumerable<HostDefinition> hostDefinitions = null)
        {
            var startPage = _contentRepository.GetDefault<StartPage>(SiteDefinition.Current.RootPage);
            startPage.Name = startPageName;
            var startPageLink = _contentRepository.Save(startPage, SaveAction.Publish, AccessLevel.NoAccess).ToReferenceWithoutVersion();

            IContent siteFolder = _contentRepository.Get<ContentFolder>(CreateSystemFolder(startPageLink, SiteDefinition.SiteAssetsName).ToReferenceWithoutVersion());

            var siteDefinition = new SiteDefinition
            {
                Name = siteName,
                StartPage = startPageLink,
                SiteUrl = new Uri(siteUrl),
                SiteAssetsRoot = siteFolder.ContentLink
            };

            if (hostDefinitions != null)
            {
                foreach (var hostDefinition in hostDefinitions)
                {
                    siteDefinition.Hosts.Add(hostDefinition);
                }
            }

            _siteDefinitionRepository.Save(siteDefinition);
            SiteDefinition.Current = null;

            return startPageLink;
        }

        private void DeleteSite(ContentReference startPage, string siteName = "TestSite")
        {
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
            var siteDefinitionRepository = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();

            contentRepository.Delete(startPage, true, AccessLevel.NoAccess);

            var siteDefinition = siteDefinitionRepository.Get(siteName);
            siteDefinitionRepository.Delete(siteDefinition.Id);
        }
        #endregion

        #region SyncUser
        [Route("/Automation/DeleteUserAsync")]
        public async Task<IActionResult> DeleteUserAsync()
        {
            string username = GetRequestQueryString("username");
            await _synchronizedUsersRepository.DeleteUserAsync(username);
            return Ok(new { message = $"DeleteUserAsync with username {username}" });
        }

        [Route("/Automation/GetRolesForUserAsync")]
        public async Task<IActionResult> GetRolesForUserAsync()
        {
            string username = GetRequestQueryString("username");
            var roles = await _synchronizedUsersRepository.GetRolesForUserAsync(username);
            return Ok(new { message = $"GetRolesForUserAsync with username {username}: [{string.Join(',', roles)}]" });
        }

        [Route("/Automation/DeleteInactiveUsersAsync")]
        public async Task<IActionResult> DeleteInactiveUsersAsync()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(int.Parse(GetRequestQueryString("fromSeconds")));

            var users = await _synchronizedUsersRepository.DeleteInactiveUsersAsync(timeSpan);
            return Ok(new { message = $"DeleteInactiveUsersAsync with timespan {timeSpan}: [{string.Join(',', users)}]" });
        }
        #endregion
        private string GetRequestQueryString(string paramName)
        {
            if (Request.QueryString.Value == null || string.IsNullOrEmpty(Request.Query[paramName].ToString()))
            {
                return string.Empty;
            }

            return HttpUtility.UrlDecode(Request.Query[paramName]);
        }
    }
}