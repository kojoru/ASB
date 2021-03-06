﻿namespace AutonomousServiceBus
    
    open System
    open ServiceStack.ServiceHost
    open ServiceStack.WebHost.Endpoints
    open ServiceStack.ServiceInterface
    open Interfaces
    open Agents
    open ServiceContracts
    open Processor

    module AutonomousService =

//        [<Route("/events")>]
//        type EventType (event, name, init, dispose) =
//            interface IEventProvider with
//                [<CLIEvent>]
//                member this.OnEvent = event.Publish 
//                member val Name = name
//                member this.Initialize() = init
//                member this.Dispose() = dispose
//            new() =
//                let emptyEvent = Event<ASBEventDelegate, ASBEventArgs>() 
//                new EventType(emptyEvent,"",(),())

//        [<Route("/actions")>]
//        type ActionType (name, action) =
//            interface IProcessProvider with
//                member this.Process = action
//                member val Name = name with get, set
            

//        let mutable Events : EventType list = []
//        let mutable Actions = []
//        let mutable Rules = []


           
        type TestTypeService() =
            inherit Service()
            member this.proc (request:TestType) =
                TestTypeReturn(request.test)
            member this.Any (request:TestType):TestTypeReturn =
                this.proc request

        type StoreTypeService() =
            inherit Service()
            member val keepers : AgentAgent = AgentAgent() with get, set
            member this.Post(request: StoreType) =
                this.keepers.Take(request.pipe).Give(request.toStore)
            member this.Get(request: StoreType) =
                StoreTypeReturn(this.keepers.Take(request.pipe).Take(10))

        type EventTypeService() =
            inherit Service()
            //member val keeper : KeepingAgent = KeepingAgent() with get, set
            member this.Post(request: EventType) =
                let host uri = System.Uri(uri).GetLeftPart(UriPartial.Authority).ToString()
                Processor.startSelfPing (host(this.Request.AbsoluteUri)) request.period request.pipe

//        type EventTypeService() =
//            inherit Service()
//            member this.Get(request: EventType) =
//                Events


            (*interface IService<TestType> with
                member this.Execute (req:TestType) = req :> Object*)

        type AppHost =
            inherit AppHostBase
            new() = { inherit AppHostBase ("Test service",typeof<AppHost>.Assembly) }
            override this.Configure container = 
                container.Register(AgentAgent())


