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
            let rec callEvery2Seconds'(seconds) =
                Thread.Sleep(2000)
                func(System.Linq.Enumerable.ToDictionary( [|seconds+2|], fun(x) -> "seconds"))
                callEvery2Seconds' (seconds+2)
            callEvery2Seconds' 0 |> ignore

        let initialize (evt:Event<ASBEventDelegate, ASBEventArgs>) =
            let fireEvent(dic:Dictionary<string, obj>) = 
                evt.Trigger(null, ASBEventArgs(dic))
            callEvery2Seconds fireEvent
                
        let printAnything (propertyName:string, data:Dictionary<string, obj>) : unit =
            let seconds = data.TryGetValue(propertyName)
            let message = 
                match seconds with
                | (a, b) when a=true -> "Got "+b.ToString()+" "+propertyName
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
            processor [getRules (getProcess(fun dic -> printAnything ("seconds", dic)), getEvent(initialize))]

        //let Rules = [yield ]

