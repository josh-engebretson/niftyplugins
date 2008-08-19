// Copyright (C) 2006-2007 Jim Tilander. See COPYING for and README for more details.
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
			private SolutionBuildTimings m_timings;

			public Connect()
			{
			}

			public void OnConnection(object application_, ext_ConnectMode connectMode, object addInInst, ref Array custom)
			{
				if(null != m_plugin)
					return;

				DTE2 application = (DTE2)application_;
				m_plugin = new Plugin(application, (AddIn)addInInst, "NiftySolution", "Aurora.NiftySolution.Connect");

				// Initialize the logging system.
				if(Log.HandlerCount == 0)
				{
#if DEBUG
					Log.AddHandler(new DebugLogHandler());
#endif

					Log.AddHandler(new VisualStudioLogHandler(m_plugin.OutputPane));
					Log.Prefix = "NiftySolution";
				}

				// Now we can take care of registering ourselves and all our commands and hooks.
				Log.Debug("Booting up...");
				Log.IncIndent();

				m_plugin.RegisterCommand("NiftyOpen", new QuickOpen());
				m_plugin.RegisterCommand("NiftyToggle", new ToggleFile());
				m_plugin.RegisterCommand("NiftyClose", new CloseToolWindow());

				CommandBar commandBar = m_plugin.AddCommandBar("NiftySolution", MsoBarPosition.msoBarTop);
				m_plugin.AddToolbarCommand(commandBar, "NiftyOpen", "Shows the quickopen dialog", "Opens any file in the solution", 1, 1);
				m_plugin.AddToolbarCommand(commandBar, "NiftyToggle", "Quickly toggles between .h/.cpp", "Quickly toggles between .h/.cpp", 2, 2);

				m_plugin.AddConsoleOnlyCommand("NiftyOpen", "Open Dialog", "Quickly open any file in the solution");
				m_plugin.AddConsoleOnlyCommand("NiftyToggle", "Toggle .h/.cpp", "Quickly go between .cpp and .h file");
				m_plugin.AddConsoleOnlyCommand("NiftyClose", "Close Tool Windows", "Closes all the tool windows that clutter the space");

				m_timings = new SolutionBuildTimings(application);

				Log.DecIndent();
				Log.Debug("Initialized...");
			}

			public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
			{
				if(null == m_plugin)
					return;

				Log.Debug("Disconnect called...");

				// TODO: This could be a good place to save the configuration out to disk.

				Log.ClearHandlers();
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
				if(null == m_plugin)
					return;

				if(neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone &&
					m_plugin.CanHandleCommand(commandName))
				{
					if(m_plugin.IsCommandEnabled(commandName))
						status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
					else
						status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported;
				}
			}

			public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
			{
				if(null == m_plugin)
					return;
				handled = false;
				if(executeOption != vsCommandExecOption.vsCommandExecOptionDoDefault)
					return;

				Log.Debug("Trying to execute command \"{0}\"", commandName);
				handled = m_plugin.OnCommand(commandName);
				if(handled)
					Log.Debug("Command handled");
			}
		}
	}
}
