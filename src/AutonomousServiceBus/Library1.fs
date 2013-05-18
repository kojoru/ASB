namespace AutonomousServiceBus
    
    open System
    open ServiceStack.ServiceHost
    open ServiceStack.WebHost.Endpoints

    module AutonomousServiceBus =
        

        type TestType = {mutable test : string}
        type TestTypeService() =
            interface IService<TestType> with
                member this.Execute (req:TestType) = req :> Object

        type AppHost =
            inherit AppHostBase
            new() = { inherit AppHostBase ("Test service",typeof<TestType>.Assembly) }
            override this.Configure container = 
                base.Routes
                    .Add<TestType>("/service/{test}")
                        |>ignore




        type Global =
            inherit System.Web.HttpApplication
            new() = { }
            member x.Application_Start() = 
                let appHost = new AppHost()
                appHost.Init()

