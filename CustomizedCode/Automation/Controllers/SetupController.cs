using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AlloyTemplates.Automation.Models;
using AlloyTemplates.Models.Pages;
using EPiServer;
using EPiServer.Approvals;
using EPiServer.Approvals.ContentApprovals;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentManagementApi;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Security;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AlloyTemplates.Automation.Controllers
{
    public class SetupController: Controller
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
        private readonly RoutingOptions _routingOption;
        private readonly ContentLanguageSettingRepository _contentLanguageSettingRepository;


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
            RoutingOptions routingOption,
            UIRoleProvider roleProvider,
            IApprovalDefinitionRepository approvalDefinitionRepository,
            ContentLanguageSettingRepository contentLanguageSettingRepository)
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
            _routingOption = routingOption;
            _contentLanguageSettingRepository = contentLanguageSettingRepository;
        }

        #region New Setup
        [Route("/Automation/Setup")]
        public async Task<EnvironmentBase> Setup(string siteDomain)
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

        private async Task<EnvironmentBase> SetupPagesAndFolders()
        {
            await AddRole(EnvironmentBase.ContentApiRead, AccessLevel.Read);
            await AddRole(EnvironmentBase.ContentApiWrite, AccessLevel.Read);
            await AddApplication(EnvironmentBase.ApplicationName, AccessLevel.FullAccess);

            RevokeAccessOfEveryoneGroup();

            var environment = new CmsEnvironment();

            await foreach (var item in GenerateEnvironmentVariableItemsAsync())
            {
                environment.Values.Add(item);
            }

            return environment;
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

        private async IAsyncEnumerable<VariableItem> GenerateEnvironmentVariableItemsAsync()
        {
            yield return new VariableItem(Key: "cmsUrl", Value: SiteDefinition.Current.SiteUrl.ToString());
            yield return new VariableItem(Key: "forThisSiteId", Value: SiteDefinition.Current.SiteAssetsRoot.ID);
            yield return new VariableItem(Key: "testContainerFolderId", Value: CreateEmptyTestFolderUnderGlobalAssetRoot().ID);

            var testPageReference = CreateContainerPageUnderStartPage();
            yield return new VariableItem(Key: "testContainerPageId", Value: testPageReference.ID);

            yield return new VariableItem(Key: "deniedPageId", Value: CreatePublishedPageWithNoAccessForCmsadmin(testPageReference).ID);
            yield return new VariableItem(Key: "draftPageId", Value: CreateDraft(testPageReference).ID);
            yield return new VariableItem(Key: "approvalSequenceParentId", Value: (await CreatePublishedPageWithApprovalSequenceEnabled(testPageReference)).ID);
            yield return new VariableItem(Key: "noPublishedRightParentId", Value: CreatePublishedPageWithoutPublishAccessRight(testPageReference).ID);
            yield return new VariableItem(Key: "noAPIAccessParentId", Value: CreatePublishedPageWithoutContentApiWriteAccessRight(testPageReference).ID);
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

        [Produces("application/json")]
        [HttpPost("/Automation/SetContentApiOption")]
        public IActionResult SetContentApiOption([FromBody] SetApiOptions options)
        {
            if(!string.IsNullOrEmpty(options.flatten))
                _contentApiOptions.SetFlattenPropertyModel(bool.Parse(options.flatten));
            if(!string.IsNullOrEmpty(options.forceabsolute))
                _contentApiOptions.SetForceAbsolute(bool.Parse(options.forceabsolute));
            if(!string.IsNullOrEmpty(options.includeInternalContentRoots))
                _contentApiOptions.SetIncludeInternalContentRoots(bool.Parse(options.includeInternalContentRoots));
            if(!string.IsNullOrEmpty(options.expandedBehavior))
            {
                ExpandedLanguageBehavior expOpt = (options.expandedBehavior == "RequestedLanguage") ? ExpandedLanguageBehavior.RequestedLanguage : ExpandedLanguageBehavior.ContentLanguage;
                _contentApiOptions.SetExpandedBehavior(expOpt);
            }
            if (!string.IsNullOrEmpty(options.includeEmptyContentProperties))
                _contentApiOptions.IncludeEmptyContentProperties = bool.Parse(options.includeEmptyContentProperties);
            if (!string.IsNullOrEmpty(options.includeMasterLanguage))
                _contentApiOptions.SetIncludeMasterLanguage(bool.Parse(options.includeMasterLanguage));
            if (!string.IsNullOrEmpty(options.includeNumericContentIdentifier))
                _contentApiOptions.SetIncludeNumericContentIdentifier(bool.Parse(options.includeNumericContentIdentifier));
            if (!string.IsNullOrEmpty(options.includeSiteHosts))
                _contentApiOptions.SetIncludeSiteHosts(bool.Parse(options.includeSiteHosts));
            return Ok(new { message = $"Set ContentApiOption done" });
        }

        public class SetApiOptions
        {
            public string flatten { get; set; }
            public string forceabsolute { get; set; }
            public string includeInternalContentRoots { get; set; }
            public string expandedBehavior { get; set; }
            public string includeEmptyContentProperties { get; set; }
            public string includeMasterLanguage { get; set; }
            public string includeNumericContentIdentifier { get; set; }
            public string includeSiteHosts { get; set; }
        }

        [Route("/Automation/SetAssetFriendlyUrl")]
        public IActionResult SetAssetFriendlyUrl()
        {
            bool isEnable = bool.Parse(GetRequestQueryString("enable"));
            //_routingOption.ContentAssetsBasePath = isEnable ? ContentAssetsBasePath.ContentOwner: ContentAssetsBasePath.Root;
            return Ok(new { message = $"Set SetAssetFriendlyUrl = {isEnable}" });
        } 

        [Route("/Automation/SetReplacementLanguage")]
        public IActionResult SetReplacementLanguage()
        {
            string contentId = GetRequestQueryString("id");
            string from = GetRequestQueryString("from");
            string to = GetRequestQueryString("to");
            
            var pageRef = new PageReference(contentId);
            var languageSetting = new ContentLanguageSetting(pageRef, from, to, Array.Empty<string>());
            _contentLanguageSettingRepository.Save(languageSetting);

            return Ok(new { message = $"set ok" });
        }

        [Route("/Automation/SetFallBackLanguage")]
        public IActionResult SetFallBackLanguage()
        {
            string contentId = GetRequestQueryString("id");
            string from = GetRequestQueryString("from");
            string to = GetRequestQueryString("to");

            var pageRef = new PageReference(contentId);
            var setting = new ContentLanguageSetting();
            setting.DefinedOnContent = pageRef;
            setting.LanguageBranch = from;
            setting.LanguageBranchFallback = new[] { to };
            _contentLanguageSettingRepository.Save(setting);

            return Ok(new { message = $"set ok" });
        }

        #region Host
        [Route("/Automation/CreateHost")]
        public IActionResult CreateHost()
        {
            string hostName = GetRequestQueryString("name");
            string type = GetRequestQueryString("type");
            string language = GetRequestQueryString("language");

            var clone = SiteDefinition.Current.CreateWritableClone();
            HostDefinition newHost = new HostDefinition();
            newHost.Name = hostName;
            if (!string.IsNullOrEmpty(type))
                newHost.Type = (HostDefinitionType)Enum.Parse(typeof(HostDefinitionType), type);
            if (!string.IsNullOrEmpty(language))
                newHost.Language = new CultureInfo(language);

            clone.Hosts.Add(newHost);
            _siteDefinitionRepository.Save(clone);
            return Ok(new { message = $"create host ok" });
        }

        [Route("/Automation/DeleteEditHost")]
        public IActionResult DeleteHost()
        {
            string hostName = GetRequestQueryString("name");
            var clone = SiteDefinition.Current.CreateWritableClone();

            clone.Hosts.Remove(clone.GetHost(hostName, false));
            _siteDefinitionRepository.Save(clone);
            return Ok(new { message = $"delete host ok" });
        }

        [Route("/Automation/CreateSite")]
        public IActionResult CreateSite()
        {
            string siteName = GetRequestQueryString("siteName");
            string startPageName = GetRequestQueryString("startPageName");

            var startPageLink = CreateSite(siteName: siteName, startPageName: startPageName);
            return Ok(new { message = $"create site ok with start page id {startPageLink}" });
        }

        [Route("/Automation/DeleteSite")]
        public IActionResult RemoveSite()
        {
            string siteName = GetRequestQueryString("siteName");
            string startPage = GetRequestQueryString("startPage");

            DeleteSite(new PageReference(startPage), siteName);
            return Ok(new { message = $"delete site {siteName} with start page id {startPage} ok" });
        }

        private ContentReference CreateSite(string siteUrl = "http://localHost/", string siteName = "TestSite", string startPageName = "StartPage", IEnumerable<HostDefinition> hostDefinitions = null)
        {
            var startPage = _contentRepository.GetDefault<StartPage>(SiteDefinition.Current.RootPage);
            startPage.Name = startPageName;
            var startPageLink = _contentRepository.Save(startPage, SaveAction.Publish, AccessLevel.NoAccess).ToReferenceWithoutVersion();

            var siteDefinition = new SiteDefinition
            {
                Name = siteName,
                StartPage = startPageLink,
                SiteUrl = new Uri(siteUrl),
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
