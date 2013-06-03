open System   
open Bus   
let x = "
 event 'shootEvery2Seconds' : 'repeat'
with period = 2000;
transform 'elapsedToMessage' : 'rename'
with renames = [('elapsedMs','message')];
command 'callMyService' : 'callRest'
with host = 'http://localhost:3883/store/message';
protocol = 'JSON';
rule 'rule1': when 'shootEvery2Seconds' transform 'elapsedToMessage' do 'callMyService'
"   
//    SELECT x, y, z   
//    FROM t1   
//    LEFT JOIN t2   
//    INNER JOIN t3 ON t3.ID = t2.ID   
//    WHERE x = 50 AND y = 20   
//    ORDER BY x ASC, y DESC, z   
//"   
 
let lexbuf = Lexing.LexBuffer<_>.FromString x
let y = BusParser.start BusLexer.tokenize lexbuf   
printfn "%A" y   
 
Console.WriteLine("(press any key)")   
Console.ReadKey(true) |> ignore