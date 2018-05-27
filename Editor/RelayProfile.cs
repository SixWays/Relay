#if SIGTRAP_RELAY_TEST
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using Sigtrap.Relays;

namespace Sigtrap.Relays.Tests {
	public static class RelayProfile {
		const int LOOPS = 100000;
		const int DELEGATES = 10;
		static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
		static List<string> LOGS = new List<string>();
		// Delegates are all unique just to make absolutely sure the compiler doesn't do anything super duper clever
		static System.Action<int,float,string>[] allDelegates = new System.Action<int,float,string>[]{
			TestM1, TestM2, TestM3, TestM4, TestM5, TestM6, TestM7, TestM8, TestM9, TestM10
		};

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		static void DrawProgress(int done, int total, string msg){
			EditorUtility.DisplayProgressBar("Relay Profile All", msg, (float)done/(float)total);
		}

		static bool _writeCsv = false;
		static List<string> _csv = new List<string>();

		#if UNITY_EDITOR
		[MenuItem("Relay/Profile/Run All", false, 0)]
		#endif
		public static void RunAll(){
			_writeCsv = true;
			string div = "=======================================================";
			Print("## POPULATE ##");
			DrawProgress(0,5,"Populate");
			Pop();
			Print(div);
			Print("## CREATE AND POPULATE ##");
			DrawProgress(1,5,"Create And Populate");
			CreateAndPop();
			Print(div);
			Print("## REMOVE ##");
			DrawProgress(2,5,"Remove");
			Remove();
			Print(div);
			Print("## REMOVE ALL ##");
			DrawProgress(3,5,"Remove All");
			RemoveAll();
			Print(div);
			Print("## DISPATCH ##");
			DrawProgress(4,5,"Dispatch");
			Dispatch();
			#if UNITY_EDITOR
			EditorUtility.ClearProgressBar();
			#endif
			_writeCsv = false;
			var sb = new System.Text.StringBuilder();
			foreach (string s in _csv){
				sb.Append(s);
			}
			Print(sb.ToString());
			_csv.Clear();
		}

