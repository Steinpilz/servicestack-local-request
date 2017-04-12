using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confifu.Abstractions;
using Confifu.Autofac;
using ServiceStack.LocalRequest.Confifu;
using Xunit;
using ServiceStack.Confifu;
using Confifu.Abstractions.DependencyInjection;

namespace ServiceStack.LocalRequest.Tests
{
    public class AppTests
    {

        class App : global::Confifu.AppSetup
        {
            public App() : base(new EmptyConfigVariables())
            {
                AppConfig
                    .UseServiceStack(c =>
                    {
                        c.ServiceHostAssemblies.Add(GetType().Assembly);
                    })
                    .UseServiceStackLocalRequest();
                
                AppConfig.AddAppRunnerAfter(() =>
                {
                    AppConfig.SetupAutofacContainer();
                });
            }
        }

        static AppTests()
        {
            new App().Setup().Run();
        }

        [Fact]
        public void it_does_not_smoke()
        {

        }
    }
}
