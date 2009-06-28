// (c) Microsoft Corporation 2005-2009.

namespace Microsoft.FSharp.Linq

    open System

    module Query =

        /// Evaluate the quotation expression by first converting to a LINQ expression tree
        /// making use of IQueryable operators and then executing expression tree
        ///
        /// Exceptions: <c>InvalidArgumentException</c> will be raised if the input expression is
        /// not in the subset that can be converted to a LINQ expression tree
        val query : Quotations.Expr<'T> -> 'T


        /// When used in queries, this operator corresponds to the LINQ Min operator and the <c>query</c> convertor recognises it as such
        /// It differs in return type from <c>Seq.min_by</c>
        val contains : 
            key:'T -> 
            source:seq<'T> -> 
               bool

        /// When used in queries, this operator corresponds to the LINQ Min operator and the <c>query</c> convertor recognises it as such
        /// It differs in return type from <c>Seq.min_by</c>
        val minBy : 
            keySelector:('T -> 'Key) -> 
            source:seq<'T> -> 
               'Key

        /// When used in queries, this operator corresponds to the LINQ Max operator and the <c>query</c> convertor recognises it as such
        /// It differs in return type from <c>Seq.max_by</c>
        val maxBy : 
            keySelector:('T -> 'Key) -> 
            source:seq<'T> -> 
               'Key

        /// When used in queries, this operator corresponds to the LINQ Join operator and the <c>query</c> convertor recognises it as such
        val groupBy : 
            keySelector:('T -> 'Key) -> 
            source:seq<'T> -> 
               seq<System.Linq.IGrouping<'Key,'T>>

        /// This join operator corresponds to the LINQ Join operator and the <c>query</c> convertor recognises it as such
        val join : 
            outerSource:seq<'Outer> -> 
            innerSource:seq<'Inner> -> 
            outerKeySelector:('Outer -> 'Key) -> 
            innerKeySelector:('Inner -> 'Key) -> 
            resultSelector:('Outer -> 'Inner -> 'Result) ->
               seq<'Result>


        /// This join operator implements the LINQ GroupJoin operator and the <c>query</c> convertor recognises it as such
        val groupJoin : 
            outerSource:seq<'Outer> -> 
            innerSource:seq<'Inner> -> 
            outerKeySelector:('Outer -> 'Key) -> 
            innerKeySelector:('Inner -> 'Key) -> 
            resultSelector:('Outer -> seq<'Inner> -> 'Result) ->
               seq<'Result>

        /// When used in queries, this operator corresponds to the LINQ Min operator and the <c>query</c> convertor recognises it as such
        /// It differs in return type from <c>Seq.min_by</c>
        [<Obsolete("This F# library function has been renamed. Use 'minBy' instead",true)>]
        val min_by : 
            keySelector:('T -> 'Key) -> 
            source:seq<'T> -> 
               'Key

        /// When used in queries, this operator corresponds to the LINQ Max operator and the <c>query</c> convertor recognises it as such
        /// It differs in return type from <c>Seq.max_by</c>
        [<Obsolete("This F# library function has been renamed. Use 'maxBy' instead",true)>]
        val max_by : 
            keySelector:('T -> 'Key) -> 
            source:seq<'T> -> 
               'Key

        /// When used in queries, this operator corresponds to the LINQ Join operator and the <c>query</c> convertor recognises it as such
        [<Obsolete("This F# library function has been renamed. Use 'groupBy' instead",true)>]
        val group_by : 
            keySelector:('T -> 'Key) -> 
            source:seq<'T> -> 
               seq<System.Linq.IGrouping<'Key,'T>>


        /// This join operator implements the LINQ GroupJoin operator and the <c>query</c> convertor recognises it as such
        [<Obsolete("This F# library function has been renamed. Use 'groupJoin' instead",true)>]
        val group_join : 
            outerSource:seq<'Outer> -> 
            innerSource:seq<'Inner> -> 
            outerKeySelector:('Outer -> 'Key) -> 
            innerKeySelector:('Inner -> 'Key) -> 
            resultSelector:('Outer -> seq<'Inner> -> 'Result) ->
               seq<'Result>