		#region Tests
		#if UNITY_EDITOR
		[MenuItem("Relay/Profile/Populate", false, 20)]
		#endif
		public static void Pop(){
			sw.Stop();
			sw.Reset();

			System.Action<int,float,string> multicast;
			List<System.Action<int,float,string>> list = new List<System.Action<int, float, string>>();
			Relay<int,float,string> rly = new Relay<int, float, string>();

			#region RELAY POPULATE fast
			for (int i = 0; i < LOOPS; ++i) {
				rly.RemoveAll();
				PopulateRelayTimed(rly);
			}
			LogStopwatch("Populate Relay (Allow Duplicates)", "ns", 1e6);
			sw.Reset();
			#endregion

			#region RELAY POPULATE slow
			for (int i = 0; i < LOOPS; ++i) {
				rly.RemoveAll();
				PopulateRelaySlowTimed(rly);
			}
			LogStopwatch("Populate Relay (Disallow Duplicates)", "ns", 1e6);
			sw.Reset();
			#endregion

			#region MULTICAST POPULATE
			for (int i = 0; i < LOOPS; ++i) {
				multicast = delegate{};
				PopulateMulticastTimed(ref multicast);
			}
			sw.Stop();
			LogStopwatch("Populate Multicast", "ns", 1e6);
			sw.Reset();
			#endregion

			#region LIST POPULATE fast
			for (int i = 0; i < LOOPS; ++i) {
				list.Clear();
				PopulateListTimed(list);
			}
			LogStopwatch("Populate List (Allow Duplicates)", "ns", 1e6);
			sw.Reset();
			#endregion

			#region LIST POPULATE slow
			for (int i = 0; i < LOOPS; ++i) {
				list.Clear();
				PopulateListSlowTimed(list);
			}
			LogStopwatch("Populate List (Disallow Duplicates)", "ns", 1e6);
			sw.Reset();
			#endregion

			PrintLogs();
		}
		#if UNITY_EDITOR
		[MenuItem("Relay/Profile/Create and Populate (re-alloc each loop)", false, 21)]
		#endif
		public static void CreateAndPop(){
			sw.Stop();
			sw.Reset();

			#region RELAY POPULATE fast
			for (int i = 0; i < LOOPS; ++i) {
				Relay<int,float,string> rly = new Relay<int, float, string>();
				PopulateRelayTimed(rly);
			}
			LogStopwatch("Create and Populate Relay (Allow Duplicates)", "ns", 1e6);
			sw.Reset();
			#endregion

			#region RELAY POPULATE slow
			for (int i = 0; i < LOOPS; ++i) {
				Relay<int,float,string> rly = new Relay<int, float, string>();
				PopulateRelaySlowTimed(rly);
			}
			LogStopwatch("Create and Populate Relay (Disallow Duplicates)", "ns", 1e6);
			sw.Reset();
			#endregion

			#region MULTICAST POPULATE
			for (int i = 0; i < LOOPS; ++i) {
				System.Action<int,float,string> multicast = delegate {};
				PopulateMulticastTimed(ref multicast);
			}
			sw.Stop();
			LogStopwatch("Create and Populate Multicast", "ns", 1e6);
			sw.Reset();
			#endregion

			#region LIST POPULATE fast
			for (int i = 0; i < LOOPS; ++i) {
				List<System.Action<int,float,string>> list = new List<System.Action<int, float, string>>();
				PopulateListTimed(list);
			}
			LogStopwatch("Create and Populate List (Allow Duplicates)", "ns", 1e6);
			sw.Reset();
			#endregion

			#region LIST POPULATE slow
			for (int i = 0; i < LOOPS; ++i) {
				List<System.Action<int,float,string>> list = new List<System.Action<int, float, string>>();
				PopulateListSlowTimed(list);
			}
			LogStopwatch("Create and Populate List (Disallow Duplicates)", "ns", 1e6);
			sw.Reset();
			#endregion

			PrintLogs();
		}
		#if UNITY_EDITOR
		[MenuItem("Relay/Profile/Remove", false, 22)]
		#endif
		public static void Remove(){
			sw.Stop();
			sw.Reset();

			System.Action<int,float,string> multicast;
			List<System.Action<int,float,string>> list = new List<System.Action<int, float, string>>();
			Relay<int,float,string> rly = new Relay<int, float, string>();

			#region RELAY REMOVE
			for (int i = 0; i < LOOPS; ++i) {
				PopulateRelay(rly);
				for (int j = 0; j < DELEGATES; ++j) {
					var d = allDelegates[j];
					sw.Start();
					rly.RemoveListener(d);
					sw.Stop();
				}
			}
			LogStopwatch("Remove Relay", "ns", 1e6);
			sw.Reset();
			#endregion

			#region MULTICAST REMOVE
			for (int i = 0; i < LOOPS; ++i) {
				multicast = delegate{};
				PopulateMulticast(ref multicast);
				for (int j = 0; j < DELEGATES; ++j) {
					var d = allDelegates[j];
					sw.Start();
					multicast -= d;
					sw.Stop();
				}
			}
			LogStopwatch("Remove Multicast", "ns", 1e6);
			sw.Reset();
			#endregion

			#region LIST REMOVE
			for (int i = 0; i < LOOPS; ++i) {
				PopulateList(list);
				for (int j = 0; j < DELEGATES; ++j) {
					var d = allDelegates[j];
					sw.Start();
					list.Remove(d);
					sw.Stop();
				}
			}
			LogStopwatch("Remove List", "ns", 1e6);
			sw.Reset();
			#endregion

			PrintLogs();
		}
		#if UNITY_EDITOR
		[MenuItem("Relay/Profile/Remove All", false, 23)]
		#endif
		public static void RemoveAll(){
			sw.Stop();
			sw.Reset();

			System.Action<int,float,string> multicast;
			List<System.Action<int,float,string>> list = new List<System.Action<int, float, string>>();
			Relay<int,float,string> rly = new Relay<int, float, string>();

			#region RELAY CLEAR
			rly.RemoveAll();
			for (int i = 0; i < LOOPS; ++i) {
				PopulateRelay(rly);
				sw.Start();
				rly.RemoveAll();
				sw.Stop();
			}
			LogStopwatch("Clear Relay", "ns", 1e6);
			sw.Reset();
			#endregion

			#region MULTICAST CLEAR
			multicast = delegate{};
			for (int i = 0; i < LOOPS; ++i) {
				PopulateMulticast(ref multicast);
				sw.Start();
				multicast = delegate{};
				sw.Stop();
			}
			LogStopwatch("Clear Multicast", "ns", 1e6);
			sw.Reset();
			#endregion

			#region LIST CLEAR
			list.Clear();
			for (int i = 0; i < LOOPS; ++i) {
				PopulateList(list);
				sw.Start();
				list.Clear();
				sw.Stop();
			}
			LogStopwatch("Clear List", "ns", 1e6);
			sw.Reset();
			#endregion

			PrintLogs();
		}
		#if UNITY_EDITOR
		[MenuItem("Relay/Profile/Dispatch", false, 24)]
		#endif
		public static void Dispatch(){
			sw.Stop();
			sw.Reset();

			System.Action<int,float,string> multicast;
			List<System.Action<int,float,string>> list = new List<System.Action<int, float, string>>();
			Relay<int,float,string> rly = new Relay<int, float, string>();

			#region RELAY CALL
			rly.RemoveAll();
			PopulateRelay(rly);
			sw.Reset();
			for (int i=0; i<LOOPS; ++i){
				CallRelay(rly);
			}
			LogStopwatch("Call Relay", "ns", 1e6);
			sw.Reset();
			#endregion

			#region MULTICAST CALL
			multicast = delegate {};
			PopulateMulticast(ref multicast);
			sw.Reset();
			for (int i=0; i<LOOPS; ++i){
				CallMulticast(multicast);
			}
			LogStopwatch("Call Multicast", "ns", 1e6);
			sw.Reset();
			#endregion

			#region LIST CALL
			list.Clear();
			PopulateList(list);
			sw.Reset();
			for (int i=0; i<LOOPS; ++i){
				CallList(list);
			}
			LogStopwatch("Call List", "ns", 1e6);
			sw.Reset();
			#endregion

			#region ARRAY CALL
			System.Action<int,float,string>[] arr = new System.Action<int, float, string>[5];
			arr[0] = TestM1;
			arr[1] = TestM2;
			arr[2] = TestM3;
			arr[3] = TestM4;
			arr[4] = TestM5;
			for (int i=0; i<LOOPS; ++i){
				CallArray(arr);
			}
			LogStopwatch("Call Array", "ns", 1e6);
			sw.Reset();
			#endregion

			PrintLogs();
		}
		#endregion

