namespace AutonomousServiceBus
module Agents =
    type Agent<'T> = MailboxProcessor<'T>

    type Message =
        |  TakeDataChunk of string
        |  GetData of AsyncReplyChannel<List<string>>
        |  GetFirst of int*AsyncReplyChannel<List<string>>
        |  Reset

    type KeepingAgent() =
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
                    return! loop [] }
            loop [] )
        member this.Give(s) =
            chat.Post(TakeDataChunk s)
        member this.Take() =
            chat.PostAndReply(GetData)
        member this.Take(num) =
            chat.PostAndReply(fun x -> GetFirst(num, x))
    
    type AgentMessage<'a, 'b when 'a:comparison> =
        |  TakeAgent of 'a*AsyncReplyChannel<'b>
        |  Reset

    type DictionaryAgent<'a,'b  when 'a:comparison and 'b:(new: unit->'b)>() =
        let agentHolder = Agent.Start(fun agent ->
            let rec loop (agents:Map<'a,'b>) = async {
                let! msg = agent.Receive()
                match msg with
                | TakeAgent (name, reply) ->
                    match agents.TryFind name with
                    | Some(agent) -> 
                        reply.Reply(agent)
                        return! loop agents
                    | None -> 
                        let newAgent = new 'b()
                        reply.Reply(newAgent)
                        return! loop (agents.Add(name, newAgent))
                | Reset ->
                    return! loop Map.empty<'a,'b>}
            loop Map.empty<'a, 'b>)
        member this.Take name =
            agentHolder.PostAndReply(fun x -> TakeAgent(name, x))

     type AgentAgent = DictionaryAgent<string, KeepingAgent>
