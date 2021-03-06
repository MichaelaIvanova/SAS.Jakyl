﻿using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using SAS.Jakyl.Core.BootManager;

namespace SAS.Jakyl.Core
{
    public static class UmbracoUnitTestHelper
    {
        public static ApplicationContext GetApplicationContext(bool disabledCache = true, ServiceContext serviceContext = null, DatabaseContext databaseContext = null, CacheHelper cacheHelper = null, ILogger logger = null, IProfiler profiler = null)
        {
            return ApplicationContext.EnsureContext(
                databaseContext ?? GetDatabaseContext(logger: logger),
                serviceContext ?? GetServiceContext(),
                disabledCache ? CacheHelper.CreateDisabledCacheHelper() : cacheHelper ?? /*CacheHelper.CreateDisabledCacheHelper()*/
                    GetCacheHelper(),
                new ProfilingLogger(
                    logger ?? Mock.Of<ILogger>(),
                    profiler ?? Mock.Of<IProfiler>()), true);
        }

        public static UmbracoContext GetUmbracoContext(ApplicationContext applicationContext = null, IWebRoutingSection webRoutingSettings = null, HttpContextBase httpContext = null, WebSecurity webSecurity = null,
            IUmbracoSettingsSection settingsSection = null, IEnumerable<IUrlProvider> urlProviders = null)
        {
            httpContext = httpContext ?? new Mock<HttpContextBase>().Object;
            applicationContext = applicationContext ?? GetApplicationContext();
            return UmbracoContext.EnsureContext(
                httpContext,
                applicationContext,
                webSecurity ?? GetWebSecurity(http: httpContext, application: applicationContext),
                settingsSection ?? Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == (webRoutingSettings ?? GetBasicWebRoutingSettings())),
                urlProviders ?? Enumerable.Empty<IUrlProvider>(),
                true);
        }

        public static WebSecurity GetWebSecurity(HttpContextBase http = null, ApplicationContext application = null)
        {
            return new WebSecurity(http ?? UmbracoContext.Current.HttpContext, application ?? GetApplicationContext());
        }

        public static CacheHelper GetCacheHelper(IRuntimeCacheProvider httpCache = null, ICacheProvider staticCache = null, ICacheProvider requestCache = null, IsolatedRuntimeCache isolastedRuntime = null)
        {
            return new CacheHelper(httpCache ?? Mock.Of<IRuntimeCacheProvider>(),
                        staticCache ?? Mock.Of<ICacheProvider>(),
                        requestCache ?? Mock.Of<ICacheProvider>(),
                        isolastedRuntime ?? GetIsolatedRuntimeCache());
        }

        public static IsolatedRuntimeCache GetIsolatedRuntimeCache(IRuntimeCacheProvider cacheFactory = null)
        {
            return new IsolatedRuntimeCache(f => cacheFactory ?? Mock.Of<IRuntimeCacheProvider>());
        }

        public static DatabaseContext GetDatabaseContext(IDatabaseFactory factory = null, ILogger logger = null, SqlSyntaxProviders sqlSyntaxProviers = null)
        {
            return new DatabaseContext(factory ?? Mock.Of<IDatabaseFactory>(), logger ?? Mock.Of<ILogger>(), sqlSyntaxProviers ?? new SqlSyntaxProviders(new[] { Mock.Of<ISqlSyntaxProvider>() }));
        }

        /// <summary>
        /// This will take some to build out...
        /// </summary>
        public static ServiceContext GetServiceContext(MockServiceContext mockServiceContext = null)
        {
            return mockServiceContext != null ? mockServiceContext.ServiceContext : new ServiceContext();
        }

        private static IWebRoutingSection GetBasicWebRoutingSettings()
        {
            return GetBasicWebRoutingSettings(UrlProviderMode.AutoLegacy);
        }

        private static IWebRoutingSection GetBasicWebRoutingSettings(UrlProviderMode mode = default(UrlProviderMode))
        {
            return Mock.Of<IWebRoutingSection>(section => section.UrlProviderMode == (mode.ToString())); //should default to AutoLegacy
        }

