﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging.Exceptions;
using Syncless.Filters;
namespace Syncless.Tagging
{
    public class Tag
    {
        private string _tagName;
        private long _lastUpdated;
        private long _created;
        private bool _isDeleted;
        private long _deletedDate;
        private List<TaggedPath> _pathList;
        private List<Filter> _filters;
        private long _filtersUpdated;
        private TagConfig _config;

        public string TagName
        {
            get { return _tagName; }
            set { this._tagName = value; }
        }
        public List<string> PathStringList
        {
            get
            {
                List<string> pathList = new List<string>();
                foreach (TaggedPath path in _pathList)
                {
                    pathList.Add(path.Path);
                }
                return pathList;
            }
        }
        public long LastUpdated
        {
            get { return _lastUpdated; }
            set { _lastUpdated = value; }
        }
        public long Created
        {
            get { return _created; }
            set { _created = value; }
        }
        public bool IsSeamless
        {
            get { return _config.IsSeamless; }
            set { _config.IsSeamless = value; }
        }
        public string ArchiveName
        {
            get { return _config.ArchiveFolderName; }
            set { _config.ArchiveFolderName = value; }
        }
        public int ArchiveCount
        {
            get { return _config.ArchiveCount; }
            set { _config.ArchiveCount = value; }
        }
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; }
        }
        public long DeletedDate
        {
            get { return _deletedDate; }
            set { _deletedDate = value; }
        }
        public List<TaggedPath> PathList
        {
            get { return _pathList; }
            set { _pathList = value; }
        }
        public List<Filter> Filters
        {
            get { return _filters; }
            set { _filters = value; }
        }
        public long FiltersUpdated
        {
            get { return _filtersUpdated; }
            set { _filtersUpdated = value; }
        }
        internal TagConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        public Tag(string tagname, long created)
        {
            this._tagName = tagname;
            this._created = created;
            this._lastUpdated = created;
            this._isDeleted = false;
            this._deletedDate = 0;
            this._pathList = new List<TaggedPath>();
            this._filters = new List<Filter>();
            this._filtersUpdated = created;
            this._config = new TagConfig();
        }

        public void RenameTag(string newname, long updated)
        {
            _tagName = newname;
            _lastUpdated = updated;
        }

        public bool AddPath(string path, long created)
        {
            if (!Contains(path))
            {
                TaggedPath taggedPath = new TaggedPath();
                taggedPath.Path = path;
                taggedPath.LogicalDriveId = TaggingHelper.GetLogicalID(path);
                taggedPath.Created = created;
                taggedPath.LastUpdated = created;
                _lastUpdated = TaggingHelper.GetCurrentTime();
                _pathList.Add(taggedPath);
                return true;
            }
            return false;
        }

        public bool AddPath(TaggedPath path)
        {
            if (!Contains(path.Path))
            {
                _lastUpdated = TaggingHelper.GetCurrentTime();
                _pathList.Add(path);
                return true;
            }
            return false;
        }

        public bool RemovePath(string path, long lastupdated)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.Path.Equals(path))
                {
                    _pathList.Remove(p);
                    _lastUpdated = TaggingHelper.GetCurrentTime();
                    return true;
                }
            }
            return false;
        }

        public bool RemovePath(TaggedPath path)
        {
            bool removed = _pathList.Remove(path);
            if (removed)
            {
                _lastUpdated = TaggingHelper.GetCurrentTime();
            }
            return removed;
        }

        public void RemoveAllPaths()
        {
            while (_pathList.Count > 0)
            {
                _pathList.RemoveAt(0);
            }
        }

        public bool Contains(string path)
        {
            foreach (TaggedPath p in _pathList)
            {
                if ((p.Path.ToLower()).Equals(path.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        public void RenamePath(string oldPath, string newPath, long updated)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.Path.StartsWith(oldPath))
                {
                    p.Replace(oldPath, newPath);
                }
                else if (p.Path.Equals(oldPath))
                {
                    p.Path = newPath;
                }
                _lastUpdated = updated;
            }
        }

        public string FindMatchedParentDirectory(string path, bool isFolder)
        {
            string[] pathTokens = TaggingHelper.TrimEnd(path.Split('\\'));
            string logicalid = TaggingHelper.GetLogicalID(path);
            foreach (TaggedPath p in _pathList)
            {
                if (path.StartsWith(p.Path))
                {
                    if (!path.Equals(p.Path))
                    {
                        string[] pTokens = TaggingHelper.TrimEnd(p.Path.Split('\\'));
                        int trailingIndex = TaggingHelper.Match(pathTokens, pTokens);
                        if (trailingIndex > 0)
                        {
                            return TaggingHelper.CreatePath(trailingIndex, pathTokens, isFolder);
                        }
                    }
                }
            }
            return null;
        }

        public void AddFilter(Filter filter, long updated)
        {
            if (!_filters.Contains(filter))
            {
                _filters.Add(filter);
                _filtersUpdated = updated;
                _lastUpdated = TaggingHelper.GetCurrentTime();
            }
        }

        public Filter RemoveFilter(Filter filter, long updated)
        {
            if (_filters.Contains(filter))
            {
                _filters.Remove(filter);
                _filtersUpdated = updated;
                _lastUpdated = updated;
                return filter;
            }
            else
            {
                return null;
            }
        }

        #region private implementations
        protected TaggedPath RetrieveTaggedPath(string path)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.Path.Equals(path))
                {
                    return p;
                }
            }
            return null;
        }

        #region Deprecated
        /// <summary>
        /// Deprecated - Find if pathTokens starts with pTokens (replaced by StartsWith method of String class)
        /// </summary>
        /// <param name="pathTokens"></param>
        /// <param name="pTokens"></param>
        /// <returns></returns>
        protected bool CheckMatch(string[] pathTokens, string[] pTokens)
        {
            for (int i = 0; i < pTokens.Length; i++)
            {
                if (!pTokens[i].Equals(pathTokens[i]))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #endregion
    }
}
