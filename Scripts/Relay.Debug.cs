// Track all Relays to diagnose dangling listeners.
//   Define SIGTRAP_RELAY_DBG to enable
//   Adds ENORMOUS CPU/GC overhead!
//   set Relay.recordDebugData = false to disable temporarily
//     (still has some overhead on RemoveListener)
//
// Relay.ListRelays() gives a nice(ish) rundown of all existing listeners across all existing Relays
// To isolate a particular observer (i.e. something with methods/delegates listening to one or more Relays)
//   use Relay.ListRelays(<your listener object>).
//
// It's impossible to know which object "owns" a particular Relay since any instance field is just a reference to the Relay on the heap.
// So it CAN be tricky to work out where exactly a dangling listener might be.
// To try and alleviate this, in debug mode a stack trace is stored of every AddListener call, which gets output by ListRelays.
// Hopefully in most cases these traces can help you identify which Relay it refers to.

#if SIGTRAP_RELAY_DBG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace Sigtrap.Relays {
	public static class _RelayDebugger {
		class ListenerData {
			public object owner { get; private set; }
			public object relay { get; private set; }
			public MethodInfo listener { get; private set; }
			public int copies { get; private set; }
			/// <summary>
			/// Stack traces from when listeners were added.
			/// No way to relate removals to additions, 
			/// so all traces stored until no listeners left
			/// </summary>
			/// <value>The add stacks.</value>
			public List<StackFrame[]> addStacks { get; private set; }
			List<string[]> _stackStrings = new List<string[]>();

			public ListenerData(object owner, object relay, MethodInfo listener){
				this.owner = owner;
				this.relay = relay;
				this.listener = listener;
				addStacks = new List<StackFrame[]>();
			}

			public void Push(StackFrame[] trace){
				++copies;
				int len = trace.Length;
				string[] traceStrings = new string[len-2];
				for (int i=2; i<len; ++i){
					var m = trace[i].GetMethod();
					traceStrings[i-2] = string.Format(
						"{0}::{1}({2}) (at {3}:{4})",
						m.DeclaringType.Name, m.Name,
						GetMethodArgs(m), trace[i].GetFileName(),
						trace[i].GetFileLineNumber().ToString()
					);
				}
				foreach (var s in _stackStrings){
					if (s.Length == len-2){
						bool match = true;
						for (int i=0; i<s.Length; ++i){
							if (!string.Equals(s[i], traceStrings[i])){
								match = false;
								break;
							}
						}
						if (match) return;
					}
				}
				_stackStrings.Add(traceStrings);
				addStacks.Add(trace);
			}
			public void Pop(){
				if (copies == 0){
					throw new IndexOutOfRangeException("Trying to pop an empty ListenerData");
				}
				--copies;
				if (copies == 0){
					addStacks.Clear();
				}
			}
			public string GetStackTrace(string indent){
				string result = "";
				for (int i=0; i<_stackStrings.Count; ++i){
					result += indent+"[" + i.ToString() + "]:\n";
					for (int j=0; j<_stackStrings[i].Length; ++j){
						result += indent + "  " + _stackStrings[i][j]+"\n";
					}
				}
				return result;
			}
		}

		public static bool recordDebugData = true;
		static Dictionary<
			object, 				// Owner of listener (delegate target)
			Dictionary<
				object, 			// Relay
				Dictionary<
					MethodInfo,		// Listener
					ListenerData	// Data
				>
			>
		> _listenerData = new Dictionary<object, Dictionary<object, Dictionary<MethodInfo,ListenerData>>>();

		#if UNITY_EDITOR	
		[UnityEditor.MenuItem("Relay/Log Listeners")]
		public static void LogRelaysEditor(){
			UnityEngine.Debug.Log(LogRelays());
		}
		#endif

		/// <summary>
		/// Output a log of all existing Relays and their listeners.
		/// </summary>
		/// <returns>The listeners.</returns>
		public static string LogRelays(){
			string log = "";
			foreach (var ld in _listenerData){
				string s = LogRelays(ld.Key);
				if (!string.IsNullOrEmpty(s)) {
					log += s;
				}
			}
			return log;
		}
		/// <summary>
		/// Output a log of all Relays the specified object has subscribed to.
		/// </summary>
		/// <returns>The listeners.</returns>
		/// <param name="observer">Owner of listeners.</param>
		public static string LogRelays(object observer){
			Dictionary<object, Dictionary<MethodInfo, ListenerData>> listenersByRelay = null;
			if (_listenerData.TryGetValue(observer, out listenersByRelay)){
				string log = "";
				int total = 0;
				foreach (var a in listenersByRelay){
					foreach (var b in a.Value){
						total += b.Value.copies;
						log += string.Format(
							" {1} {2}({3}) ({0} copies) \n   AddListener traces: (including for listeners which have since been removed!)\n{4}",
							b.Value.copies.ToString(), b.Key.ReturnType.Name,
							b.Key.Name, GetMethodArgs(b.Key), b.Value.GetStackTrace("     ")
						);
					}
				}
				if (total == 0) return null;
				log = total.ToString()+" current relay listeners for "+observer.ToString()+":\n"+log;
				return log;
			}
			return null;
		}

		public static void DebugAddListener(object relay, object dlgt){
			if (recordDebugData) {
				DebugGetListenerData(relay, dlgt as Delegate).Push(new StackTrace(true).GetFrames());
			}
		}
		public static void DebugRemListener(object relay, object dlgt){
			DebugGetListenerData(relay, dlgt as Delegate).Pop();
		}

		static ListenerData DebugGetListenerData(object relay, Delegate d){
			ListenerData result = null;
			Dictionary<object, Dictionary<MethodInfo, ListenerData>> byRelay = null;
			if (!_listenerData.TryGetValue(d.Target, out byRelay)){
				byRelay = new Dictionary<object, Dictionary<MethodInfo, ListenerData>>();
				_listenerData.Add(d.Target, byRelay);
			}
			Dictionary<MethodInfo, ListenerData> byMethod = null;
			if (!byRelay.TryGetValue(relay, out byMethod)){
				byMethod = new Dictionary<MethodInfo, ListenerData>();
				byRelay.Add(relay, byMethod);
			}
			if (!byMethod.TryGetValue(d.Method, out result)){
				result = new ListenerData(d.Target, relay, d.Method);
				byMethod.Add(d.Method, result);
			}
			return result;
		}
		static string GetMethodArgs(MethodBase m){
			var ps = m.GetParameters();
			string pss = "";
			for (int i=0; i<ps.Length; ++i){
				pss += ps[i].ParameterType.Name;
				if (i != ps.Length-1){
					pss += ", ";
				}
			}
			return pss;
		}
	}
}
#endif