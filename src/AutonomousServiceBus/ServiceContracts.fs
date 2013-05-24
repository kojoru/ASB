namespace AutonomousServiceBus
    
    open System
    open ServiceStack.ServiceHost
    open ServiceStack.WebHost.Endpoints
    open ServiceStack.ServiceInterface
    open Interfaces
    open Agents
    module ServiceContracts =
            type TestTypeReturn(testString) =
                member val test:string = "Hello, "+testString+"!" with get, set

            [<Route("/service/{test}")>]
            type TestType() =
                interface IReturn<TestTypeReturn>
                member val test:String = "" with get, set

                
            type StoreTypeReturn(stored) =
                member val stored:List<string> = stored with get, set

            [<Route("/store")>]
            [<Route("/store/{pipe}")>]
            type StoreType(toStore, pipe) =
                member val toStore = toStore with get,set
                member val pipe = pipe with get,set
                new() = StoreType("", "default")
                new(toStore) = StoreType(toStore, "default")
                interface IReturn<StoreTypeReturn>
        
            type EventTypeReturn() =
                member val period:int = 0 with get, set

            [<Route("/event")>]
            type EventType(period, pipe) =
                member val period = period with get, set
                member val pipe = pipe with get, set
                new() = EventType(2000, "default")
                interface IReturn<EventTypeReturn>


