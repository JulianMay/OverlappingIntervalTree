# OverlappingIntervalTree
Helpfull in-memory index for values in (overlapping) intervals.
The initial case was retrieving all policies and modifiers that could potentially affect the outcome of a query to a complex rate-calculation. 

Maybe I'll write some more, but for now; here are the salient points:
* Immutable internal structure (for concurrent reads)
* Super simple to use
* Define your own Intervals or use the supplied "Period" interval
* Optimized for read-efficiency (meaning writes are comparitively slow/heavy)

For deeper documentation, see unittests and the respective graphical representations in "Documentation" folder
