using System;
using Sigtrap.Relays;

namespace Sigtrap.Relays.Link {
	#region Interfaces
	public interface IRelayLinkBase<TDelegate> where TDelegate:class {
		uint listenerCount {get;}
		uint oneTimeListenersCount {get;}

		bool Contains(TDelegate listener);
		void AddListener(TDelegate listener, bool allowDuplicates = false);
		void AddOnce(TDelegate listener, bool allowDuplicates = false);

		bool RemoveListener(TDelegate listener);
		void RemoveAll(bool removePersistentListeners = true, bool removeOneTimeListeners = false);
	}
	public interface IRelayLink : IRelayLinkBase<System.Action> {}
	public interface IRelayLink<T> : IRelayLinkBase<System.Action<T>> {}
	public interface IRelayLink<T, U> : IRelayLinkBase<System.Action<T, U>> {}
	public interface IRelayLink<T, U, V> : IRelayLinkBase<System.Action<T, U, V>> {}
	public interface IRelayLink<T, U, V, W> : IRelayLinkBase<System.Action<T, U, V, W>> {}
	#endregion

	#region Classes
	public abstract class RelayLinkBase<TDelegate> : IRelayLinkBase<TDelegate> where TDelegate:class {
		protected RelayBase<TDelegate> _relay;

		#region IRelayLinkBase implementation
		public uint listenerCount {get {return _relay.listenerCount;}}
		public uint oneTimeListenersCount {get {return _relay.oneTimeListenersCount;}}

		public bool Contains(TDelegate listener){
			return _relay.Contains(listener);
		}
		public void AddListener(TDelegate listener, bool allowDuplicates = false){
			_relay.AddListener(listener, allowDuplicates);
		}
		public void AddOnce(TDelegate listener, bool allowDuplicates = false){
			_relay.AddOnce(listener, allowDuplicates);
		}
		public bool RemoveListener(TDelegate listener){
			return _relay.RemoveListener(listener);
		}
		public void RemoveAll(bool removePersistentListeners = true, bool removeOneTimeListeners = false){
			_relay.RemoveAll(removePersistentListeners, removeOneTimeListeners);
		}
		#endregion
	}

	public class RelayLink : RelayLinkBase<Action>, IRelayLink {
		public static RelayLink CreateInstance(Relay relay){
			var result = new RelayLink();
			result._relay = relay;
			return result;
		}
	}
	public class RelayLink<T> : RelayLinkBase<Action<T>>, IRelayLink<T> {
		public static RelayLink<T> CreateInstance(Relay<T> relay){
			var result = new RelayLink<T>();
			result._relay = relay;
			return result;
		}
	}
	public class RelayLink<T, U> : RelayLinkBase<Action<T, U>>, IRelayLink<T, U> {
		public static RelayLink<T, U> CreateInstance(Relay<T, U> relay){
			var result = new RelayLink<T, U>();
			result._relay = relay;
			return result;
		}
	}
	public class RelayLink<T, U, V> : RelayLinkBase<Action<T, U, V>>, IRelayLink<T, U, V> {
		public static RelayLink<T, U, V> CreateInstance(Relay<T, U, V> relay){
			var result = new RelayLink<T, U, V>();
			result._relay = relay;
			return result;
		}
	}
	public class RelayLink<T, U, V, W> : RelayLinkBase<Action<T, U, V, W>>, IRelayLink<T, U, V, W> {
		public static RelayLink<T, U, V, W> CreateInstance(Relay<T, U, V, W> relay){
			var result = new RelayLink<T, U, V, W>();
			result._relay = relay;
			return result;
		}
	}
	#endregion
}