        public static UmbracoHelper GetUmbracoHelper(UmbracoContext context, ICultureDictionary cultureDictionary = null, MembershipHelper membershipHelper = null, UrlProvider urlProvider = null,
            IPublishedContent content = null, ITypedPublishedContentQuery typedQuery = null, IDynamicPublishedContentQuery dynamicQuery = null, ITagQuery tagQuery = null, IDataTypeService typeService = null,
            IUmbracoComponentRenderer componentRenderer = null)
        {
            return new UmbracoHelper(context,
                content ?? Mock.Of<IPublishedContent>(),
                typedQuery ?? Mock.Of<ITypedPublishedContentQuery>(),
                dynamicQuery ?? Mock.Of<IDynamicPublishedContentQuery>(),
                tagQuery ?? Mock.Of<ITagQuery>(),
                typeService ?? Mock.Of<IDataTypeService>(),
                urlProvider ?? GetUmbracoUrlProvider(context),
                cultureDictionary ?? Mock.Of<ICultureDictionary>(),
                componentRenderer ?? Mock.Of<IUmbracoComponentRenderer>(),
                membershipHelper ?? GetUmbracoMembershipHelper(context));
        }

        public static MembershipHelper GetUmbracoMembershipHelper(UmbracoContext context, MembershipProvider membershipProvider = null, RoleProvider roleProvider = null)
        {
            return new MembershipHelper(context, membershipProvider ?? Mock.Of<MembershipProvider>(), roleProvider ?? Mock.Of<RoleProvider>());
        }

        public static UrlProvider GetUmbracoUrlProvider(UmbracoContext context, IWebRoutingSection routingSection = null, IEnumerable<IUrlProvider> urlProviders = null)
        {
            return new UrlProvider(context, routingSection ?? GetBasicWebRoutingSettings(UrlProviderMode.Auto), urlProviders ?? new[] { Mock.Of<IUrlProvider>() });
        }

        public static IPublishedContent GetPublishedContent()
        {
            return GetPublishedContentMock().Object;
        }

        public static Mock<IPublishedContent> GetPublishedContentMock(string name = null, int? id = null, string path = null, string url = null, int? templateId = null, DateTime? updateDate = null, DateTime? createDate = null, PublishedContentType contentType = null, IPublishedContent parent = null, IEnumerable<IPublishedContent> Children = null, IEnumerable<IPublishedProperty> properties = null, int? index = null, PublishedItemType itemType = PublishedItemType.Content, string docType = null)
        {
            return SetPublishedContentMock(new Mock<IPublishedContent>(), name, id, path, url, templateId, updateDate, createDate, contentType, parent, Children, properties, index, itemType, docType);
        }

        public static Mock<IPublishedContent> SetPublishedContentMock(Mock<IPublishedContent> mock, string name = null, int? id = null, string path = null, string url = null, int? templateId = null, DateTime? updateDate = null, DateTime? createDate = null, PublishedContentType contentType = null, IPublishedContent parent = null, IEnumerable<IPublishedContent> Children = null, IEnumerable<IPublishedProperty> properties = null, int? index = null, PublishedItemType itemType = PublishedItemType.Content, string docType = null)
        {
            mock.Setup(s => s.Name).Returns(name);
            if (id.HasValue)
                mock.Setup(s => s.Id).Returns(id.Value);
            mock.Setup(s => s.Path).Returns(path);
            mock.Setup(s => s.Url).Returns(url);
            if (createDate.HasValue)
                mock.Setup(s => s.CreateDate).Returns(createDate.Value);
            if (updateDate.HasValue)
                mock.Setup(s => s.UpdateDate).Returns(updateDate.Value);
            if (templateId.HasValue)
                mock.Setup(s => s.TemplateId).Returns(templateId.Value);
            if (contentType != null)
                mock.Setup(s => s.ContentType).Returns(contentType);
            //            else
            //                mock.Setup(s => s.ContentType).Returns(GetPublishedContentType);
            if (!string.IsNullOrEmpty(docType))
            {
                mock.Setup(s => s.DocumentTypeAlias).Returns(docType);
            }
            else if (mock.Object.ContentType != null)
            {
                mock.Setup(s => s.DocumentTypeAlias).Returns(mock.Object.ContentType.Alias);
                mock.Setup(s => s.DocumentTypeId).Returns(mock.Object.ContentType.Id);
            }
            if (parent != null)
                mock.Setup(s => s.Parent).Returns(parent);
            //else
            //    mock.Setup(s => s.Parent).Returns(GetPublishedContentMock(parent: null).Object /*GetPublishedContent*/); //was too dangerous to auto resolve parent like this
            if (Children != null)
                mock.Setup(s => s.Children).Returns(Children);
            //else
            //    mock.Setup(s => s.Children).Returns(() => new[] { GetPublishedContent() });
            if (properties != null)
            {
                mock.Setup(s => s.GetProperty(It.IsAny<string>())).Returns<string>(a => properties.FirstOrDefault(s => s.PropertyTypeAlias == a));
                mock.Setup(s => s.GetProperty(It.IsAny<string>(),It.IsAny<bool>())).Returns<string,bool>((a,b) => properties.FirstOrDefault(s => s.PropertyTypeAlias == a));
                mock.Setup(s => s.Properties).Returns(properties.ToList());
            }
            if (index.HasValue)
            {
                mock.Setup(s => s.GetIndex()).Returns(index.Value);
                mock.Setup(s => s.Level).Returns(index.Value);
            }
            mock.Setup(s => s.ItemType).Returns(itemType);
            return mock;
        }