		#region Helpers
		static void PopulateRelay(Relay<int,float,string> rly){
			for (int i=0; i<DELEGATES; ++i){
				rly.AddListener(allDelegates[i], true);
			}
		}
		static void PopulateRelayTimed(Relay<int,float,string> rly){
			for (int i=0; i<DELEGATES; ++i){
				var d = allDelegates[i];
				sw.Start();
				rly.AddListener(d, true);
				sw.Stop();
			}
		}
		static void PopulateRelaySlowTimed(Relay<int,float,string> rly){
			for (int i=0; i<DELEGATES; ++i){
				var d = allDelegates[i];
				sw.Start();
				rly.AddListener(d);
				sw.Stop();
			}
		}
		static void PopulateMulticast(ref System.Action<int,float,string> multicast){
			for (int i = 0; i < DELEGATES; ++i) {
				multicast += allDelegates[i];
			}
		}
		static void PopulateMulticastTimed(ref System.Action<int,float,string> multicast){
			for (int i = 0; i < DELEGATES; ++i) {
				var d = allDelegates[i];
				sw.Start();
				multicast += d;
				sw.Stop();
			}
		}
		static void PopulateList(List<System.Action<int,float,string>> list){
			for (int i = 0; i < DELEGATES; ++i) {
				list.Add(allDelegates[i]);
			}
		}
		static void PopulateListTimed(List<System.Action<int,float,string>> list){
			for (int i = 0; i < DELEGATES; ++i) {
				var d = allDelegates[i];
				sw.Start();
				list.Add(d);
				sw.Stop();
			}
		}
		static void PopulateListSlowTimed(List<System.Action<int,float,string>> list){
			for (int i = 0; i < DELEGATES; ++i) {
				var d = allDelegates[i];
				sw.Start();
				if (!list.Contains(d)) {
					list.Add(d);
				}
				sw.Stop();
			}
		}

