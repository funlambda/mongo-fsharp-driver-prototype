namespace FSharp.MongoDB.Driver

open MongoDB.Bson

open MongoDB.Driver.Core
open MongoDB.Driver.Core.Protocol

[<AutoOpen>]
module Fluent =

    let makeQueryDoc query sort (options : Scope.QueryOptions) =
        let addElem name value (doc : BsonDocument) =
            match value with
            | Some x -> doc.Add(name, BsonValue.Create(x))
            | None -> doc

        match query with
        | Some x ->
            BsonDocument("$query", x)
            |> addElem "$orderby" sort
            |> addElem "$comment" options.Comment
            |> addElem "$hint" options.Hint
            |> addElem "$maxScan" options.MaxScan
            |> addElem "$max" options.Max
            |> addElem "$min" options.Min
            |> addElem "$snapshot" options.Snapshot
        | None -> failwith "unset query"

    let makeTextSearchDoc clctn text query project limit (options : Scope.TextSearchOptions) =
        let addElem name value (doc : BsonDocument) =
            match value with
            | Some x -> doc.Add(name, BsonValue.Create(x))
            | None -> doc

        BsonDocument([ BsonElement("text", BsonString(clctn))
                       BsonElement("search", BsonString(text))
                       BsonElement("limit", BsonInt32(limit)) ])
        |> addElem "filter" query
        |> addElem "project" project
        |> addElem "language" options.Language

    [<RequireQualifiedAccess>]
    module Scope =

        let find query scope =
            { scope with Query = Some query }

        let fields project scope =
            { scope with Project = Some project }

        let sort order scope =
            { scope with Sort = Some order }

        let limit n scope =
            { scope with Limit = n }

        let skip n scope =
            { scope with Skip = n }

        let withQueryOptions options scope =
            { scope with QueryOptions = options }

        let withReadPreference readPref scope =
            { scope with ReadPreference = readPref }

        let withWriteOptions options scope =
            { scope with WriteOptions = options }

        let count (scope : Scope<'DocType>) =
            let backbone = scope.Backbone
            let db = scope.Database
            let clctn = scope.Collection

            let cmd = BsonDocument("count", BsonString(clctn))

            match scope.Query with
            | Some x -> cmd.Add("query", x) |> ignore
            | None -> ()

            let limit = scope.Limit
            let skip = scope.Skip

            cmd.AddRange([ BsonElement("limit", BsonInt32(limit))
                           BsonElement("skip", BsonInt32(skip)) ]) |> ignore

            backbone.Run db cmd

        let remove (scope : Scope<'DocType>) =
            let backbone = scope.Backbone
            let db = scope.Database
            let clctn = scope.Collection

            // Raise error if sort has been specified
            if scope.Sort.IsSome then failwith "sort has been specified"

            // Raise error if limit has been specified (as other than 1)
            if scope.Limit <> 0 && scope.Limit <> 1 then failwith "limit has been specified"

            // Raise error if skip has been specified
            if scope.Skip <> 0 then failwith "skip has been specified"

            let query = makeQueryDoc scope.Query None scope.QueryOptions
            if scope.WriteOptions.Isolated then query.Add("$isolated", BsonInt32(1)) |> ignore

            let flags = DeleteFlags.None
            let settings = { Operation.DefaultSettings.remove with WriteConcern = scope.WriteOptions.WriteConcern }

            backbone.Remove db clctn query flags settings

        let removeOne (scope : Scope<'DocType>) =
            let backbone = scope.Backbone
            let db = scope.Database
            let clctn = scope.Collection

            // Raise error if sort has been specified
            if scope.Sort.IsSome then failwith "sort has been specified"

            // Ignore limit

            // Raise error if skip has been specified
            if scope.Skip <> 0 then failwith "skip has been specified"

            let query = makeQueryDoc scope.Query None scope.QueryOptions
            if scope.WriteOptions.Isolated then query.Add("$isolated", BsonInt32(1)) |> ignore

            let flags = DeleteFlags.Single
            let settings = { Operation.DefaultSettings.remove with WriteConcern = scope.WriteOptions.WriteConcern }

            backbone.Remove db clctn query flags settings

        let update update (scope : Scope<'DocType>) =
            let backbone = scope.Backbone
            let db = scope.Database
            let clctn = scope.Collection

            // Raise error if sort has been specified
            if scope.Sort.IsSome then failwith "sort has been specified"

            // Raise error if limit has been specified (as other than 1)
            if scope.Limit <> 0 && scope.Limit <> 1 then failwith "limit has been specified"

            // Raise error if skip has been specified
            if scope.Skip <> 0 then failwith "skip has been specified"

            let query = makeQueryDoc scope.Query None scope.QueryOptions
            if scope.WriteOptions.Isolated then query.Add("$isolated", BsonInt32(1)) |> ignore

            let flags = UpdateFlags.Multi
            let settings = { Operation.DefaultSettings.update with CheckUpdateDocument = true
                                                                   WriteConcern = scope.WriteOptions.WriteConcern }

            backbone.Update db clctn query update flags settings

        let updateOne update (scope : Scope<'DocType>) =
            let backbone = scope.Backbone
            let db = scope.Database
            let clctn = scope.Collection

            // Raise error if sort has been specified
            if scope.Sort.IsSome then failwith "sort has been specified"

            // Ignore limit

            // Raise error if skip has been specified
            if scope.Skip <> 0 then failwith "skip has been specified"

            let query = makeQueryDoc scope.Query None scope.QueryOptions

            let flags = UpdateFlags.None
            let settings = { Operation.DefaultSettings.update with CheckUpdateDocument = true
                                                                   WriteConcern = scope.WriteOptions.WriteConcern }

            backbone.Update db clctn query update flags settings

        let replace update (scope : Scope<'DocType>) =
            let backbone = scope.Backbone
            let db = scope.Database
            let clctn = scope.Collection

            // Raise error if sort has been specified
            if scope.Sort.IsSome then failwith "sort has been specified"

            // Raise error if limit has been specified (as other than 1)
            if scope.Limit <> 0 && scope.Limit <> 1 then failwith "limit has been specified"

            // Raise error if skip has been specified
            if scope.Skip <> 0 then failwith "skip has been specified"

            let query = makeQueryDoc scope.Query None scope.QueryOptions
            if scope.WriteOptions.Isolated then query.Add("$isolated", BsonInt32(1)) |> ignore

            let flags = UpdateFlags.Multi
            let settings = { Operation.DefaultSettings.update with CheckUpdateDocument = false
                                                                   WriteConcern = scope.WriteOptions.WriteConcern }

            backbone.Update db clctn query update flags settings

        let replaceOne update (scope : Scope<'DocType>) =
            let backbone = scope.Backbone
            let db = scope.Database
            let clctn = scope.Collection

            // Raise error if sort has been specified
            if scope.Sort.IsSome then failwith "sort has been specified"

            // Ignore limit

            // Raise error if skip has been specified
            if scope.Skip <> 0 then failwith "skip has been specified"

            let query = makeQueryDoc scope.Query None scope.QueryOptions

            let flags = UpdateFlags.None
            let settings = { Operation.DefaultSettings.update with CheckUpdateDocument = false
                                                                   WriteConcern = scope.WriteOptions.WriteConcern }

            backbone.Update db clctn query update flags settings

        let explain (scope : Scope<'DocType>) =
            let backbone = scope.Backbone
            let db = scope.Database
            let clctn = scope.Collection

            let query = makeQueryDoc scope.Query scope.Sort scope.QueryOptions

            let project =
                match scope.Project with
                | Some x -> x
                | None -> null

            query.Add("$explain", BsonInt32(1)) |> ignore

            let limit = scope.Limit
            let skip = scope.Skip

            let flags = QueryFlags.None
            let settings = Operation.DefaultSettings.query

            let res = backbone.Find<BsonDocument> db clctn query project limit skip flags settings
            use iter = res.GetEnumerator()

            if not (iter.MoveNext()) then raise <| MongoOperationException("explain command missing response document")
            iter.Current

        let textSearch text (scope : Scope<'DocType>) =
            let backbone = scope.Backbone
            let db = scope.Database
            let clctn = scope.Collection

            let cmd = makeTextSearchDoc clctn text scope.Query scope.Project scope.Limit { Language = None }

            backbone.Run db cmd

        let textSearchWithOptions text (options : Scope.TextSearchOptions) (scope : Scope<'DocType>) =
            let backbone = scope.Backbone
            let db = scope.Database
            let clctn = scope.Collection

            let cmd = makeTextSearchDoc clctn text scope.Query scope.Project scope.Limit options

            backbone.Run db cmd
