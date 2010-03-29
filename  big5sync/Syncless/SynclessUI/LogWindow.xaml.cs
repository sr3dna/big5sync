﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using Syncless.Logging;
using Syncless.Core.Exceptions;
using SynclessUI.Helper;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {		
		private MainWindow _main;
        private DataTable _LogData;
        
		public LogWindow(MainWindow main)
        {
            InitializeComponent();
			
			_main = main;
		    bool encounteredError = false;
            
            try
            {
                List<LogData> log = _main.Gui.ReadLog();
                PopulateLogData(log);
            }  catch (LogFileCorruptedException)
            {
                encounteredError = true;
                DialogsHelper.ShowError("Log File Corrupted", "Stored log files have been corrupted and will be deleted.");
            }
            catch (UnhandledException)
            {
                encounteredError = true;
                DialogsHelper.DisplayUnhandledExceptionMessage();
            }

            if (encounteredError)
            {
                CloseWindow();
            }
            else
            {
                this.ShowDialog();
            }
        }

        private void PopulateLogData(List<LogData> log)
        {
            _LogData = new DataTable();
            _LogData.Columns.Add(new DataColumn("category", typeof(string)));
            _LogData.Columns.Add(new DataColumn("eventtype", typeof(string)));
            _LogData.Columns.Add(new DataColumn("message", typeof(string)));
            _LogData.Columns.Add(new DataColumn("timestamp", typeof(string)));
            
            foreach (LogData l in log)
            {
                LogEventType @event = l.LogEvent;

                var row = _LogData.NewRow();
                _LogData.Rows.Add(row);

                string category = "";
                string eventType = "";

                switch(l.LogCategory)
                {
                    case LogCategoryType.APPEVENT:
                        category = "Application";
                        break;
                    case LogCategoryType.FSCHANGE:
                        category = "Filesystem";
                        break;
                    case LogCategoryType.SYNC:
                        category = "Synchronization";
                        break;
                    case LogCategoryType.UNKNOWN:
                        category = "Unknown";
                        break;
                }

                switch(l.LogEvent)
                {
                    case LogEventType.SYNC_STARTED:
                        eventType = "Synchronization Started";
                        break;
                    case LogEventType.SYNC_STOPPED:
                        eventType = "Synchronization Stopped";
                        break;
                    case LogEventType.APPEVENT_DRIVE_ADDED:
                        eventType = "Drive Added";
                        break;
                    case LogEventType.APPEVENT_DRIVE_RENAMED:
                        eventType = "Drive Renamed";
                        break;
                    case LogEventType.APPEVENT_PROFILE_LOAD_FAILED:
                        eventType = "Profile Loading Failed";
                        break;
                    case LogEventType.APPEVENT_TAG_CREATED:
                        eventType = "Tag Created";
                        break;
                    case LogEventType.APPEVENT_TAG_DELETED:
                        eventType = "Tag Deleted";
                        break;
                    case LogEventType.APPEVENT_TAG_CONFIG_UPDATED:
                        eventType = "Tag Updated";
                        break;
                    case LogEventType.APPEVENT_FOLDER_TAGGED:
                        eventType = "Folder Tagged";
                        break;
                    case LogEventType.APPEVENT_FOLDER_UNTAGGED:
                        eventType = "Folder Untagged";
                        break;
                    case LogEventType.FSCHANGE_CREATED:
                        eventType = "File Created";
                        break;
                    case LogEventType.FSCHANGE_MODIFIED:
                        eventType = "File Modified";
                        break;
                    case LogEventType.FSCHANGE_DELETED:
                        eventType = "File Deleted";
                        break;
                    case LogEventType.FSCHANGE_RENAMED:
                        eventType = "File Renamed";
                        break;
                    case LogEventType.FSCHANGE_ERROR:
                        eventType = "File Error";
                        break;
                    case LogEventType.UNKNOWN:
                        category = "Unknown";
                        break;
                }

                row["category"] = category;
                row["eventtype"] = eventType;
                row["message"] = l.Message;
                row["timestamp"] = l.Timestamp;
            }
        }

        private DataTable LogData()
        {
            return _LogData;
        } 

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			CloseWindow();
        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	CloseWindow();
        }

		private void Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			this.DragMove();
		}
		
		private void CloseWindow() {
            FormFadeOut.Begin();
		}
		
        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
