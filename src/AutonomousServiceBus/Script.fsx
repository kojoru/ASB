// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#load "Interfaces.fs"
#load "Agents.fs"
open AutonomousServiceBus.Agents
#I @"E:\Source\ASB\src\ClassLibrary\bin\Release\"
#r "ClassLibrary"
open ClassLibrary
#I @"E:\Source\ASB\src\packages\HtmlAgilityPack.1.4.6\lib\Net45\"
#r "HtmlAgilityPack"
open HtmlAgilityPack
//#load "Library1.fs"
open System.Net
let ck = new CookieContainer()
let cl = new CookieAwareWebClient(ck)
let x = new System.Collections.Specialized.NameValueCollection()
x.Add ("action", "login")
x.Add("textfield", "tzakharova@hse.ru")
x.Add("textfield2", "NIRS542admin")
let ans = cl.UploadValues("http://nirs.hse.ru/nirs/auth.php", x)
let ans2 = cl.DownloadString("http://nirs.hse.ru/nirs/admin/works.php?status=Unchecked")
let d = HtmlAgilityPack.HtmlDocument()
d.LoadHtml(ans2)
let docs = d.DocumentNode.SelectNodes("//td/table/tr/td/a/strong")
let f = fun (x:HtmlNode) -> printf "%s" (x.GetAttributeValue("href"))
List.iter f (List.ofSeq docs )