		static void CallList(List<System.Action<int,float,string>> list){
			sw.Start();
			int max = list.Count;
			for (int j=0; j<max; ++j){
				list[j](0,1,"TEST");
			}
			sw.Stop();
		}
		static void CallArray(System.Action<int,float,string>[] arr){
			sw.Start();
			int max = arr.Length;
			for (int j=0; j<max; ++j){
				arr[j](0,1,"TEST");
			}
			sw.Stop();
		}
		static void CallRelay(Relay<int,float,string> rly){
			sw.Start();
			rly.Dispatch(0,1,"TEST");
			sw.Stop();
		}
		static void CallMulticast(System.Action<int,float,string> multicast){
			sw.Start();
			multicast(0,1,"TEST");
			sw.Stop();
		}

		static void LogStopwatch(string prefix, string units, double mulFac){
			string total = sw.Elapsed.TotalMilliseconds.ToString();
			LOGS.Add(
				string.Format(
					"{0} [LOOPS:{1} - DELEGATES:{2}]\n{3}ms\n\tPER CALL: {4}{5}",
					prefix, LOOPS.ToString(), DELEGATES.ToString(), total,
					((sw.Elapsed.TotalMilliseconds * mulFac) / (LOOPS*DELEGATES)).ToString(), units
				)
			);
			if (_writeCsv){
				_csv.Add(string.Format("{0},{1}\n", prefix, total));
			}
		}
		static void PrintLogs(){
			foreach (string s in LOGS){
				Print(s);
			}
			LOGS.Clear();
		}
		static void Print(string s){
			#if UNITY_EDITOR
			Debug.Log(s);
			#else
			System.Console.WriteLine(s);
			#endif
		}
		#endregion

		#region Delegates
		#pragma warning disable 0219
		static void TestM1(int i, float f, string s){
			sw.Stop();
			float result = i+f+1;
			sw.Start();
		}
		static void TestM2(int i, float f, string s){
			sw.Stop();
			float result = i+f+2;
			sw.Start();
		}
		static void TestM3(int i, float f, string s){
			sw.Stop();
			float result = i+f+3;
			sw.Start();
		}
		static void TestM4(int i, float f, string s){
			sw.Stop();
			float result = i+f+4;
			sw.Start();
		}
		static void TestM5(int i, float f, string s){
			sw.Stop();
			float result = i+f+5;
			sw.Start();
		}
		static void TestM6(int i, float f, string s){
			sw.Stop();
			float result = i+f+6;
			sw.Start();
		}
		static void TestM7(int i, float f, string s){
			sw.Stop();
			float result = i+f+7;
			sw.Start();
		}
		static void TestM8(int i, float f, string s){
			sw.Stop();
			float result = i+f+8;
			sw.Start();
		}
		static void TestM9(int i, float f, string s){
			sw.Stop();
			float result = i+f+9;
			sw.Start();
		}
		static void TestM10(int i, float f, string s){
			sw.Stop();
			float result = i+f+10;
			sw.Start();
		}
		#pragma warning restore 0219
		#endregion
	}
}
#endif