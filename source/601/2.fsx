(* [omit:Fake definition of Vector3] *)
type Vector3 =
    struct
        val X : float32
        val Y : float32
        val Z : float32

        new(x, y, z) = { X = x; Y = y; Z = z }

        static member public (+) (U : Vector3, V : Vector3) : Vector3 = failwith ""
        static member public (-) (U : Vector3, V : Vector3) : Vector3 = failwith ""
        static member public Dot (U : Vector3, V : Vector3) : float32 = failwith ""
        static member public (*) (k : float32, V : Vector3) : Vector3 = failwith ""
        static member public Multiply (V : Vector3, k : float32) : Vector3 = failwith ""
        member public this.Length() : float32 = failwith ""
    end(*[/omit]*)

/// A three-dimensional vector with a unit of measure. Built on top of Xna's Vector3.
type TypedVector3<[<Measure>] 'M> =
    struct
        val v : Vector3
        new(x : float32<'M>, y : float32<'M>, z : float32<'M>) =
            { v = Vector3(float32 x, float32 y, float32 z) }
        new(V) = { v = V }
    end

[<RequireQualifiedAccessAttribute>]
module TypedVector =
    let add3 (U : TypedVector3<'M>, V : TypedVector3<'M>) =
        new TypedVector3<'M>(U.v + V.v)

    let sub3 (U : TypedVector3<'M>, V : TypedVector3<'M>) =
        new TypedVector3<'M>(U.v - V.v)

    let dot3 (U : TypedVector3<'M>, V : TypedVector3<'N>) =
        Vector3.Dot(U.v, V.v)
        |> LanguagePrimitives.Float32WithMeasure<'M 'N>

    let len3 (U : TypedVector3<'M>) =
        LanguagePrimitives.Float32WithMeasure<'M> (U.v.Length())

    let scale3 (k : float32<'K>, U : TypedVector3<'M>) : TypedVector3<'K 'M> =
        let conv = LanguagePrimitives.Float32WithMeasure<'K 'M>
        let v = Vector3.Multiply(U.v, float32 k)
        new TypedVector3<_>(conv v.X, conv v.Y, conv v.Z)

    let normalize3 (U : TypedVector3<'M>) =
        let len = len3 U
        scale3 ((1.0f / len), U)

type TypedVector3<[<Measure>] 'M>
with
    static member public (*) (k, U) = TypedVector.scale3 (k, U)
    static member public (+) (U, V) = TypedVector.add3 (U, V)
    static member public (-) (U, V) = TypedVector.sub3 (U, V)
    member public this.Length = this |> TypedVector.len3

// Example
[<Measure>] type m
[<Measure>] type s

let speed = new TypedVector3<m/s>(1.0f<_>, 0.0f<_>, 0.0f<_>)
let time = 10.0f<s>
let movement = time * speed
