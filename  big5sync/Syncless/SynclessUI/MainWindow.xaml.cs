﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SynclessUI;
using Syncless.Core;
using Syncless.Tagging;
using System.Collections;
using System.IO;
using SynclessUI.Helper;
using System.Windows.Navigation;
using System.Diagnostics;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IUIInterface
    {
        public IUIControllerInterface gui;

        private const string BI_DIRECTIONAL = "Bi-Dir..";
        private const string UNI_DIRECTIONAL = "Uni-Dir..";
        private string _selectedTag {
            get { if (ListBoxTag.SelectedItem == null) return null; else return ListBoxTag.SelectedItem.ToString(); }
        }
        private string _filter {
            get { return TxtBoxFilterTag.Text.Trim(); }
        }
        private string _app_path;
        private bool _firstopen = true;
        
        public MainWindow() {
            MinimizeToTray.Enable(this);

            InitializeComponent();
            InitializeSyncless();
        }

        #region Application Settings

        private void SaveApplicationSettings()
        {
            Properties.Settings.Default.Save();
        }

        #endregion

        /// <summary>
        ///     Starts up the system logic layer and initializes it
        /// </summary>
        private void InitializeSyncless()
        {
            _app_path = @System.Reflection.Assembly.GetExecutingAssembly().Location;
            gui = ServiceLocator.GUI;

            if (gui.Initiate(this))
            {
                RegistryHelper.CreateRegistry(_app_path);
                InitializeTagInfoPanel();
                InitializeTagList();
            }
            else {
                string messageBoxText = "Syncless has failed to initialize and will now exit.";
                string caption = "Syncless Initialization Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);

                this.Close();
            }
        }

        private void ListBoxTag_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ListBoxTag.SelectedItem != null)
            {
                ViewTagInfo(ListBoxTag.SelectedItem.ToString());
            }
        }

        public void ViewTagInfo(string tagname)
        {
            TagView tv = gui.GetTag(tagname);

            TagTitle.Text = tagname;
			// tag.direction not implemented yet
			
            if (tv.IsSeamless)
            {
                AutoMode();
            }
            else
            {
                ManualMode();
            }

            LblStatusText.Content = "";
            ListTaggedPath.ItemsSource = tv.PathStringList;
			
			TagIcon.Visibility = System.Windows.Visibility.Visible;
			TagStatusPanel.Visibility = System.Windows.Visibility.Visible;
			SyncPanel.Visibility = System.Windows.Visibility.Visible;
            if (tv.PathStringList.Count == 0)
            {
                BdrTaggedPath.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                BdrTaggedPath.Visibility = System.Windows.Visibility.Visible;
            }
        }
		
        /// <summary>
        ///     Gets the list of tags and then populates the Tag List Box and keeps a count
        /// </summary>
        public void InitializeTagList()
        {
            List<string> taglist = gui.GetAllTags();

            ListBoxTag.ItemsSource = taglist;

            LblTagCount.Content = "[" + taglist.Count + "/" + taglist.Count + "]";

            if (taglist.Count != 0)
            {
                SelectTag(taglist[0]);
            }
        }

        public void SelectTag(string tagname)
        {
            List<string> taglist = gui.GetAllTags();
            int index = taglist.IndexOf(tagname);
            ListBoxTag.SelectedIndex = index;
            ViewTagInfo(tagname);
        }
		
        /// <summary>
        ///     If lists of tag is empty, reset the UI back to 0, else displayed the first Tag on the list.
        /// </summary>
		private void InitializeTagInfoPanel()
		{
            List<string> taglist = gui.GetAllTags();

            if (taglist.Count == 0)
            {
                TagTitle.Text = "Select a Tag";
                TagIcon.Visibility = System.Windows.Visibility.Hidden;
                TagStatusPanel.Visibility = System.Windows.Visibility.Hidden;
                SyncPanel.Visibility = System.Windows.Visibility.Hidden;
                BdrTaggedPath.Visibility = System.Windows.Visibility.Hidden;
            }
		}

        /// <summary>
        ///     Makes the title bar draggable and movable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }		
		
        /// <summary>
        ///     Sets the behavior of the close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        ///     Sets the behavior of the minimize button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMin_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
			this.WindowState = WindowState.Minimized;
        }

        private void BtnDirection_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	if(string.Compare((string) LblDirection.Content, UNI_DIRECTIONAL) == 0)
			{
				LblDirection.Content = BI_DIRECTIONAL;
			}
			else
			{
                LblDirection.Content = UNI_DIRECTIONAL;
			}
        }

        private void BtnSyncMode_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	if(string.Compare((string) LblSyncMode.Content, "Manual") == 0)
			{
                if (gui.MonitorTag(_selectedTag, true))
                {
                    AutoMode();
                }
                else
                {
                    string messageBoxText = "' " + _selectedTag + " ' could not be set into Seamless Mode.";
                    string caption = "Change Synchronization Mode Error";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxText, caption, button, icon);
                }
			}
			else
			{
                if (gui.MonitorTag(_selectedTag, false))
                {
                    ManualMode();
                }
                else
                {
                    string messageBoxText = "' " + _selectedTag + " ' could not be set into Manual Mode.";
                    string caption = "Change Synchronization Mode Error";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxText, caption, button, icon);
                }
			}
        }

        private void AutoMode()
        {
            LblSyncMode.Content = "Auto";
            BtnPreview.Visibility = System.Windows.Visibility.Hidden;
            BtnSyncNow.Visibility = System.Windows.Visibility.Hidden;
        }

        private void ManualMode()
        {
            LblSyncMode.Content = "Manual";
            BtnPreview.Visibility = System.Windows.Visibility.Visible;
            BtnSyncNow.Visibility = System.Windows.Visibility.Visible;
        }

        private void BtnSyncNow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!gui.StartManualSync(_selectedTag))
            {
                string messageBoxText = "' " + _selectedTag + " ' could not be synchronized.";
                string caption = "Synchronization Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);
			}
        }

        private void btnRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_selectedTag != null)
            {
                string messageBoxText = "Are you sure you want to delete the tag '" + _selectedTag + "'?";
                string caption = "Delete Tag";
                MessageBoxButton button = MessageBoxButton.OKCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;

                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

                switch (result)
                {
                    case MessageBoxResult.OK:
                        bool success = gui.DeleteTag(_selectedTag);
                        if(success)
                        {
                            InitializeTagList();
                            InitializeTagInfoPanel();
                        }
                        else
                        {
                            messageBoxText = "' " + _selectedTag + " ' could not be deleted.";
                            caption = "Delete Tag Error";
                            button = MessageBoxButton.OK;
                            icon = MessageBoxImage.Error;

                            MessageBox.Show(messageBoxText, caption, button, icon);
                        }
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
            else
            {
                string messageBoxText = "Please select a tag.";
                string caption = "No Tag Selected";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);
            }
        }
		
		public bool CreateTag(string tagName) {
            try
            {
                TagView tv = gui.CreateTag(tagName);
                if (tv != null)
                {
                    InitializeTagList();
                    SelectTag(tagName);
                }
                else
                {
                    string messageBoxText = "Tag could not be created.";
                    string caption = "Tag Creation Error";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxText, caption, button, icon);
                }
            }
            catch (Syncless.Tagging.Exceptions.TagAlreadyExistsException)
            {
				return true;
            }
			
			return false;
		}

		private void BtnCreate_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			CreateTagWindow ctw = new CreateTagWindow(this);
			
			ctw.ShowDialog();
		}

		private void btnTag_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            TagWindow tw = new TagWindow(this, "", _selectedTag);
        }

		private void TxtBoxFilterTag_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
            if (gui != null)
            {
                List<string> taglist = gui.GetAllTags();
                List<string> filteredtaglist = new List<string>();

                int initial = taglist.Count;

                foreach (string x in taglist)
                {
                    if (x.ToLower().Contains(_filter.ToLower()))
                        filteredtaglist.Add(x);
                }

                int after = filteredtaglist.Count;

                LblTagCount.Content = "[" + after + "/" + initial + "]";

                ListBoxTag.ItemsSource = filteredtaglist;
            }
		}

		private void btnUntag_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            Untag();
		}

        private void Untag()
        {
            if (!ListTaggedPath.HasItems)
            {
                string messageBoxText = "There is nothing to untag.";
                string caption = "Nothing to Untag";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);
            }
            else
            {
                if (ListTaggedPath.SelectedIndex == -1)
                {
                    string messageBoxText = "Please select a path to untag.";
                    string caption = "No Path Selected";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxText, caption, button, icon);
                }
                else
                {
                    TagView tv = gui.GetTag((string)TagTitle.Text);

                    gui.Untag(tv.TagName, new DirectoryInfo((string)ListTaggedPath.SelectedValue));

                    SelectTag(tv.TagName);
                }
            }
        }

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
            // Prepares the SLL for termination

            if (gui.PrepareForTermination())
            {
                string messageBoxText = "Are you sure you want to exit Syncless?" +
                    "\nExiting Syncless will disable seamless synchronization.";
                string caption = "Exit";
                MessageBoxButton button = MessageBoxButton.OKCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;

                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

                switch (result)
                {
                    case MessageBoxResult.OK:
                        // Terminates the SLL and closes the UI
                        gui.Terminate();
                        SaveApplicationSettings();
                        if (Properties.Settings.Default.PersistRegistryIntegration == false)
                        {
                            RegistryHelper.RemoveRegistry();
                        }
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
            else
            {
                string messageBoxText = "Syncless is not ready for termination. Please try again later";
                string caption = "Syncless Termination Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);
                e.Cancel = true;
            }
        }

        #region Commandline Interface: Tag/Untag

        public void ProcessCommandLine(string[] args)
        {
            if (args.Length != 0)
            {
                SynclessUI.Helper.CommandLineHelper.ProcessCommandLine(args, this);
            }
        }

        public void CLI_Tag(string clipath)
        {
            TagWindow tw = new TagWindow(this, clipath, "");
            if (_firstopen == true)
            {
                this.WindowState = WindowState.Minimized;
                _firstopen = false;
            }
        }

        public void CLI_Untag(string clipath)
        {
            UntagWindow tw = new UntagWindow(this, clipath);
            if (_firstopen == true)
            {
                this.WindowState = WindowState.Minimized;
                _firstopen = false;
            }
        }

        #endregion

        #region TagTitle Functionality: Renaming

        private bool RenameTag(String oldtagname, String newtagname)
        {
            /*
            if (gui.RenameTag(oldtagname, newtagname))
            {
                InitializeTagList();
                SelectTag(newtagname);
                return true;
            }
            else
            {
                string messageBoxText = "Tag could not be renamed. There might be another tag with the same name.";
                string caption = "Rename Tag Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);

                return false;
            }
            */
            return true;
        }

        private void TagTitle_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_selectedTag == TagTitle.Text) return;
            if (!RenameTag(_selectedTag, TagTitle.Text)) TagTitle.Text = _selectedTag;
        }

        private void TagTitleOnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TagTitle.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
        
        #endregion 

        #region Tag Info Panel Context Menu

        private void OpenInExplorerRightClick_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			OpenFolderInWindowsExplorer();
        }

        private void ListTaggedPath_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	OpenFolderInWindowsExplorer();
        }

        private void OpenFolderInWindowsExplorer()
        {
            String path = (string)ListTaggedPath.SelectedItem;
            if (path != "")
            {
                var runExplorer = new System.Diagnostics.ProcessStartInfo();
                runExplorer.FileName = "explorer.exe";
                runExplorer.Arguments = path;
                System.Diagnostics.Process.Start(runExplorer);
            }
        }

        #endregion

        #region Implements Methods & Supporting Methods in IUIInterface

        public string getAppPath()
        {
            return System.IO.Path.GetDirectoryName(_app_path);
        }

        public void DriveChanged()
        {
            RepopulateTagList();
        }

        public void TagChanged()
        {
            RepopulateTagList();
        }

        private void RepopulateTagList()
        {
            String current = _selectedTag; // current tag selected
            InitializeTagList();
            SelectTag(current);
        }

        #endregion
		
		private void BtnDetails_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			ViewTagDetails();
        }

		private void TagIcon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			ViewTagDetails();
		}
		
		private void ViewTagDetails() {
			if(_selectedTag != null) {
				TagDetailsWindow tdw = new TagDetailsWindow(_selectedTag, this);
				tdw.ShowDialog();
			}
		}

		private void BtnOptions_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			OptionsWindow ow = new OptionsWindow();
			ow.ShowDialog();
		}
		
		private void OpenSynclessWebpage()
  		{
			Process.Start(new ProcessStartInfo("http://code.google.com/p/big5sync/"));
  		}

		private void SynclessLogo_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			OpenSynclessWebpage();
		}

		private void BtnPreview_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			PreviewSyncWindow psw = new PreviewSyncWindow(this, _selectedTag);
			psw.ShowDialog();
		}

		private void BtnEject_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            ContextMenu driveMenu = new ContextMenu();

            List<DriveInfo> removableDrives = this.GetAllRemovableDrives();
            if (removableDrives.Count == 0)
            {
                MenuItem driveMenuItem = new MenuItem();
                driveMenuItem.Header = "No Removable Drives Found";
                driveMenu.Items.Add(driveMenuItem);
            }
            else
            {
                foreach (DriveInfo di in removableDrives)
                {
                    MenuItem driveMenuItem = new MenuItem();
                    driveMenuItem.Header = di.Name;
                    driveMenuItem.Click += new RoutedEventHandler(driveMenuItem_Click);
                    driveMenu.Items.Add(driveMenuItem);
                }
            }
			
			driveMenu.PlacementTarget = this;
			driveMenu.IsOpen = true;
        }
		
		private List<DriveInfo> GetAllRemovableDrives() {
            DriveInfo[] allDrives = System.IO.DriveInfo.GetDrives();
            List<DriveInfo> removableDrives = new List<DriveInfo>();

            foreach (DriveInfo di in allDrives)
            {
                if(di.DriveType == DriveType.Removable) {
                    removableDrives.Add(di);
                }
            }

            return removableDrives;
		}

        void driveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem source = (MenuItem)sender;
            string driveletter = (string) source.Header;
            DriveInfo drive = new DriveInfo(driveletter);
            if (!gui.AllowForRemoval(drive))
            {
                string messageBoxText = "Syncless could not prepare " + driveletter + " for removal.";
                string caption = "Drive Removal Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);
            }
            else
            {
                string messageBoxText = "You may proceed to eject " + driveletter;
                string caption = "Drive Allowed for Removal";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);
            }
		}
    }
}
