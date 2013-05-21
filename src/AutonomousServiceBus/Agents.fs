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
    

