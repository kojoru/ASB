﻿namespace AutonomousServiceBus
    module Processor =
        open interfaces
        open System
        open System.Collections.Generic
        open System.Threading
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
                member this.Dispose() = ()
                member this.Initialize() = initFunc event
                [<CLIEvent>]
                member this.OnEvent = event.Publish
               interface IDisposable with
                member this.Dispose() = ()
            }

        let getProcess (processFunc:Dictionary<string, obj>->unit) =
            { new IProcessProvider with
                member this.Process(x) = processFunc x
            }
    
        let callEvery2Seconds (func:Dictionary<string, obj>->unit) =
            let rec loop(seconds) =
                Thread.Sleep(2000)
                let newSeconds = seconds+2
                func(System.Linq.Enumerable.ToDictionary( [|newSeconds|], fun(x) -> "seconds"))
                loop newSeconds
            loop 0 |> ignore

        let every2Seconds (evt:Event<ASBEventDelegate, ASBEventArgs>) =
            (fun dic -> evt.Trigger(null, ASBEventArgs(dic))) |> callEvery2Seconds 
         
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

        //let Rules = [yield ]

