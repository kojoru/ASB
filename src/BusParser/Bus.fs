// Sql.fs
module Bus

type value =   
    | Int of int  
    | Float of float  
    | String of string  
 
type param = 
    | Def of string * value
    | And of param * param

type obj = 
    |Inline of string * string * option<param>
    |Outline of string

type event = obj
type command = obj
type transform = obj

type transforms = 
    | OneTransform of transform
    | Transfroms of transforms * transforms

type rule = option<string> * event * option<transforms> * command

//type dir = Asc | Desc   
//type op = Eq | Gt | Ge | Lt | Le    // =, >, >=, <, <=   
// 
//type order = string * dir   
// 
//type where =   
//    | Cond of (value * op * value)   
//    | And of where * where   
//    | Or of where * where   
// 
//type joinType = Inner | Left | Right   
// 
//type join = string * joinType * where option    // table name, join, optional "on" clause   
// 
//type sqlStatement =   
//    { Table : string;   
//        Columns : string list;   
//        Joins : join list;   
//        Where : where option;   
//        OrderBy : order list }