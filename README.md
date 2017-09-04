# Relay
Fast, light, GC-friendly C# signals/events for Unity.

C# `event` multicasting is a nice language-level feature, but in Unity's version of Mono, events allocate a lot on addition/removal and aren't always easy to manipulate. Relay aims to address these issues.

* Additional functionality
  * `AddOnce()` automatically removes a listener after dispatch
  * Guard against duplicate listeners for safety, or disable checking for increased performance
  * `Contains()` tells you whether a delegate is already subscribed
  * Debug mode helps diagnose ["dangling" or "lapsed" listeners](https://en.wikipedia.org/wiki/Lapsed_listener_problem) - a common cause of GC issues in event-driven architectures
* Minimal allocation using arrays of delegates
  * C# `event` re-allocates the _entire multicast object_ each time `+=` or `-=` called
  * Relay uses a dynamic array of singlecast delegates
* Performant addition, removal and dispatch
  * Geometric array resize (like `List<>.Add()`) amortizes addition costs
  * Array item removal is faster than `List<>.Remove()`
  * Dispatch virtually as fast as native multicast
* Battle-tested
  * Heavily used in core code of our game [Sublevel Zero](http://www.sublevelzerogame.com) on PC, VR, PS4 and Xbox One

Relay is inspired by the Signals implementation in [StrangeIoC](https://github.com/strangeioc/strangeioc) (but drastically improves performance and allocation).

## Performance (Tested in Unity 5.6)
Profiled performance comparison of `event`, `Relay` and a `List` of delegates. Each operation is performed 100000 times with 10 listeners. Add Unique means disallowing duplicate listeners.

### Time (ms)
|Method|Add|Add Unique|Remove|Clear|Dispatch|
|-----|-----|-----|-----|-----|-----|
|event|1465 |N/A  |1155 |3.688|67.28|
|Relay|150.5|318.2|270.0|10.27|79.92|
|List |183.5|466.0|368.0|9.436|79.00|

### GC Alloc (MB)
|Method|Add|Remove|
|-----|----|----|
|event|630 |530 |
|Relay|38.1|0   |
|List |32.8|0   |

All other operations are allocation-free. Note that this is *worst-case* GC performance for `Relay` (and `List`) as each loop creates a completely new `Relay`. Populating and clearing the same `Relay` 100000 times just allocates 400B initially, while `event` still allocates 630MB.

Note: Under .NET 4.6 `event` addition and removal is 2-7x faster than under Unity.

Perf tests and a full suite of unit tests are included in Relay/Editor.