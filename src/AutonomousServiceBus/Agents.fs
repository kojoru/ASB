namespace AutonomousServiceBus
module Agents =
    type Agent<'T> = MailboxProcessor<'T>

    type Message<'a> =
        |  TakeDataChunk of 'a
        |  GetData of AsyncReplyChannel<List<'a>>
        |  GetFirst of int*AsyncReplyChannel<List<'a>>
        |  Reset

    type StackAgent<'a>() =
        let chat = Agent.Start(fun agent -> 
            let rec loop texts = async {
                let! msg = agent.Receive()
                match msg with 
                | TakeDataChunk text -> 
                    return! loop (text :: texts)
                | GetData reply -> 
                    reply.Reply(texts)
                    return! loop texts 
                | GetFirst (num, reply) ->
                    reply.Reply (Seq.truncate num texts |> Seq.toList)
                    return! loop texts
                | Reset ->
                    return! loop List.Empty }
            loop List.Empty )
        member this.Give(s:'a) =
            chat.Post(TakeDataChunk s)
        member this.Take() =
            chat.PostAndReply(GetData)
        member this.Take(num) =
            chat.PostAndReply(fun x -> GetFirst(num, x))
    type EventKeeper = StackAgent<string>
    type TextKeepingAgent = StackAgent<string>
    
    type LazyInitMessage<'key, 'value when 'key:comparison> =
        |  Take of 'key*AsyncReplyChannel<'value>
        |  Give of 'key*'value
        |  Reset

    type LazyInitAgent<'a,'b  when 'a:comparison and 'b:(new: unit->'b)>() =
        let agentHolder = Agent.Start(fun agent ->
            let rec loop (items:Map<'a,'b>) = async {
                let! msg = agent.Receive()
                match msg with
                | Take (name, reply) ->
                    match items.TryFind name with
                    | Some(agent) -> 
                        reply.Reply(agent)
                        return! loop items
                    | None -> 
                        let newAgent = new 'b()
                        reply.Reply(newAgent)
                        return! loop (items.Add(name, newAgent))
                | Give (name, newItem) ->
                    return! loop (items.Add(name, newItem))
                | Reset ->
                    return! loop Map.empty}
            loop Map.empty<'a, 'b>)
        member this.Take name =
            agentHolder.PostAndReply(fun x -> Take(name, x))  
        member this.Give name item =
            agentHolder.Post(Give (name,item))
              
//    type DictionaryMessage<'a, 'b when 'a:comparison> =
//        |  Take of 'a*AsyncReplyChannel<'b option>
//        |  Give of 'a*'b
//        |  Reset
//
//    type DictionaryAgent<'a when 'a:comparison>() =
//        let agentHolder = Agent.Start(fun agent ->
//            let rec loop (items:Map<'a,'b>) = async {
//                let! msg = agent.Receive()
//                match msg with
//                | Take (name, reply) ->
//                    match items.TryFind name with
//                    | Some(agent) -> 
//                        reply.Reply(agent)
//                        return! loop items
//                    | None -> 
//                        reply.Reply(None)
//                        return! loop items
//                | Give (name, item) ->
//                    return! loop (items.Add (name, Option.Some(item)))
//                | Reset ->
//                    return! loop Map.empty}
//            loop Map.empty<'a, 'b>)
//        member this.Take name =
//            agentHolder.PostAndReply(fun x -> Take(name, x))

     type AgentAgent = LazyInitAgent<string, TextKeepingAgent>
