namespace AutonomousServiceBus
    module Processor =
        open Interfaces
        open System
        open System.Collections.Generic
        open System.Threading
        open System.Threading.Tasks
        open ServiceStack.ServiceClient.Web
        open AutonomousService
        (*type Rule(event, proc) =
            interface IRule with
                member this.ProcessProvider = proc
                member this.EventProvider = event
    
        type EventProvider (initFunc, disposeFunc, event) = 
            new(init, evnt) =
                new EventProvider(init, ignore(), evnt)
            interface IEventProvider with
                member this.Initialize = initFunc
                member IDisposable.Dispose = disposeFunc
                member this.OnEvent = event*)

//        ///Преобразовать Dictionary в map
//        ///https://gist.github.com/theburningmonk/3363893
//        let toMap dictionary = 
//            (dictionary :> seq<_>)
//            |> Seq.map (|KeyValue|)
//            |> Map.ofSeq


        let getRules proc event =
            { new IRule with
                member this.ProcessProvider = proc
                member this.EventProvider = event
            }

        let prepareRule (rule:IRule) = 
            rule.EventProvider.OnEvent.Add(fun args -> rule.ProcessProvider.Process(args.Message))

        let getEvent (initFunc:Event<ASBEventDelegate, ASBEventArgs> -> unit) =
            let event = new Event<ASBEventDelegate, ASBEventArgs>()
            { new IEventProvider with
                member this.Initialize() = initFunc event
                [<CLIEvent>]
                member this.OnEvent = event.Publish
                member this.Name = "Unnamed event"
               interface IDisposable with
                member this.Dispose() = ()
            }

        let getProcess (processFunc:Dictionary<string, obj>->unit) =
            { new IProcessProvider with
                member this.Process(x) = processFunc x
            }
    
        let callEvery (ms:int) (func:Dictionary<string, obj>->unit) =
            let rec loop(milliseconds) =
                 async{
                do! Async.Sleep(ms)
                let newMilliseconds = milliseconds + ms
                func(System.Linq.Enumerable.ToDictionary( [|newMilliseconds/1000|], fun _ -> "seconds"))
                do! loop newMilliseconds}
            Async.RunSynchronously(loop 0) |> ignore

        let callEvery2Seconds (func:Dictionary<string, obj>->unit) =
            callEvery 2000 func

        let dicToEvent func (evt:Event<ASBEventDelegate, ASBEventArgs>) =
            (fun dic -> evt.Trigger(null, ASBEventArgs(dic))) |> func 

        let every2Seconds = dicToEvent callEvery2Seconds
         
        let processProperty action propertyName (data:Dictionary<string, obj>) =
            action (data.TryGetValue(propertyName))

        let branchOnExistence (successAct:string->string->unit) (failAct:string->unit) propertyName (data:Dictionary<string, obj>) =
            let go(prop) =
                match prop with
                | (a, b) when a=true -> successAct (b.ToString()) propertyName
                | _ -> failAct propertyName
            processProperty go propertyName data


        let printParameter propertyName (data:Dictionary<string, obj>) : unit =
            branchOnExistence (fun b propertyName -> printfn "Got %s %s" (b.ToString()) propertyName) (fun text -> printfn "No %s provided" text) propertyName data
        
        let sendToService serviceUrl propertyName data: unit =
            let client = new JsonServiceClient(serviceUrl)
            let send text _ =
                let storeObj = StoreType(text)
                client.Post storeObj |>ignore
            branchOnExistence send (fun x-> ignore()) propertyName data
        
        let printSeconds = printParameter "seconds"

        let processor(rules) =
           let rec loop list =
               match list with
               | head :: tail -> 
                    prepareRule head
                    head.EventProvider.Initialize()
                    loop tail
               | [] -> ()
           loop rules

        let start() =
            processor [getRules (getProcess printSeconds) (getEvent every2Seconds)]
        let startSelfPing url =
            Task.Factory.StartNew (fun()-> processor [getRules (getProcess (sendToService url "seconds")) (getEvent (dicToEvent (callEvery 200)))])

        //let Rules = [yield ]

