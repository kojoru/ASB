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
        
        type StoreTypeReturn(stored) =
            member val stored:List<string> = stored with get, set

        [<Route("/store")>]
        type StoreType(toStore) =
            member val toStore = toStore with get,set
            new() = StoreType("")
            interface IReturn<StoreTypeReturn>
        let mutable Stored = ["0"] 
        type StoreTypeService() =
            inherit Service()
            member this.Post(request: StoreType) =
                //Stored <- List.append Stored [request.toStore]
                //this.Stored
                Stored <- request.toStore :: Stored
            member this.Get(request: StoreType) =
                StoreTypeReturn(Stored)


            (*interface IService<TestType> with
                member this.Execute (req:TestType) = req :> Object*)

        type AppHost =
            inherit AppHostBase
            new() = { inherit AppHostBase ("Test service",typeof<AppHost>.Assembly) }
            override this.Configure container = 
                ignore()


