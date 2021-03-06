﻿namespace AutonomousServiceBus
    module Processor =
        open Interfaces
        open System
        open System.Collections.Generic
        open System.Threading
        open System.Threading.Tasks
        open ServiceStack.ServiceClient.Web
        open ServiceContracts
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

        let getEvent initFunc =
            let event = new Event<ASBEventDelegate, ASBEventArgs>()
            let dicToEvent func =
                (fun dic -> event.Trigger(null, ASBEventArgs(dic))) |> func 
            { new IEventProvider with
                member this.Initialize() = dicToEvent initFunc 
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
    
        let callEvery (ms:int) func =
            let rec loop(milliseconds) =
                 async{
                do! Async.Sleep(ms)
                let newMilliseconds = milliseconds + ms
                let arr = [|("seconds", newMilliseconds/1000);("milliseconds", newMilliseconds);("every", ms)|]
                let dic = System.Linq.Enumerable.ToDictionary(arr, (fun (x,y) -> x), (fun (x,y)->y:>obj))
                func dic
                do! loop newMilliseconds}
            Async.Start(loop 0) |> ignore

        let callEvery2Seconds (func:Dictionary<string, obj>->unit) =
            callEvery 2000 func

        //let every2Seconds = dicToEvent callEvery2Seconds
         
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
        
        let sendToService serviceUrl pipeName (propertyNameFunc:Dictionary<string,obj>->string) data: unit =
            let client = new JsonServiceClient(serviceUrl)
            let send text =
                let storeObj = StoreType(text, pipeName)
                client.Post storeObj |> ignore
            send (propertyNameFunc data)
            //branchOnExistence send (fun x-> ignore()) propertyName data
        
        let printSeconds = printParameter "seconds"

        let getOneParameter name (dic:Dictionary<string, obj>)=
            match dic.TryGetValue(name) with
            | (a, b) when a=true -> b.ToString()
            | _ -> ""

        let toJson dic =
            ServiceStack.Text.StringExtensions.ToJson(dic)

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
            processor [getRules (getProcess printSeconds) (getEvent (callEvery 2000))]
        let startSelfPing url ms pipeName =
            Task.Factory.StartNew (fun()-> processor [getRules (getProcess (sendToService url pipeName toJson)) (getEvent (callEvery ms))])

        //let Rules = [yield ]

        //(getOneParameter "milliseconds")