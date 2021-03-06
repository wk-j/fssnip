// Consider two distinct interfaces
type IA = interface end
type IB = interface end

// And two classes that implement both of the interfaces
type First() = 
  interface IA
  interface IB

type Second() = 
  interface IA
  interface IB

// Now, what implicit upcast should the compiler insert here?
// The return type could be either IA, IB or obj, but there is 
// no _unique_ solution.
let test = 
  if 1 = 1 then First()
  else Second()