using Microsoft.QualityTools.Testing.Emulators;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Fakes;
using Microsoft.SharePoint.Emulators;
using Microsoft.SharePoint.Fakes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPEmulatorTest
{
    public class UnitTests
    {
        [Test]
        public void ServerModelTest()
        {
            using (var scope = new SharePointEmulationScope(EmulationMode.Enabled))
            {
                var site = new SPSite("http://server");
                var web = site.RootWeb;
                var id = web.Lists.Add("sample list", "this is a sample list that only exists in emulation", SPListTemplateType.GenericList);
                var list = web.Lists[id];

                // Act
                var item = list.Items.Add();
                item["Title"] = "abc";
                item.Update();

                // Assert
                Assert.That(item["Title"], Is.EqualTo("abc"));
            }
        }


        [Test]
        public void ClientModelTest()
        {
            using (var scope = new SharePointEmulationScope(EmulationMode.Enabled))
            {
                // Create shims for the ClientContext class and its base class
                var ctx = new ShimClientContext();
                var ctxBase = new ShimClientRuntimeContext(ctx);

                var site = new ShimSite();
                var web = new ShimWeb();

                ctx.SiteGet = () => { return site; };
                ctx.WebGet = () => { return web; };
                ctx.ExecuteQuery = () => { };

                ImplementLoadFake<Web>(ctxBase);
                ImplementLoadFake<Site>(ctxBase);
                ImplementLoadFake<User>(ctxBase);

                var user = new ShimUser();
                var userPrincipal = new ShimPrincipal(user);
                userPrincipal.LoginNameGet = () => { return "i:0#.f|membership|user@domain.onmicrosoft.com"; };
                web.CurrentUserGet = () => { return user; };
                web.TitleGet = () => { return "Title"; };

                // Test with the ClientContext ctx
                var rCtx = ctx.Instance;
                rCtx.Load(rCtx.Web, w => w.Title);
                rCtx.Load(rCtx.Web.CurrentUser, u => u.LoginName);
                rCtx.ExecuteQuery();

                Assert.That(user.Instance.LoginName, Is.EqualTo("i:0#.f|membership|user@domain.onmicrosoft.com"));
            }
        }
               

        private static void ImplementLoadFake<T>(ShimClientRuntimeContext ctxBase) where T : ClientObject
        {
            // Add implementation for ctx.Load<Web>(...)
            ctxBase.LoadOf1M0ExpressionOfFuncOfM0ObjectArray<T>((a, b) => { });
            ctxBase.LoadQueryOf1ClientObjectCollectionOfM0<T>(delegate { return null; });
            ctxBase.LoadQueryOf1IQueryableOfM0<T>(delegate { return null; });
        }

    }

}
