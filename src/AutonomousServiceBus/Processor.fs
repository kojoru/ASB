namespace AutonomousServiceBus
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

        let getRules (proc, event) =
            { new IRule with
                member this.ProcessProvider = proc
                member this.EventProvider = event
            }

        let prepareRule (rule:IRule) = 
            rule.EventProvider.OnEvent.Add(fun args -> rule.ProcessProvider.Process(args.Message))

        let eventForUs = new Event<ASBEventDelegate, ASBEventArgs>()
        let getEvent (initFunc:IEventProvider -> unit) =
        
            { new IEventProvider with
                member this.Dispose() = ()
                member this.Initialize() = initFunc this
                [<CLIEvent>]
                member this.OnEvent = eventForUs.Publish
               interface IDisposable with
                member this.Dispose() = ()
            }

        let getProcess (processFunc:Dictionary<string, obj>->unit) =
            { new IProcessProvider with
                member this.Process(x) = processFunc x
            }
    
        let callEvery2Seconds (func:Dictionary<string, obj>->unit) =
            let rec callEvery2Seconds'(seconds) =
                Thread.Sleep(2000)
                func(System.Linq.Enumerable.ToDictionary( [|seconds+2|], fun(x) -> "seconds"))
                callEvery2Seconds' seconds+2
            callEvery2Seconds' 0 |> ignore

        let initialize (evt:IEventProvider) =
            let fireEvent(dic:Dictionary<string, obj>) = 
                eventForUs.Trigger(null, ASBEventArgs(dic))
            callEvery2Seconds fireEvent
                
        let printSeconds (data:Dictionary<string, obj>) =
            let seconds = data.TryGetValue("seconds")
            let message = 
                match seconds with
                | (a, b) when a=true -> "Got "+b.ToString()+" seconds"
                | _ -> "error"
            printfn "%s" message

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
            processor [getRules (getProcess(printSeconds), getEvent(initialize))]

        //let Rules = [yield ]

