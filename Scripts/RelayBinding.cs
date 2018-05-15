using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Interface
namespace Sigtrap.Relays {
	public interface IRelayBinding {
		/// <summary>
		/// Is the listener currently subscribed to the Relay?
		/// </summary>
		bool enabled {get;}
		/// <summary>
		/// Should enabling the binding add the listener to the Relay if already added elsewhere?
		/// </summary>
		bool allowDuplicates {get; set;}
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
	public class RelayBinding<TDelegate> : IRelayBinding where TDelegate:class {
		protected IRelayLinkBase<TDelegate> _relay {get; private set;}
		protected TDelegate _listener {get;	private set;}

		#region Constructors
		private RelayBinding(){}	// Private empty constructor to force use of params
		public RelayBinding(IRelayLinkBase<TDelegate> relay, TDelegate listener, bool allowDuplicates, bool isListening) : this(){
			_relay = relay;
			_listener = listener;
			this.allowDuplicates = allowDuplicates;
			enabled = isListening;
		}
		#endregion

		#region IRelayBinding implementation
		/// <summary>
		/// Is the listener currently subscribed to the Relay?
		/// </summary>
		public bool enabled {get; private set;}
		/// <summary>
		/// Should enabling the binding add the listener to the Relay if already added elsewhere?
		/// </summary>
		public bool allowDuplicates {get; set;}
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
		#endregion
	}
}
#endregion