        public static T GetContentTypeComposition<T>(int? id = null, string alias = "default", string name = null, IEnumerable<PropertyType> propertyTypes = null)
            where T : class, IContentTypeComposition
        {
            var mock = new Mock<T>();
            mock.Setup(s => s.Id).Returns(id.HasValue ? id.Value : new Random().Next());
            mock.Setup(s => s.Alias).Returns(alias);
            mock.Setup(s => s.Name).Returns(name);
            mock.Setup(s => s.CompositionPropertyTypes).Returns(propertyTypes ?? new PropertyType[] { }); //Issue gettting converters
            return mock.Object;
        }
        public static PublishedContentType GetPublishedContentType()
        {
            return GetPublishedContentType(PublishedItemType.Content);
        }

        public static PublishedContentType GetPublishedContentType(PublishedItemType type = PublishedItemType.Content, string alias = "default")
        {
            return PublishedContentType.Get(type, alias);
        }

        public static PublishedPropertyType GetPublishedPropertyType(PublishedContentType contentType, PropertyType propertyType = null)
        {
            return new PublishedPropertyType(contentType, propertyType ?? GetPropertyType());
        }

        public static PropertyType GetPropertyType(IDataTypeDefinition dataTypeDef = null, string alias = null)
        {
            return new PropertyType(dataTypeDef ?? Mock.Of<IDataTypeDefinition>(d => d.PropertyEditorAlias == "default"), string.IsNullOrEmpty(alias) ? "_umb_default" : alias); //use _umb_ to avoid StringExtentions (causes config loading errors)
        }

        public static IPublishedProperty GetPublishedProperty(object dataValue = null, object value = null, string alias = null)
        {
            var prop = new Mock<IPublishedProperty>();
            prop.Setup(s => s.DataValue).Returns(dataValue);
            prop.Setup(s => s.Value).Returns(value);
            prop.Setup(s => s.HasValue).Returns(value != null);
            prop.Setup(s => s.PropertyTypeAlias).Returns(alias);
            return prop.Object;

        }

        public static ControllerContext GetControllerContext(UmbracoContext context, Controller controller, PublishedContentRequest publishedContentRequest = null, RouteData routeData = null)
        {
            var contextBase = context.HttpContext;

            var pcr = publishedContentRequest ?? GetPublishedContentRequest(context);

            var routeDefinition = new RouteDefinition
            {
                PublishedContentRequest = pcr
            };

            var rd = routeData ?? new RouteData();
            rd.DataTokens.Add("umbraco-route-def", routeDefinition);
            return new ControllerContext(contextBase, rd, controller);
        }

        public static HttpControllerContext GetApiControllerContext(HttpRouteData routeData = null, HttpRequestMessage requestMessage = null, HttpConfiguration httpConfiguration = null)
        {
            var rd = routeData ?? new HttpRouteData(Mock.Of<IHttpRoute>());

            var httpConfig = httpConfiguration ?? new HttpConfiguration();

            var httpRequest = requestMessage ?? new HttpRequestMessage();

            return new HttpControllerContext(httpConfig, rd, httpRequest);
        }

        public static PublishedContentRequest SetPublishedContentRequest(UmbracoContext context = null, PublishedContentRequest request = null)
        {
            return (context ?? UmbracoContext.Current).PublishedContentRequest = request ?? GetPublishedContentRequest();
        }

