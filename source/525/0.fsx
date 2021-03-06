module GASig

open System
open System.Collections.Generic
open System.IO
open System.Reflection

open Microsoft.Build.Framework
open Microsoft.Build.Utilities

let loadAssembly references (qname: string) =
    let name = qname.Split([|','|]).[0].ToLower()

    match references |> Array.tryFind (fun r -> Path.GetFileNameWithoutExtension(r).ToLower() = name) with
    | Some path -> Assembly.ReflectionOnlyLoadFrom(path)
    | None -> Assembly.ReflectionOnlyLoad(qname)

let outsort fd prefix data =
    data |> Seq.map (sprintf "%O") |> Seq.sort |> Seq.iter (fprintfn fd "%s%s" prefix)

let outasm fd (asm: Assembly) =
    asm.GetCustomAttributesData() |> outsort fd ""
    fprintfn fd "assembly %s\n" asm.FullName

    for t in asm.GetExportedTypes() |> Array.sortBy (fun t -> t.FullName) do
        t.GetCustomAttributesData() |> outsort fd ""
        if t.IsEnum then
            fprintfn fd "enum %O: %O = %s" t (t.GetEnumUnderlyingType()) (t.GetEnumNames() |> String.concat ", ")
        else
            fprintfn fd "%s %O [%O]" (if t.IsValueType then "struct" else "class") t t.Attributes
            if t.BaseType <> null then fprintfn fd "\tinherit %O" t.BaseType
            t.GetInterfaces() |> outsort fd "\tinterface "

            t.GetMembers()
            |> Seq.map (fun m ->
                m,
                match m with
                | :? EventInfo as e -> sprintf "e %O" e
                | :? FieldInfo as f -> sprintf "f %O" f
                | :? ConstructorInfo as c -> sprintf "c %s(%s)" c.Name (c.GetParameters() |> Array.map string |> String.concat ", ")
                | :? MethodInfo as m -> sprintf "m %O %s(%s)" m.ReturnType m.Name (m.GetParameters() |> Array.map string |> String.concat ", ")
                | :? PropertyInfo as p -> sprintf "p %O" p
                | :? Type as t -> sprintf "t %O" t
                | x -> failwithf "Unknown member %O" x)
            |> Seq.sortBy snd
            |> Seq.iter (fun (m, s) ->
                m.GetCustomAttributesData() |> outsort fd "\t"
                fprintfn fd "\t%s" s)
        fprintfn fd ""

let writeFileIfDiffers path contents =
    try
        let current = File.ReadAllText(path)
        if current <> contents then File.WriteAllText(path, contents)
    with e -> 
        File.WriteAllText(path, contents)

type ResolveHandlerScope(references) =
    let handler = ResolveEventHandler(fun _ args -> loadAssembly references args.Name)

    do AppDomain.CurrentDomain.add_ReflectionOnlyAssemblyResolve(handler)

    interface IDisposable with
        member this.Dispose() =
            AppDomain.CurrentDomain.remove_ReflectionOnlyAssemblyResolve(handler)

type AssemblySignatureGenerator() =
    inherit MarshalByRefObject()

    member this.Generate(input, output: string, references) =
        printfn "Generate %s" output
        use scope = new ResolveHandlerScope(references)
        let asm = Assembly.ReflectionOnlyLoadFrom(input)

        let fd = new StringWriter()
        outasm fd asm

        writeFileIfDiffers output (fd.ToString())

type GenerateAssemblySignature() =
    inherit Task()

    member val Input: ITaskItem = null with get, set
    member val Output: ITaskItem = null with get, set
    member val References: ITaskItem[] = [||] with get, set

    override this.Execute() =
        let domain = AppDomain.CreateDomain("gasig")

        try
            let gen = domain.CreateInstanceFromAndUnwrap(typeof<AssemblySignatureGenerator>.Assembly.Location, typeof<AssemblySignatureGenerator>.FullName)
            (gen :?> AssemblySignatureGenerator).Generate(this.Input.ItemSpec, this.Output.ItemSpec, this.References |> Array.map (fun r -> r.ItemSpec))
            true
        finally
            AppDomain.Unload(domain)

type GetAssemblySignatureList() =
    inherit Task()

    member val Inputs: ITaskItem[] = [||] with get, set

    override this.Execute() =
        printfn "Get!"
        for i in this.Inputs do
            let sigf = i.ItemSpec + ".sig"
            i.SetMetadata("Signature", if File.Exists(sigf) then sigf else i.ItemSpec)

        true
