using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;

namespace Aurora
{

	namespace NiftyPerforce
	{
		// Main stub that interfaces towards Visual Studio.
		public class Connect : IDTExtensibility2, IDTCommandTarget
		{
			#region "Unused interface"
			public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom) { }
			public void OnAddInsUpdate(ref Array custom) { }
			public void OnStartupComplete(ref Array custom) { }

			#endregion

			private Plugin m_plugin = null;

			public Connect()
			{
			}

			public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
			{
				m_plugin = new Plugin((DTE2)application, (AddIn)addInInst);
			}


			public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
			{
				if (neededText != vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
					return;

				m_plugin.m_perforce.OnQueryStatus(ref status, commandName);
			}

			public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
			{
				handled = false;
				if (executeOption != vsCommandExecOption.vsCommandExecOptionDoDefault)
					return;

				handled = m_plugin.m_perforce.OnExecuteCommand(commandName);
			}

			public void OnBeginShutdown(ref Array custom)
			{
				//TODO: Make this thing unregister all the callbacks we've just made... gahhh... C# and destructors... 
			}
		}
	}

}
