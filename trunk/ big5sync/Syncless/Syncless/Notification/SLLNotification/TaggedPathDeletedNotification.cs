﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;

namespace Syncless.Notification.SLLNotification
{
    public class TaggedPathDeletedNotification : AbstractNotification
    {
        private List<string> _deletedPaths;
        public List<string> DeletedPaths
        {
          get { return _deletedPaths; }
          set { _deletedPaths = value; }
        }

        public TaggedPathDeletedNotification(List<string> deletedPaths)
            : base("Tagged Path Deleted Notification", Syncless.Notification.NotificationCode.TAGGED_PATH_DELETED_NOTIFICATION)
        {
            this._deletedPaths = deletedPaths;
        }
    }
}