        public static PublishedContentRequest GetPublishedContentRequest(UmbracoContext context = null, string url = null, IWebRoutingSection routingSection = null, IEnumerable<string> rolesForLogic = null, IPublishedContent currentContent = null)
        {
            return new PublishedContentRequest(new Uri(string.IsNullOrEmpty(url) ? "http://localhost/test" : url),
                (context ?? UmbracoContext.Current).RoutingContext,
                routingSection ?? GetBasicWebRoutingSettings(),
                s => rolesForLogic ?? Enumerable.Empty<string>())
            {
                PublishedContent = currentContent ?? Mock.Of<IPublishedContent>(publishedContent => publishedContent.Id == 12345)
            };
        }

        public static UmbracoApplication GetUmbracoApplication()
        {
            return new UmbracoApplication();
        }

        /// <summary>
        /// To allow Helper.GetPublishedContentType and PublishedContentType.Get to work
        /// 
        /// https://github.com/umbraco/Umbraco-CMS/blob/67c3ea7c00f44cf3426a37c2cc62e7b561fc859a/src/Umbraco.Core/Models/PublishedContent/PublishedContentType.cs ln 133-170
        /// </summary>
        /// <param name="mockServiceContext"></param>
        public static void SetupServicesForPublishedContentTypeResolution(MockServiceContext mockServiceContext, IEnumerable<PropertyType> propertyTypes = null)
        {
            mockServiceContext.ContentTypeService.Setup(s => s.GetContentType(It.IsAny<string>())).Returns<string>(s => GetContentTypeComposition<IContentType>(alias: s, propertyTypes: propertyTypes));
            mockServiceContext.ContentTypeService.Setup(s => s.GetMediaType(It.IsAny<string>())).Returns<string>(s => GetContentTypeComposition<IMediaType>(alias: s, propertyTypes: propertyTypes));
            mockServiceContext.MemberTypeService.Setup(s => s.Get(It.IsAny<string>())).Returns<string>(s => GetContentTypeComposition<IMemberType>(alias: s, propertyTypes: propertyTypes));
        }

        public static CustomBoot GetCustomBootManager(UmbracoApplication umbracoApplication = null, ServiceContext serviceContext = null)
        {
            return new CustomBoot(umbracoApplication ?? GetUmbracoApplication(), serviceContext ?? GetServiceContext());
        }

        public static CoreBootManager StartCoreBootManager(CustomBoot bm = null)
        {
            bm = bm ?? GetCustomBootManager();
            if (!bm.Initialized)
                bm.Initialize();
            if (!bm.Started)
                bm.Startup(null);
            if (!bm.Completed)
                bm.Complete(null);
            return bm;
        }

        public static void CleanupCoreBootManager(ApplicationContext appCtx = null)
        {
            (appCtx ?? GetApplicationContext()).DisposeIfDisposable();
        }

        public static IUser GetUser(Mock<IUser> _user = null, int? id = null, string name = null, string username = null, string email = null, string comments = null, DateTime? createDate = null, DateTime? updateDate = null, string language = null, bool isApproved = true, bool isLocked = false, int? startContentId = null, int? startMediaId = null)
        {
            _user = _user ?? new Mock<IUser>();
            _user.SetupAllProperties();
            _user.SetupProperty(s => s.Comments, comments);
            if (createDate.HasValue)
                _user.SetupProperty(s => s.CreateDate, createDate.Value);
            _user.SetupProperty(s => s.Name, name);
            _user.SetupProperty(s => s.Email, email);
            if (id.HasValue)
                _user.SetupProperty(s => s.Id, id.Value);
            _user.SetupProperty(s => s.IsApproved, isApproved);
            _user.SetupProperty(s => s.IsLockedOut, isLocked);
            _user.SetupProperty(s => s.Language, language);
            _user.SetupProperty(s => s.Username, username);
            if(startContentId.HasValue)
                _user.SetupProperty(s => s.StartContentId, startContentId.Value);
            if(startMediaId.HasValue)
                _user.SetupProperty(s => s.StartMediaId, startMediaId.Value);
            if (updateDate.HasValue)
                _user.SetupProperty(s => s.UpdateDate, updateDate.Value);
            return _user.Object;
        }

    }
}
