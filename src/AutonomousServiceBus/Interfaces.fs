﻿module interfaces
    open System
    open System.Collections.Generic

    type ASBEventArgs(msg:string) =
        inherit EventArgs()
        member this.Message = msg
    type ASBEventDelegate =
        delegate of obj * ASBEventArgs -> unit

    type IEventProvider =
        inherit IDisposable
        abstract Initialize: unit -> unit
        abstract Dispose: unit -> unit
        [<CLIEvent>]
        abstract OnEvent: IEvent<ASBEventDelegate, ASBEventArgs>

    type IProcessProvider =
        abstract Process: Dictionary<string, obj> -> unit

    type IRule =
        abstract EventProvider : IEventProvider with get
        abstract ProcessProvider : IProcessProvider with get

