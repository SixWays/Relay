using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Interface
namespace Sigtrap.Relays {
	public interface IRelayBinding<TDelegate> where TDelegate:class {
		/// <summary>
		/// Is the listener currently subscribed to the Relay?
		/// </summary>
		bool enabled {get;}
		/// <summary>
		/// Should enabling the binding add the listener to the Relay if already added elsewhere?
		/// </summary>
		bool allowDuplicates {get; set;}
		/// <summary>
		/// Checks if the bound Relay is null.
		/// </summary>
		bool relayExists {get;}
		/// <summary>
		/// How many persistent listeners does the bound Relay currently have?
		/// </summary>
		uint listenerCount {get;}

		/// <summary>
		/// Enable or disable the listener on the bound Relay.
		/// </summary>
		/// <returns><c>True</c> if listener was enabled/disabled successfully, <c>false</c> otherwise.true</returns>
		bool Enable(bool enable);
	}
}
#endregion

#region Implementation
namespace Sigtrap.Relays.Binding {
	public class RelayBinding<TDelegate> : IRelayBinding<TDelegate> where TDelegate:class {
		public static RelayBinding<TDelegate> CreateInstance(IRelayLinkBase<TDelegate> relay, TDelegate listener, bool allowDuplicates){
			RelayBinding<TDelegate> result = new RelayBinding<TDelegate>();
			result._relay = relay;
			result._listener = listener;
			result.allowDuplicates = allowDuplicates;
			return result;
		}

		protected IRelayLinkBase<TDelegate> _relay {get; private set;}
		protected TDelegate _listener {get;	private set;}

		/// <summary>
		/// Is the listener currently subscribed to the Relay?
		/// </summary>
		public bool enabled {get; private set;}
		/// <summary>
		/// Should enabling the binding add the listener to the Relay if already added elsewhere?
		/// </summary>
		public bool allowDuplicates {get; set;}
		/// <summary>
		/// Checks if the bound Relay is null.
		/// </summary>
		public bool relayExists {get {return _relay != null;}}
		/// <summary>
		/// How many persistent listeners does the bound Relay currently have?
		/// </summary>
		public uint listenerCount {get {return _relay.listenerCount;}}

		/// <summary>
		/// Enable or disable the listener on the bound Relay.
		/// </summary>
		/// <returns><c>True</c> if listener was enabled/disabled successfully, <c>false</c> otherwise.true</returns>
		public bool Enable(bool enable){
			if (enable){
				if (!enabled){
					if (_relay.AddListener(_listener, allowDuplicates)){
						enabled = true;
						return true;
					}					
				}
			} else {
				if (enabled){
					if (_relay.RemoveListener(_listener)){
						enabled = false;
						return true;
					}
				}
			}
			return false;
		}
	}
}
#endregion