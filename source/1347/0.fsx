/// Attempts to initialize a 2D array using the specified base offsets and lengths.
/// The provided function can return 'None' to indicate a failure - if the initializer
/// fails for any of the location inside the array, the construction is stopped and
/// the function returns 'None'.
let tryInitBased base1 base2 length1 length2 f = 
  let arr = Array2D.createBased base1 base2 length1 length2 (Unchecked.defaultof<_>)
  /// Recursive function that fills a specified 'x' line
  /// (returns false as soon as any call to 'f' fails, or true)
  let rec fillY x y = 
    if y < (base2+length2) then
      match f x y with
      | Some v -> 
          arr.[x, y] <- v
          fillY x (y + 1)
      | _ -> false
    else true
  /// Recursive function that iterates over all 'x' positions
  /// and calls 'fillY' to fill individual lines
  let rec fillX x = 
    if x < (base1+length1) then 
      if fillY x base2 then fillX (x + 1)
      else false
    else true
  if fillX base1 then Some arr else None
