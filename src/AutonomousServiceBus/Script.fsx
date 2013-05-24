// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#load "Interfaces.fs"
#load "Agents.fs"
open AutonomousServiceBus.Agents
//#load "Library1.fs"

    type Agent<'T> = MailboxProcessor<'T>
    type AgentMessage =
        |  TakeAgent of string*AsyncReplyChannel<KeepingAgent>
        |  Reset

    type AgentAgent() =
        let agentHolder = Agent.Start(fun agent ->
            let rec loop (agents:Map<string, KeepingAgent>) = async {
                let! msg = agent.Receive()
                match msg with
                | TakeAgent (name, reply) ->
                    match agents.TryFind name with
                    | Some(agent) -> 
                        reply.Reply(agent)
                        return! loop agents
                    | None -> 
                        let newAgent = KeepingAgent()
                        reply.Reply(newAgent)
                        return! loop (agents.Add(name, newAgent))
                | Reset ->
                    return! loop Map.empty<string, KeepingAgent>}
            loop Map.empty<string, KeepingAgent>)
        member this.Take name =
            agentHolder.PostAndReply(fun x -> TakeAgent(name, x))
    

// Define your library scripting code here

