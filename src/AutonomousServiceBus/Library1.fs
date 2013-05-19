namespace AutonomousServiceBus
    
    open System
    open ServiceStack.ServiceHost
    open ServiceStack.WebHost.Endpoints
    open ServiceStack.ServiceInterface

    module AutonomousService =
        

        type TestTypeReturn(testString) =
            member val test:String = "Hello, "+testString+"!" with get, set

        [<Route("/service/{test}")>]
        type TestType() =
            interface IReturn<TestTypeReturn>
            member val test:String = "" with get, set
           
        type TestTypeService() =
            inherit Service()
            member this.proc (request:TestType) =
                TestTypeReturn(request.test)
            member this.Any (request:TestType):TestTypeReturn =
                this.proc request
                


            (*interface IService<TestType> with
                member this.Execute (req:TestType) = req :> Object*)

        type AppHost =
            inherit AppHostBase
            new() = { inherit AppHostBase ("Test service",typeof<AppHost>.Assembly) }
            override this.Configure container = 
                ignore()


