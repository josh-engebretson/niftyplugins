using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.IO;

// Setup stuff taken from: http://blogs.msdn.com/jim_glass/archive/2005/08/18/453218.aspx
namespace Aurora
{

	namespace NiftySolution
	{
		// This class is the required interface to Visual Studio. 
		// Simple a very lightweight wrapper around the plugin object.
		public class Connect : IDTExtensibility2, IDTCommandTarget
		{
			private Plugin m_plugin;

			public Connect()
			{
			}

			public void OnConnection(object application_, ext_ConnectMode connectMode, object addInInst, ref Array custom)
			{
				DTE2 application = (DTE2)application_;
				m_plugin = new Plugin(application);

				// Start registering all the stuff that we know about...
				m_plugin.RegisterCommand((AddIn)addInInst,
						"NiftyOpen", "", "", "Pops open a dialog with all the files in the solution",
						new Plugin.OnCommandFunction(new QuickOpen(application).OnCommand));
				m_plugin.RegisterCommand((AddIn)addInInst,
						"NiftyToggle", "", "", "Toggles the current .h/.cpp file",
						new Plugin.OnCommandFunction(new ToggleFile(application).OnCommand));
			}

			public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
			{
			}

			public void OnAddInsUpdate(ref Array custom)
			{
			}

			public void OnStartupComplete(ref Array custom)
			{
			}

			public void OnBeginShutdown(ref Array custom)
			{
			}

			public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
			{
				if (neededText != vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
					return;

				if (!m_plugin.CanHandleCommand(commandName))
					return;

				status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
			}

			public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
			{
				handled = false;
				if (executeOption != vsCommandExecOption.vsCommandExecOptionDoDefault)
					return;

				handled = m_plugin.OnCommand(commandName);
			}
		}
	}

}